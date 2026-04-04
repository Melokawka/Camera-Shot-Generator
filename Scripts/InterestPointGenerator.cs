using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// // fajnie byloby przygotowac jakas wizualizacje na potrzeby magisterki

// fajnie byłoby zrobić plan wedlug ktorego kazdy komponent/skrypt posiada scisle okreslone elementy
// np InterestPointgenerator że potrzebuje terrain, environment
// albo np charrandomizer potrzebuje prefabow
public class InterestPointGenerator : MonoBehaviour  // Must be MonoBehaviour for runtime raycasting
{
    public float minDistance = 7f;   // Minimum distance between points // stupid
    public float navMeshSampleDistance = 2f; 
    public float minDistanceFromNavMeshEdges = 1.5f;

    public List<Vector3> GenerateInterestPoints(int numInterestPoints)
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        Mesh navMeshTriangulated = new Mesh();
        navMeshTriangulated.vertices = triangulation.vertices;
        navMeshTriangulated.triangles = triangulation.indices;

        List<Vector3> interestPoints = new();
        for (int i = 0; i < numInterestPoints; i++)
        {
            while (interestPoints.Count < numInterestPoints)  // i dont like the first loop guard, just add a break instead and calculate 1 point at a time
            {
                Vector3 candidate = GetRandomPointOnMesh(navMeshTriangulated);

                if (IsAwayFromEdge(candidate, minDistanceFromNavMeshEdges))  // IsFarEnough(candidate, interestPoints, minDistance)
                {
                    interestPoints.Add(candidate);
                }
            }
        }

        return interestPoints;
    }

    // bool IsFarEnough(Vector3 candidate, List<Vector3> existingPoints, float minDist)  // poisson  // nope
    // {
    //     foreach (var point in existingPoints)
    //     {
    //         if (Vector3.Distance(candidate, point) < minDist)
    //             return false;
    //     }
    //     return true;
    // }

    bool IsAwayFromEdge(Vector3 point, float minDistance)
    {
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(point, out hit, NavMesh.AllAreas))
        {
            float dist = Vector3.Distance(point, hit.position);
            return dist >= minDistance;
        }

        // Jeśli nie udało się znaleźć krawędzi, traktuj jako niepoprawny punkt
        return false;
    }

    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        // Oblicz rozmiary trójkątów
        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        // Losowy punkt
        float randomsample = Random.value * total;
        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1)
        {
            Debug.LogError("triIndex should never be -1");
            triIndex = 0;
        }

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        // Losowe barycentryczne współrzędne
        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }

        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);

        return pointOnMesh;
    }

    float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = 0.5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }

    public List<Vector3> GetPointsCountOnNavMesh(GameObject environment, float spacing = 1f)
    {
        //GameObject temp = Instantiate(environment);

        List<Vector3> validPoints = new List<Vector3>();

        Renderer[] renderers = environment.GetComponentsInChildren<Renderer>();
        Bounds bounds = new();
        if (renderers.Length != 0) {
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (Terrain.activeTerrain != null)
            bounds.Encapsulate(Terrain.activeTerrain.terrainData.bounds);

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;

        for (float x = minX; x <= maxX; x += spacing)
        {
            for (float z = minZ; z <= maxZ; z += spacing)
            {
                Vector3 samplePoint = new Vector3(x, 20f, z);

                if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, 40f, NavMesh.AllAreas) &&
                    Mathf.Abs(samplePoint.x - hit.position.x) + Mathf.Abs(samplePoint.z - hit.position.z) < 0.1f)
                {
                    validPoints.Add(hit.position); // optionally: use samplePoint instead of hit.position
                }
            }
        }

       // Destroy(temp);

        return validPoints;
    }
}


    // public float minDistance = 7f;  // Minimum distance between points  // how do we calculate a maximum that doesnt break the code?
    // public int maxSamples = 30;      // Number of samples before rejection

    // public List<Vector3> GenerateInterestPoints(GameObject environment, int numInterestPoints)
    // {   
    //     Terrain terrain = MiscFunctions.GetTerrainFromEnvironment();
    //     bool isTerrain = terrain != null;

    //     return isTerrain 
    //         ? GenerateTerrainPoints(terrain, numInterestPoints) 
    //         : GenerateGenericPoints(environment, numInterestPoints);
    // }

    // private List<Vector3> GenerateTerrainPoints(Terrain terrain, int numInterestPoints)
    // {
    //     Vector3 terrainPos = terrain.transform.position;
    //     Vector3 terrainSize = terrain.terrainData.size;

    //     // Generate Poisson disk points in the terrain area
    //     List<Vector2> poissonPoints = PoissonDiskSampling.Generate(
    //         terrainPos.x, terrainPos.z, terrainSize.x, terrainSize.z, minDistance, numInterestPoints, maxSamples
    //     );

    //     List<Vector3> interestPoints = new();
    //     foreach (Vector2 point in poissonPoints)
    //     {
    //         Vector3 clusterPos = new Vector3(point.x, 0, point.y);
    //         clusterPos.y = terrain.SampleHeight(clusterPos) + terrainPos.y;  // Get Y height
    //         interestPoints.Add(clusterPos);
    //     }

    //     return interestPoints;
    // }

    // private List<Vector3> GenerateGenericPoints(GameObject environment, int numInterestPoints)
    // {
    //     List<Vector3> interestPoints = new();
    //     Renderer[] renderers = environment.GetComponentsInChildren<Renderer>();

    //     if (renderers.Length == 0)
    //     {
    //         Debug.LogError("No renderers found in environment. Returning empty interest points.");
    //         return interestPoints;
    //     }

    //     Bounds bounds = renderers[0].bounds;
    //     foreach (Renderer renderer in renderers)
    //     {
    //         bounds.Encapsulate(renderer.bounds);
    //     }

    //     // Generate Poisson disk points in the bounds area
    //     List<Vector2> poissonPoints = PoissonDiskSampling.Generate(
    //         bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, minDistance, numInterestPoints, maxSamples
    //     );

    //     foreach (Vector2 point in poissonPoints)
    //     {
    //         Vector3 clusterPos = new Vector3(point.x, 50f, point.y);
    //         clusterPos.y = PerformRaycast(clusterPos);  // Get actual Y height
    //         interestPoints.Add(clusterPos);
    //     }

    //     return interestPoints;
    // }

    // private float PerformRaycast(Vector3 position)
    // {
    //     Ray ray = new Ray(position, Vector3.down);
    //     if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    //     {
    //         return hit.point.y;
    //     }
    //     return 0f; // Default to ground level if no hit.
    // }
