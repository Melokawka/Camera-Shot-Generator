using UnityEngine;
public class ShotPlanSidewaysRight : ShotPlan
{
    public Vector3 endPoint;
    public ShotPlanSidewaysRight(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, Vector3 endPoint) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.endPoint = endPoint;
    }

    public ShotPlanSidewaysRight() {}
}
