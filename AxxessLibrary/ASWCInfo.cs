using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public enum SectionChanged
    {
        Radio = 0x01,
        Car = 0x02,
        Stalk = 0x04,
        PressHold = 0x08,
        Buttons = 0x10,
        SpeedControl = 0x20
    };

    public enum ASWCButtonTypes
    {
        Default,
        VolumeUp,
        VolumeDown,
        SeekUp,
        SeekDown,
        ModeOrSource,
        Mute,
        PresetUp,
        PresetDown,
        Power,
        Band,
        PlayOrEnter,
        PTT,
        OnHook,
        OffHook,
        FanUp,
        FanDown,
        TemperatureUp,
        TemperatureDown
    };

    public enum ResponseError
    {
        GOOD = 0x00,
        FAILURE_RADIO_ID = 0xA2,
        FAILURE_STALK = 0xA4,
        FAILURE_PRESS_HOLD = 0xA6,
        FAILURE_PRESS_HOLD_BUTTON = 0xA7,
        FAILURE_REMAP = 0x08
    };

    public class ASWCInfo
    {
        /*        
        RemappingModel.ASWCVersionByteOne = bytes[3 + add]
        RemappingModel.ASWCVersionByteTwo = bytes[4 + add]
        RemappingModel.ASWCVersionByteThree = bytes[5 + add]
        RemappingModel.radioType = bytes[6 + add]
        RemappingModel.carBusType = bytes[7 + add]
        RemappingModel.carFlavor = bytes[9 + add]
        RemappingModel.stalkPresentFlag = bytes[13 + add]
        RemappingModel.stalkOrientation = bytes[14 + add]
        RemappingModel.pressAndHoldFlagOne = bytes[15 + add]
        RemappingModel.pressAndHoldFlagTwo = bytes[16 + add]
        RemappingModel.pressAndHoldFlagThree = bytes[17 + add]
        
        RemappingModel.phVolumeUp = bytes[18 + add]
        RemappingModel.phVolumeDown = bytes[19 + add]
        RemappingModel.phSeekUp = bytes[20 + add]
        RemappingModel.phSeekDown = bytes[21 + add]
        RemappingModel.phModeOrSource = bytes[22 + add]
        RemappingModel.phMute = bytes[23 + add]
        RemappingModel.phPresetUp = bytes[24 + add]
        RemappingModel.phPresetDown = bytes[25 + add]
        RemappingModel.phPower = bytes[26 + add]
        RemappingModel.phBand = bytes[27 + add]
        RemappingModel.phPlayOrEnter = bytes[28 + add]
        RemappingModel.phPTT = bytes[29 + add]
        RemappingModel.phOnHook = bytes[30 + add]
        RemappingModel.phOffHook = bytes[31 + add]
        RemappingModel.phFanUp = bytes[32 + add]
        RemappingModel.phFanDown = bytes[33 + add]
        RemappingModel.phTempUp = bytes[34 + add]
        RemappingModel.phTempDown = bytes[35 + add]
        
        RemappingModel.buttonRemapFlag = bytes[39 + add]
        RemappingModel.volumeUp = bytes[40 + add]
        RemappingModel.volumeDown = bytes[41 + add]
        RemappingModel.seekUp = bytes[42 + add]
        RemappingModel.seekDown = bytes[43 + add]
        RemappingModel.modeOrSource = bytes[44 + add]
        RemappingModel.mute = bytes[45 + add]
        RemappingModel.presetUp = bytes[46 + add]
        RemappingModel.presetDown = bytes[47 + add]
        RemappingModel.power = bytes[48 + add]
        RemappingModel.band = bytes[49 + add]
        RemappingModel.playOrEnter = bytes[50 + add]
        RemappingModel.PTT = bytes[51 + add]
        RemappingModel.onHook = bytes[52 + add]
        RemappingModel.offHook = bytes[53 + add]
        RemappingModel.fanUp = bytes[54 + add]
        RemappingModel.fanDown = bytes[55 + add]
        RemappingModel.tempUp = bytes[56 + add]
        RemappingModel.tempDown = bytes[57 + add]
        */

        public string[] ButtonList { get { return Enum.GetNames(typeof(ASWCButtonTypes)); } }
        private Dictionary<byte, string> _radioList;
        public string[] RadioList { get { return _radioList.Values.ToArray(); } }
        private Dictionary<byte, string> _carBusList;
        public string[] BusList { get { return _carBusList.Values.ToArray(); } }
        private Dictionary<byte, string> _carMethodList;
        public string[] MethodList { get { return _carMethodList.Values.ToArray(); } }

        public string[] StalkOptions { get { return new string[] { "Unknown", "Left", "Right" }; } }
        public string[] SpeedOptions { get { return new string[] { "Off", "Low", "Medium", "High"}; } }

        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionBuild { get; set; }
        public string VersionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(VersionMajor.ToString());
                sb.Append(".");
                sb.Append(VersionMinor.ToString());
                sb.Append(".");
                sb.Append(VersionBuild.ToString());
                return sb.ToString();
            }
        }

        public byte SpeedControl { get; set; }
        public byte RadioType { get; set; }
        public byte CarCommBus { get; set; }
        public byte CarFlavor { get; set; }
        public bool StalkPresent { get; set; }
        public byte StalkOrientation { get; set; }
        public bool[] PressHoldFlags { get; set; }
        //byte PressHoldFlags1 { get { return _phFlag1; } }
        //byte PressHoldFlags2 { get { return _phFlag2; } }
        //byte PressHoldFlags3 { get { return _phFlag3; } }
        public byte[] PressHoldValues { get; set; }
        /*public byte PressHoldVolumeUp { get; set; }
        public byte PressHoldVolumeDown { get; set; }
        public byte PressHoldSeekUp { get; set; }
        public byte PressHoldSeekDown { get; set; }
        public byte PressHoldModeSource { get; set; }
        public byte PressHoldMute { get; set; }
        public byte PressHoldPresetUp { get; set; }
        public byte PressHoldPresetDown { get; set; }
        public byte PressHoldPower { get; set; }
        public byte PressHoldBand { get; set; }
        public byte PressHoldPlayEnter { get; set; }
        public byte PressHoldPTT { get; set; }
        public byte PressHoldOnHook { get; set; }
        public byte PressHoldOffHook { get; set; }
        public byte PressHoldFanUp { get; set; }
        public byte PressHoldFanDown { get; set; }
        public byte PressHoldTempUp { get; set; }
        public byte PressHoldTempDown { get; set; }*/
        public bool ButtonRemapActive { get; set; }
        public byte[] ButtonRemapValues { get; set; }
        /*public byte VolumeUp { get; set; }
        public byte VolumeDown { get; set; }
        public byte SeekUp { get; set; }
        public byte SeekDown { get; set; }
        public byte ModeSource { get; set; }
        public byte Mute { get; set; }
        public byte PresetUp { get; set; }
        public byte PresetDown { get; set; }
        public byte Power { get; set; }
        public byte Band { get; set; }
        public byte PlayEnter { get; set; }
        public byte PTT { get; set; }
        public byte OnHook { get; set; }
        public byte OffHook { get; set; }
        public byte FanUp { get; set; }
        public byte FanDown { get; set; }
        public byte TempUp { get; set; }
        public byte TempDown { get; set; }*/

        byte _phFlag1, _phFlag2, _phFlag3;
        byte[] _rawPacket;

        public ASWCInfo()
        {

            //Radios
            _radioList = new Dictionary<byte, string>();
            _radioList.Add(0x00, "Unknown");
            _radioList.Add(0x01, "Eclipse");
            _radioList.Add(0x02, "Kenwood");
            _radioList.Add(0x03, "Clarion");
            _radioList.Add(0x04, "Sony / Dual");
            _radioList.Add(0x05, "JVC");
            _radioList.Add(0x06, "Jensen / Pioneer");
            _radioList.Add(0x07, "Alpine (default)");
            _radioList.Add(0x08, "Visteon");
            _radioList.Add(0x09, "Valor");
            _radioList.Add(0x0A, "Clarion – Type II");
            _radioList.Add(0x0B, "Freeway");
            _radioList.Add(0x0C, "Eclipse – Type II");
            _radioList.Add(0x0D, "LG");
            _radioList.Add(0x0E, "Parrot");
            _radioList.Add(0x0F, "Xite Solutions");

            //Car bus type
            _carBusList = new Dictionary<byte, string>();
            _carBusList.Add(0x00, "Unknown");
            _carBusList.Add(0x01, "Resistive");
            _carBusList.Add(0x02, "CAN Car");
            _carBusList.Add(0x03, "J1850 Car");
            _carBusList.Add(0x04, "J1850 Slow Car");
            _carBusList.Add(0x05, "IBUS");
            _carBusList.Add(0x06, "Volvo");
            _carBusList.Add(0x07, "J1850 + Resistive");
            _carBusList.Add(0x08, "LIN Car");

            //Car communication method
            _carMethodList = new Dictionary<byte, string>();
            _carMethodList.Add(0x00, "Unknown");
            _carMethodList.Add(0x01, "GM LAN33-11");
            _carMethodList.Add(0x02, "GM LAN11-29");
            _carMethodList.Add(0x03, "CAN 83.3");
            _carMethodList.Add(0x04, "CAN 95");
            _carMethodList.Add(0x05, "CAN 100");
            _carMethodList.Add(0x06, "CAN 125");
            _carMethodList.Add(0x07, "CAN 500");
            _carMethodList.Add(0x08, "CAN 50");
            _carMethodList.Add(0x09, "Valor");
            _carMethodList.Add(0x0A, "BMW I-BUS");
            _carMethodList.Add(0x0B, "VOLVO I-BUS");
            _carMethodList.Add(0x0C, "J1850");
            _carMethodList.Add(0x0D, "J1850-slow");
            _carMethodList.Add(0x0E, "LIN 10000");

            this.PressHoldFlags = new bool[24];
            this.PressHoldValues = new byte[this.ButtonList.Length - 1];
            this.ButtonRemapValues = new byte[this.ButtonList.Length - 1];
        }

        public ASWCInfo(byte[] raw) : this()
        {
            this._rawPacket = raw;   
            ProcessRawPacket(raw);
        }

        private bool[] ParsePHFlags(byte f1, byte f2, byte f3)
        {
            bool[] flags = new bool[24];

            flags[0] = (f1 & 0x01) != 0;
            flags[1] = (f1 & 0x02) != 0;
            flags[2] = (f1 & 0x04) != 0;
            flags[3] = (f1 & 0x08) != 0;
            flags[4] = (f1 & 0x16) != 0;
            flags[5] = (f1 & 0x32) != 0;
            flags[6] = (f1 & 0x64) != 0;
            flags[7] = (f1 & 0x128) != 0;
            flags[8] = (f2 & 0x01) != 0;
            flags[9] = (f2 & 0x02) != 0;
            flags[10] = (f2 & 0x04) != 0;
            flags[11] = (f2 & 0x08) != 0;
            flags[12] = (f2 & 0x16) != 0;
            flags[13] = (f2 & 0x32) != 0;
            flags[14] = (f2 & 0x64) != 0;
            flags[15] = (f2 & 0x128) != 0;
            flags[16] = (f3 & 0x01) != 0;
            flags[17] = (f3 & 0x02) != 0;
            flags[18] = (f3 & 0x04) != 0;

            return flags;
        }

        private void UpdatePHFlags()
        {
            int ph1 = 0, ph2 = 0, ph3 = 0;
            for (int i = 0; i < 8; i++)
            {
                ph3 += this.PressHoldFlags[i] ? 1 << i : 0;
                ph2 += this.PressHoldFlags[i + 8] ? 1 << i : 0;
            }

            ph1 += this.PressHoldFlags[16] ? 1 : 0;
            ph1 += this.PressHoldFlags[17] ? 2 : 0;
            ph1 += this.PressHoldFlags[18] ? 4 : 0;

            this._phFlag1 = (byte)ph1;
            this._phFlag2 = (byte)ph2;
            this._phFlag3 = (byte)ph3;
        }

        private byte[] ParseButtonValues(byte[] raw, int offset)
        {
            byte[] values = new byte[this.ButtonList.Length - 1];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = raw[i + offset];
            }
            return values;
        }

        public byte[] GetRawPacket(IList<SectionChanged> changes)
        {
            byte[] packet = new byte[59];

            packet[0] = 0x01;
            packet[1] = 0xF0;
            packet[2] = 0xA1;

            packet[3] = (changes.Count > 0) ? ((byte)changes.Aggregate((acc, flag) => (SectionChanged)((int)acc | (int)flag))) : (byte)0x00;

            packet[6] = this.RadioType;

            packet[13] = (byte)(this.StalkPresent ? 0x01 : 0x00);
            packet[14] = this.StalkOrientation;

            UpdatePHFlags();
            packet[15] = this._phFlag1;
            packet[16] = this._phFlag2;
            packet[17] = this._phFlag3;
            packet[36] = this.SpeedControl;
            packet[39] = (byte)(this.ButtonRemapActive ? 0x01 : 0x00);
            packet[40] = 0x01;
            packet[41] = 0x02;

            for (int i = 0; i < this.PressHoldValues.Length; i++)
            {
                packet[i + 18] = this.PressHoldValues[i];
            }
            for (int i = 2; i < this.PressHoldValues.Length; i++)
            {
                packet[i + 40] = this.ButtonRemapValues[i];
            }

            packet[58] = 0xDD;

            return packet;
        }

        public void ProcessRawPacket(byte[] raw)
        {
            this.VersionMajor = raw[3];
            this.VersionMinor = raw[4];
            this.VersionBuild = raw[5];
            this.RadioType = raw[6];
            this.CarCommBus = raw[7];
            this.CarFlavor = raw[9];
            
            this.StalkPresent = raw[13].Equals(0x01);
            this.StalkOrientation = raw[14];

            this._phFlag1 = raw[15];
            this._phFlag2 = raw[16];
            this._phFlag3 = raw[17];
            this.PressHoldFlags = this.ParsePHFlags(this._phFlag3, this._phFlag2, this._phFlag1);
            this.PressHoldValues = this.ParseButtonValues(raw, 18);
            this.SpeedControl = raw[36];
            this.ButtonRemapActive = raw[39].Equals(0x01);
            this.ButtonRemapValues = this.ParseButtonValues(raw, 40);
        }

        public ASWCInfo Clone()
        {
            return new ASWCInfo(_rawPacket);
        }

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Version Major: " + this.VersionMajor.ToString());
            sb.AppendLine("Version Minor: " + this.VersionMinor.ToString());
            sb.AppendLine("Version Build: " + this.VersionBuild.ToString());
            sb.AppendLine("Radio Type: " + this.RadioType.ToString());
            sb.AppendLine("Car Communication Bus: " + this.CarCommBus.ToString());
            sb.AppendLine("Car Flavor: " + this.CarFlavor.ToString());
            sb.AppendLine("Stalk Present: " + this.StalkPresent.ToString());
            sb.AppendLine("Press & Hold - Flags 1: " + this._phFlag1.ToString());
            sb.AppendLine("Press & Hold - Flags 2: " + this._phFlag2.ToString());
            sb.AppendLine("Press & Hold - Flags 3: " + this._phFlag3.ToString());
            sb.AppendLine("Press & Hold - Volume Up: " + this.PressHoldValues[0]);
            sb.AppendLine("Press & Hold - Volume Down: " + this.PressHoldValues[1]);
            sb.AppendLine("Press & Hold - Seek Up: " + this.PressHoldValues[2]);
            sb.AppendLine("Press & Hold - Seek Down: " + this.PressHoldValues[3]);
            sb.AppendLine("Press & Hold - Mode Or Source: " + this.PressHoldValues[4]);
            sb.AppendLine("Press & Hold - Mute: " + this.PressHoldValues[5]);
            sb.AppendLine("Press & Hold - Preset Up: " + this.PressHoldValues[6]);
            sb.AppendLine("Press & Hold - Preset Down: " + this.PressHoldValues[7]);
            sb.AppendLine("Press & Hold - Power: " + this.PressHoldValues[8]);
            sb.AppendLine("Press & Hold - Band: " + this.PressHoldValues[9]);
            sb.AppendLine("Press & Hold - Play Or Enter: " + this.PressHoldValues[10]);
            sb.AppendLine("Press & Hold - PTT: " + this.PressHoldValues[11]);
            sb.AppendLine("Press & Hold - On Hook: " + this.PressHoldValues[12]);
            sb.AppendLine("Press & Hold - Off Hook: " + this.PressHoldValues[13]);
            sb.AppendLine("Press & Hold - Fan Up: " + this.PressHoldValues[14]);
            sb.AppendLine("Press & Hold - Fan Down: " + this.PressHoldValues[15]);
            sb.AppendLine("Press & Hold - Temp Up: " + this.PressHoldValues[16]);
            sb.AppendLine("Press & Hold - Temp Down: " + this.PressHoldValues[17]);
            sb.AppendLine("Button Remap Active: " + this.ButtonRemapActive.ToString());
            sb.AppendLine("Volume Up: " + this.ButtonRemapValues[0]);
            sb.AppendLine("Volume Down: " + this.ButtonRemapValues[1]);
            sb.AppendLine("Seek Up: " + this.ButtonRemapValues[2]);
            sb.AppendLine("Seek Down: " + this.ButtonRemapValues[3]);
            sb.AppendLine("Mode Or Source: " + this.ButtonRemapValues[4]);
            sb.AppendLine("Mute: " + this.ButtonRemapValues[5]);
            sb.AppendLine("Preset Up: " + this.ButtonRemapValues[6]);
            sb.AppendLine("Preset Down: " + this.ButtonRemapValues[7]);
            sb.AppendLine("Power: " + this.ButtonRemapValues[8]);
            sb.AppendLine("Band: " + this.ButtonRemapValues[9]);
            sb.AppendLine("Play Or Enter: " + this.ButtonRemapValues[10]);
            sb.AppendLine("PTT: " + this.ButtonRemapValues[11]);
            sb.AppendLine("On Hook: " + this.ButtonRemapValues[12]);
            sb.AppendLine("Off Hook: " + this.ButtonRemapValues[13]);
            sb.AppendLine("Fan Up: " + this.ButtonRemapValues[14]);
            sb.AppendLine("Fan Down: " + this.ButtonRemapValues[15]);
            sb.AppendLine("Temp Up: " + this.ButtonRemapValues[16]);
            sb.AppendLine("Temp Down: " + this.ButtonRemapValues[17]);

            return sb.ToString();
        }
    }
}
