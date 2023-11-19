namespace Tunnlr.Client.Features.Interceptors.Core;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class UiElement : Attribute
{
    public required string DisplayName { get; set; }
    public required UiElementTypes ElementType { get; set; }
    public string? ValidationRegex { get; set; }
}

public enum UiElementTypes
{
    Label,
    TextInput,
    NumbersInput,
}