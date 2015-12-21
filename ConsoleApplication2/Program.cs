using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Management;
using System.Diagnostics;

namespace Metra.Axxess
{
    //0, 1, 15, 32, 0, 204, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    //0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    class Program
    {
        public static void Main()
        {
            byte[] array = new byte[] {0, 85, 176, 59, 1, 240, 161, 255, 0, 0, 160, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 221, 4, 249};
            byte[] array2 = new byte[] { 0, 85, 160, 7, 1, 15, 161, 1, 162, 169, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 83 };
            string[] array3 = "00, 55, b0, 3b, 01, f0, a1, ff, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 01, 01, 02, ff, 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 0a, 0b, 0c, 0d, 0e, dd, 04, 5a".Split(',');
            string ASWCpacket = @"0x01, 0xF0, 0xA1, 0xFF, 0x00, 0x00, RemappingModel.radioType, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, RemappingModel.stalkPresentFlag, RemappingModel.stalkOrientation, RemappingModel.pressAndHoldFlagOne, RemappingModel.pressAndHoldFlagTwo, RemappingModel.pressAndHoldFlagThree, RemappingModel.phVolumeUp, RemappingModel.phVolumeDown, RemappingModel.phSeekUp, RemappingModel.phSeekDown, RemappingModel.phModeOrSource, RemappingModel.phMute, RemappingModel.phPresetUp, RemappingModel.phPresetDown, RemappingModel.phPower, RemappingModel.phBand, RemappingModel.phPlayOrEnter, RemappingModel.phPTT, RemappingModel.phOnHook, RemappingModel.phOffHook, RemappingModel.phFanUp, RemappingModel.phFanDown, RemappingModel.phTempUp,RemappingModel.phTempDown,0x00, 0x00, 0x00, RemappingModel.buttonRemapFlag, 0x01, 0x02, RemappingModel.seekUp, RemappingModel.seekDown, RemappingModel.modeOrSource, RemappingModel.mute, RemappingModel.presetUp, RemappingModel.presetDown, RemappingModel.power, RemappingModel.band, RemappingModel.playOrEnter, RemappingModel.PTT, RemappingModel.onHook, RemappingModel.offHook, RemappingModel.fanUp, RemappingModel.fanDown, RemappingModel.tempUp, RemappingModel.fanDown, 0xDD";
            string[] ASWCsplit = ASWCpacket.Split(',');
            
            /*for (int i = 0; i < ASWCsplit.Length; i++)
            {
                Console.WriteLine("{0}) {1:x2}", i, array[i]);
                Debug.WriteLine("{0}) {1:x2}", i, array[i]);
            }*/
            for (int i = 0; i < ASWCsplit.Length; i++)
            {
                Console.WriteLine("{0}) {1}", i, ASWCsplit[i]);
                Debug.WriteLine("{0}) {1}", i, ASWCsplit[i]);
            }
            Console.ReadKey();
        }

    }
}
