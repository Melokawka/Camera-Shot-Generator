using UnityEngine;
public class ShotPlanRollRight : ShotPlan
{
    public int rollDegrees;
    public ShotPlanRollRight(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, int rollDegrees) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.rollDegrees = rollDegrees;
    }
    public ShotPlanRollRight() {}
}
