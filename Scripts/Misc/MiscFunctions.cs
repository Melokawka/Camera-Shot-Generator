using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

public class MiscFunctions
{
    // static public Vector3 SpherePoint(Vector3 start, float minDist, float maxDist, float minY = 0f) { // works by discarding values outside desired range
    //     int tries = 0;

    //     Vector3 randPoint;
    //     float dist;
    //     do
    //     {
    //         float x = Random.Range(-maxDist, maxDist);
    //         float y = Random.Range(-maxDist, maxDist);
    //         float z = Random.Range(-maxDist, maxDist);

    //         randPoint = new(start.x + x, start.y + y, start.z + z);

    //         dist = Vector3.Distance(start, randPoint);

    //         tries++;
    //         if (tries > 100) break;

    //         Vector3 rayOrigin = new(start.x, 50f, start.z);
    //         if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f)) 
    //         {
    //             if (start.y < hit.point.y) continue;
    //         }
    //     } while (dist > maxDist || dist < minDist);

    //     if (randPoint.y < 0) randPoint.y = -randPoint.y;  // if below ground then take negative


    //     Debug.Log(randPoint);
    //     return randPoint;
    // }

    static public Vector3 SpherePoint(Vector3 origin, float minDist, float maxDist, float minY = Mathf.NegativeInfinity, float maxY = Mathf.Infinity)
    {
        int tries = 0;
        Vector3 point = Vector3.zero;

        do
        {
            float distance = Random.Range(minDist, maxDist);
            point = origin + Random.onUnitSphere * distance;
            tries++;
            if (tries == 99) Debug.LogWarning(tries);
        } while (!(Physics.Raycast(point, Vector3.down, out RaycastHit hit, 20f) && point.y >= minY && point.y <= maxY && tries < 100));
        // for raycasting to work we need to instantiate an env
        return point;
    } 
    
    public static void SerializeCameraPositions(List<CameraPositionData> positionDataList, string savePath)
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        jsonSerializerSettings.Converters.Add(new Vector3Converter());

        string json = JsonConvert.SerializeObject(positionDataList, jsonSerializerSettings);
        string filePath = Path.Combine(savePath, $"camera_positions.json");
        File.WriteAllText(filePath, json);
    }

    // public static Terrain GetTerrainFromEnvironment() {
    //     Terrain[] terrains = Terrain.activeTerrains;
    //     Terrain terrain = null;

    //     if (terrains != null && terrains.Length > 0)
    //     {
    //         // Loop through terrains and select the one whose bounds contain (0,0)
    //         foreach (Terrain t in terrains)
    //         {
    //             Vector3 pos = t.transform.position;
    //             Vector3 size = t.terrainData.size;
    //             if (0 >= pos.x && 0 <= pos.x + size.x &&
    //                 0 >= pos.z && 0 <= pos.z + size.z)
    //             {
    //                 terrain = t;
    //                 break;
    //             }
    //         }
    //     }

    //     return terrain;
    // }
}
