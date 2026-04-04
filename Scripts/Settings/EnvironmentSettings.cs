using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class EnvironmentSettings : MonoBehaviour
{
    [Header("Character spawn attributes")]
    [Tooltip("Number of immobile pedestrians to spawn")]
    public int idleCharactersPerCluster = 15;
    [Tooltip("Number of running pedestrians to spawn")]
    public int runCharactersPerCluster = 10;
    [Tooltip("Multiplier for how much the number of pedestrians can vary (0–1)")]
    public float clusterCharactersNumberVariability = 0.51f;  // percentage
    [Tooltip("Radius constricting characters (both running and idle) around the central point in a cluster")]
    public float clusterRadius = 15f;
    [Tooltip("Minimum speed multiplier for running pedestrians")]
    public float pedMinSpeed = 1.5f;
    [Tooltip("Maximum speed multiplier for running pedestrians")]
    public float pedMaxSpeed = 4.5f;

    [Header("Static shots")]
    public int staticShotsNr = 15;
    public float staticShotsMinDist = 5f;
    public float staticShotsMaxDist = 20f;

    [Header("Arc-Left shots")]
    public int arcLeftShotsNr = 15;
    public float arcLeftShotsMinDist = 5f;
    public float arcLeftShotsMaxDist = 20f;
    public int arcLeftShotsMinDegrees = 30;
    public int arcLeftShotsMaxDegrees = 240; 
    [Tooltip("Maximum number of degrees for skewing the general orbit")]
    public int arcLeftShotsMaxTiltAngle = 15;

    [Header("Arc-Right shots")]
    public int arcRightShotsNr = 15;
    public float arcRightShotsMinDist = 5f;
    public float arcRightShotsMaxDist = 20f;
    public int arcRightShotsMinDegrees = 30;
    public int arcRightShotsMaxDegrees = 240; 
    [Tooltip("Maximum number of degrees for skewing the general orbit")]
    public int arcRightShotsMaxTiltAngle = 15;

    [Header("Push-Out shots")]
    public int pushOutShotsNr = 15;
    public float pushOutStartMinDist = 2f;
    public float pushOutStartMaxDist = 4f;
    public float pushOutEndMinDist = 5f;
    public float pushOutEndMaxDist = 20f;

    [Header("Push-In shots")]
    public int pushInShotsNr = 15;
    public float pushInStartMinDist = 5f;
    public float pushInStartMaxDist = 20f;
    public float pushInEndMinDist = 2f;
    public float pushInEndMaxDist = 4f;

    [Header("Pan-Left shots")]
    public int panLeftShotsNr = 15;
    public float panLeftMinCenterDist = 3f;
    public float panLeftMaxCenterDist = 7f;
    public int panLeftShotsMinDegrees = 30;
    public int panLeftShotsMaxDegrees = 240; 
    public float panLeftMinHeight = 0.2f;
    public float panLeftMaxHeight = 1f;

    [Header("Pan-Right shots")]
    public int panRightShotsNr = 15;
    public float panRightMinCenterDist = 3f;
    public float panRightMaxCenterDist = 7f;
    public int panRightShotsMinDegrees = 30;
    public int panRightShotsMaxDegrees = 240; 
    public float panRightMinHeight = 0.2f;
    public float panRightMaxHeight = 1f;

    [Header("Roll-Left shots")]
    public int rollLeftShotsNr = 15;
    public float rollLeftShotsMinDist = 10f;
    public float rollLeftShotsMaxDist = 20f;
    public int rollLeftShotsMinDegrees = 120;
    public int rollLeftShotsMaxDegrees = 480; 

    [Header("Roll-Right shots")]
    public int rollRightShotsNr = 15;
    public float rollRightShotsMinDist = 10f;
    public float rollRightShotsMaxDist = 20f;
    public int rollRightShotsMinDegrees = 120;
    public int rollRightShotsMaxDegrees = 480; 

    [Header("Dolly Zoom-In shots")]
    public int dollyZoomInShotsNr = 15;
    public int dollyZoomInStartMinFOV = 80;
    public int dollyZoomInStartMaxFOV = 100;
    public int dollyZoomInEndMinFOV = 60;
    public int dollyZoomInEndMaxFOV = 70;
    public float dollyZoomInStartMinDist = 5f;
    public float dollyZoomInStartMaxDist = 20f;
    public float dollyZoomInEndMinDist = 2f;
    public float dollyZoomInEndMaxDist = 4f;

    [Header("Dolly Zoom-Out shots")]
    public int dollyZoomOutShotsNr = 15;
    public int dollyZoomOutStartMinFOV = 60;
    public int dollyZoomOutStartMaxFOV = 70;
    public int dollyZoomOutEndMinFOV = 80;
    public int dollyZoomOutEndMaxFOV = 100;
    public float dollyZoomOutStartMinDist = 2;
    public float dollyZoomOutStartMaxDist = 4f;
    public float dollyZoomOutEndMinDist = 5f;
    public float dollyZoomOutEndMaxDist = 20f;

    [Header("Sideways-Left shots")]
    public int   sidewaysLeftShotsNr = 15;
    public float sidewaysLeftMinDist = 4f;
    public float sidewaysLeftMaxDist = 7f;
    public float sidewaysLeftMinPath = 3f;
    public float sidewaysLeftMaxPath = 6f;

    [Header("Sideways-Right shots")]
    public int   sidewaysRightShotsNr = 15;
    public float sidewaysRightMinDist = 4f;
    public float sidewaysRightMaxDist = 7f;
    public float sidewaysRightMinPath = 3f;
    public float sidewaysRightMaxPath = 6f;

    [Header("Whip-pan shots")]
    public int whipPanShotsNr = 15;
    public float whipPanMinHeight = 0.2f;
    public float whipPanMaxHeight = 1f;
    public float whipPanMinCenterDist = 3f;
    public float whipPanMaxCenterDist = 7f;
    public int whipPanShotsMinDegrees = 30;
    public int whipPanShotsMaxDegrees = 240; 
    [Tooltip("The minimum number of times the camera switches between the given 2 points")]
    public int whipPanMinRepetitions = 3;
    [Tooltip("The maximum number of times the camera switches between the given 2 points")]
    public int whipPanMaxRepetitions = 5;

    [Header("Zoom-In shots")]
    public int   zoomInShotsNr     = 15;
    public int   zoomInStartMinFOV = 80;
    public int   zoomInStartMaxFOV = 100;
    public int   zoomInEndMinFOV   = 60;
    public int   zoomInEndMaxFOV   = 70;
    public float zoomInMinDist     = 5f;
    public float zoomInMaxDist     = 20f;

    [Header("Zoom-Out shots")]
    public int   zoomOutShotsNr     = 15;
    public int   zoomOutStartMinFOV = 60;
    public int   zoomOutStartMaxFOV = 70;
    public int   zoomOutEndMinFOV   = 80;
    public int   zoomOutEndMaxFOV   = 100;
    public float zoomOutMinDist     = 5f;
    public float zoomOutMaxDist     = 20f;

    [Header("Vertical-Up shots")]
    public int   verticalUpShotsNr = 15;
    public float verticalUpStartMinDist = 5f;
    public float verticalUpStartMaxDist = 8f;
    public float verticalUpMinDist = 2f;
    public float verticalUpMaxDist = 4f;

    [Header("Vertical-Down shots")]
    public int verticalDownShotsNr = 15;
    public float verticalDownStartMinDist = 5f;
    public float verticalDownStartMaxDist = 8f;
    public float verticalDownMinDist = 2f;
    public float verticalDownMaxDist = 4f;
}
