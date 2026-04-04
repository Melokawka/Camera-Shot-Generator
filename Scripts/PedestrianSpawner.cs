using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UMA;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianSpawner : MonoBehaviour
{
    public float minDistanceBetweenCharacters = 3f;
    public List<AnimationClip> idleAnimations;
    public List<AnimationClip> runAnimations;
    public List<string> pedestrianPrefabPaths;
    [Tooltip("Use UMA character creation instead of predefined character pool - very slow and not recommended")]
    public bool useDynamicCharacterCreation = false;

    public int idleCharactersPerCluster;
    public int runCharactersPerCluster;
    public float clusterCharactersNumberVariability;  // percentage

    public float clusterRadius;

    public float pathfindingCloseEnoughDistance;
    public float pathfindingRecheckDelay;
    
    public float pedMinSpeed;
    public float pedMaxSpeed;

    public List<GameObject> runningPedestrianList;
    public List<GameObject> idlePedestrianList;

    private Dictionary<GameObject, float> stationaryTimers = new Dictionary<GameObject, float>();
    float idleThreshold = 0.15f;
    
    public const int firstPedestrianLayer = 11;

    public void InitializeSettings(SettingsManager settingsManager, EnvironmentSettings envSettings)
    {
        idleCharactersPerCluster = envSettings.idleCharactersPerCluster;
        runCharactersPerCluster = envSettings.runCharactersPerCluster;
        clusterRadius = envSettings.clusterRadius;
        pathfindingCloseEnoughDistance = settingsManager.pathfindingCloseEnoughDistance;
        pathfindingRecheckDelay = settingsManager.pathfindingRecheckDelay;
        pedMinSpeed = envSettings.pedMinSpeed;
        pedMaxSpeed = envSettings.pedMaxSpeed;
        clusterCharactersNumberVariability = envSettings.clusterCharactersNumberVariability;
    }

    void Start()
    {
        SpawnIdlePedestrians(transform.position, 1);  // transform refers to the object with this script attached
        SpawnRunningPedestrians(transform.position, 1);
    }

    public List<GameObject> SpawnRunningPedestrians(Vector3 spawnPoint, int clusterId) {
        stationaryTimers = new();
        CancelInvoke(nameof(CheckDestinationReached));

        runningPedestrianList = new List<GameObject>();

        int totalCharacters = runCharactersPerCluster + (int)(Random.Range(0, clusterCharactersNumberVariability) * runCharactersPerCluster * Random.Range(0,2)*2 -1);
        for (int i = 0; i < totalCharacters; i++) {
            Vector3 randomSpawnPoint = GetValidRandomPoint(spawnPoint);

            GameObject characterInstance;
            if (useDynamicCharacterCreation)
            {
                gameObject.GetComponent<UMARandomAvatar>().ParentObject = this.gameObject;  // might not be necessary but not gonna risk it (script order thing)
                characterInstance = gameObject.GetComponent<UMARandomAvatar>().GenerateRandomCharacter(randomSpawnPoint, transform.rotation, "Pat");
            }
            else
            {
                string path = pedestrianPrefabPaths[Random.Range(0, pedestrianPrefabPaths.Count)]
                                .Replace("Resources/", "").Replace("Assets/", "").Replace(".prefab", "");
                characterInstance = Instantiate(Resources.Load<GameObject>(path), randomSpawnPoint, transform.rotation);
            }
            characterInstance.name = "Pat";//"pat" + i;
            int layerNr = clusterId + firstPedestrianLayer;
            characterInstance.layer = layerNr;

            runningPedestrianList.Add(characterInstance);

            characterInstance.tag = "Pat";  // where did I use it besides here? I think it's only for destroying objects after recording a shot

            ClusterIdentifier clusterComponent = characterInstance.AddComponent<ClusterIdentifier>();
            clusterComponent.clusterId = clusterId;

            Animator animator = characterInstance.GetComponent<Animator>();
            animator.applyRootMotion = true;  // it prevents characters from glitching out when pathfinding with navmesh
            animator.updateMode = AnimatorUpdateMode.Fixed; // fixedUpdate
            AssignUniqueAnimatorRun(characterInstance);

            NavMeshAgent agent = characterInstance.AddComponent<NavMeshAgent>();
            if (!agent.isOnNavMesh) Debug.Log("Agent not on NavMesh");

            Vector3 targetPoint = GetValidRandomPoint(transform.position);  // get a valid random walking destination point on the NavMesh
            agent.speed = Random.Range(pedMinSpeed, pedMaxSpeed);
            agent.SetDestination(targetPoint);
            InvokeRepeating(nameof(CheckDestinationReached), pathfindingRecheckDelay, pathfindingRecheckDelay);
        }

        return runningPedestrianList;
    }

    public async Task<List<GameObject>> SpawnIdlePedestrians(Vector3 spawnPoint, int clusterId) {
        idlePedestrianList = new List<GameObject>();
        List<Task> generationTasks = new List<Task>();
        
        int totalCharacters = idleCharactersPerCluster + (int)(Random.Range(0, clusterCharactersNumberVariability) * idleCharactersPerCluster * Random.Range(0,2)*2 -1);
        for (int i = 0; i < totalCharacters; i++) {
            Vector3 randomSpawnPoint = GetValidRandomPoint(spawnPoint, minDistanceBetweenCharacters);

            GameObject characterInstance;
            if (useDynamicCharacterCreation)
            {
                gameObject.GetComponent<UMARandomAvatar>().ParentObject = gameObject;
                characterInstance = gameObject.GetComponent<UMARandomAvatar>().GenerateRandomCharacter(randomSpawnPoint, transform.rotation, "Pat");
            }
            else
            {
                string path = pedestrianPrefabPaths[Random.Range(0, pedestrianPrefabPaths.Count)]
                                .Replace("Resources/", "").Replace("Assets/", "").Replace(".prefab", "");
                characterInstance = Instantiate(Resources.Load<GameObject>(path), randomSpawnPoint, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            }
            characterInstance.name = "Pat";//"pat" + i;
            int layerNr = clusterId + firstPedestrianLayer;
            characterInstance.layer = layerNr;

            idlePedestrianList.Add(characterInstance);

            characterInstance.tag = "Pat";

            ClusterIdentifier clusterComponent = characterInstance.AddComponent<ClusterIdentifier>();
            clusterComponent.clusterId = clusterId;

            //AssignUniqueAnimatorIdle(characterInstance);

            if (useDynamicCharacterCreation) {  // UMA-related waiting
                var umaData = characterInstance.GetComponent<UMA.UMAData>();
                if (umaData != null)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    umaData.OnCharacterCreated += (data) =>     // Kiedy postać się wygeneruje
                    {
                        tcs.TrySetResult(true);
                    };

                    generationTasks.Add(tcs.Task);
                }
            }
        }

        await Task.WhenAll(generationTasks);

        // foreach (GameObject characterInstance in idlePedestrianList)
        // {
        //     Rigidbody rb = characterInstance.GetComponent<Rigidbody>();
        //     if (rb == null)
        //     {
        //         rb = characterInstance.AddComponent<Rigidbody>();
        //         rb.useGravity = false;  // maybe?
        //         rb.isKinematic = true;  // it's necessary for running pedestrians to detect and avoid them on navmesh
        //     }

        //     rb.constraints = RigidbodyConstraints.FreezeAll;  // no need for idle characters to move or rotate
        // }

        return idlePedestrianList;
    }

    void AssignUniqueAnimatorRun(GameObject character)
    {
        var animator = character.GetComponent<Animator>();

        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);

        AnimationClip runClip = runAnimations[Random.Range(0, runAnimations.Count)];  // pick a random animation from the pool and assign it for the character

        aoc["Idle"] = runClip;
        aoc["Run"] = runClip;

        animator.runtimeAnimatorController = aoc;
    }

    void CheckDestinationReached()  // wonky but good enough
    {
        if (runningPedestrianList == null || runningPedestrianList.All(p => p == null)) {
            CancelInvoke(nameof(CheckDestinationReached));
            return;
        }

        foreach (GameObject ped in runningPedestrianList) {
            NavMeshAgent agent = ped.GetComponent<NavMeshAgent>();

            if (!stationaryTimers.ContainsKey(ped))
            {
                stationaryTimers[ped] = 0f;
            }

            if (agent.velocity.magnitude < 0.1f)  // Agent prawie stoi
            {
                stationaryTimers[ped] += pathfindingRecheckDelay;

                if (stationaryTimers[ped] >= idleThreshold)
                {
                    //Debug.Log($"Postać {ped.name} zbyt długo w bezruchu. Zmieniam cel.");
                    stationaryTimers[ped] = 0f;  // Resetuj timer

                    agent.SetDestination(GetValidRandomPoint(transform.position));
                }
            }
            else
            {
                stationaryTimers[ped] = 0f;  // Resetuj, jeśli się porusza
            }

            if (!agent.pathPending && agent.remainingDistance <= pathfindingCloseEnoughDistance)
            {
                agent.SetDestination(GetValidRandomPoint(transform.position));
            }
        }
    }

    Vector3 GetValidRandomPoint(Vector3 pos, float minDistanceBetweenCharacters = 0f)
    {
        Vector3 finalPosition = new();

        int tries = 100;
        for (int i = 0; i < tries; i++) {
            Vector2 randomCircle = Random.insideUnitCircle * clusterRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            Vector3 randomSpawnPoint = pos + randomOffset;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomSpawnPoint, out hit, clusterRadius, 1);
            finalPosition = hit.position;

            if (randomSpawnPoint.x != hit.position.x || randomSpawnPoint.z != hit.position.z)
                continue;

            if (minDistanceBetweenCharacters < 0.1f) break;

            // this block is only for calculating idle pedestrians spawn points
            float minDist = Mathf.Infinity;
            foreach (GameObject ped in idlePedestrianList) {
                float dist = Vector3.Distance(finalPosition, ped.transform.position);
                if (dist < minDist) minDist = dist;
            }
            if (minDist > minDistanceBetweenCharacters) break;
            if (tries == 99) Debug.LogWarning(tries);
        }

        return finalPosition;  // make sure the point is on navmesh, if not, the method will move it onto it
    }
}
