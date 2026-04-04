using UnityEngine;

public struct CameraPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fieldOfView;

        public CameraPose(Vector3 pos, Quaternion rot, float fov = 0)
        {
            position = pos;
            rotation = rot;
            fieldOfView = fov;
        }
    }