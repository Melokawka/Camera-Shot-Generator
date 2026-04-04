using UnityEngine;

[System.Serializable]
public class EnvironmentEntry
{
    public GameObject environment;
    public int envNr;
    public EnvironmentSettings envSettings;
    public bool isEnabled = true;
}