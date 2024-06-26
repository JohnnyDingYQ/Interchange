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
        JObject obj = new JObject()
        {
            ["x"] = value.x,
            ["y"] = value.y,
            ["z"] = value.z
        };
        obj.WriteTo(writer);
    }
    public override float3 ReadJson(JsonReader reader, Type objectType, float3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new float3((float)obj.GetValue("x"), (float)obj.GetValue("y"), (float)obj.GetValue("z"));
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