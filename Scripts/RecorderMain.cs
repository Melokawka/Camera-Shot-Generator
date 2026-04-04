using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RecorderMain : MonoBehaviour
{
    [SerializeField] public GlobalVars globalVars;
    [SerializeField] public PedestrianSpawner pedestrianSpawner;
    public SettingsManager settingsManager;
    public PostProcessingRandomizer postProcessingRandomizer;  // instead of assigning it here, maybe assign it via main script?

    public int batchSize = 10;
    
    public async Task<List<int>> DoShots<T>(List<T> plans) where T : ShotPlan  // at this moment if one shotPlan is bad and needs fixing, we need to redo the whole recording - how to fix this?
    {
        // this method only accepts 1 environment and 1 shot type at a time so we use the same environment for all
        //GameObject currentEnvironment = Instantiate(globalVars.environments[plans[0].environmentType], Vector3.zero, Quaternion.identity);
        //currentEnvironment.name = "o";

        List<int> failedIds = new();
        for (int i = 0; i < plans.Count; i += batchSize)
        {
            RenderSettings.skybox = globalVars.skyboxList[Random.Range(0, globalVars.skyboxList.Length)];

            if (settingsManager.isPostProcessingEnabled)
                postProcessingRandomizer.ApplyRandomPostProcessingEffects();

            List<Task<(int id, bool failed)>> recordingTasks = new();
            for (int j = 0; j < batchSize; j++)
            {
                if (i + j == plans.Count) break;
                List<GameObject> runningPedestrianList = pedestrianSpawner.SpawnRunningPedestrians(plans[i + j].observedPoint, j);
                List<GameObject> idlePedestrianList = await pedestrianSpawner.SpawnIdlePedestrians(plans[i + j].observedPoint, j);  // j is used here as clusterId for camera to hide foreign clusters from view during recording

                GameObject cameraObject = PrepareCamera(plans[i + j], j);

                ClusterAnimationUpdater clusterAnimationUpdater = cameraObject.AddComponent<ClusterAnimationUpdater>();

                List<AnimType> randomizedAnimTypes = new List<AnimType>();
                List<int> randomizedAnimTypes2 = new List<int>();
                System.Random rand = new System.Random();

                List<AnimType> types = globalVars.animationTypes;
                float totalWeight = globalVars.animationTypesWeights.Sum();
                for (int iter1 = 0; iter1 < idlePedestrianList.Count; iter1++)
                {
                    float randomValue = Random.Range(0f, totalWeight);
                    float cumulativeWeight = 0f;

                    for (int iter2 = 0; iter2 < types.Count; iter2++)
                    {
                        cumulativeWeight += globalVars.animationTypesWeights[iter2];
                        if (randomValue <= cumulativeWeight)
                        {
                            randomizedAnimTypes.Add(types[iter2]);
                            break;
                        }
                    }
                }

                for (int iter3 = 0; iter3 < idlePedestrianList.Count; iter3++)
                {
                    randomizedAnimTypes2.Add(rand.Next(0, globalVars.uncombinableIdleAnimations.Count)); // losuje jaką uncombinable animację ma robić każdy przechodzień
                }

                clusterAnimationUpdater.idlePedestrianList = idlePedestrianList;
                clusterAnimationUpdater.whichIdleAnimTypeForPeds = randomizedAnimTypes;
                clusterAnimationUpdater.whichUncombinableAnimTypeForPeds = randomizedAnimTypes2;
                clusterAnimationUpdater.runningPedestrianList = runningPedestrianList;

                RecorderSettings recorderSettings = PrepareSettings(plans[i + j], cameraObject);

                Recorder2 recorder = cameraObject.AddComponent<Recorder2>();
                VisibilityTester visibilityTester = cameraObject.AddComponent<VisibilityTester>();
                visibilityTester.idlePedestrianList = idlePedestrianList;

                async Task<(int id, bool failed)> StartRecording(Recorder2 recorder, RecorderSettings settings, int id)
                {
                    bool failed = await recorder.Record(settings);
                    return (id, failed);
                }

                recordingTasks.Add(StartRecording(recorder, recorderSettings, plans[i + j].id));
                Debug.LogWarning("Recording: " + plans[i + j].id);
            }
            var results = await Task.WhenAll(recordingTasks);

            foreach (var result in results)
            {
                if (result.failed)
                {
                    failedIds.Add(result.id);
                }
            }
            ClearObjects();
        }
        //Destroy(currentEnvironment);

        return failedIds;
    }

    public GameObject PrepareCamera(ShotPlan shotPlan, int clusterId) {
        GameObject cameraObject = new GameObject($"{shotPlan.shotType}Camera_{shotPlan.id}");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.transform.position = shotPlan.cameraLocation;
        camera.transform.LookAt(shotPlan.observedPoint);
        camera.fieldOfView = shotPlan.FOV;
       
        cameraObject.tag = "Camera";

        // BoxCollider boxCollider = cameraObject.AddComponent<BoxCollider>();
        // boxCollider.center = Vector3.zero;
        // boxCollider.size = Vector3.one * settingsManager.cameraBoxColliderSize;
        // boxCollider.isTrigger = true;

        PostProcessLayer postProcessLayer = camera.AddComponent<PostProcessLayer>();

        if (settingsManager.antiAliasingStrength == AALevel.Low) {
            postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            postProcessLayer.fastApproximateAntialiasing.fastMode = true;
        }

        if (settingsManager.antiAliasingStrength == AALevel.High) {
            postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
            postProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
        }

        postProcessLayer.volumeTrigger = camera.transform;
        
        postProcessLayer.volumeLayer = 1 << 7;

        postProcessLayer.Init(globalVars.postProcessResources);

        HideOtherClustersForCamera(clusterId, cameraObject);

        return cameraObject;
    }

    public void HideOtherClustersForCamera(int visibleClusterId, GameObject cameraObject) {
        CameraVisibilityController visibilityController = cameraObject.AddComponent<CameraVisibilityController>();
        visibilityController.visibleClusterId = visibleClusterId;  // important // only render the cluster in this layer
        //camera.depth = visibleClusterId;
    }

    public RecorderSettings PrepareSettings(ShotPlan shotPlan, GameObject cameraObject) {
        string outPath = Path.Combine(GlobalVars.savePath, globalVars.environments[shotPlan.environmentType].name, $"{shotPlan.shotType}Camera_{shotPlan.id}");

        RecorderSettings recorderSettings = new RecorderSettings(shotPlan.id, shotPlan.fps, settingsManager.resolution, 
                            shotPlan.duration, outPath, shotPlan.shotType, shotPlan.observedPoint);

        recorderSettings.startFOV = cameraObject.GetComponent<Camera>().fieldOfView;  // default
        recorderSettings.endFOV = cameraObject.GetComponent<Camera>().fieldOfView;

        recorderSettings.startPoint = cameraObject.transform.position;
        recorderSettings.endPoint = cameraObject.transform.position;

        switch (shotPlan.shotType)
        {
            case ShotTypes.Static:
                break;

            case ShotTypes.ArcLeft:
                // Create the greater pivot (for tilting the orbit)
                GameObject greaterPivot = new GameObject($"GreaterPivot_{shotPlan.id}");
                greaterPivot.transform.position = shotPlan.observedPoint;

                // Create the lesser pivot (for Y-axis rotation)
                GameObject lesserPivot = new GameObject($"LesserPivot_{shotPlan.id}");
                lesserPivot.transform.position = shotPlan.observedPoint;
                lesserPivot.transform.parent = greaterPivot.transform;  // Lesser pivot is child of Greater pivot

                // Attach the camera to the lesser pivot
                cameraObject.transform.parent = lesserPivot.transform;

                // Tilt the greater pivot once at the start
                float tiltAngle = ((ShotPlanArcLeft)(object)shotPlan).tiltAngle;
                greaterPivot.transform.Rotate(Vector3.right, tiltAngle);

                int arcDegrees = ((ShotPlanArcLeft)(object)shotPlan).arcDegrees;  // for arc-specific properties, cast shotPlan to ShotPlanArc.
                float rotationSpeed = 360f / shotPlan.duration * ((float)arcDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;
                recorderSettings.tiltAngle = tiltAngle;

                recorderSettings.startFOV = cameraObject.GetComponent<Camera>().fieldOfView;
                recorderSettings.endFOV = cameraObject.GetComponent<Camera>().fieldOfView;
                break;

            case ShotTypes.ArcRight:
                // Create the greater pivot (for tilting the orbit)
                greaterPivot = new GameObject($"GreaterPivot_{shotPlan.id}");
                greaterPivot.transform.position = shotPlan.observedPoint;

                // Create the lesser pivot (for Y-axis rotation)
                lesserPivot = new GameObject($"LesserPivot_{shotPlan.id}");
                lesserPivot.transform.position = shotPlan.observedPoint;
                lesserPivot.transform.parent = greaterPivot.transform;  // Lesser pivot is child of Greater pivot

                // Attach the camera to the lesser pivot
                cameraObject.transform.parent = lesserPivot.transform;

                // Tilt the greater pivot once at the start
                tiltAngle = ((ShotPlanArcRight)(object)shotPlan).tiltAngle;
                greaterPivot.transform.Rotate(Vector3.right, tiltAngle);

                arcDegrees = ((ShotPlanArcRight)(object)shotPlan).arcDegrees;  // for arc-specific properties, cast shotPlan to ShotPlanArc.
                rotationSpeed = 360f / shotPlan.duration * ((float)arcDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;
                recorderSettings.tiltAngle = tiltAngle;

                recorderSettings.startFOV = cameraObject.GetComponent<Camera>().fieldOfView;
                recorderSettings.endFOV = cameraObject.GetComponent<Camera>().fieldOfView;
                break;

            case ShotTypes.PushOut:
                Vector3 direction = (shotPlan.cameraLocation - shotPlan.observedPoint).normalized;
                Vector3 startPosition = shotPlan.observedPoint + direction * ((ShotPlanPushOut)(object)shotPlan).pushOutStartDistance;

                cameraObject.transform.position = startPosition;  // Set initial camera position

                recorderSettings.startPoint = startPosition;
                recorderSettings.endPoint = shotPlan.cameraLocation;
                break;

            case ShotTypes.PushIn:
                direction = (shotPlan.cameraLocation - shotPlan.observedPoint).normalized;
                startPosition = shotPlan.observedPoint + direction * ((ShotPlanPushIn)(object)shotPlan).pushInEndDistance;

                cameraObject.transform.position = shotPlan.cameraLocation;  // Set initial camera position

                recorderSettings.startPoint = startPosition;
                recorderSettings.endPoint = shotPlan.cameraLocation;
                break;

            case ShotTypes.PanLeft:
                int panDegrees = ((ShotPlanPanLeft)(object)shotPlan).panDegrees;
                rotationSpeed = 360f / shotPlan.duration * ((float)panDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;

                float randomAngle = Random.Range(0f, 360f);
                direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
                cameraObject.transform.LookAt(cameraObject.transform.position + direction);

                Vector3 pos = cameraObject.transform.position;
                pos.y += ((ShotPlanPanLeft)(object)shotPlan).panHeight;
                cameraObject.transform.position = pos;

                recorderSettings.startPoint = cameraObject.transform.position;
                recorderSettings.endPoint = cameraObject.transform.position;
                break;

            case ShotTypes.PanRight:
                panDegrees = ((ShotPlanPanRight)(object)shotPlan).panDegrees;
                rotationSpeed = 360f / shotPlan.duration * ((float)panDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;

                randomAngle = Random.Range(0f, 360f);
                direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
                cameraObject.transform.LookAt(cameraObject.transform.position + direction);

                pos = cameraObject.transform.position;
                pos.y += ((ShotPlanPanRight)(object)shotPlan).panHeight;
                cameraObject.transform.position = pos;

                recorderSettings.startPoint = cameraObject.transform.position;
                recorderSettings.endPoint = cameraObject.transform.position;
                break;

            case ShotTypes.RollLeft:
                int rollDegrees = ((ShotPlanRollLeft)(object)shotPlan).rollDegrees;
                rotationSpeed = 360f / shotPlan.duration * ((float)rollDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;
                break;

            case ShotTypes.RollRight:
                rollDegrees = ((ShotPlanRollRight)(object)shotPlan).rollDegrees;
                rotationSpeed = 360f / shotPlan.duration * ((float)rollDegrees / 360f);

                recorderSettings.rotationSpeed = rotationSpeed;
                break;

            case ShotTypes.DollyZoomIn:
                direction = (shotPlan.cameraLocation - shotPlan.observedPoint).normalized;
                Vector3 endPosition = shotPlan.observedPoint + direction * ((ShotPlanDollyZoomIn)(object)shotPlan).dollyEndDistance;

                recorderSettings.startPoint = shotPlan.cameraLocation;
                recorderSettings.endPoint = endPosition;

                recorderSettings.startFOV = shotPlan.FOV;
                recorderSettings.endFOV = ((ShotPlanDollyZoomIn)(object)shotPlan).dollyEndFOV;
                break;

            case ShotTypes.DollyZoomOut:
                direction = (shotPlan.cameraLocation - shotPlan.observedPoint).normalized;
                endPosition = shotPlan.observedPoint + direction * ((ShotPlanDollyZoomOut)(object)shotPlan).dollyEndDistance;

                recorderSettings.startPoint = shotPlan.cameraLocation;
                recorderSettings.endPoint = endPosition;

                recorderSettings.startFOV = shotPlan.FOV;
                recorderSettings.endFOV = ((ShotPlanDollyZoomOut)(object)shotPlan).dollyEndFOV;
                break;

            case ShotTypes.SidewaysLeft:
                recorderSettings.startPoint = shotPlan.cameraLocation;
                recorderSettings.endPoint = ((ShotPlanSidewaysLeft)(object)shotPlan).endPoint;
                break;

            case ShotTypes.SidewaysRight:
                recorderSettings.startPoint = shotPlan.cameraLocation;
                recorderSettings.endPoint = ((ShotPlanSidewaysRight)(object)shotPlan).endPoint;
                break;

            case ShotTypes.WhipPan:
                panDegrees = ((ShotPlanWhipPan)(object)shotPlan).panDegrees;
                rotationSpeed = 360f / shotPlan.duration * ((float)panDegrees / 360f);

                rotationSpeed *= ((ShotPlanWhipPan)(object)shotPlan).repetitions;

                recorderSettings.rotationSpeed = rotationSpeed;

                randomAngle = Random.Range(0f, 360f);
                direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
                cameraObject.transform.LookAt(cameraObject.transform.position + direction);

                recorderSettings.startPoint = cameraObject.transform.position + direction;
                recorderSettings.repetitions = ((ShotPlanWhipPan)(object)shotPlan).repetitions;

                pos = cameraObject.transform.position;
                pos.y += ((ShotPlanWhipPan)(object)shotPlan).panHeight;
                cameraObject.transform.position = pos;

                recorderSettings.startPoint = cameraObject.transform.position;
                recorderSettings.endPoint = cameraObject.transform.position;
                break;

            case ShotTypes.ZoomIn:
                recorderSettings.startFOV = shotPlan.FOV;
                recorderSettings.endFOV = ((ShotPlanZoomIn)(object)shotPlan).zoomEndFOV;
                break;

            case ShotTypes.ZoomOut:
                recorderSettings.startFOV = shotPlan.FOV;
                recorderSettings.endFOV = ((ShotPlanZoomOut)(object)shotPlan).zoomEndFOV;
                break;

            case ShotTypes.VerticalUp:
                cameraObject.transform.rotation = new(); // with rotation 0,0,0,0 camera looks forward. If camera looked down then it would be a simple push out shot

                recorderSettings.endPoint = shotPlan.cameraLocation;

                Vector3 startPoint = shotPlan.observedPoint + Vector3.up * ((ShotPlanVerticalUp)(object)shotPlan).travelDistance;

                recorderSettings.startPoint = startPoint;
                break;

            case ShotTypes.VerticalDown:
                cameraObject.transform.rotation = new();
                startPosition = shotPlan.cameraLocation;

                recorderSettings.startPoint = startPosition;

                startPosition.y = ((ShotPlanVerticalDown)(object)shotPlan).travelDistance + shotPlan.observedPoint.y;  // travel distance is now the Y at the end of the shot
                recorderSettings.endPoint = startPosition;
                break;
        }

        return recorderSettings;
    }

    public List<T> ReadShotPlans<T>(string filePath) where T : ShotPlan
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Converters.Add(new Vector3Converter());
        
        string json = File.ReadAllText(filePath);
        List<T> plans = JsonConvert.DeserializeObject<List<T>>(json, settings);
        
        return plans;
    }

    void ClearObjects() {  // musi byc w monobehaviour wiec nie mozna przeniesc do miscFunctions
        //foreach (var obj in FindObjectsOfType<ClusterIdentifier>()) Destroy(obj.gameObject);
        foreach (var rec in GameObject.FindGameObjectsWithTag("Camera")) Destroy(rec.transform.root.gameObject);
        foreach (var obj in GameObject.FindGameObjectsWithTag("Pat")) Destroy(obj.gameObject);
    }
}