using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    [Tooltip("Types of shots that are recorded during runtime")]
    public List<ShotTypes> shotTypes;
    //public bool hideOtherClusters;
    [Tooltip("Path of the top-level directory where shots are saved")]
    public string savePath;

    public bool showCameraPreview;
    public bool shouldCharactersBeInFrame;
    public int batchSize;
    [Tooltip("Accelerates time during recording - limited by computer performance")]
    public float timeScale;

    [Tooltip("Distance from the goal which allows running pedestrians to move on to another target - adjusting the parameter might prevent pedestrians from getting stuck")]
    public float pathfindingCloseEnoughDistance;
    [Tooltip("Time before a stuck pedestrian changes destination")]
    public float pathfindingRecheckDelay;
    [Tooltip("How close to the edge of the walkable area (Nav Mesh) can pedestrians walk")]
    public float minDistanceFromNavMeshEdges;
    [Tooltip("Lower bound for the idle animation speed multiplier - the idle animation speed is randomized with every animation cycle")]
    public float animMinSpeed;
    [Tooltip("Upper bound for the idle animation speed multiplier - the idle animation speed is randomized with every animation cycle")]
    public float animMaxSpeed;

    [Tooltip("Example: entering '512' sets the output resolution to 512x512px")]
    public int resolution;
    public int fps;
    [Tooltip("Duration of a single shot in seconds")]
    public float duration;
    [Tooltip("Recommended, the method used is SMAA")]
    public AALevel antiAliasingStrength;
    [Tooltip("Minimum Field of View value for the camera in each shot")]
    public int minFOV;
    [Tooltip("Maximum Field of View value for the camera in each shot")]
    public int maxFOV;
    [Tooltip("Minimum Y coordinate of camera at the beginning of a shot")]
    public float minCameraStartHeight;
    [Tooltip("Maximum Y coordinate of camera at the beginning of a shot")]
    public float maxCameraStartHeight;
    [Tooltip("Should be increased if camera clips into the ground - requires increasing camera starting height")]
    public float cameraBoxColliderSize = 1.5f;
    [Tooltip("The percentage of how many idle pedestrians must stay visible (not behind any obstacle) in each single frame - determines if a shot should be entirely scrapped")]
    public float requiredVisibleCharPercentageInFrame; // percentage

    [Tooltip("Enables the process of randomly picking 2 post-processing effects with randomized parameters")]
    public bool isPostProcessingEnabled;
    public float bloomIntensityMin;
    public float bloomIntensityMax;
    public float vignetteIntensityMin;
    public float vignetteIntensityMax;
    public float vignetteSmoothnessMin;
    public float vignetteSmoothnessMax;
    public float dofFocusDistanceMin;
    public float dofFocusDistanceMax;
    public float chromaticIntensityMin;
    public float chromaticIntensityMax;
    public float saturationMin;
    public float saturationMax;
    public float contrastMin;
    public float contrastMax;
}
