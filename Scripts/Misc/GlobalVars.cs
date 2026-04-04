using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using Unity.VisualScripting;
using System;

public class GlobalVars : MonoBehaviour
{
    public List<EnvironmentEntry> environmentsList = new();
    public List<GameObject> environments = new();
    public PostProcessResources postProcessResources;
    public Material[] skyboxList;
    public List<AnimType> animationTypes;
    [Tooltip("Default value: 1. How many times more likely for a type to appear")]
    public List<float> animationTypesWeights;
    public List<AnimationClip> idleAnimations;
    public List<AnimationClip> sittingIdleAnimations;
    public List<AnimationClip> standingIdleAnimations;
    public List<AnimationClip> uncombinableIdleAnimations;
    public List<AnimSelection> uncombinableIdleAnimationsList;
    public List<AnimationClip> runAnimations;
    
    public static string savePath;

    public EnvironmentSettings GetEnvironmentSettings(string name) {
        foreach (EnvironmentEntry env in environmentsList) {
            if (env.environment.name == name) return env.envSettings;
        }
        return null;
    }

    public List<string> GetEnvironmentNames() {
        List<string> names = new();
        foreach (GameObject env in environments) {
            names.Add(env.name);
        }
        return names;
    }
}