using EWeLink.Cube.Api.Models.Capabilities;
using EWeLink.Cube.Api.Models.Devices;

namespace EWeLink.Cube.Bridge;

using EWeLink.Api.Models;
using EWeLink.Api.Models.Parameters;
using Api.Models.States;

public static class LinkParameterExtensions
{
    private static readonly Dictionary<Type, Func<SubDeviceState, Parameters?>> Converters = new()
    {
        { typeof(ButtonState), (s) => ToParameters((ButtonState)s) },
        { typeof(ZbMicroState), (s) => ToParameters((ZbMicroState)s) },
        { typeof(MicroState), (s) => ToParameters((MicroState)s) },
        { typeof(CurtainState), (s) => ToParameters((CurtainState)s) },
        { typeof(WindowDoorSensor), (s) => ToParameters((WindowDoorSensor)s) },
        { typeof(TemperatureAndHumiditySensor), (s) => ToParameters((TemperatureAndHumiditySensor)s) },
        { typeof(MotionSensorPro), (s) => ToParameters((MotionSensorPro)s) },
        { typeof(MotionSensor), (s) => ToParameters((MotionSensor)s) },
        { typeof(PresenceSensor), (s) => ToParameters((PresenceSensor)s) },
        { typeof(SwvState), (s) => ToParameters((SwvState)s) },
        { typeof(ThreeStateToggle), (s) => ToParameters((ThreeStateToggle)s) },
        { typeof(TwoStateToggle), (s) => ToParameters((TwoStateToggle)s) },
        { typeof(OneStateToggle), (s) => ToParameters((OneStateToggle)s) },
        { typeof(SingleSwitchState), (s) => ToParameters((SingleSwitchState)s) },
        { typeof(MiniState), (s) => ToParameters((MiniState)s) },
        { typeof(TemperatureAndHumidityControl), (s) => ToParameters((TemperatureAndHumidityControl)s) },
    };
    
    public static Parameters? ToParameters(this Api.ILinkEvent<SubDeviceState> linkEvent)
    {
        var state = linkEvent.State;
        if (Converters.TryGetValue(state.GetType(), out var converter))
            return converter(state);
        
        throw new NotImplementedException($"{linkEvent.State.GetType()}");
    }
    
    public static Parameters? GetParameters(this ISubDevice<SubDeviceState> subDevice)
    {
        var state = subDevice.State;
        if (Converters.TryGetValue(state.GetType(), out var converter))
            return converter(state);
        
        throw new NotImplementedException($"{subDevice.State.GetType()}");
    }
    
    public static SnZbButtonParameters? ToParameters(this ButtonState state)
    {
        if (state.Press is null)
            return null;
        
        return new SnZbButtonParameters
        {
            Key = state.Press!.Value.ToKeyTrigger(),
            Battery = (int?)state.Battery?.Value ?? 100,
        };
    }
    
    public static ZbMicroParameters? ToParameters(this ZbMicroState state)
    {
        if (state.Power is null)
            return null;
        
        return new ZbMicroParameters { Switch = state.Power.State.ToSwitchState() };
    }
    
    public static SwitchParameters? ToParameters(this MicroState state)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchParameters { Switch = state.Power.State.ToSwitchState() };
    }
    
    public static SwitchParameters? ToParameters(this SingleSwitchState state)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchParameters { Switch = state.Power.State.ToSwitchState() };
    }
    
    public static OneSwitchParameters? ToParameters(this OneStateToggle state)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;

        return new OneSwitchParameters
            { Switches = new[] { new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off } } };
    }
    
    public static TwoSwitchParameters? ToParameters(this TwoStateToggle state)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;
        return new TwoSwitchParameters
        {
            Switches = new[]
            {
                new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 1, Switch = toggle.Two?.State.ToSwitchState() ?? SwitchState.Off }
            }
        };
    }
    
    public static ThreeSwitchParameters? ToParameters(this ThreeStateToggle state)
    {
        var toggle = state.Toggle;
        if (toggle is null)
            return null;
        return new ThreeSwitchParameters
        {
            Switches = new[]
            {
                new LinkSwitch { Outlet = 0, Switch = toggle.One?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 1, Switch = toggle.Two?.State.ToSwitchState() ?? SwitchState.Off },
                new LinkSwitch { Outlet = 2, Switch = toggle.Three?.State.ToSwitchState() ?? SwitchState.Off }
            }
        };
    }
    
    public static ZbCurtainParameters ToParameters(this CurtainState state)
    {
        return new ZbCurtainParameters
        {
            Battery = (int?)state.Battery?.Value ?? 100,
            CurrentPercent = (int?)state.Percentage?.Value ?? 0,
            MotorDirection = state.MotorReverse?.ToMotorDirection() ?? MotorDirection.Forward,
            MotorCalibration = state.MotorCalibration?.ToMotorCalibration() ?? MotorCalibration.Normal,
            OpenPercent = (int?)state.Percentage?.Value ?? 0,
        };
    }
    
    public static SnZbDoorWindowParameters? ToParameters(this WindowDoorSensor state)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbDoorWindowParameters
        {
            Battery = (int?)state.Battery?.Value ?? 100,
            Open = state.Detect.Detected,
        };
    }
    
    public static SnZbThermostatParameters ToParameters(this TemperatureAndHumiditySensor state)
    {
        return new SnZbThermostatParameters
        {
            Temperature = state.Temperature?.Value,
            Humidity = state.Humidity?.Value,
        };
    }
    
    public static SnZbMotionPParameters? ToParameters(this MotionSensorPro state)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbMotionPParameters
        {
            Battery = (int?)state.Battery?.Value ?? 100,
            Motion = state.Detect.ToMotion(),
            BrState = state.IlluminationLevel?.ToIlluminationLevel() ?? IlluminationLevel.Darker,
        };
    }
    
    public static SnZbMotionParameters? ToParameters(this MotionSensor state)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbMotionParameters
        {
            Battery = (int?)state.Battery?.Value ?? 100,
            Motion = state.Detect.ToMotion(),
        };
    }
    
    public static SnZbHumanPresenceParameters? ToParameters(this PresenceSensor state)
    {
        if (state.Detect is null)
            return null;
        
        return new SnZbHumanPresenceParameters
        {
            Human = state.Detect.ToPresence(),
            
        };
    }
    
    public static ZbSmartWaterValveParameters? ToParameters(this SwvState state)
    {
        if (state.Power is null)
            return null;
        
        return new ZbSmartWaterValveParameters
        {
            Switch = state.Power.ToSwitchState()
        };
    }

    public static SwitchParameters? ToParameters(this MiniState state)
    {
        if (state.Power is null)
            return null;
        
        return new SwitchParameters { Switch = state.Power.State.ToSwitchState() };
    }
    
    public static THOriginParameters? ToParameters(this TemperatureAndHumidityControl state)
    {
        if (state.Power is null)
            return null;
        
        return new THOriginParameters
        {
            Switch = state.Power.State.ToSwitchState(),
            Humidity = state.Humidity?.Value,
            Temperature = state.Temperature?.Value,
            AutoControlEnabled = state.ThermostatMode?.Mode.Value == ThermostatModeState.Auto  
        };
    }
}