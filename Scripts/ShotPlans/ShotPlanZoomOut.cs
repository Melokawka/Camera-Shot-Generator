using UnityEngine;
public class ShotPlanZoomOut : ShotPlan
{
    public int zoomEndFOV;
    public ShotPlanZoomOut(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, int zoomEndFOV) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.zoomEndFOV = zoomEndFOV;
    }

    public ShotPlanZoomOut() {}
}
