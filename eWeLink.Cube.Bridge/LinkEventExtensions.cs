using EWeLink.Api.Models.EventParameters;
using EWeLink.Cube.Api;
using EWeLink.Cube.Api.Extensions;
using EWeLink.Cube.Api.Models.States;

namespace EWeLink.Cube.Bridge;

public static class LinkEventExtensions
{
    private static readonly Dictionary<Type, Func<SubDeviceState, DateTimeOffset?, IEventParameters?>> Converters = new()
    {
        { typeof(ButtonState), (s, t) => ToEventParameters((ButtonState)s, t) },
        { typeof(ZbMicroState), (s, t) => ToEventParameters((ZbMicroState)s, t) },
        { typeof(CurtainState), (s, t) => ToEventParameters((CurtainState)s, t) },
        { typeof(WindowDoorSensor), (s, t) => ToEventParameters((WindowDoorSensor)s, t) },
        { typeof(TemperatureAndHumiditySensor), (s, t) => ToEventParameters((TemperatureAndHumiditySensor)s, t) },
        { typeof(MotionSensorPro), (s, t) => ToEventParameters((MotionSensorPro)s, t) },
        { typeof(MotionSensor), (s, t) => ToEventParameters((MotionSensor)s, t) },
        { typeof(PresenceSensor), (s, t) => ToEventParameters((PresenceSensor)s, t) },
    };
    
    public static IEventParameters? ToEventParameters(this ILinkEvent<SubDeviceState> linkEvent, DateTimeOffset? triggerTime = null)
    {
        var state = linkEvent.State;
        if (Converters.TryGetValue(state.GetType(), out var converter))
            return converter(state, triggerTime);
        
        throw new NotImplementedException($"{linkEvent.State.GetType()}");
    }
    
    public static ISnZbButtonEventParameters? ToEventParameters(this ButtonState state, DateTimeOffset? triggerTime = null)
    {
        if (state.Press is null)
            return null;
        
        return new SnZbButtonEventParameters { Key = state.Press!.Value.ToKeyTrigger(), TriggerTime = triggerTime ?? DateTimeOffset.Now };
    }
    
    public static ISwitchEventParameters? ToEventParameters(this ZbMicroState state, DateTimeOffset? triggerTime = null)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchEventParameters { Switch = state.Power.State.ToSwitchState(), TriggerTime = triggerTime ?? DateTimeOffset.Now };
    }
    
    public static IZbCurtainEventParameters ToEventParameters(this CurtainState state, DateTimeOffset? triggerTime = null)
    {
        return new ZbCurtainEventParameters
        {
            CurrentPercent = (int?)state.Percentage?.Value,
            MotorDirection = state.MotorReverse?.ToMotorDirection(),
            MotorCalibration = state.MotorCalibration?.Mode.ToMotorCalibration(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ISnZbDoorWindowEventParameters? ToEventParameters(this WindowDoorSensor state, DateTimeOffset? triggerTime = null)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbDoorWindowEventParameters
        {
            Open = state.Detect.Detected,
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ISnZbThermostatParameters ToEventParameters(this TemperatureAndHumiditySensor state, DateTimeOffset? triggerTime = null)
    {
        return new SnZbThermostatParameters
        {
            Temperature = state.Temperature?.Value,
            Humidity = state.Humidity?.Value,
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ISnZbMotionPEventParameters? ToEventParameters(this MotionSensorPro state, DateTimeOffset? triggerTime = null)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbMotionPEventParameters
        {
            Motion = state.Detect.ToMotion(),
            BrState = state.IlluminationLevel?.ToIlluminationLevel(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ISnZbMotionEventParameters? ToEventParameters(this MotionSensor state, DateTimeOffset? triggerTime = null)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbMotionEventParameters
        {
            Motion = state.Detect.ToMotion(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ISnZbHumanPresenceEventParameters? ToEventParameters(this PresenceSensor state, DateTimeOffset? triggerTime = null)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbHumanPresenceEventParameters
        {
            Human = state.Detect.ToPresence(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
}