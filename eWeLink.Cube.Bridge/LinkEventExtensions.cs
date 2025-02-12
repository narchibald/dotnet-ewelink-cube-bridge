using EWeLink.Api.Models;
using EWeLink.Api.Models.EventParameters;
using EWeLink.Api.Models.Parameters;
using EWeLink.Cube.Api.Extensions;
using EWeLink.Cube.Api.Models.Capabilities;
using EWeLink.Cube.Api.Models.States;
using SwitchState = EWeLink.Api.Models.SwitchState;
using THOriginParameters = EWeLink.Api.Models.EventParameters.THOriginParameters;

namespace EWeLink.Cube.Bridge;

public static class LinkEventExtensions
{
    private static readonly Dictionary<Type, Func<SubDeviceState, DateTimeOffset?, IEventParameters?>> Converters = new()
    {
        { typeof(ButtonState), (s, t) => ToEventParameters((ButtonState)s, t) },
        { typeof(ZbMicroState), (s, t) => ToEventParameters((ZbMicroState)s, t) },
        { typeof(MicroState), (s, t) => ToEventParameters((MicroState)s, t) },
        { typeof(CurtainState), (s, t) => ToEventParameters((CurtainState)s, t) },
        { typeof(WindowDoorSensor), (s, t) => ToEventParameters((WindowDoorSensor)s, t) },
        { typeof(TemperatureAndHumiditySensor), (s, t) => ToEventParameters((TemperatureAndHumiditySensor)s, t) },
        { typeof(MotionSensorPro), (s, t) => ToEventParameters((MotionSensorPro)s, t) },
        { typeof(MotionSensor), (s, t) => ToEventParameters((MotionSensor)s, t) },
        { typeof(PresenceSensor), (s, t) => ToEventParameters((PresenceSensor)s, t) },
        { typeof(ThreeStateToggle), (s, t) => ToEventParameters((ThreeStateToggle)s, t) },
        { typeof(TwoStateToggle), (s, t) => ToEventParameters((TwoStateToggle)s, t) },
        { typeof(OneStateToggle), (s, t) => ToEventParameters((OneStateToggle)s, t) },
        { typeof(SingleSwitchState), (s, t) => ToEventParameters((SingleSwitchState)s, t) },
        { typeof(PowerMeterSwitch), (s, t) => ToEventParameters((PowerMeterSwitch)s, t) },
        { typeof(MiniState), (s, t) => ToEventParameters((MiniState)s, t) },
        { typeof(TemperatureAndHumidityControl), (s, t) => ToEventParameters((TemperatureAndHumidityControl)s, t) },
        //{ typeof(SwvState), (s, t) => ToEventParameters((SwvState)s, t) },
    };
    
    public static IEventParameters? ToEventParameters(this Api.ILinkEvent<SubDeviceState> linkEvent, DateTimeOffset? triggerTime = null)
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
        => ToEventParameters((MicroState)state, triggerTime);
    
    public static ISwitchEventParameters? ToEventParameters(this MicroState state, DateTimeOffset? triggerTime = null)
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
    
    public static ISnZbThermostatEventParameters ToEventParameters(this TemperatureAndHumiditySensor state, DateTimeOffset? triggerTime = null)
    {
        return new SnZbThermostatEventParameters
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
    
    public static ISwitchEventParameters? ToEventParameters(this SingleSwitchState state, DateTimeOffset? triggerTime = null)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchEventParameters
        {
            Switch = state.Power.State.ToSwitchState(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now,
        };
    }
    
    public static IOneSwitchEventParameters? ToEventParameters(this OneStateToggle state, DateTimeOffset? triggerTime = null)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;
        
        int? lastUpdatedSwitch = GetLastUpdatedSwitch(toggle.One);

        return new OneSwitchEventParameters()
        {
            Switches = new[] { new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off } },
            TriggeredOutlet = lastUpdatedSwitch,
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static ITwoSwitchEventParameters? ToEventParameters(this TwoStateToggle state, DateTimeOffset? triggerTime = null)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;
        
        int? lastUpdatedSwitch = GetLastUpdatedSwitch(toggle.One, toggle.Two);
        
        return new TwoSwitchEventParameters()
        {
            Switches = new[]
            {
                new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 1, Switch = toggle.Two?.State.ToSwitchState() ?? SwitchState.Off }
            },
            TriggeredOutlet = lastUpdatedSwitch,
            TriggerTime = triggerTime ?? DateTimeOffset.Now
        };
    }
    
    public static IThreeSwitchEventParameters? ToEventParameters(this ThreeStateToggle state, DateTimeOffset? triggerTime = null)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;
        
        int? lastUpdatedSwitch = GetLastUpdatedSwitch(toggle.One, toggle.Two, toggle.Three); 
        
        return new ThreeSwitchEventParameters()
        {
            Switches = new[]
            {
                new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 1, Switch = toggle.Two?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 2, Switch = toggle.Three?.State.ToSwitchState() ?? SwitchState.Off }
            },
            TriggeredOutlet = lastUpdatedSwitch,
            TriggerTime = triggerTime ?? DateTimeOffset.Now,
        };
    }
    
    public static IPowEventParameters? ToEventParameters(this PowerMeterSwitch state, DateTimeOffset? triggerTime = null)
    {
        if (state.ElectricCurrent is null || state.Voltage is null || state.ElectricPower is null)
            return null;
        return new PowEventParameters()
        {
            Current = state.ElectricCurrent?.Value ?? 0,
            Voltage = state.Voltage?.Value ?? 0,
            Power = state.ElectricPower?.Value ?? 0,
            TriggerTime = triggerTime ?? DateTimeOffset.Now,
        };
    }
    
    public static ISwitchEventParameters? ToEventParameters(this MiniState state, DateTimeOffset? triggerTime = null)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchEventParameters
        {
            Switch = state.Power.State.ToSwitchState(),
            TriggerTime = triggerTime ?? DateTimeOffset.Now,
        };
    }
    
    public static IThermostatSwitchParameters? ToEventParameters(this TemperatureAndHumidityControl state, DateTimeOffset? triggerTime = null)
    {
        if (state.Power is null)
            return null;
        
        return new THOriginParameters
        {
            Switch = state.Power.State.ToSwitchState(),
            Humidity = state.Humidity?.Value,
            Temperature = state.Temperature?.Value,
        };
    }
    
    /*public static ZbSmartWaterValveParameters? ToParameters(this SwvState state)
    {
        if (state.Power is null)
            return null;
        
        return new ZbSmartWaterValveParameters
        {
            Switch = state.Power.ToSwitchState()
        };
    }*/

    private static int? GetLastUpdatedSwitch(params ToggleState?[] toggles)
    {
        int? lastUpdatedSwitch = (new List<ToggleState?>(toggles))
            .Select((toggleState, index) => new { index, toggleState })
            .Where(x => x.toggleState?.UpdatedAt != null)
            .OrderByDescending(x => x.toggleState?.UpdatedAt)
            .Select(x => (int?)x.index)
            .FirstOrDefault();
        return lastUpdatedSwitch;
    }
}