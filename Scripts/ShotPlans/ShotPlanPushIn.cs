using UnityEngine;

public class ShotPlanPushIn : ShotPlan
{
    public float pushInEndDistance;
    public ShotPlanPushIn(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, float pushInEndDistance) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.pushInEndDistance = pushInEndDistance;
    }

    public ShotPlanPushIn() {}
}
