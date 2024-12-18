using SDL;

namespace GameKit.Input;

public enum VirtualKey: uint
{
    Unknown = SDL_Keycode.SDLK_UNKNOWN,
    Backspace = SDL_Keycode.SDLK_BACKSPACE,
    Tab = SDL_Keycode.SDLK_TAB,
    Return = SDL_Keycode.SDLK_RETURN,
    Escape = SDL_Keycode.SDLK_ESCAPE,
    Space = SDL_Keycode.SDLK_SPACE,
    Exclaim = SDL_Keycode.SDLK_EXCLAIM,
    DoubleQuote = SDL_Keycode.SDLK_DBLAPOSTROPHE,
    Hash = SDL_Keycode.SDLK_HASH,
    Dollar = SDL_Keycode.SDLK_DOLLAR,
    Percent = SDL_Keycode.SDLK_PERCENT,
    Ampersand = SDL_Keycode.SDLK_AMPERSAND,
    Quote = SDL_Keycode.SDLK_APOSTROPHE,
    LeftParenthesis = SDL_Keycode.SDLK_LEFTPAREN,
    RightParenthesis = SDL_Keycode.SDLK_RIGHTPAREN,
    Asterisk = SDL_Keycode.SDLK_ASTERISK,
    Plus = SDL_Keycode.SDLK_PLUS,
    Comma = SDL_Keycode.SDLK_COMMA,
    Minus = SDL_Keycode.SDLK_MINUS,
    Period = SDL_Keycode.SDLK_PERIOD,
    Slash = SDL_Keycode.SDLK_SLASH,
    Number0 = SDL_Keycode.SDLK_0,
    Number1 = SDL_Keycode.SDLK_1,
    Number2 = SDL_Keycode.SDLK_2,
    Number3 = SDL_Keycode.SDLK_3,
    Number4 = SDL_Keycode.SDLK_4,
    Number5 = SDL_Keycode.SDLK_5,
    Number6 = SDL_Keycode.SDLK_6,
    Number7 = SDL_Keycode.SDLK_7,
    Number8 = SDL_Keycode.SDLK_8,
    Number9 = SDL_Keycode.SDLK_9,
    Colon = SDL_Keycode.SDLK_COLON,
    Semicolon = SDL_Keycode.SDLK_SEMICOLON,
    Less = SDL_Keycode.SDLK_LESS,
    Equals = SDL_Keycode.SDLK_EQUALS,
    Greater = SDL_Keycode.SDLK_GREATER,
    Question = SDL_Keycode.SDLK_QUESTION,
    At = SDL_Keycode.SDLK_AT,
    LeftBracket = SDL_Keycode.SDLK_LEFTBRACKET,
    Backslash = SDL_Keycode.SDLK_BACKSLASH,
    RightBracket = SDL_Keycode.SDLK_RIGHTBRACKET,
    Caret = SDL_Keycode.SDLK_CARET,
    Underscore = SDL_Keycode.SDLK_UNDERSCORE,
    BackQuote = SDL_Keycode.SDLK_GRAVE,
    A = SDL_Keycode.SDLK_A,
    B = SDL_Keycode.SDLK_B,
    C = SDL_Keycode.SDLK_C,
    D = SDL_Keycode.SDLK_D,
    E = SDL_Keycode.SDLK_E,
    F = SDL_Keycode.SDLK_F,
    G = SDL_Keycode.SDLK_G,
    H = SDL_Keycode.SDLK_H,
    I = SDL_Keycode.SDLK_I,
    J = SDL_Keycode.SDLK_J,
    K = SDL_Keycode.SDLK_K,
    L = SDL_Keycode.SDLK_L,
    M = SDL_Keycode.SDLK_M,
    N = SDL_Keycode.SDLK_N,
    O = SDL_Keycode.SDLK_O,
    P = SDL_Keycode.SDLK_P,
    Q = SDL_Keycode.SDLK_Q,
    R = SDL_Keycode.SDLK_R,
    S = SDL_Keycode.SDLK_S,
    T = SDL_Keycode.SDLK_T,
    U = SDL_Keycode.SDLK_U,
    V = SDL_Keycode.SDLK_V,
    W = SDL_Keycode.SDLK_W,
    X = SDL_Keycode.SDLK_X,
    Y = SDL_Keycode.SDLK_Y,
    Z = SDL_Keycode.SDLK_Z,
    LeftBrace = SDL_Keycode.SDLK_LEFTBRACE,
    Pipe = SDL_Keycode.SDLK_PIPE,
    RightBrace = SDL_Keycode.SDLK_RIGHTBRACE,
    Tilde = SDL_Keycode.SDLK_TILDE,
    Delete = SDL_Keycode.SDLK_DELETE,
    PlusMinus = SDL_Keycode.SDLK_PLUSMINUS,
    ScancodeMask = SDL_Keycode.SDLK_SCANCODE_MASK,
    CapsLock = SDL_Keycode.SDLK_CAPSLOCK,
    F1 = SDL_Keycode.SDLK_F1,
    F2 = SDL_Keycode.SDLK_F2,
    F3 = SDL_Keycode.SDLK_F3,
    F4 = SDL_Keycode.SDLK_F4,
    F5 = SDL_Keycode.SDLK_F5,
    F6 = SDL_Keycode.SDLK_F6,
    F7 = SDL_Keycode.SDLK_F7,
    F8 = SDL_Keycode.SDLK_F8,
    F9 = SDL_Keycode.SDLK_F9,
    F10 = SDL_Keycode.SDLK_F10,
    F11 = SDL_Keycode.SDLK_F11,
    F12 = SDL_Keycode.SDLK_F12,
    PrintScreen = SDL_Keycode.SDLK_PRINTSCREEN,
    ScrollLock = SDL_Keycode.SDLK_SCROLLLOCK,
    Pause = SDL_Keycode.SDLK_PAUSE,
    Insert = SDL_Keycode.SDLK_INSERT,
    Home = SDL_Keycode.SDLK_HOME,
    PageUp = SDL_Keycode.SDLK_PAGEUP,
    End = SDL_Keycode.SDLK_END,
    PageDown = SDL_Keycode.SDLK_PAGEDOWN,
    Right = SDL_Keycode.SDLK_RIGHT,
    Left = SDL_Keycode.SDLK_LEFT,
    Down = SDL_Keycode.SDLK_DOWN,
    Up = SDL_Keycode.SDLK_UP,
    NumLockClear = SDL_Keycode.SDLK_NUMLOCKCLEAR,
    KeypadDivide = SDL_Keycode.SDLK_KP_DIVIDE,
    KeypadMultiply = SDL_Keycode.SDLK_KP_MULTIPLY,
    KeypadMinus = SDL_Keycode.SDLK_KP_MINUS,
    KeypadPlus = SDL_Keycode.SDLK_KP_PLUS,
    KeypadEnter = SDL_Keycode.SDLK_KP_ENTER,
    Keypad1 = SDL_Keycode.SDLK_KP_1,
    Keypad2 = SDL_Keycode.SDLK_KP_2,
    Keypad3 = SDL_Keycode.SDLK_KP_3,
    Keypad4 = SDL_Keycode.SDLK_KP_4,
    Keypad5 = SDL_Keycode.SDLK_KP_5,
    Keypad6 = SDL_Keycode.SDLK_KP_6,
    Keypad7 = SDL_Keycode.SDLK_KP_7,
    Keypad8 = SDL_Keycode.SDLK_KP_8,
    Keypad9 = SDL_Keycode.SDLK_KP_9,
    Keypad0 = SDL_Keycode.SDLK_KP_0,
    KeypadPeriod = SDL_Keycode.SDLK_KP_PERIOD,
    Application = SDL_Keycode.SDLK_APPLICATION,
    Power = SDL_Keycode.SDLK_POWER,
    KeypadEquals = SDL_Keycode.SDLK_KP_EQUALS,
    F13 = SDL_Keycode.SDLK_F13,
    F14 = SDL_Keycode.SDLK_F14,
    F15 = SDL_Keycode.SDLK_F15,
    F16 = SDL_Keycode.SDLK_F16,
    F17 = SDL_Keycode.SDLK_F17,
    F18 = SDL_Keycode.SDLK_F18,
    F19 = SDL_Keycode.SDLK_F19,
    F20 = SDL_Keycode.SDLK_F20,
    F21 = SDL_Keycode.SDLK_F21,
    F22 = SDL_Keycode.SDLK_F22,
    F23 = SDL_Keycode.SDLK_F23,
    F24 = SDL_Keycode.SDLK_F24,
    Execute = SDL_Keycode.SDLK_EXECUTE,
    Help = SDL_Keycode.SDLK_HELP,
    Menu = SDL_Keycode.SDLK_MENU,
    Select = SDL_Keycode.SDLK_SELECT,
    Stop = SDL_Keycode.SDLK_STOP,
    Again = SDL_Keycode.SDLK_AGAIN,
    Undo = SDL_Keycode.SDLK_UNDO,
    Cut = SDL_Keycode.SDLK_CUT,
    Copy = SDL_Keycode.SDLK_COPY,
    Paste = SDL_Keycode.SDLK_PASTE,
    Find = SDL_Keycode.SDLK_FIND,
    Mute = SDL_Keycode.SDLK_MUTE,
    VolumeUp = SDL_Keycode.SDLK_VOLUMEUP,
    VolumeDown = SDL_Keycode.SDLK_VOLUMEDOWN,
    KeypadComma = SDL_Keycode.SDLK_KP_COMMA,
    KeypadEqualsAs400 = SDL_Keycode.SDLK_KP_EQUALSAS400,
    AltErase = SDL_Keycode.SDLK_ALTERASE,
    SysReq = SDL_Keycode.SDLK_SYSREQ,
    Cancel = SDL_Keycode.SDLK_CANCEL,
    Clear = SDL_Keycode.SDLK_CLEAR,
    Prior = SDL_Keycode.SDLK_PRIOR,
    Return2 = SDL_Keycode.SDLK_RETURN2,
    Separator = SDL_Keycode.SDLK_SEPARATOR,
    Out = SDL_Keycode.SDLK_OUT,
    Oper = SDL_Keycode.SDLK_OPER,
    ClearAgain = SDL_Keycode.SDLK_CLEARAGAIN,
    CrSel = SDL_Keycode.SDLK_CRSEL,
    ExSel = SDL_Keycode.SDLK_EXSEL,
    Keypad00 = SDL_Keycode.SDLK_KP_00,
    Keypad000 = SDL_Keycode.SDLK_KP_000,
    ThousandsSeparator = SDL_Keycode.SDLK_THOUSANDSSEPARATOR,
    DecimalSeparator = SDL_Keycode.SDLK_DECIMALSEPARATOR,
    CurrencyUnit = SDL_Keycode.SDLK_CURRENCYUNIT,
    CurrencySubUnit = SDL_Keycode.SDLK_CURRENCYSUBUNIT,
    KeypadLeftParenthesis = SDL_Keycode.SDLK_KP_LEFTPAREN,
    KeypadRightParenthesis = SDL_Keycode.SDLK_KP_RIGHTPAREN,
    KeypadLeftBrace = SDL_Keycode.SDLK_KP_LEFTBRACE,
    KeypadRightBrace = SDL_Keycode.SDLK_KP_RIGHTBRACE,
    KeypadTab = SDL_Keycode.SDLK_KP_TAB,
    KeypadBackspace = SDL_Keycode.SDLK_KP_BACKSPACE,
    KeypadA = SDL_Keycode.SDLK_KP_A,
    KeypadB = SDL_Keycode.SDLK_KP_B,
    KeypadC = SDL_Keycode.SDLK_KP_C,
    KeypadD = SDL_Keycode.SDLK_KP_D,
    KeypadE = SDL_Keycode.SDLK_KP_E,
    KeypadF = SDL_Keycode.SDLK_KP_F,
    KeypadXor = SDL_Keycode.SDLK_KP_XOR,
    KeypadPower = SDL_Keycode.SDLK_KP_POWER,
    KeypadPercent = SDL_Keycode.SDLK_KP_PERCENT,
    KeypadLess = SDL_Keycode.SDLK_KP_LESS,
    KeypadGreater = SDL_Keycode.SDLK_KP_GREATER,
    KeypadAmpersand = SDL_Keycode.SDLK_KP_AMPERSAND,
    KeypadDoubleAmpersand = SDL_Keycode.SDLK_KP_DBLAMPERSAND,
    KeypadVerticalBar = SDL_Keycode.SDLK_KP_VERTICALBAR,
    KeypadDoubleVerticalBar = SDL_Keycode.SDLK_KP_DBLVERTICALBAR,
    KeypadColon = SDL_Keycode.SDLK_KP_COLON,
    KeypadHash = SDL_Keycode.SDLK_KP_HASH,
    KeypadSpace = SDL_Keycode.SDLK_KP_SPACE,
    KeypadAt = SDL_Keycode.SDLK_KP_AT,
    KeypadExclaim = SDL_Keycode.SDLK_KP_EXCLAM,
    KeypadMemStore = SDL_Keycode.SDLK_KP_MEMSTORE,
    KeypadMemRecall = SDL_Keycode.SDLK_KP_MEMRECALL,
    KeypadMemClear = SDL_Keycode.SDLK_KP_MEMCLEAR,
    KeypadMemAdd = SDL_Keycode.SDLK_KP_MEMADD,
    KeypadMemSubtract = SDL_Keycode.SDLK_KP_MEMSUBTRACT,
    KeypadMemMultiply = SDL_Keycode.SDLK_KP_MEMMULTIPLY,
    KeypadMemDivide = SDL_Keycode.SDLK_KP_MEMDIVIDE,
    KeypadPlusMinus = SDL_Keycode.SDLK_KP_PLUSMINUS,
    KeypadClear = SDL_Keycode.SDLK_KP_CLEAR,
    KeypadClearEntry = SDL_Keycode.SDLK_KP_CLEARENTRY,
    KeypadBinary = SDL_Keycode.SDLK_KP_BINARY,
    KeypadOctal = SDL_Keycode.SDLK_KP_OCTAL,
    KeypadDecimal = SDL_Keycode.SDLK_KP_DECIMAL,
    KeypadHexadecimal = SDL_Keycode.SDLK_KP_HEXADECIMAL,
    LeftControl = SDL_Keycode.SDLK_LCTRL,
    LeftShift = SDL_Keycode.SDLK_LSHIFT,
    LeftAlt = SDL_Keycode.SDLK_LALT,
    LeftGui = SDL_Keycode.SDLK_LGUI,
    RightControl = SDL_Keycode.SDLK_RCTRL,
    RightShift = SDL_Keycode.SDLK_RSHIFT,
    RightAlt = SDL_Keycode.SDLK_RALT,
    RightGui = SDL_Keycode.SDLK_RGUI,
    Mode = SDL_Keycode.SDLK_MODE,
    Sleep = SDL_Keycode.SDLK_SLEEP,
    Wake = SDL_Keycode.SDLK_WAKE,
    ChannelIncrement = SDL_Keycode.SDLK_CHANNEL_INCREMENT,
    ChannelDecrement = SDL_Keycode.SDLK_CHANNEL_DECREMENT,
    MediaPlay = SDL_Keycode.SDLK_MEDIA_PLAY,
    MediaPause = SDL_Keycode.SDLK_MEDIA_PAUSE,
    MediaRecord = SDL_Keycode.SDLK_MEDIA_RECORD,
    MediaFastForward = SDL_Keycode.SDLK_MEDIA_FAST_FORWARD,
    MediaRewind = SDL_Keycode.SDLK_MEDIA_REWIND,
    MediaNextTrack = SDL_Keycode.SDLK_MEDIA_NEXT_TRACK,
    MediaPreviousTrack = SDL_Keycode.SDLK_MEDIA_PREVIOUS_TRACK,
    MediaStop = SDL_Keycode.SDLK_MEDIA_STOP,
    MediaEject = SDL_Keycode.SDLK_MEDIA_EJECT,
    MediaPlayPause = SDL_Keycode.SDLK_MEDIA_PLAY_PAUSE,
    MediaSelect = SDL_Keycode.SDLK_MEDIA_SELECT,
    AppControlNew = SDL_Keycode.SDLK_AC_NEW,
    AppControlOpen = SDL_Keycode.SDLK_AC_OPEN,
    AppControlClose = SDL_Keycode.SDLK_AC_CLOSE,
    AppControlExit = SDL_Keycode.SDLK_AC_EXIT,
    AppControlSave = SDL_Keycode.SDLK_AC_SAVE,
    AppControlPrint = SDL_Keycode.SDLK_AC_PRINT,
    AppControlProperties = SDL_Keycode.SDLK_AC_PROPERTIES,
    AppControlSearch = SDL_Keycode.SDLK_AC_SEARCH,
    AppControlHome = SDL_Keycode.SDLK_AC_HOME,
    AppControlBack = SDL_Keycode.SDLK_AC_BACK,
    AppControlForward = SDL_Keycode.SDLK_AC_FORWARD,
    AppControlStop = SDL_Keycode.SDLK_AC_STOP,
    AppControlRefresh = SDL_Keycode.SDLK_AC_REFRESH,
    AppControlBookmarks = SDL_Keycode.SDLK_AC_BOOKMARKS,
    SoftLeft = SDL_Keycode.SDLK_SOFTLEFT,
    SoftRight = SDL_Keycode.SDLK_SOFTRIGHT,
    Call = SDL_Keycode.SDLK_CALL,
    EndCall = SDL_Keycode.SDLK_ENDCALL
}
