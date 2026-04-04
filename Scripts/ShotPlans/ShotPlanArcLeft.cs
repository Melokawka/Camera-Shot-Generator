using UnityEngine;
public class ShotPlanArcLeft : ShotPlan  // add inheritance?
{
    public int arcDegrees;
    public float tiltAngle;

    public ShotPlanArcLeft(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, int arcDegrees, float tiltAngle) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.arcDegrees = arcDegrees;
        this.tiltAngle = tiltAngle;
    }

    public ShotPlanArcLeft() {}
}
