using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ShotPlanConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(ShotPlan).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON object from the reader.
        JObject jo = JObject.Load(reader);

        // Get the shotType property from the JSON.
        JToken typeToken = jo["shotType"];
        if (typeToken == null)
            throw new Exception("Missing shotType property.");

        ShotTypes shotType = typeToken.ToObject<ShotTypes>();
        ShotPlan target;

        // Instantiate the appropriate derived class.
        switch (shotType)
        {
            case ShotTypes.Static:
                target = new ShotPlanStatic();
                break;
                
            case ShotTypes.ArcLeft:
                target = new ShotPlanArcLeft();
                break;

            case ShotTypes.ArcRight:
                target = new ShotPlanArcRight();
                break;

            case ShotTypes.PushOut:
                target = new ShotPlanPushOut();
                break;

            case ShotTypes.PushIn:
                target = new ShotPlanPushIn();
                break;

            case ShotTypes.PanLeft:
                target = new ShotPlanPanLeft();
                break;

            case ShotTypes.PanRight:
                target = new ShotPlanPanRight();
                break;

            case ShotTypes.RollLeft:
                target = new ShotPlanRollLeft();
                break;

            case ShotTypes.RollRight:
                target = new ShotPlanRollRight();
                break;

            case ShotTypes.DollyZoomIn:
                target = new ShotPlanDollyZoomIn();
                break;

            case ShotTypes.DollyZoomOut:
                target = new ShotPlanDollyZoomOut();
                break;

            case ShotTypes.SidewaysLeft:
                target = new ShotPlanSidewaysLeft();
                break;

            case ShotTypes.SidewaysRight:
                target = new ShotPlanSidewaysRight();
                break;

            case ShotTypes.WhipPan:
                target = new ShotPlanWhipPan();
                break;

            case ShotTypes.ZoomIn:
                target = new ShotPlanZoomIn();
            break;

            case ShotTypes.ZoomOut:
                target = new ShotPlanZoomOut();
            break;

            default:
                throw new Exception("Unknown shot type: " + shotType);
        }

        // Populate the object with the JSON data.
        serializer.Populate(jo.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Simply let Json.NET serialize the object into a JObject.
        // JObject jo = JObject.FromObject(value, serializer);
        // jo.WriteTo(writer);
        serializer.Serialize(writer, value, value.GetType());
    }
}
