using System.Numerics;
using SDL;

namespace GameKit.Input;

public class Gamepad
{
    internal Gamepad(uint deviceId)
    {
        DeviceId = deviceId;
    }

    public uint DeviceId { get; }
    public Vector2 LeftStick { get; set; }
    public Vector2 RightStick { get; set; }
    public float LeftTrigger { get; set; }
    public float RightTrigger { get; set; }
    public int ButtonFlags { get; internal set; }
}

public delegate void GamepadMotionEventHandler(Gamepad gamepad, Vector2 motion);
public delegate void GamepadTriggerEventHandler(Gamepad gamepad, float value);
public delegate void GamepadButtonPressedHandler(Gamepad gamepad, GamepadButton button);
public delegate void GamepadButtonReleasedHandler(Gamepad gamepad, GamepadButton button);
public delegate void GamepadConnectionEventHandler(Gamepad gamepad);

public class GamepadService : IGamepadService
{
    private readonly Dictionary<SDL_JoystickID, Gamepad> _gamepads = new();

    private const float JoystickMinDivisor = -1 * SDL3.SDL_JOYSTICK_AXIS_MIN;
    private const float JoystickMaxDivisor = SDL3.SDL_JOYSTICK_AXIS_MAX;

    public event GamepadMotionEventHandler? LeftStickMotion;
    public event GamepadMotionEventHandler? RightStickMotion;
    public event GamepadTriggerEventHandler? LeftTriggerMotion;
    public event GamepadTriggerEventHandler? RightTriggerMotion;
    public event GamepadButtonPressedHandler? ButtonPress;
    public event GamepadButtonReleasedHandler? ButtonRelease;
    public event GamepadConnectionEventHandler? GamepadConnected;
    public event GamepadConnectionEventHandler? GamepadDisconnected;
    
    internal void SetupGamepads()
    {
        SDL3.SDL_SetGamepadEventsEnabled(true);
        unsafe
        {
            
            SDL_JoystickID *gamepads = SDL3.SDL_GetGamepads(null);

            if (gamepads == null)
            {
                return;
            } 

            int i = 0;
            while (gamepads[i] != 0)
            {
                // TODO: close in Dispose
                // TODO: initialize state based on the returned gamepad
                SDL_Gamepad* gamepad = SDL3.SDL_OpenGamepad(gamepads[i]);
                
                //string? mapping = SDL3.SDL_GetGamepadMapping(gamepad);
                
                _gamepads.Add(gamepads[i], new Gamepad((uint)gamepads[i]));
                i++;
            }
            SDL3.SDL_free(gamepads);
        }
    }

    internal unsafe void OnGamepadAdded(SDL_JoystickID joystickId)
    {
        if (_gamepads.ContainsKey(joystickId))
        {
            return;
        }

        SDL_Gamepad* gamepad = SDL3.SDL_OpenGamepad(joystickId);

        if (gamepad == null)
        {
            return;
        }

        var pad = new Gamepad((uint)joystickId);
        _gamepads.Add(joystickId, pad);
        GamepadConnected?.Invoke(pad);
    }

    internal void OnGamepadRemoved(SDL_JoystickID joystickId)
    {
        if (!_gamepads.Remove(joystickId, out Gamepad? pad))
        {
            return;
        }

        GamepadDisconnected?.Invoke(pad);
    }

    internal void OnGamepadButtonPressed(SDL_GamepadButtonEvent gamepadButtonEvent)
    {
        SDL_JoystickID joystickId = gamepadButtonEvent.which;
        
        Gamepad gamepad = _gamepads[joystickId];

        if (gamepadButtonEvent.Button == SDL_GamepadButton.SDL_GAMEPAD_BUTTON_INVALID)
        {
            return;
        }
        
        int buttonState = (1 << (int)gamepadButtonEvent.Button);
        bool isPressedAlready = (buttonState & gamepad.ButtonFlags) != 0;

        if (isPressedAlready)
        {
            return;
        }

        gamepad.ButtonFlags |= buttonState;
        
        ButtonPress?.Invoke(gamepad, (GamepadButton)gamepadButtonEvent.Button);
    }
    
    internal void OnGamepadButtonReleased(SDL_GamepadButtonEvent gamepadButtonEvent)
    {
        SDL_JoystickID joystickId = gamepadButtonEvent.which;
    
        Gamepad gamepad = _gamepads[joystickId];

        if (gamepadButtonEvent.Button == SDL_GamepadButton.SDL_GAMEPAD_BUTTON_INVALID)
        {
            return;
        }
    
        int buttonState = (1 << (int)gamepadButtonEvent.Button);
        bool isPressed = (buttonState & gamepad.ButtonFlags) != 0;

        if (!isPressed)
        {
            return;
        }

        gamepad.ButtonFlags &= ~buttonState;
    
        ButtonRelease?.Invoke(gamepad, (GamepadButton)gamepadButtonEvent.Button);
    }

    internal void OnGamepadStickMotion(in SDL_GamepadAxisEvent gamepadAxisEvent)
    {
        SDL_JoystickID joystickId = gamepadAxisEvent.which;

        Gamepad gamepad = _gamepads[joystickId];

        short value = gamepadAxisEvent.value;

        float normalizedValue = value switch
        {
            < 0 => value / JoystickMinDivisor,
            > 0 => value / JoystickMaxDivisor,
            _ => 0
        };

        // dead zone
        if (normalizedValue is < 0.2f and > -0.2f)
        {
            normalizedValue = 0f;
        }

        SDL_GamepadAxis gamepadAxis = (SDL_GamepadAxis)gamepadAxisEvent.axis;

        if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTX)
        {
            Vector2 originalLeftStickState = gamepad.LeftStick;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (originalLeftStickState.X == value)
            {
                return;
            }
            
            gamepad.LeftStick = originalLeftStickState with { X = normalizedValue };
            LeftStickMotion?.Invoke(gamepad, gamepad.LeftStick);
        }
        else if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY)
        {
            Vector2 originalLeftStickState = gamepad.LeftStick;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (originalLeftStickState.Y == value)
            {
                return;
            }
            
            gamepad.LeftStick = originalLeftStickState with { Y = normalizedValue };
            LeftStickMotion?.Invoke(gamepad, gamepad.LeftStick);
        }
        else if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTX)
        {
            Vector2 originalRightStickState = gamepad.RightStick;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (originalRightStickState.X == value)
            {
                return;
            }
            
            gamepad.RightStick = originalRightStickState with { X = normalizedValue };
            RightStickMotion?.Invoke(gamepad, gamepad.RightStick);
        }
        else if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY)
        {
            Vector2 originalRightStickState = gamepad.RightStick;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (originalRightStickState.Y == value)
            {
                return;
            }

            gamepad.RightStick = originalRightStickState with { Y = normalizedValue };
            RightStickMotion?.Invoke(gamepad, gamepad.RightStick);
        }
        else if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER)
        {
            float triggerValue = value / JoystickMaxDivisor;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (gamepad.LeftTrigger == triggerValue)
            {
                return;
            }

            gamepad.LeftTrigger = triggerValue;
            LeftTriggerMotion?.Invoke(gamepad, triggerValue);
        }
        else if (gamepadAxis == SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER)
        {
            float triggerValue = value / JoystickMaxDivisor;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (gamepad.RightTrigger == triggerValue)
            {
                return;
            }

            gamepad.RightTrigger = triggerValue;
            RightTriggerMotion?.Invoke(gamepad, triggerValue);
        }
    }
}
