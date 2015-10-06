using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
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

        public int VersionMajor { get { return Convert.ToInt32(_vMajor); } }
        public int VersionMinor { get { return Convert.ToInt32(_vMinor); } }
        public int VersionBuild { get { return Convert.ToInt32(_vBuild); } }
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

        public byte RadioType { get { return _rType; } }
        public byte CarCommBus { get { return _cBus; } }
        public byte CarFlavor { get { return _cFlavor; } }
        public bool StalkPresent { get { return (_sPresent.Equals(0x01)); } }
        public byte StalkOrientation { get { return _sOrientation; } }
        public byte PressHoldFlags1 { get { return _phFlag1; } }
        public byte PressHoldFlags2 { get { return _phFlag2; } }
        public byte PressHoldFlags3 { get { return _phFlag3; } }
        public byte PressHoldVolumeUp { get { return _phVolUp; } }
        public byte PressHoldVolumeDown { get { return _phVolDown; } }
        public byte PressHoldSeekUp { get { return _phSeekUp; } }
        public byte PressHoldSeekDown { get { return _phSeekDown; } }
        public byte PressHoldModeSource { get { return _phModeSource; } }
        public byte PressHoldMute { get { return _phMute; } }
        public byte PressHoldPresetUp { get { return _phPresetUp; } }
        public byte PressHoldPresetDown { get { return _phPresetDown; } }
        public byte PressHoldPower { get { return _phPower; } }
        public byte PressHoldBand { get { return _phBand; } }
        public byte PressHoldPlayEnter { get { return _phPlayEnter; } }
        public byte PressHoldPTT { get { return _phPTT; } }
        public byte PressHoldOnHook { get { return _phOnHook; } }
        public byte PressHoldOffHook { get { return _phOffHook; } }
        public byte PressHoldFanUp { get { return _phFanUp; } }
        public byte PressHoldFanDown { get { return _phFanDown; } }
        public byte PressHoldTempUp { get { return _phTempUp; } }
        public byte PressHoldTempDown { get { return _phTempDown; } }
        public bool ButtonRemapActive { get { return (_bRemapFlag.Equals(0x01)); } }
        public byte VolumeUp { get { return _volUp; } }
        public byte VolumeDown { get { return _volDown; } }
        public byte SeekUp { get { return _seekUp; } }
        public byte SeekDown { get { return _seekDown; } }
        public byte ModeSource { get { return _modeSource; } }
        public byte Mute { get { return _mute; } }
        public byte PresetUp { get { return _presetUp; } }
        public byte PresetDown { get { return _presetDown; } }
        public byte Power { get { return _power; } }
        public byte Band { get { return _band; } }
        public byte PlayEnter { get { return _playEnter; } }
        public byte PTT { get { return _PTT; } }
        public byte OnHook { get { return _onHook; } }
        public byte OffHook { get { return _offHook; } }
        public byte FanUp { get { return _fanUp; } }
        public byte FanDown { get { return _fanDown; } }
        public byte TempUp { get { return _tempUp; } }
        public byte TempDown { get { return _tempDown; } }


        byte _vMajor, _vMinor, _vBuild;
        byte _rType, _cBus, _cFlavor;
        byte _sPresent, _sOrientation;
        byte _phFlag1, _phFlag2, _phFlag3;
        byte _phVolUp, _phVolDown, _phSeekUp, _phSeekDown, _phModeSource, _phMute,
            _phPresetUp, _phPresetDown, _phPower, _phBand, _phPlayEnter, _phPTT, _phOnHook, _phOffHook,
            _phFanUp, _phFanDown, _phTempUp, _phTempDown;
        byte _bRemapFlag;
        byte _volUp, _volDown, _seekUp, _seekDown, _modeSource, _mute, _presetUp, _presetDown,
            _power, _band, _playEnter, _PTT, _onHook, _offHook, _fanUp, _fanDown, _tempUp, _tempDown;

        public ASWCInfo(byte[] raw)
        {
            ProcessRawPacket(raw);
        }

        private void ProcessRawPacket(byte[] raw)
        {
            this._vMajor = raw[3];
            this._vMinor = raw[4];
            this._vBuild = raw[5];
            this._rType = raw[6];
            this._cBus = raw[7];
            this._cFlavor = raw[9];
            
            this._sPresent = raw[13];
            this._sOrientation = raw[14];

            this._phFlag1 = raw[15];
            this._phFlag2 = raw[16];
            this._phFlag3 = raw[17];
            this._phVolUp = raw[18];
            this._phVolDown = raw[19];
            this._phSeekUp = raw[20];
            this._phSeekDown = raw[21];
            this._phModeSource = raw[22];
            this._phMute = raw[23];
            this._phPresetUp = raw[24];
            this._phPresetDown = raw[25];
            this._phPower = raw[26];
            this._phBand = raw[27];
            this._phPlayEnter = raw[28];
            this._phPTT = raw[29];
            this._phOnHook = raw[30];
            this._phOffHook = raw[31];
            this._phFanUp = raw[32];
            this._phFanDown = raw[33];
            this._phTempUp = raw[34];
            this._phTempDown = raw[35];

            this._bRemapFlag = raw[39];
            this._volUp = raw[40];
            this._volDown = raw[41];
            this._seekUp = raw[42];
            this._seekDown = raw[43];
            this._modeSource = raw[44];
            this._mute = raw[45];
            this._presetUp = raw[46];
            this._presetDown = raw[47];
            this._power = raw[48];
            this._band = raw[49];
            this._playEnter = raw[50];
            this._PTT = raw[51];
            this._onHook = raw[52];
            this._offHook = raw[53];
            this._fanUp = raw[54];
            this._fanDown = raw[55];
            this._tempUp = raw[56];
            this._tempDown = raw[57];
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
            sb.AppendLine("Press & Hold - Flags 1: " + this.PressHoldFlags1.ToString());
            sb.AppendLine("Press & Hold - Flags 2: " + this.PressHoldFlags2.ToString());
            sb.AppendLine("Press & Hold - Flags 3: " + this.PressHoldFlags3.ToString());
            sb.AppendLine("Press & Hold - Volume Up: " + this.PressHoldVolumeUp.ToString());
            sb.AppendLine("Press & Hold - Volume Down: " + this.PressHoldVolumeDown.ToString());
            sb.AppendLine("Press & Hold - Seek Up: " + this.PressHoldSeekUp.ToString());
            sb.AppendLine("Press & Hold - Seek Down: " + this.PressHoldSeekDown.ToString());
            sb.AppendLine("Press & Hold - Mode Or Source: " + this.PressHoldModeSource.ToString());
            sb.AppendLine("Press & Hold - Mute: " + this.PressHoldMute.ToString());
            sb.AppendLine("Press & Hold - Preset Up: " + this.PressHoldPresetUp.ToString());
            sb.AppendLine("Press & Hold - Preset Down: " + this.PressHoldPresetDown.ToString());
            sb.AppendLine("Press & Hold - Power: " + this.PressHoldPower.ToString());
            sb.AppendLine("Press & Hold - Band: " + this.PressHoldBand.ToString());
            sb.AppendLine("Press & Hold - Play Or Enter: " + this.PressHoldPlayEnter.ToString());
            sb.AppendLine("Press & Hold - PTT: " + this.PressHoldPTT.ToString());
            sb.AppendLine("Press & Hold - On Hook: " + this.PressHoldOnHook.ToString());
            sb.AppendLine("Press & Hold - Off Hook: " + this.PressHoldOffHook.ToString());
            sb.AppendLine("Press & Hold - Fan Up: " + this.PressHoldFanUp.ToString());
            sb.AppendLine("Press & Hold - Fan Down: " + this.PressHoldFanDown.ToString());
            sb.AppendLine("Press & Hold - Temp Up: " + this.PressHoldTempUp.ToString());
            sb.AppendLine("Press & Hold - Temp Down: " + this.PressHoldTempDown.ToString());
            sb.AppendLine("Button Remap Active: " + this.ButtonRemapActive.ToString());
            sb.AppendLine("Volume Up: " + this.VolumeUp.ToString());
            sb.AppendLine("Volume Down: " + this.VolumeDown.ToString());
            sb.AppendLine("Seek Up: " + this.SeekUp.ToString());
            sb.AppendLine("Seek Down: " + this.SeekDown.ToString());
            sb.AppendLine("Mode Or Source: " + this.ModeSource.ToString());
            sb.AppendLine("Mute: " + this.Mute.ToString());
            sb.AppendLine("Preset Up: " + this.PresetUp.ToString());
            sb.AppendLine("Preset Down: " + this.PresetDown.ToString());
            sb.AppendLine("Power: " + this.Power.ToString());
            sb.AppendLine("Band: " + this.Band.ToString());
            sb.AppendLine("Play Or Enter: " + this.PlayEnter.ToString());
            sb.AppendLine("PTT: " + this.PTT.ToString());
            sb.AppendLine("On Hook: " + this.OnHook.ToString());
            sb.AppendLine("Off Hook: " + this.OffHook.ToString());
            sb.AppendLine("Fan Up: " + this.FanUp.ToString());
            sb.AppendLine("Fan Down: " + this.FanDown.ToString());
            sb.AppendLine("Temp Up: " + this.TempUp.ToString());
            sb.AppendLine("Temp Down: " + this.TempDown.ToString());

            return sb.ToString();
        }
    }
}
