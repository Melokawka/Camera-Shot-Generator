using UnityEngine;

[System.Serializable]
public class CameraPositionData
{
    public ShotTypes shotType;
    public int shotPlanId;
    public float timestamp;
    public Vector3 position;
    public Vector3 direction;
    public float fov;
}