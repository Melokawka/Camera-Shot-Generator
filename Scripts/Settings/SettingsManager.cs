using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public Settings settings = new();
    public string settingsPath = "settings.txt";
    
    /// 
    //public bool hideOtherClusters = true;
    public List<ShotTypes> shotTypes;
    public string savePath = "CapturedFrames";

    [Header("Processing attributes")]
    public bool showCameraPreview = false;
    public bool shouldCharactersBeInFrame = false;
    public int batchSize = 5;
    public float timeScale = 2f;

    [Header("Common shot attributes")]
    public int resolution = 512;
    public int fps = 24;
    public float duration = 1f;
    public AALevel antiAliasingStrength = AALevel.High;
    public int minFOV = 50;
    public int maxFOV = 100;
    public float minCameraStartHeight = 1f;
    public float maxCameraStartHeight = 100f;
    public float cameraBoxColliderSize = 1.5f;
    public float requiredVisibleCharPercentageInFrame = 0.50f;

    [Header("Post-processing")]
    public bool isPostProcessingEnabled = true;
    public float bloomIntensityMin = 1f;
    public float bloomIntensityMax = 3f;

    public float vignetteIntensityMin = 0.2f;
    public float vignetteIntensityMax = 0.5f;
    public float vignetteSmoothnessMin = 0.2f;
    public float vignetteSmoothnessMax = 0.5f;

    public float dofFocusDistanceMin = 2.8f;
    public float dofFocusDistanceMax = 3f;

    public float chromaticIntensityMin = 0.2f;
    public float chromaticIntensityMax = 0.5f;

    public float saturationMin = -50f;
    public float saturationMax = 50f;
    public float contrastMin = -70f;
    public float contrastMax = 30f;

    [Header("Character spawn attributes")]
    public float pathfindingCloseEnoughDistance = 2f;
    public float pathfindingRecheckDelay = 0.5f;
    public float minDistanceFromNavMeshEdges = 1.5f;
    public float animMinSpeed = 0.7f;
    public float animMaxSpeed = 1.3f;
    
    ///

    public void UpdateSettingsFromEditor() {
        settings = new Settings
        {
            shotTypes = shotTypes,
            //hideOtherClusters = hideOtherClusters,
            savePath = savePath,

            showCameraPreview = showCameraPreview,
            shouldCharactersBeInFrame = shouldCharactersBeInFrame,
            batchSize = batchSize,
            timeScale = timeScale,

            pathfindingCloseEnoughDistance = pathfindingCloseEnoughDistance,
            pathfindingRecheckDelay = pathfindingRecheckDelay,
            minDistanceFromNavMeshEdges = minDistanceFromNavMeshEdges,
            animMinSpeed = animMinSpeed,
            animMaxSpeed = animMaxSpeed,

            resolution = resolution,
            fps = fps,
            duration = duration,
            antiAliasingStrength = antiAliasingStrength,
            
            minFOV = minFOV,
            maxFOV = maxFOV,
            cameraBoxColliderSize = cameraBoxColliderSize,
            minCameraStartHeight = minCameraStartHeight,
            maxCameraStartHeight = maxCameraStartHeight,
            requiredVisibleCharPercentageInFrame = requiredVisibleCharPercentageInFrame,

            isPostProcessingEnabled = isPostProcessingEnabled,
            bloomIntensityMin = bloomIntensityMin,
            bloomIntensityMax = bloomIntensityMax,
            vignetteIntensityMin = vignetteIntensityMin,
            vignetteIntensityMax = vignetteIntensityMax,
            vignetteSmoothnessMin = vignetteSmoothnessMin,
            vignetteSmoothnessMax = vignetteSmoothnessMax,
            dofFocusDistanceMin = dofFocusDistanceMin,
            dofFocusDistanceMax = dofFocusDistanceMax,
            chromaticIntensityMin = chromaticIntensityMin,
            chromaticIntensityMax = chromaticIntensityMax,

            saturationMin = saturationMin,
            saturationMax = saturationMax,
            contrastMin = contrastMin,
            contrastMax = contrastMax,
        };
    }

    public void SaveSettings()
    {
        JsonSerializerSettings serializerSettings = new()
        {
            Formatting = Formatting.Indented
        };
        
        string json = JsonConvert.SerializeObject(settings, serializerSettings);
        File.WriteAllText(settingsPath, json);

        Debug.Log($"Settings saved to {settingsPath}.");
    }

    public void LoadSettings()
    {
        if (!File.Exists(settingsPath))
        {
            Debug.Log("Settings file not found: " + settingsPath);
            return;
        }
        
        string json = File.ReadAllText(settingsPath);

        settings = JsonConvert.DeserializeObject<Settings>(json);

        Debug.Log("Settings loaded from " + settingsPath);
    }
}
