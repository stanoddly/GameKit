namespace Pixely.Architecture;

public record struct CommandResult(int Code, string Message)
{
    public static readonly CommandResult Success = new(0, string.Empty);

    public static CommandResult FromError(string message)
    {
        return new CommandResult(1, message);
    }

    public static CommandResult FromError(int code, string message)
    {
        return new CommandResult(code, message);
    }

    public bool IsSuccess => Code == 0;

    public static implicit operator bool(CommandResult result)
    {
        return result.IsSuccess;
    }
}
