namespace Tunnlr.Client.Features.Interceptors.Core;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class InterceptorProperty : Attribute
{
    /// <summary>
    /// <para>By setting this attribute, this will:</para>
    /// 1. display the property in the UI<br />
    /// 2. save the value in the database<br />
    /// 3. use this value when the interceptor is executed
    /// </summary>
    public InterceptorProperty()
    {
    }
    public required string DisplayName { get; set; }
    public required PropertyType PropertyType { get; set; }
    public string? ValidationRegex { get; set; }
}

/// <summary>
/// This type will be used on the frontend to display the correct (input) type
/// </summary>
public enum PropertyType
{
    Label,
    Text,
    Numbers,
}