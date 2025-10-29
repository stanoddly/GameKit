namespace GameKit.Input;

public enum GamepadButton
{
    Invalid = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_INVALID,
    South = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH,
    East = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_EAST,
    West = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_WEST,
    North = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_NORTH,
    Back = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_BACK,
    Guide = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_GUIDE,
    Start = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_START,
    LeftStick = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_STICK,
    RightStick = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_STICK,
    LeftShoulder = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_SHOULDER,
    RightShoulder = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER,
    DPadUp = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP,
    DPadDown = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN,
    DPadLeft = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT,
    DPadRight = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT,
    Misc1 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC1,
    RightPaddle1 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE1,
    LeftPaddle1 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE1,
    RightPaddle2 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE2,
    LeftPaddle2 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE2,
    Touchpad = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_TOUCHPAD,
    Misc2 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC2,
    Misc3 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC3,
    Misc4 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC4,
    Misc5 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC5,
    Misc6 = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC6,
    Count = SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_COUNT
}
