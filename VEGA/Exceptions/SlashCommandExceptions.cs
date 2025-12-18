namespace Exceptions;


/// <summary>
/// Abstract class, derived into SlashCommandBusinessException, or SlashCommandGenericException
/// </summary>
public abstract class SlashCommandException : Exception
{
    public bool Deferred {get; set;}

    protected SlashCommandException(string message, bool deferred) 
        : base(message)
    {
        Deferred = deferred;
    }
}


/// <summary>
/// To be used as wrapper for expected exceptions and business logic violations
/// </summary>
public class SlashCommandBusinessException : SlashCommandException
{
    public SlashCommandBusinessException(string message, bool deferred = false) : base(message, deferred) { }
}

/// <summary>
/// To be used as wrapper for unexpected exceptions in slash command executions
/// </summary>
public class SlashCommandGenericException : SlashCommandException
{
    public SlashCommandGenericException(string message, bool deferred = false) : base(message, deferred) { }
}