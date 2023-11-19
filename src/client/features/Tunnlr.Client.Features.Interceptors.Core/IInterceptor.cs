using System.Text.RegularExpressions;

namespace Tunnlr.Client.Features.Interceptors.Core;

public interface IInterceptor
{
    /// <summary>
    /// Returns a human readable friendly name, which is used to show the name in the UI.
    /// </summary>
    public string FriendlyName { get; }
    
    /// <summary>
    /// Returns a subtitle which is shown in the UI. For instance, the configured header name.
    /// </summary>
    public string Subtitle { get; }

    /// <summary>
    /// Returns whether the configuration is valid.
    /// </summary>
    /// <returns></returns>
    public bool ValidateConfiguration()
    {
        var properties = this.GetUiPropertiesWithValues();

        foreach (var property in properties)
        {
            if (string.IsNullOrWhiteSpace(property.Key.uiElement.ValidationRegex)) continue;

            var regex = new Regex(property.Key.uiElement.ValidationRegex);
            if (!regex.Match(property.Value).Success) return false;
        }

        return true;
    }
}