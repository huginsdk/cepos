using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Printer
{
    public class TLV
    {
        private int tag;
        private int length;
        private byte[] value;

        public int Tag
        {
            get { return tag; }
        }
        public int Length
        {
            get { return length; }
        }
        public byte[] Value
        {
            get { return value; }
        }

        public TLV(int tag, int length, byte[] value)
        {
            this.tag = tag;
            this.length = length;
            this.value = value;
        }

        /// <summary>
        /// Parse TLV data 
        /// </summary>
        /// <param name="data"></param>
        /// byte array of tlv 
        /// <returns>parsed tlv class</returns>
        internal static TLV[] Parse(byte[] bytesRead)
        {
            int index = 0;
            List<TLV> tlvList = new List<TLV>();
            //int len = MessageBuilder.GetLength(bytesRead, index, out index);
            while (index < bytesRead.Length)
            {
                int tag = MessageBuilder.GetTag(bytesRead, index, out index);
                if (tag == 14675986)
                {
                    break;
                }
                int tagLenValue = MessageBuilder.GetLength(bytesRead, index, out index);
                byte[] value = new byte[tagLenValue];

                Array.Copy(bytesRead, index, value, 0, tagLenValue);
                index += tagLenValue;

                tlvList.Add(new TLV(tag, tagLenValue, value));
            }

            return tlvList.ToArray();
        }
    }
    public class TLVGroup
    {
        private int groupTag;
        private List<TLV> tags;

        public int GroupTag
        {
            get { return groupTag; }
        }
        public List<TLV> Tags
        {
            get { return tags; }
        }

        public TLVGroup(int groupTag)
        {
            this.groupTag = groupTag;
            tags = new List<TLV>();
        }

        public void Add(TLV tlv)
        {
            tags.Add(tlv);
        }

        public TLV FindTag(int tag)
        {
            TLV matchTlv = this.Tags.Find(delegate(TLV tlvItem)
            {
                return tlvItem.Tag == tag;
            });
            return matchTlv;
        }
    }
    public class FPUMessage
    {
        private int command;
        private List<TLVGroup> tlvGroups;
        private List<TLV> tags;

        public int Command
        {
            get { return command; }
        }
        public List<TLVGroup> TlvGroups
        {
            get { return tlvGroups; }
        }
        public List<TLV> Tags
        {
            get { return tags; }
        }
        private FPUMessage()
        {
            tlvGroups = new List<TLVGroup>();
            tags = new List<TLV>();
        }
        public FPUMessage(int command):this()
        {
            this.command = command;
        }

        public void AddGroup(TLVGroup tlvGroup)
        {
            tlvGroups.Add(tlvGroup);
        }

        public void AddTag(TLV tag)
        {
            tags.Add(tag);
        }
        public TLVGroup FindGroup(int groupTag)
        {
            TLVGroup matchTlv = this.TlvGroups.Find(delegate(TLVGroup grp)
            {
                return grp.GroupTag == groupTag;
            });
            return matchTlv;
        }
        public TLV FindTag(int tag)
        {
            TLV matchTlv = this.Tags.Find(delegate(TLV tlvItem)
            {
                return tlvItem.Tag == tag;
            });
            return matchTlv;
        }
        internal static FPUMessage Parse(byte[] bytesRead)
        {
            FPUMessage fpuMessage = new FPUMessage();
            int index = 0;
            int len = bytesRead.Length;
            int lastGrpId = 0;
            byte[] value;

            while (true)
            {
                try
                {
                    if (index >= (len))
                        break;
                    //get next tag
                    int currIndex = index + 1;
                    int tag = MessageBuilder.GetTag(bytesRead, index, out index);
                    int tagLength = MessageBuilder.GetLength(bytesRead, index, out index);

                    if (tag == FPUDataTags.ENDOFMSG)
                    {
                        index += tagLength;
                        continue;
                    }
                    value = new byte[tagLength];
                    Array.Copy(bytesRead, index, value, 0, tagLength);

                    // if it is tag
                    if (index - currIndex == GMPConstants.LEN_DATA_TAG)
                    {
                        TLV tlv = new TLV(tag, tagLength, value);
                        // check there is defined group id before
                        if (lastGrpId > 0)
                        {
                            TLVGroup matchTlv = fpuMessage.FindGroup(lastGrpId);
                            if (matchTlv == null)
                            {
                                matchTlv = new TLVGroup(tag);
                            }
                            matchTlv.Add(tlv);
                        }
                        else
                        {
                            fpuMessage.AddTag(tlv);
                        }
                        index += tagLength;
                    }
                    else
                    {
                        lastGrpId = tag;
                        fpuMessage.AddGroup(new TLVGroup(tag));
                    }

                }
                catch (System.Exception ex)
                {

                }
            }
            return fpuMessage;
        }
    }
}
