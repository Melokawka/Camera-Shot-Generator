using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

//interestPointGenerator.PrepareNavMeshTriangulations();  // because (baked) navmesh is a singleton the triangulation cannot be done in advance i think

public class Main : MonoBehaviour
{
    [Tooltip("Shows the shot viewer after succesfully recording shots")]
    [SerializeField] public bool showViewer = true;
    [SerializeField] public bool makePlans = true;
    [Tooltip(@"E.g. [05-05][18-02-55]")]
    [SerializeField] public string premadePlansPath = "";
    [SerializeField] public bool makeShots = true;
    [Tooltip("Used to estimate map (walkable area) size")]
    [SerializeField] public bool showPossiblePointsGrid = false;
    [SerializeField] public GlobalVars globalVars;
    [SerializeField] public TimeScaler timeScaler;
    [SerializeField] public RecorderMain recorderMain;
    [SerializeField] public ShotsPlanner shotsPlanner;
    [SerializeField] public SettingsManager settingsManager;
    [SerializeField] public InterestPointGenerator interestPointGenerator;
    [SerializeField] public PedestrianSpawner pedestrianSpawner;
    [SerializeField] public PostProcessingRandomizer postProcessingRandomizer;
    [SerializeField] public ShotViewer shotViewer;
    [SerializeField] public EnvironmentSettingsManager environmentSettingsManager;

    private async Task HandleShotType<TPlan>(ShotTypes shotType, int shotsNr, int planIndexStart, GameObject environment, int environmentNr, EnvironmentSettings envSettings) where TPlan : ShotPlan
    {
        List<TPlan> plans = new();

        int planIndex = planIndexStart;

        int numShots = shotsNr;

        string plansPath = Path.Combine(GlobalVars.savePath, environment.name, $"{shotType}.txt");

        GameObject env = Instantiate(environment, Vector3.zero, Quaternion.identity);

        if (makePlans)
        {
            interestPointGenerator.minDistanceFromNavMeshEdges = settingsManager.minDistanceFromNavMeshEdges;
            List<Vector3> interestPoints = interestPointGenerator.GenerateInterestPoints(numShots);
            //foreach (Vector3 point in interestPoints) Debug.DrawRay(point, Vector3.up * 25f, Color.green, 10f);

            if (File.Exists(plansPath))
            {  // after deleting some shots we want to keep all viable old shotPlans
                plans.AddRange(recorderMain.ReadShotPlans<TPlan>(plansPath));
            }

            plans.AddRange(shotsPlanner.PlanShots<TPlan>(shotType, environmentNr, environment, interestPoints, numShots, planIndex, settingsManager, envSettings));
            shotsPlanner.SaveShotPlans(plans, plansPath);
        }

        if (makeShots)
        {
            plans = recorderMain.ReadShotPlans<TPlan>(plansPath);

            List<int> failedIds = await recorderMain.DoShots(plans.GetRange(planIndex, plans.Count - planIndex));

            plans = plans.Where(plan => !failedIds.Contains(plan.id)).ToList();

            for (int i = planIndex; i < plans.Count; i++)
            {
                plans[i].id = i;
            }

            shotsPlanner.SaveShotPlans(plans, Path.Combine(GlobalVars.savePath, environment.name, $"{shotType}.txt"));
        }
        Destroy(env);
    }

    public async void Start() {
        shotViewer.isLoaded = false;

        settingsManager.UpdateSettingsFromEditor();
        settingsManager.SaveSettings();
        settingsManager.LoadSettings();

        if (makePlans) 
            GlobalVars.savePath = Path.Combine(settingsManager.savePath, System.DateTime.Now.ToString("[MM-dd][HH-mm-ss]"));
        else
            GlobalVars.savePath = Path.Combine(settingsManager.savePath, premadePlansPath);

        Directory.CreateDirectory(GlobalVars.savePath);

        List<GameObject> environments = new();
        int iter = 0;
        foreach (EnvironmentEntry env in globalVars.environmentsList) {
            env.envSettings = environmentSettingsManager.environmentSettingsList[iter];
            iter++;
            if (env.isEnabled) environments.Add(env.environment);
        }
        globalVars.environments = environments;

        postProcessingRandomizer.LoadSettingsFromManager(settingsManager);
        postProcessingRandomizer.OrganizeSettings();

        Recorder2.showCameraPreview = settingsManager.showCameraPreview;
        VisibilityTester.shouldCharactersBeInFrame = settingsManager.shouldCharactersBeInFrame;
        VisibilityTester.cameraBoxColliderSize = settingsManager.cameraBoxColliderSize;
        recorderMain.batchSize = settingsManager.batchSize;
        timeScaler.TimeScale = settingsManager.timeScale;

        recorderMain.settingsManager = settingsManager;

        ClusterAnimationUpdater.idleAnimations = globalVars.idleAnimations;
        ClusterAnimationUpdater.sittingIdleAnimations = globalVars.sittingIdleAnimations;
        ClusterAnimationUpdater.standingIdleAnimations = globalVars.standingIdleAnimations;
        ClusterAnimationUpdater.uncombinableIdleAnimations = globalVars.uncombinableIdleAnimations;
        ClusterAnimationUpdater.animMinSpeed = settingsManager.animMinSpeed;
        ClusterAnimationUpdater.animMaxSpeed = settingsManager.animMaxSpeed;

        VisibilityTester.requiredVisibleCharPercentageInFrame = settingsManager.requiredVisibleCharPercentageInFrame;

        // test how many different interest points can be generated on each map (for uniform-random point distribution - poisson disk sampling)
        if (showPossiblePointsGrid) {
            foreach (GameObject env in globalVars.environments) {
                List<Vector3> points = interestPointGenerator.GetPointsCountOnNavMesh(env, 1);
                Debug.LogWarning(env.name + " point count: " + points.Count);    
                foreach (var point in points)
                    Debug.DrawRay(point, Vector3.up * 7f, Color.green, 5f); // 0.5f wysokości, 5s widoczności
            }
        }

        for (int i = 0; i < globalVars.environments.Count; i++) {
            Directory.CreateDirectory(Path.Combine(GlobalVars.savePath, globalVars.environments[i].name));

            EnvironmentSettings envSettings = globalVars.GetEnvironmentSettings(globalVars.environments[i].name);
            pedestrianSpawner.InitializeSettings(settingsManager, envSettings);
            pedestrianSpawner.idleAnimations = globalVars.idleAnimations;
            pedestrianSpawner.runAnimations = globalVars.runAnimations;
            pedestrianSpawner.pedestrianPrefabPaths = PreparePedestrianPrefabPaths();

            foreach (ShotTypes shotType in settingsManager.shotTypes) {
                int planIndexStart = 0;
                int shotsCount = GetShotsCount(envSettings, shotType);

                while (planIndexStart < shotsCount)
                {
                    int numberOfShots = GetShotsCount(envSettings, shotType) - planIndexStart;
                    if (planIndexStart > 0) Debug.LogWarning("Not enough shots, index is (" + (planIndexStart) + "), code continues.");

                    Type planType = GetShotPlanType(shotType);
                    await InvokeGenericHandler(planType, shotType, numberOfShots, planIndexStart, globalVars.environments[i], i, envSettings);

                    await RenumberShotFolders.Renumber(Path.Combine(GlobalVars.savePath), shotType);
                    
                    if (!makeShots || !makePlans)
                        planIndexStart = GetShotsCount(envSettings, shotType);

                    else planIndexStart = await CountShotsOfType(Path.Combine(GlobalVars.savePath, globalVars.environments[i].name), shotType);
                }
            } 
        }
        
        if (showViewer && makeShots) {
            shotViewer.environmentNames = globalVars.GetEnvironmentNames();
            shotViewer.basePath = GlobalVars.savePath;
            shotViewer.shotTypes = settingsManager.shotTypes;
            shotViewer.isLoaded = true;
            shotViewer.LoadRandomFramesFromShots();
        }
        else EditorApplication.isPlaying = false;  // editor only, exit play mode after filming the shots
    }

    int GetShotsCount(EnvironmentSettings settings, ShotTypes type)
    {
        return type switch
        {
            ShotTypes.Static => settings.staticShotsNr,
            ShotTypes.ArcLeft => settings.arcLeftShotsNr,
            ShotTypes.ArcRight => settings.arcRightShotsNr,
            ShotTypes.PushOut => settings.pushOutShotsNr,
            ShotTypes.PushIn => settings.pushInShotsNr,
            ShotTypes.PanLeft => settings.panLeftShotsNr,
            ShotTypes.PanRight => settings.panRightShotsNr,
            ShotTypes.RollLeft => settings.rollLeftShotsNr,
            ShotTypes.RollRight => settings.rollRightShotsNr,
            ShotTypes.DollyZoomIn => settings.dollyZoomInShotsNr,
            ShotTypes.DollyZoomOut => settings.dollyZoomOutShotsNr,
            ShotTypes.SidewaysLeft => settings.sidewaysLeftShotsNr,
            ShotTypes.SidewaysRight => settings.sidewaysRightShotsNr,
            ShotTypes.WhipPan => settings.whipPanShotsNr,
            ShotTypes.ZoomIn => settings.zoomInShotsNr,
            ShotTypes.ZoomOut => settings.zoomOutShotsNr,
            ShotTypes.VerticalUp => settings.verticalUpShotsNr,
            ShotTypes.VerticalDown => settings.verticalDownShotsNr,
            _ => 0
        };
    }

    Type GetShotPlanType(ShotTypes type)
    {
        return type switch
        {
            ShotTypes.Static => typeof(ShotPlanStatic),
            ShotTypes.ArcLeft => typeof(ShotPlanArcLeft),
            ShotTypes.ArcRight => typeof(ShotPlanArcRight),
            ShotTypes.PushOut => typeof(ShotPlanPushOut),
            ShotTypes.PushIn => typeof(ShotPlanPushIn),
            ShotTypes.PanLeft => typeof(ShotPlanPanLeft),
            ShotTypes.PanRight => typeof(ShotPlanPanRight),
            ShotTypes.RollLeft => typeof(ShotPlanRollLeft),
            ShotTypes.RollRight => typeof(ShotPlanRollRight),
            ShotTypes.DollyZoomIn => typeof(ShotPlanDollyZoomIn),
            ShotTypes.DollyZoomOut => typeof(ShotPlanDollyZoomOut),
            ShotTypes.SidewaysLeft => typeof(ShotPlanSidewaysLeft),
            ShotTypes.SidewaysRight => typeof(ShotPlanSidewaysRight),
            ShotTypes.WhipPan => typeof(ShotPlanWhipPan),
            ShotTypes.ZoomIn => typeof(ShotPlanZoomIn),
            ShotTypes.ZoomOut => typeof(ShotPlanZoomOut),
            ShotTypes.VerticalUp => typeof(ShotPlanVerticalUp),
            ShotTypes.VerticalDown => typeof(ShotPlanVerticalDown),
            _ => throw new ArgumentException($"Nieznany typ ujęcia: {type}")
        };
    }

    async Task InvokeGenericHandler(Type planType, ShotTypes type, int numberOfShots, int startIndex, GameObject environment, int environmentNr, EnvironmentSettings envSettings)
    {
        var method = typeof(Main).GetMethod(
            "HandleShotType",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        var genericMethod = method.MakeGenericMethod(planType);
        await (Task)genericMethod.Invoke(this, new object[] { type, numberOfShots, startIndex, environment, environmentNr, envSettings });
    }

    public async Task<int> CountShotsOfType(string basePath, ShotTypes shotType)
    {
        return await Task.Run(() =>
        {
            string searchPattern = $"{shotType}Camera_*";
            string[] matchingDirs = Directory.GetDirectories(basePath, searchPattern);
            return matchingDirs.Length;
        });
        // Aby metoda CountShotsOfType działała asynchronicznie, musisz ją opakować w Task.Run, ponieważ operacje na systemie plików (Directory.GetDirectories) są blokujące i nie mają wbudowanej wersji asynchronicznej.
    }

    public List<String> PreparePedestrianPrefabPaths() 
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("PedestrianPrefabPaths");
        if (jsonFile == null)
        {
            Debug.LogError("Nie znaleziono PedestrianPrefabPaths.json w Resources.");
            return null;
        }

        PrefabPathList pathList = JsonUtility.FromJson<PrefabPathList>(jsonFile.text);
        
        return pathList.paths.ToList();
    }

    [System.Serializable]
    public class PrefabPathList
    {
        public string[] paths;
    }
}
