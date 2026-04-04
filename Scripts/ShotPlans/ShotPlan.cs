using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public abstract class ShotPlan
{
    public int id;
    public int environmentType;
    public int fps;
    public float duration;
    public int FOV;
    public ShotTypes shotType;
    public Vector3 observedPoint;
    public Vector3 cameraLocation;
}
