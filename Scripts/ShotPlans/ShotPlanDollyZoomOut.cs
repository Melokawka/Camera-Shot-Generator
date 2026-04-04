using UnityEngine;
public class ShotPlanDollyZoomOut : ShotPlan
{
    public int dollyEndFOV;
    public float dollyEndDistance;
    public ShotPlanDollyZoomOut(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, int dollyEndFOV, float dollyEndDistance) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.dollyEndFOV = dollyEndFOV;
        this.dollyEndDistance = dollyEndDistance;
    }

    public ShotPlanDollyZoomOut() {}
}
