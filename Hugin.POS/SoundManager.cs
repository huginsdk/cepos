using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Media;

namespace Hugin.POS
{
    public enum SoundType
    {
        SUCCESS,
        FAILED,
        NEED_PROCESS,
        FATAL_ERROR,
        NOT_FOUND
    }

    

    public class SoundManager
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public const int MAX_SOUND_LEVEL = 10;

        static int currentVol;

        public SoundManager()
        {
        }

        public static int CurrentVolume
        {
            get { return currentVol; }
        }

        public static void SetVolume()
        {
            // By the default set the volume to 0
            uint CurrVol = 0;

            // At this point, CurrVol gets assigned the volume
            waveOutGetVolume(IntPtr.Zero, out CurrVol);

            // Calculate the volume
            ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);

            currentVol = CalcVol / (ushort.MaxValue / 10);
        }

        public static void Sound(SoundType soundType)
        {
            System.Media.SoundPlayer sp;

            switch (soundType)
            {
                case SoundType.SUCCESS:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.SUCCESS);
                    break;
                case SoundType.FAILED:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.FAILED);
                    break;
                case SoundType.NEED_PROCESS:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.NEEDPROCESS);
                    break;
                case SoundType.FATAL_ERROR:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.CRITICAL);
                    break;
                case SoundType.NOT_FOUND:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.NOT_FOUND);
                    break;
                default:
                    sp = new System.Media.SoundPlayer(Hugin.POS.Properties.Resources.SUCCESS);
                    break;
            }

            sp.Play();
        }

        private static void VolumeChanged()
        {
            // Calculate the volume that's being set. BTW: this is a trackbar!
            int NewVolume = ((ushort.MaxValue / 10) * currentVol);

            // Set the same volume for both the left and the right channels
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));

            // Set the volume
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }

        public static void VolumeUp()
        {
            currentVol++;
            VolumeChanged();
        }

        public static void VolumeDown()
        {
            currentVol--;
            VolumeChanged();
        }
    }
}
