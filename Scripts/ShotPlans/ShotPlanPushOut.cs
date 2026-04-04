using UnityEngine;

public class ShotPlanPushOut : ShotPlan
{
    public float pushOutStartDistance;
    public ShotPlanPushOut(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, float pushOutStartDistance) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.pushOutStartDistance = pushOutStartDistance;
    }

    public ShotPlanPushOut() {}
}
