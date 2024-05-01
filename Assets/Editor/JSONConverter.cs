using System;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class Float3Converter : JsonConverter<float3>
{
    public override void WriteJson(JsonWriter writer, float3 value, JsonSerializer serializer)
    {
        JObject obj = new JObject() { ["x"] = value.x, ["y"] = value.y, ["z"] = value.z };
        obj.WriteTo(writer);
    }
    public override float3 ReadJson(JsonReader reader, Type objectType, float3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new float3((float)obj.GetValue("x"), (float)obj.GetValue("y"), (float)obj.GetValue("z"));
    }
}

public class PlaneConverter : JsonConverter<Plane>
{
    public override void WriteJson(JsonWriter writer, Plane value, JsonSerializer serializer)
    {
        JObject obj = new JObject() { ["n.x"] = value.normal.x, ["n.y"] = value.normal.y, ["n.z"] = value.normal.z, ["d"] = value.distance};
        obj.WriteTo(writer);
    }
    public override Plane ReadJson(JsonReader reader, Type objectType, Plane existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Plane(new float3((float)obj.GetValue("n.x"), (float)obj.GetValue("n.y"), (float)obj.GetValue("n.z")), (float) obj.GetValue("d"));
    }
}

public static class JsonCustomSettings
{
    public static void ConfigureJsonInternal()
    {
        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Float3Converter());
            settings.Converters.Add(new PlaneConverter());
            return settings;
        };
    }
}
// this must be inside an Editor/ folder
public static class EditorJsonSettings
{
    [InitializeOnLoadMethod]
    public static void ApplyCustomConverters()
    {
        JsonCustomSettings.ConfigureJsonInternal();
    }
}
public static class RuntimeJsonSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ApplyCustomConverters()
    {
        JsonCustomSettings.ConfigureJsonInternal();
    }
}