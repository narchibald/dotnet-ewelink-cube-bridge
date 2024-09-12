using EWeLink.Api.Models;
using EWeLink.Cube.Api.Models.Capabilities;
using SwitchState = EWeLink.Cube.Api.Models.Capabilities.SwitchState;

namespace EWeLink.Cube.Bridge;

public static class StateEnumExtensions
{
    public static KeyTrigger ToKeyTrigger(this PressState pressState)
    {
        return pressState switch
        {
            PressState.DoublePress => KeyTrigger.Double,
            PressState.LongPress => KeyTrigger.Long,
            PressState.SinglePress => KeyTrigger.Single,
            _ => throw new ArgumentOutOfRangeException(nameof(pressState), pressState, null)
        };
    }

    public static EWeLink.Api.Models.SwitchState ToSwitchState(this EWeLink.Cube.Api.Models.Capabilities.SwitchState switchState)
    {
        return switchState switch
        {
            SwitchState.Off => EWeLink.Api.Models.SwitchState.Off,
            SwitchState.On => EWeLink.Api.Models.SwitchState.On,
            _ => throw new ArgumentOutOfRangeException(nameof(switchState), switchState, null)
        };
    }
    
    public static EWeLink.Api.Models.MotorDirection ToMotorDirection(this EWeLink.Cube.Api.Models.Capabilities.MotorReverseCapability capability)
    {
        return capability.IsReversed ? MotorDirection.Backward : MotorDirection.Forward;
    }
    
    public static EWeLink.Api.Models.MotorCalibration ToMotorCalibration(this EWeLink.Cube.Api.Models.Capabilities.MotorCalibrationState state)
    {
        return state switch
        {
            MotorCalibrationState.Calibration => EWeLink.Api.Models.MotorCalibration.Calibration,
            MotorCalibrationState.Normal => EWeLink.Api.Models.MotorCalibration.Normal,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    
    public static EWeLink.Api.Models.Motion ToMotion(this EWeLink.Cube.Api.Models.Capabilities.DetectCapability state)
    {
        return state.Detected ? Motion.Detected : Motion.None;
    }
    
    public static EWeLink.Api.Models.Presence ToPresence(this EWeLink.Cube.Api.Models.Capabilities.DetectCapability state)
    {
        return state.Detected ? Presence.Present : Presence.NotPresent;
    }
    
    public static EWeLink.Api.Models.IlluminationLevel ToIlluminationLevel(this EWeLink.Cube.Api.Models.Capabilities.IlluminationLevelCapability state)
    {
        return state.Level switch
        {
            EWeLink.Cube.Api.Models.Capabilities.IlluminationLevel.Brighter => EWeLink.Api.Models.IlluminationLevel.Brighter,
            EWeLink.Cube.Api.Models.Capabilities.IlluminationLevel.Darker => EWeLink.Api.Models.IlluminationLevel.Darker,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}