using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ShotsPlanner : MonoBehaviour
{
    // for now lets assume that given coordinates are in global space   
    public List<T> PlanShots<T>(ShotTypes shotType, int environmentNr, GameObject environment, List<Vector3> clusterCenters, int numShots, int startId, SettingsManager settings, EnvironmentSettings envSettings) where T : ShotPlan
    {
        List<T> plans = new();
        int planNr = startId;

            //GameObject tempEnvironment = Instantiate(environment, Vector3.zero, Quaternion.identity);  // necessary for raycasting  // cp by bylo gdyby te zmienne tez przechowywac w settingsmanager?

            for (int i = 0; i < numShots; i++)
            {
                Vector3 targetPoint = clusterCenters[i];
                int FOV = Random.Range(settings.minFOV, settings.maxFOV);
                Vector3 cameraLocation;
                
                switch (shotType)
                {
                    case ShotTypes.Static:
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.staticShotsMinDist, envSettings.staticShotsMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        ShotPlanStatic staticPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation);
                        plans.Add((T)(object)staticPlan);
                    break;
                        
                    case ShotTypes.ArcLeft:
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.arcLeftShotsMinDist, envSettings.arcLeftShotsMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        int arcDegrees = Random.Range(envSettings.arcLeftShotsMinDegrees, envSettings.arcLeftShotsMaxDegrees);
                        int tiltAngle = Random.Range(0, envSettings.arcLeftShotsMaxTiltAngle);

                        ShotPlanArcLeft arcPlanLeft = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, arcDegrees, tiltAngle);
                        plans.Add((T)(object)arcPlanLeft);
                    break;

                    case ShotTypes.ArcRight:
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.arcRightShotsMinDist, envSettings.arcRightShotsMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        arcDegrees = Random.Range(envSettings.arcRightShotsMinDegrees, envSettings.arcRightShotsMaxDegrees);
                        tiltAngle = Random.Range(0, envSettings.arcRightShotsMaxTiltAngle);

                        ShotPlanArcRight arcPlanRight = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, arcDegrees, tiltAngle);
                        plans.Add((T)(object)arcPlanRight);
                    break;

                    case ShotTypes.PushOut:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        Vector3 endPoint = MiscFunctions.SpherePoint(targetPoint, envSettings.pushOutEndMinDist, envSettings.pushOutEndMaxDist);

                        float pushOutStartDistance = Random.Range(envSettings.pushOutStartMinDist, envSettings.pushOutStartMaxDist);

                        ShotPlanPushOut pushOutPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, endPoint, pushOutStartDistance);
                        plans.Add((T)(object) pushOutPlan);
                    break;

                    case ShotTypes.PushIn:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        Vector3 startPoint = MiscFunctions.SpherePoint(targetPoint, envSettings.pushInStartMinDist, envSettings.pushInStartMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        float pushInEndDistance = Random.Range(envSettings.pushInEndMinDist, envSettings.pushInEndMaxDist);

                        ShotPlanPushIn pushInPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, startPoint, pushInEndDistance);
                        plans.Add((T)(object) pushInPlan);
                    break;

                    case ShotTypes.PanLeft:
                        //cameraLocation = MiscFunctions.SpherePoint(targetPoint, settings.panMinCenterDist, settings.panMaxCenterDist, settings.minCameraHeight);
                        cameraLocation = targetPoint;
                        int panDegrees = Random.Range(envSettings.panLeftShotsMinDegrees, envSettings.panLeftShotsMaxDegrees);
                        //panDegrees *= Random.Range(0,2)*2 -1;
                        panDegrees *= -1;

                        float panHeight = Random.Range(envSettings.panLeftMinHeight, envSettings.panLeftMaxHeight);

                        ShotPlanPanLeft panPlanLeft = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, panDegrees, panHeight);
                        plans.Add((T)(object)panPlanLeft);
                    break;

                    case ShotTypes.PanRight:
                        //cameraLocation = MiscFunctions.SpherePoint(targetPoint, settings.panMinCenterDist, settings.panMaxCenterDist, settings.minCameraHeight);
                        cameraLocation = targetPoint;
                        panDegrees = Random.Range(envSettings.panRightShotsMinDegrees, envSettings.panRightShotsMaxDegrees);
                        //panDegrees *= Random.Range(0,2)*2 -1;

                        panHeight = Random.Range(envSettings.panRightMinHeight, envSettings.panRightMaxHeight);

                        ShotPlanPanRight panPlanRight = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, panDegrees, panHeight);
                        plans.Add((T)(object)panPlanRight);
                    break;

                    case ShotTypes.RollLeft: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.rollLeftShotsMinDist, envSettings.rollLeftShotsMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        int rollDegrees = Random.Range(envSettings.rollLeftShotsMinDegrees, envSettings.rollLeftShotsMaxDegrees);

                        ShotPlanRollLeft rollPlanLeft = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, rollDegrees);
                        plans.Add((T)(object) rollPlanLeft);
                    break;

                    case ShotTypes.RollRight: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.rollRightShotsMinDist, envSettings.rollRightShotsMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        rollDegrees = Random.Range(envSettings.rollRightShotsMinDegrees, envSettings.rollRightShotsMaxDegrees);
                        rollDegrees *= -1;

                        ShotPlanRollRight rollPlanRight = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, rollDegrees);
                        plans.Add((T)(object) rollPlanRight);
                    break;

                    case ShotTypes.DollyZoomIn: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.dollyZoomInStartMinDist, envSettings.dollyZoomInStartMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        float dollyZoomInEndDistance = Random.Range(envSettings.dollyZoomInEndMinDist, envSettings.dollyZoomInEndMaxDist);

                        int dollyStartFOV = Random.Range(envSettings.dollyZoomInStartMinFOV, envSettings.dollyZoomInStartMaxFOV);
                        int dollyEndFOV = Random.Range(envSettings.dollyZoomInEndMinFOV, envSettings.dollyZoomInEndMaxFOV);

                        ShotPlanDollyZoomIn dollyZoomInPlan = new(planNr, environmentNr, settings.fps, settings.duration, dollyStartFOV, shotType, targetPoint, cameraLocation, dollyEndFOV, dollyZoomInEndDistance);
                        plans.Add((T)(object) dollyZoomInPlan);
                    break;

                    case ShotTypes.DollyZoomOut: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.dollyZoomOutStartMinDist, envSettings.dollyZoomOutStartMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        float dollyZoomOutEndDistance = Random.Range(envSettings.dollyZoomOutEndMinDist, envSettings.dollyZoomOutEndMaxDist);

                        dollyStartFOV = Random.Range(envSettings.dollyZoomOutStartMinFOV, envSettings.dollyZoomOutStartMaxFOV);
                        dollyEndFOV = Random.Range(envSettings.dollyZoomOutEndMinFOV, envSettings.dollyZoomOutEndMaxFOV);

                        ShotPlanDollyZoomOut dollyZoomOutPlan = new(planNr, environmentNr, settings.fps, settings.duration, dollyStartFOV, shotType, targetPoint, cameraLocation, dollyEndFOV, dollyZoomOutEndDistance);
                        plans.Add((T)(object) dollyZoomOutPlan);
                    break;

                    case ShotTypes.SidewaysLeft:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.sidewaysLeftMinDist, envSettings.sidewaysLeftMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        float distance = Random.Range(envSettings.sidewaysLeftMinPath, envSettings.sidewaysLeftMaxPath);

                        int sign = -1;  // -1 or 1

                        Vector3 direction = (targetPoint - cameraLocation).normalized;
                        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x).normalized;

                        //cameraLocation += perpendicular * distance * sign;
                        endPoint = cameraLocation - perpendicular * distance * 2 * sign;

                        ShotPlanSidewaysLeft sidewaysPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, endPoint);
                        plans.Add((T)(object) sidewaysPlan);
                    break;

                    case ShotTypes.SidewaysRight:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.sidewaysRightMinDist, envSettings.sidewaysRightMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);
                        distance = Random.Range(envSettings.sidewaysRightMinPath, envSettings.sidewaysRightMaxPath);

                        sign = 1; 

                        direction = (targetPoint - cameraLocation).normalized;
                        perpendicular = new Vector3(-direction.z, 0, direction.x).normalized;

                        //cameraLocation += perpendicular * distance * sign;
                        endPoint = cameraLocation - perpendicular * distance * 2 * sign;

                        ShotPlanSidewaysRight sidewaysPlanRight = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, endPoint);
                        plans.Add((T)(object) sidewaysPlanRight);
                    break;

                    case ShotTypes.WhipPan: 
                        //cameraLocation = MiscFunctions.SpherePoint(targetPoint, settings.panMinCenterDist, settings.panMaxCenterDist, settings.minCameraHeight);
                        cameraLocation = targetPoint;
                        panDegrees = Random.Range(envSettings.whipPanShotsMinDegrees, envSettings.whipPanShotsMaxDegrees);
                        panDegrees *= Random.Range(0,2)*2 -1;

                        panHeight = Random.Range(envSettings.whipPanMinHeight, envSettings.whipPanMaxHeight);

                        int repetitions = Random.Range(envSettings.whipPanMinRepetitions, envSettings.whipPanMaxRepetitions+1);

                        ShotPlanWhipPan whipPanPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, panDegrees, panHeight, repetitions);
                        plans.Add((T)(object)whipPanPlan);
                    break;

                    case ShotTypes.ZoomIn: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.zoomInMinDist, envSettings.zoomInMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        int startFOV = Random.Range(envSettings.zoomInStartMinFOV, envSettings.zoomInStartMaxFOV);
                        int endFOV = Random.Range(envSettings.zoomInEndMinFOV, envSettings.zoomInEndMaxFOV);

                        ShotPlanZoomIn zoomInPlan = new(planNr, environmentNr, settings.fps, settings.duration, startFOV, shotType, targetPoint, cameraLocation, endFOV);
                        plans.Add((T)(object) zoomInPlan);
                    break;

                    case ShotTypes.ZoomOut: 
                        cameraLocation = MiscFunctions.SpherePoint(targetPoint, envSettings.zoomInMinDist, envSettings.zoomInMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        startFOV = Random.Range(envSettings.zoomOutStartMinFOV, envSettings.zoomOutStartMaxFOV);
                        endFOV = Random.Range(envSettings.zoomOutEndMinFOV, envSettings.zoomOutEndMaxFOV);

                        ShotPlanZoomOut zoomOutPlan = new(planNr, environmentNr, settings.fps, settings.duration, startFOV, shotType, targetPoint, cameraLocation, endFOV);
                        plans.Add((T)(object) zoomOutPlan);
                    break;

                    case ShotTypes.VerticalUp:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        cameraLocation = targetPoint + Vector3.up * Random.Range(envSettings.verticalUpStartMinDist, envSettings.verticalUpStartMaxDist);//MiscFunctions.SpherePoint(targetPoint, envSettings.verticalUpStartMinDist, envSettings.verticalUpStartMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        float travelDistance = Random.Range(envSettings.verticalUpMinDist, envSettings.verticalUpMaxDist);

                        ShotPlanVerticalUp verticalUpPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, travelDistance);
                        plans.Add((T)(object) verticalUpPlan);
                    break;

                    case ShotTypes.VerticalDown:  // przemianowac pushoutshotsmindist na pushoutminstartdist
                        cameraLocation = targetPoint + Vector3.up * Random.Range(envSettings.verticalDownStartMinDist, envSettings.verticalDownStartMaxDist);//MiscFunctions.SpherePoint(targetPoint, envSettings.verticalDownStartMinDist, envSettings.verticalDownStartMaxDist, settings.minCameraStartHeight, settings.maxCameraStartHeight);

                        travelDistance = Random.Range(envSettings.verticalDownMinDist, envSettings.verticalDownMaxDist);

                        ShotPlanVerticalDown verticalDownPlan = new(planNr, environmentNr, settings.fps, settings.duration, FOV, shotType, targetPoint, cameraLocation, travelDistance);
                        plans.Add((T)(object) verticalDownPlan);
                    break;
   
                    default:
                        Debug.LogWarning("Unhandled shot type: " + shotType);
                    break;
                }
                planNr++;
            }
            //Destroy(tempEnvironment);

        return plans;
    }

    public void SaveShotPlans<T> (List<T> plans, string savePath) where T : ShotPlan
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        settings.Converters.Add(new Vector3Converter());
        
        string json = JsonConvert.SerializeObject(plans, settings);

        File.WriteAllText(savePath, json);
        
        Debug.Log("Saved " + plans.Count + " shot plans");
    }
}
