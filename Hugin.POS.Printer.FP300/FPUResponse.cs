using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    public class FPUResponse : IPrinterResponse
    {
        private const int RESPONSE_MSG_ID = 0xFF8F21;

        private string fiscalId;
        private int seqNum;
        private int errCode;
        private State fpuState;
        private byte[] tags;
        private byte[] buffer;
        private byte[] body;
        private string data = "";
        private string detail = "";

        public string FiscalId
        {
            get { return fiscalId; }
        }

        public int SequenceNum
        {
            get { return seqNum; }
        }

        public int ErrorCode
        {
            get { return errCode; }
        }

        public State FPUState
        {
            get { return fpuState; }
        }

        public byte[] Tags
        {
            get { return tags; }
        }

        public byte[] Buffer
        {
            get { return buffer; }
            set { buffer = value; }
        }

        public byte[] Body
        {
            get { return body; }
        }

        public FPUResponse()
        {

        }
        public FPUResponse(byte[] bytesRead, bool extractError)
        {
            int index = 0;
            int msgType = 0;
            try
            {
                //first 2 bytes  len
                byte lrc = 0;
                for (int i = 0; i < (bytesRead.Length - 1); i++)
                {
                    lrc ^= bytesRead[i];
                }
                if (bytesRead[bytesRead.Length - 1] != lrc)
                {
                    // Throw CRCException 
                    throw new Exception("LRC not match");
                }

                index = 0;
                //TERMINAL SERIAL
                List<byte> serial = new List<byte>();
                for (int i = 0; i < GMPConstants.LEN_SERIAL; i++)
                {
                    serial.Add(bytesRead[index + i]);
                }
                this.fiscalId = Encoding.ASCII.GetString(serial.ToArray());
                index += GMPConstants.LEN_SERIAL;

                //MESSAGE TYPE
                msgType = MessageBuilder.ByteArrayToHex(bytesRead, index, 3);


                /* get message len */
                index += 3;

                if (msgType == Identifier.KEY_RESPONSE)
                {
                    int tempIndex = 0;
                    int len = MessageBuilder.GetLength(bytesRead, index, out tempIndex);
                    tags = MessageBuilder.GetBytesFromOffset(bytesRead, index, len + tempIndex - index);
                    return;
                }

                if (msgType != RESPONSE_MSG_ID)
                {
                    if (msgType != Identifier.TRIPLEDES_RESPONSE)
                    {
                        throw new InvalidOperationException("Response Message Incorrect");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Message id error");
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid data");
            }
            try
            {
                int len = MessageBuilder.GetLength(bytesRead, index, out index);
                if (FiscalPrinter.TripleKey != null && msgType == RESPONSE_MSG_ID)
                {
                    byte[] msgBuffer=MessageBuilder.GetBytesFromOffset(bytesRead,index,len);
                    byte[] desData = MessageBuilder.DecryptTriple(msgBuffer, 
                                                    msgBuffer.Length,
                                                    FiscalPrinter.TripleKey);
                    bytesRead = desData;
                    body = bytesRead;
                    index = 0;
                    len = bytesRead.Length;
                }
                int msgOffset = index;
                int lastIndex = index;
                while (index < len + msgOffset)
                {
                    //get next tag
                    lastIndex = index;
                    int tag = MessageBuilder.GetTag(bytesRead, index, out index);

                    switch (tag)
                    {
                        case GMPCommonTags.TAG_SEQUNCE:
                            int lenSeq = MessageBuilder.GetLength(bytesRead, index, out index);
                            int seq = MessageBuilder.ConvertBcdToInt(bytesRead, index, lenSeq);
                            index += lenSeq;
                            this.seqNum = seq;
                            break;
                        case FPUDataTags.ERROR:
                            int lenErrorTag = MessageBuilder.GetLength(bytesRead, index, out index);
                            this.errCode = bytesRead[index];// MessageBuilder.ConvertBcdToInt(bytesRead, index, lenErrorTag);
                            index += lenErrorTag;
                            break;
                        case FPUDataTags.STATE:
                            int lenStateTag = MessageBuilder.GetLength(bytesRead, index, out index);
                            int state = MessageBuilder.ConvertBcdToInt(bytesRead, index, lenStateTag);
                            index += lenStateTag;
                            this.fpuState = (State)state;
                            break;
                        case FPUDataTags.ENDOFMSG:
                            int lenEndMsg = MessageBuilder.GetLength(bytesRead, index, out index);
                            index = len;
                            break;
                        case FPUDataTags.INDEX:
                            break;
                        default:
                            int tagLenValue = MessageBuilder.GetLength(bytesRead, index, out index);
                            int buffIndx = 0;
                            int counter = 0;
                            int tagIdSize = (index - lastIndex);
                            if (tags != null && tags.Length > 0)
                            {
                                buffIndx = tags.Length;
                                int buffSize = tags.Length + tagLenValue + tagIdSize;
                                Array.Resize(ref tags, buffSize);
                            }
                            else
                            {
                                tags = new byte[tagLenValue + tagIdSize];
                            }
                            while (buffIndx < tags.Length)
                            {
                                tags[buffIndx] = bytesRead[counter + lastIndex];
                                counter++;
                                buffIndx++;
                            }
                            index += tagLenValue;                            
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Data");
            }
            if (!extractError)
            {
                return;
            }

#if NONE
            if (errCode != 0)
            {
                throw new FPUException("" + errCode);
            }
#endif
            try
            {
                switch (errCode)
                {
                    case 0:
                        if (this.SequenceNum < 0)
                        {
                            throw new InvalidOperationException("Invalid Data");
                        }
                        return;
                    case 1:    // ERR_DATA_CORRUPT : Veri eksik gelmiş (uzunluk kadar gelmeli)
                        throw new FramingException();
                    case 2:    // ERR_CRC : Veri değişmiş
                        throw new ChecksumException();
                    case 3:    // ERR_INVALID_STATE : Uygulama durumu uygun değil
                        switch (fpuState)
                        {
                            case State.ON_PWR_RCOVR:
                                throw new PowerFailureException();
                            case State.IN_SERVICE:
                            // throw new SVCPasswordOrPointException();
                            case State.SRV_REQUIRED:
                                throw new BlockingException(GetErrorMessage(errCode));

                        }
                        throw new CmdSequenceException((int)fpuState);
                    case 4:    // ERR_INVALID_CMD : Böyle bir komut desteklenmiyor
                        throw new UndefinedFunctionException();
                    case 5:    // ERR_INVALID_PRM : Parametre geçersiz
                        throw new ParameterException();
                    case 6:    // ERR_OPERATION_FAILED : Operasyon başarısız
                        throw new InvalidOperationException("Error Code" + errCode);
                    case 7:    // ERR_CLEAR_REQUIRED : SİLME gerekli (hata sonrası)
                        //return FiscalPrinter.Printer.InterruptReport();
                        throw new ClearRequiredException();
                    //throw new ClearRequired("Error code : " + errCode);
                    case 8:    // ERR_NO_PAPER : Kağıt yok
                        throw new NoReceiptRollException();
                    case 11:    // ERR_FM_LOAD_ERROR : Mali bellek bilgileri alınırken hata oluştu
                        throw new FiscalCommException("Error code : " + errCode);
                    case 12:    // ERR_FM_REMOVED : Mali bellek takılı değil
                        throw new FiscalCommException("Error code : " + errCode);
                    case 13:    // ERR_FM_MISMATCH : Mali bellek uyumsuzluğu
                        throw new FiscalUndefinedException("Error code : " + errCode);
                    case 14:    // ERR_NEW_FM : Mali bellek formatlanmalı
                        throw new FMNewException();
                    case 15:    // ERR_FM_INIT : Mali bellek formatlanırken hata oluştu
                    case 16:    // ERR_FM_FISCALIZE : Mali bellek malileştirme yapılamadı
                        throw new PrinterException("Error code : " + errCode);
                    case 17:    // ERR_FM_DAILY_LIMIT : Günlük z limit
                        throw new LimitExceededOrZRequiredException("Error code : " + errCode);
                    case 18:    // ERR_FM_FULL : Mali bellek doldu
                        throw new FMFullException();
                    case 19:    // ERR_FM_FORMATTED : Mali bellek daha önce formatlanmış
                        throw new AlreadyFiscalizedException();
                    case 20:    // ERR_FM_CLOSED : Mali bellek kapatılmış
                        throw new FiscalClosedException();
                    case 21:    // ERR_FM_INVALID : Geçersiz mali bellek
                        throw new FiscalUndefinedException();
                    case 22:    // ERR_FM_SAM_CARD : Sertifika bilgisi SAM karttan alınamadı
                        throw new PrinterException("Error code : " + errCode);
                    case 31:    // ERR_EJ_LOAD : Ekü bilgileri alınırken hata oluştu
                    case 32:    // ERR_EJ_REMOVED : Ekü çıkarıldı
                        throw new EJCommException();
                    case 33:    // ERR_EJ_MISMATCH : Ekü kasaya ait değil
                        throw new EJIdMismatchException("Ej id does not match");
                    case 34:    // ERR_EJ_OLD : Eski ekü (Sadece ekü raporları)
                        throw new EJChangedException();
                    case 35:    // ERR_NEW_EJ : Yeni ekü takıldı, onay bekliyor
                        throw new EJFormatException();
                    case 36:    // ERR_EJ_ZREQUIRED : Ekü değiştirilemez, z gerekli
                        throw new NoEJAreaException();
                    case 37:    // ERR_EJ_INIT : Yeni eküye geçilemiyor
                        throw new EJWaitingForInitException();
                    case 38:    // ERR_EJ_FULL : Ekü doldu
                        throw new EJFullException();
                    case 39:    // ERR_EJ_FORMATTED : Ekü daha önce formatlanmış
                        throw new EJIdMismatchException("Formatted ej");
                    case 51:    // ERR_RCPT_TOTAL_LIMIT : Fiş limiti aşıldı
                        throw new ReceiptLimitExceededException();
                    case 52:    // ERR_RCPT_SALE_COUNT : Fiş kalem adedi aşıldı
                        throw new PrinterException("MAKSİMUM SATIR\nSAYISI AŞILDI");
                    case 53:    // ERR_INVALID_SALE : Satış işlemi geçersiz
                    case 54:    // ERR_INVALID_VOID : İptal işlemi geçersiz
                        throw new InvalidOperationException("Error Code" + errCode);
                    case 55:    // ERR_INVALID_CORR : Düzeltme işlemi yapılamaz
                    case 56:    // ERR_INVALID_ADJ : İndirim/Artırım işlemi yapılamaz
                        throw new AdjustmentException();
                    case 57:    // ERR_INVALID_PAYMENT : Ödeme işlemi geçersiz
                        throw new InvalidPaymentException();
                    case 58:    // ERR_PAYMENT_LIMIT : Asgari ödeme sayısı aşıldı
                        throw new InvalidOperationException("Error Code" + errCode);
                    case 59:    // ERR_DAILY_PLU_LIMIT : Günlük ürün satışı aşıldı
                        throw new LimitExceededOrZRequiredException();
                    case 71:    // ERR_VAT_NOT_DEFINED : Kdv oranı tanımsız
                    case 72:    // ERR_SECTION_NOT_DEFINED : Kısım tanımlanmamış
                        throw new UndefinedFunctionException();
                    case 73:    // ERR_PLU_NOT_DEFINED : Tanımsız ürün
                        throw new ProductNotFoundException();
                    case 74:    // ERR_CREDIT_NOT_DEFINED : Kredili ödeme bilgisi eksik/geçersiz
                    case 75:    // ERR_CURRENCY_NOT_DEFINED : Dövizli ödeme bilgisi eksik/geçersiz
                        throw new System.IO.InvalidDataException(PosMessage.PAYMENT_INVALID);
                    case 76:    // ERR_EJSEARCH_NO_RESULT : Eküde kayıt bulunamadı
                        throw new NoDocumentFoundException();
                    case 77:    // ERR_FMSEARCH_NO_RESULT : Mali bellekte kayıt bulunamadı
                        throw new NoProperZFound();
                    case 78:    // ERR_SUBCAT_NOT_DEFINED : Alt ürün grubu tanımlı değil
                        throw new NoDocumentFoundException();
                    case 91:    // ERR_CASHIER_AUTH : Kasiyer yetkisi yetersiz
                        throw new CashierAutorizeException(tags);
                    case 92:    // ERR_HAS_SALE : Satış var (Örn: Kdv değişemez)
                        throw new InvalidOperationException("Error Code" + errCode);
                    case 93:    // ERR_HAS_RECEIPT : Son fiş z değil (Örn: Logo değişemez)
                        throw new ZRequiredException();
                    case 94:    // ERR_NOT_ENOUGH_MONEY : Kasada yeterli para yok
                        throw new NegativeResultException();
                    case 95:    // ERR_DAILY_RCPT_COUNT : Günlük fiş sayısı limit aşıldı
                        throw new LimitExceededOrZRequiredException("Error Code" + errCode);
                    case 96:    // ERR_DAILY_TOTAL_LIMIT : Günlük toplam aşıldı
                        throw new LimitExceededOrZRequiredException("Error Code" + errCode);
                    case 97:    // ERR_ECR_NONFISCAL : Kasa mali değil
                        throw new EcrNonFiscalException();
                    case 111:    // ERR_LINE_LEN : Satır uzunluğu beklenenden fazla
                    case 112:    // ERR_INVALID_VATRATE : Kdv oranı geçersiz
                    case 113:    // ERR_INVALID_DEPTNO : Dept numarası geçersiz
                    case 114:    // ERR_INVALID_PLUNO : Plu numarası geçersiz
                    case 115:    // ERR_INVALID_NAME : Geçersiz tanım (ürün adı, kısım adı, kredi adı...vs)
                    case 116:    // ERR_INVALID_BARCODE : Barkod geçersiz
                    case 117:    // ERR_INVALID_OPTION : Geçersiz opsiyon
                        throw new ParameterException("Error code : " + errCode);
                    case 118:    // ERR_TOTAL_MISMATCH : Toplam tutmuyor
                        throw new SubtotalNotMatchException();
                    case 119:    // ERR_INVALID_QUANTITY : Geçersiz miktar
                    case 120:    // ERR_INVALID_AMOUNT   : Geçersiz tutar
                        throw new ParameterException("Error code : " + errCode);
                    case 121:    // ERR_INVALID_FISCAL_ID : //Mali numara hatalı
                        throw new FiscalMismatchException("Error code : " + errCode);
                    case 131:    // Kapaklar açıldı
                    case 132:    // Mali bellek mesh zarar verildi
                    case 133:    // HUB mesh zarar verildi
                        throw new BlockingException(GetErrorMessage(errCode));
                    case 134:    // Z Raporu gerekli
                        throw new ZRequiredException("Z RAPORU ALINMAMIS\nRAPOR ALINIZ(GiRiS)");
                    case 135:    // Ekü tak ve yeniden başlat
                    case 136:    // Sertifika yükelenemedi
                    case 137:    // Tarih-Saat ayarla
                    case 138:    // Günlük bellek ile mali bellek uyuşmazlığı
                    case 139:    // DB uyumsuzluğu
                    case 140:    // Log hatalı
                    case 141:    // SRAM hatalı
                    case 142:    // Sertifika uyumsuzluğu
                    case 143:    // Versiyon hatası
                    case 144:    // Günlük log sayısı aştı
                        throw new BlockingException(GetErrorMessage(errCode));
                    case 145:    // YAZARKASAYI YENİDEN BAŞLAT"},
                        throw new BlockingException(GetErrorMessage(errCode));
                        break;
                    case 146:    // KASİYER/SERVİS GÜNLÜK YANLIŞ ŞİFRE GİRİŞİ SAYISINI AŞTI"}
                        throw new LimitExceededOrZRequiredException();
                        break;
                    case 147:    // MALİLEŞTİRME YAPILDI. YENİDEN BAŞLAT"},    
                        throw new BlockingException(GetErrorMessage(errCode));
                        break;
                    case 148:    // GİB'e BAĞLANILAMADI. TEKRAR DENE(İŞLEM DURDURMA)"}
                        throw new CashierAutorizeException("GİB'e BAĞLANILAMADI\nTEKRAR DENE");
                        break;
                    default:
                        throw new PosException("Error code : " + errCode);
                }
            }
            catch (PosException pe)
            {
                pe.ErrorCode = errCode;
                throw pe;
            }

        }

        public bool HasError
        {
            get { return errCode != 0; }
        }

        string IPrinterResponse.Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        string IPrinterResponse.Detail
        {
            get
            {
                return detail;
            }
            set
            {
                detail = value;
            }
        }

        private static string[,] errorMessages =
        {
            /*ErrorCode , Message*/
            {"0",PosMessage.FPU_ERROR_MSG_0},
            {"1",PosMessage.FPU_ERROR_MSG_1},
            {"2",PosMessage.FPU_ERROR_MSG_2},
            {"3",PosMessage.FPU_ERROR_MSG_3},
            {"4",PosMessage.FPU_ERROR_MSG_4},
            {"5",PosMessage.FPU_ERROR_MSG_5},
            {"6",PosMessage.FPU_ERROR_MSG_6},
            {"7",PosMessage.FPU_ERROR_MSG_7},
            {"8",PosMessage.FPU_ERROR_MSG_8},
            {"9",PosMessage.FPU_ERROR_MSG_9},
            {"11",PosMessage.FPU_ERROR_MSG_11},
            {"12",PosMessage.FPU_ERROR_MSG_12},
            {"13",PosMessage.FPU_ERROR_MSG_13},
            {"14",PosMessage.FPU_ERROR_MSG_14},
            {"15",PosMessage.FPU_ERROR_MSG_15},
            {"16",PosMessage.FPU_ERROR_MSG_16},
            {"17",PosMessage.FPU_ERROR_MSG_17},
            {"18",PosMessage.FPU_ERROR_MSG_18},
            {"19",PosMessage.FPU_ERROR_MSG_19},
            {"20",PosMessage.FPU_ERROR_MSG_20},
            {"21",PosMessage.FPU_ERROR_MSG_21},
            {"22",PosMessage.FPU_ERROR_MSG_22},
            {"31",PosMessage.FPU_ERROR_MSG_31},
            {"32",PosMessage.FPU_ERROR_MSG_32},
            {"33",PosMessage.FPU_ERROR_MSG_33},
            {"34",PosMessage.FPU_ERROR_MSG_34},
            {"35",PosMessage.FPU_ERROR_MSG_35},
            {"36",PosMessage.FPU_ERROR_MSG_36},
            {"37",PosMessage.FPU_ERROR_MSG_37},
            {"38",PosMessage.FPU_ERROR_MSG_38},
            {"39",PosMessage.FPU_ERROR_MSG_39},
            {"51",PosMessage.FPU_ERROR_MSG_51},
            {"52",PosMessage.FPU_ERROR_MSG_52},
            {"53",PosMessage.FPU_ERROR_MSG_53},
            {"54",PosMessage.FPU_ERROR_MSG_54},
            {"55",PosMessage.FPU_ERROR_MSG_55},
            {"56",PosMessage.FPU_ERROR_MSG_56},
            {"57",PosMessage.FPU_ERROR_MSG_57},
            {"58",PosMessage.FPU_ERROR_MSG_58},
            {"59",PosMessage.FPU_ERROR_MSG_59},
            {"71",PosMessage.FPU_ERROR_MSG_71},
            {"72",PosMessage.FPU_ERROR_MSG_72},
            {"73",PosMessage.FPU_ERROR_MSG_73},
            {"74",PosMessage.FPU_ERROR_MSG_74},
            {"75",PosMessage.FPU_ERROR_MSG_75},
            {"76",PosMessage.FPU_ERROR_MSG_76},
            {"77",PosMessage.FPU_ERROR_MSG_77},
            {"78",PosMessage.FPU_ERROR_MSG_78},
            {"79",PosMessage.FPU_ERROR_MSG_79},
            {"91",PosMessage.FPU_ERROR_MSG_91},
            {"92",PosMessage.FPU_ERROR_MSG_92},
            {"93",PosMessage.FPU_ERROR_MSG_93},
            {"94",PosMessage.FPU_ERROR_MSG_94},
            {"95",PosMessage.FPU_ERROR_MSG_95},
            {"96",PosMessage.FPU_ERROR_MSG_96},
            {"97",PosMessage.FPU_ERROR_MSG_97},
            {"111",PosMessage.FPU_ERROR_MSG_111},
            {"112",PosMessage.FPU_ERROR_MSG_112},
            {"113",PosMessage.FPU_ERROR_MSG_113},
            {"114",PosMessage.FPU_ERROR_MSG_114},
            {"115",PosMessage.FPU_ERROR_MSG_115},
            {"116",PosMessage.FPU_ERROR_MSG_116},
            {"117",PosMessage.FPU_ERROR_MSG_117},
            {"118",PosMessage.FPU_ERROR_MSG_118},
            {"119",PosMessage.FPU_ERROR_MSG_119},
            {"120",PosMessage.FPU_ERROR_MSG_120},
            {"121",PosMessage.FPU_ERROR_MSG_121},
            {"131",PosMessage.FPU_ERROR_MSG_131},
            {"132",PosMessage.FPU_ERROR_MSG_132},
            {"133",PosMessage.FPU_ERROR_MSG_133},
            {"134",PosMessage.FPU_ERROR_MSG_134},
            {"135",PosMessage.FPU_ERROR_MSG_135},
            {"136",PosMessage.FPU_ERROR_MSG_136},
            {"137",PosMessage.FPU_ERROR_MSG_137},
            {"138",PosMessage.FPU_ERROR_MSG_138},
            {"139",PosMessage.FPU_ERROR_MSG_139},
            {"140",PosMessage.FPU_ERROR_MSG_140},
            {"141",PosMessage.FPU_ERROR_MSG_141},
            {"142",PosMessage.FPU_ERROR_MSG_142},
            {"143",PosMessage.FPU_ERROR_MSG_143},
            {"144",PosMessage.FPU_ERROR_MSG_144},
            {"145",PosMessage.FPU_ERROR_MSG_145},
            {"146",PosMessage.FPU_ERROR_MSG_146},            
            {"147",PosMessage.FPU_ERROR_MSG_147},    
            {"148",PosMessage.FPU_ERROR_MSG_148},            
            {"149",PosMessage.FPU_ERROR_MSG_149},    
            {"150",PosMessage.FPU_ERROR_MSG_150},    
            {"151",PosMessage.FPU_ERROR_MSG_151}

        };
        public static string GetErrorMessage(int errorCode)
        {
            string msg = "" + errorCode;

            for (int i = 0; i < errorMessages.LongLength; i++)
            {
                if (errorMessages[i,0] == msg)
                {
                    msg = errorMessages[i, 1];
                    break;
                }
            }

            return msg;
        }
    }
}
