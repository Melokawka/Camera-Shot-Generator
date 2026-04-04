using UnityEngine;
public class ShotPlanWhipPan : ShotPlan
{
    public int panDegrees;
    public float panHeight;
    public int repetitions;

    public ShotPlanWhipPan(int id, int environmentType, int fps, float duration, int FOV, ShotTypes shotType,
                            Vector3 observedPoint, Vector3 cameraLocation, int panDegrees, float panHeight, int repetitions) {
        this.id = id;
        this.environmentType = environmentType;
        this.fps = fps;
        this.duration = duration;
        this.FOV = FOV;
        this.shotType = shotType;
        this.observedPoint = observedPoint;
        this.cameraLocation = cameraLocation;
        this.panDegrees = panDegrees;
        this.panHeight = panHeight;
        this.repetitions = repetitions;
    }

    public ShotPlanWhipPan() {}
}
