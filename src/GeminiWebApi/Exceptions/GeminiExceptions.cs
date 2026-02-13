using System;

namespace GeminiWebApi.Exceptions;

/// <summary>
/// Base exception for Gemini API errors.
/// </summary>
public class GeminiException : Exception
{
    public GeminiException(string? message = null) : base(message)
    {
    }

    public GeminiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when API returns an error.
/// </summary>
public class ApiException : GeminiException
{
    public ApiException(string? message = null) : base(message)
    {
    }

    public ApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when authentication fails.
/// </summary>
public class AuthenticationException : GeminiException
{
    public AuthenticationException(string? message = null) : base(message)
    {
    }

    public AuthenticationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when the model is invalid or unavailable.
/// </summary>
public class ModelInvalidException : GeminiException
{
    public ModelInvalidException(string? message = null) : base(message)
    {
    }
}

/// <summary>
/// Thrown when usage limit is exceeded.
/// </summary>
public class UsageLimitExceededException : GeminiException
{
    public UsageLimitExceededException(string? message = null) : base(message)
    {
    }
}

/// <summary>
/// Thrown when IP is temporarily blocked.
/// </summary>
public class TemporarilyBlockedException : GeminiException
{
    public TemporarilyBlockedException(string? message = null) : base(message)
    {
    }
}

/// <summary>
/// Thrown on timeout errors.
/// </summary>
public class TimeoutException : GeminiException
{
    public TimeoutException(string? message = null) : base(message)
    {
    }
}
