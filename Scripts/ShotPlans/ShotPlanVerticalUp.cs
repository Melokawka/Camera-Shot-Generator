using UnityEngine;

public class ShotPlanVerticalUp : ShotPlan
{
    public float travelDistance;
    public ShotPlanVerticalUp(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, float travelDistance) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.travelDistance = travelDistance;
    }

    public ShotPlanVerticalUp() {}
}
