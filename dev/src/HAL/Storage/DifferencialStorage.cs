using System.Collections.Generic;
using System.Threading.Tasks;
using HAL.Loggin;
using HAL.Plugin;
using HAL.Storage;
using Newtonsoft.Json.Linq;

public abstract class DifferencialStorage : IStoragePlugin
{
    private readonly JTokenEqualityComparer comparer = new JTokenEqualityComparer();
    private readonly IDictionary<string, JObject> storedObject = new Dictionary<string, JObject>();

    public virtual void Init(string connectionString)
    {
    }

    public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
    {
        if (!HasDifference(plugin, obj)) return StorageCode.Pass;

        return await SaveDifferencial(plugin, obj);
    }

    public virtual void Dispose()
    {
    }

    public bool HasDifference<T>(APlugin plugin, T obj)
    {
        if (!plugin.ObserveAllAttributes && plugin.AttributesToObserve == null) return true;

        var key = plugin.Infos.Name;

        var convertedJsonObject = JObject.Parse(obj.ToString());

        if (!storedObject.ContainsKey(plugin.Infos.Name))
        {
            storedObject.Add(key, convertedJsonObject);
            return true;
        }

        List<string> attributesToObserve;

        if (plugin.ObserveAllAttributes)
        {
            attributesToObserve = new List<string>();

            foreach (var token in convertedJsonObject) attributesToObserve.Add(token.Key);
        }
        else
        {
            attributesToObserve = plugin.AttributesToObserve;
        }

        foreach (var attr in attributesToObserve)
        {
            var storedAttr = storedObject[key][attr];

            if (convertedJsonObject[attr] == null)
            {
                Log.Instance?.Warn(
                    $"Attribute {attr} in differencial mode on plugin '{plugin.Infos.FileName}' is uknown. Difference returned is true.");
                return true;
            }

            if (!comparer.Equals(convertedJsonObject[attr], storedAttr))
            {
                storedObject[key] = convertedJsonObject;
                return true;
            }
        }

        return false;
    }

    public abstract Task<StorageCode> SaveDifferencial<T>(APlugin plugin, T obj);
}