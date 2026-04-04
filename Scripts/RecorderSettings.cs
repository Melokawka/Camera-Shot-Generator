using UnityEngine;

public class RecorderSettings
{
    public int shotPlanId;
    public int fps;
    public int resolution;
    public float duration;
    public string savePath;
    public ShotTypes shotType;
    public Vector3 interestPoint;

    public float rotationSpeed;
    public float tiltAngle;

    public Vector3 endPoint;
    public Vector3 startPoint;

    public float startFOV;
    public float endFOV;

    #region WhipPan
        public int repetitions;
    #endregion

    public RecorderSettings(int shotPlanId, int fps, int resolution, float duration, string savePath,
                                 ShotTypes shotType, Vector3 interestPoint,
                                 float rotationSpeed = 0f, float tiltAngle = 0f, 
                                 Vector3 startPoint = new(), Vector3 endPoint = new(),
                                 float startFOV = 0, float endFOV = 0,
                                 int repetitions = 0) {
        this.shotPlanId = shotPlanId;
        this.fps = fps;
        this.resolution = resolution;
        this.duration = duration;
        this.savePath = savePath;
        this.shotType = shotType;
        this.interestPoint = interestPoint;

        this.rotationSpeed = rotationSpeed;
        this.tiltAngle = tiltAngle;

        this.startPoint = startPoint;
        this.endPoint = endPoint;

        this.startFOV = startFOV;
        this.endFOV = endFOV;
        this.repetitions = repetitions;
    }
}
