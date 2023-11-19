namespace Tunnlr.Client.Features.Interceptors.Core;

public static class InterceptorExtensions
{
    public static Dictionary<(string propertyName, UiElement uiElement), string> GetUiPropertiesWithValues(this IInterceptor interceptor)
    {
        var result = new Dictionary<(string propertyName, UiElement uiElement), string>();
        var properties = interceptor.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            var uiElementAttributes = property.GetCustomAttributes(typeof(UiElement), false).Cast<UiElement>();
            
            foreach (var uiElement in uiElementAttributes)
            {
                var value = property.GetValue(interceptor) as string;
                
                result.Add((property.Name, uiElement), string.IsNullOrWhiteSpace(value) ? string.Empty : value);
            }
        }

        return result;
    }
}