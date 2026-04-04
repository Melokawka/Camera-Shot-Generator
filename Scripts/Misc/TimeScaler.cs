using UnityEngine;
using System.Collections.Generic;

public class TimeScaler : MonoBehaviour
{

    [ReadOnly] [SerializeField] private float timeScale;
    public float TimeScale 
    {
        get => timeScale;
        set
        {
            timeScale = value;
            UpdateSpeed();
            Debug.Log("TimeScale Change");
        }
    }
    
    private float fixedDeltaTime;

    void Awake()
    {
        // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        //UpdateSpeed();
    }

    void UpdateSpeed() {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    void OnValidate()  // This method is called whenever a value changes in the Inspector
    {
        //TimeScale = timeScale; // This ensures the setter is triggered
    }
}
