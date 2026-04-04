using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class EnvironmentSettingsManager : MonoBehaviour
{
    GlobalVars globalVars;
    [SerializeField] public List<EnvironmentSettings> environmentSettingsList = new(8);
    public string settingsPath = "";

    private void Awake()
    {
        if (environmentSettingsList.Count != 8)
        {
            environmentSettingsList = new List<EnvironmentSettings>(8);
            for (int i = 0; i < 8; i++)
            {
                environmentSettingsList.Add(new EnvironmentSettings());
            }
        }
    }

    //public void UpdateSettingsFromEditor() {
    //     environmentSettingsList[selectedEnvironment] = new EnvironmentSettings
    //     {
    //         idleCharactersPerCluster = idleCharactersPerCluster,
    //         runCharactersPerCluster = runCharactersPerCluster,
    //         clusterRadius = clusterRadius,
    //         pedMinSpeed = pedMinSpeed,
    //         pedMaxSpeed = pedMaxSpeed,

    //         staticShotsNr = staticShotsNr,
    //         staticShotsMinDist = staticShotsMinDist,
    //         staticShotsMaxDist = staticShotsMaxDist,

    //         arcLeftShotsNr = arcLeftShotsNr,
    //         arcLeftShotsMinDist = arcLeftShotsMinDist,
    //         arcLeftShotsMaxDist = arcLeftShotsMaxDist,
    //         arcLeftShotsMinDegrees = arcLeftShotsMinDegrees,
    //         arcLeftShotsMaxDegrees = arcLeftShotsMaxDegrees,
    //         arcLeftShotsMaxTiltAngle = arcLeftShotsMaxTiltAngle,

    //         arcRightShotsNr = arcRightShotsNr,
    //         arcRightShotsMinDist = arcRightShotsMinDist,
    //         arcRightShotsMaxDist = arcRightShotsMaxDist,
    //         arcRightShotsMinDegrees = arcRightShotsMinDegrees,
    //         arcRightShotsMaxDegrees = arcRightShotsMaxDegrees,
    //         arcRightShotsMaxTiltAngle = arcRightShotsMaxTiltAngle,

    //         pushOutShotsNr = pushOutShotsNr,
    //         pushOutStartMinDist = pushOutStartMinDist,
    //         pushOutStartMaxDist = pushOutStartMaxDist,
    //         pushOutEndMinDist = pushOutEndMinDist,
    //         pushOutEndMaxDist = pushOutEndMaxDist,

    //         pushInShotsNr = pushInShotsNr,
    //         pushInStartMinDist = pushInStartMinDist,
    //         pushInStartMaxDist = pushInStartMaxDist,
    //         pushInEndMinDist = pushInEndMinDist,
    //         pushInEndMaxDist = pushInEndMaxDist,

    //         rollLeftShotsNr = rollLeftShotsNr,
    //         rollLeftShotsMinDist = rollLeftShotsMinDist,
    //         rollLeftShotsMaxDist = rollLeftShotsMaxDist,
    //         rollLeftShotsMinDegrees = rollLeftShotsMinDegrees,
    //         rollLeftShotsMaxDegrees = rollLeftShotsMaxDegrees, 

    //         rollRightShotsNr = rollRightShotsNr,
    //         rollRightShotsMinDist = rollRightShotsMinDist,
    //         rollRightShotsMaxDist = rollRightShotsMaxDist,
    //         rollRightShotsMinDegrees = rollRightShotsMinDegrees,
    //         rollRightShotsMaxDegrees = rollRightShotsMaxDegrees, 

    //         dollyZoomInShotsNr = dollyZoomInShotsNr,
    //         dollyZoomInStartMinFOV = dollyZoomInStartMinFOV,
    //         dollyZoomInStartMaxFOV = dollyZoomInStartMaxFOV,
    //         dollyZoomInEndMinFOV = dollyZoomInEndMinFOV,
    //         dollyZoomInEndMaxFOV = dollyZoomInEndMaxFOV,
    //         dollyZoomInStartMinDist = dollyZoomInStartMinDist,
    //         dollyZoomInStartMaxDist = dollyZoomInStartMaxDist,
    //         dollyZoomInEndMinDist = dollyZoomInEndMinDist,
    //         dollyZoomInEndMaxDist = dollyZoomInEndMaxDist,

    //         dollyZoomOutShotsNr = dollyZoomOutShotsNr,
    //         dollyZoomOutStartMinFOV = dollyZoomOutStartMinFOV,
    //         dollyZoomOutStartMaxFOV = dollyZoomOutStartMaxFOV,
    //         dollyZoomOutEndMinFOV = dollyZoomOutEndMinFOV,
    //         dollyZoomOutEndMaxFOV = dollyZoomOutEndMaxFOV,
    //         dollyZoomOutStartMinDist = dollyZoomOutStartMinDist,
    //         dollyZoomOutStartMaxDist = dollyZoomOutStartMaxDist,
    //         dollyZoomOutEndMinDist = dollyZoomOutEndMinDist,
    //         dollyZoomOutEndMaxDist = dollyZoomOutEndMaxDist,

    //         panLeftShotsNr = panLeftShotsNr,
    //         panLeftMinCenterDist = panLeftMinCenterDist,
    //         panLeftMaxCenterDist = panLeftMaxCenterDist,
    //         panLeftShotsMinDegrees = panLeftShotsMinDegrees,
    //         panLeftShotsMaxDegrees = panLeftShotsMaxDegrees,
    //         panLeftMinHeight = panLeftMinHeight,
    //         panLeftMaxHeight = panLeftMaxHeight,

    //         panRightShotsNr = panRightShotsNr,
    //         panRightMinCenterDist = panRightMinCenterDist,
    //         panRightMaxCenterDist = panRightMaxCenterDist,
    //         panRightShotsMinDegrees = panRightShotsMinDegrees,
    //         panRightShotsMaxDegrees = panRightShotsMaxDegrees,
    //         panRightMinHeight = panRightMinHeight,
    //         panRightMaxHeight = panRightMaxHeight,

    //         sidewaysLeftShotsNr = sidewaysLeftShotsNr,
    //         sidewaysLeftMinDist = sidewaysLeftMinDist,
    //         sidewaysLeftMaxDist = sidewaysLeftMaxDist,
    //         sidewaysLeftMinPath = sidewaysLeftMinPath,
    //         sidewaysLeftMaxPath = sidewaysLeftMaxPath,

    //         sidewaysRightShotsNr = sidewaysRightShotsNr,
    //         sidewaysRightMinDist = sidewaysRightMinDist,
    //         sidewaysRightMaxDist = sidewaysRightMaxDist,
    //         sidewaysRightMinPath = sidewaysRightMinPath,
    //         sidewaysRightMaxPath = sidewaysRightMaxPath,

    //         whipPanShotsNr = whipPanShotsNr,
    //         whipPanMinHeight = whipPanMinHeight,
    //         whipPanMaxHeight = whipPanMaxHeight,
    //         whipPanMinCenterDist = whipPanMinCenterDist,
    //         whipPanMaxCenterDist = whipPanMaxCenterDist,
    //         whipPanShotsMinDegrees = whipPanShotsMinDegrees,
    //         whipPanShotsMaxDegrees = whipPanShotsMaxDegrees,
    //         whipPanMinRepetitions = whipPanMinRepetitions,
    //         whipPanMaxRepetitions = whipPanMaxRepetitions, 

    //         zoomInShotsNr     = zoomInShotsNr,    
    //         zoomInStartMinFOV = zoomInStartMinFOV,
    //         zoomInStartMaxFOV = zoomInStartMaxFOV,
    //         zoomInEndMinFOV   = zoomInEndMinFOV,
    //         zoomInEndMaxFOV   = zoomInEndMaxFOV,
    //         zoomInMinDist     = zoomInMinDist,   
    //         zoomInMaxDist     = zoomInMaxDist,

    //         zoomOutShotsNr     = zoomOutShotsNr,    
    //         zoomOutStartMinFOV = zoomOutStartMinFOV,
    //         zoomOutStartMaxFOV = zoomOutStartMaxFOV,
    //         zoomOutEndMinFOV   = zoomOutEndMinFOV,
    //         zoomOutEndMaxFOV   = zoomOutEndMaxFOV,
    //         zoomOutMinDist     = zoomOutMinDist,   
    //         zoomOutMaxDist     = zoomOutMaxDist,

    //         verticalUpShotsNr = verticalUpShotsNr,
    //         verticalUpStartMinDist = verticalUpStartMinDist,
    //         verticalUpStartMaxDist = verticalUpStartMaxDist,
    //         verticalUpMinDist = verticalUpMinDist,
    //         verticalUpMaxDist = verticalUpMaxDist,

    //         verticalDownShotsNr = verticalDownShotsNr,
    //         verticalDownStartMinDist = verticalDownStartMinDist,
    //         verticalDownStartMaxDist = verticalDownStartMaxDist,
    //         verticalDownMinDist = verticalDownMinDist,
    //         verticalDownMaxDist = verticalDownMaxDist,
    //     };
    // }

    public void SaveSettings()
    {
        JsonSerializerSettings serializerSettings = new()
        {
            Formatting = Formatting.Indented
        };
        
        //string json = JsonConvert.SerializeObject(environmentSettings, serializerSettings);
        //File.WriteAllText(settingsPath, json);

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

        //environmentSettings = JsonConvert.DeserializeObject<EnvironmentSettings>(json);

        Debug.Log("Settings loaded from " + settingsPath);
    }
}
