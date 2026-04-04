using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        float x = jo["x"].ToObject<float>();
        float y = jo["y"].ToObject<float>();
        float z = jo["z"].ToObject<float>();
        return new Vector3(x, y, z);
    }
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector3 vec = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vec.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vec.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vec.z);
        writer.WriteEndObject();
    }
    
    public override bool CanRead => true;
    public override bool CanWrite => true;
}