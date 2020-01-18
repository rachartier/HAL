using System.Collections.Generic;
using HAL.Loggin;
using HAL.Plugin;
using Newtonsoft.Json.Linq;

public class DifferencialStorage
{
    private JObject storedObject;

    public bool HasDifference<T>(APlugin plugin, T obj)
    {
        if (!plugin.ObserveAllAttributes && plugin.AttributesToObserve == null)
        {
            return true;
        }

        JObject convertedJsonObject = JObject.Parse(obj.ToString());

        if (storedObject == null)
        {
            storedObject = convertedJsonObject;
            return true;
        }

        List<string> attributesToObserve;

        if (plugin.ObserveAllAttributes)
        {
            attributesToObserve = new List<string>();

            foreach (var token in convertedJsonObject)
            {
                attributesToObserve.Add(token.Key);
            }
        }
        else
        {
            attributesToObserve = plugin.AttributesToObserve;
        }

        foreach (var attr in attributesToObserve)
        {
            var storedAttr = storedObject[attr]?.Value<string>();

            if (convertedJsonObject[attr] == null)
            {
                Log.Instance?.Warn($"Attribute {attr} in differencial mode on plugin '{plugin.Infos.FileName}' is uknown. Difference returned is true.");
                return true;
            }

            if (!convertedJsonObject[attr]?.Value<string>().Equals(storedAttr) == true)
            {
                storedObject = convertedJsonObject;
                return true;
            }
        }

        return false;
    }
}
