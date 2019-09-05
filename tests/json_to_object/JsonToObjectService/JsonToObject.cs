using Newtonsoft.Json.Linq;

public class Json
{
    public JObject ToObject(string json)
    {
        return JObject.Parse(json);
    }
}
