using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingRandomizer : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;

    private Bloom bloom;
    private Vignette vignette;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;
    private ColorGrading colorGrading;
    private List<PostProcessEffectSettings> effects; 

    // Bloom
    public float bloomIntensityMin = 1f;
    public float bloomIntensityMax = 3f;

    // Vignette
    public float vignetteIntensityMin = 0.2f;
    public float vignetteIntensityMax = 0.5f;
    public float vignetteSmoothnessMin = 0.2f;
    public float vignetteSmoothnessMax = 0.5f;

    // Depth of Field
    public float dofFocusDistanceMin = 2.8f;
    public float dofFocusDistanceMax = 3f;

    // Chromatic Aberration
    public float chromaticIntensityMin = 0.2f;
    public float chromaticIntensityMax = 0.5f;

    // Color Grading
    public float exposureMin = -2f;
    public float exposureMax = 3f;
    public float saturationMin = -50f;
    public float saturationMax = 50f;
    public float contrastMin = -70f;
    public float contrastMax = 30f;

    //public SettingsManager settingsManager;

    void Start()
    {
        OrganizeSettings();

        // Losowa zmiana intensywności efektów
        ApplyRandomPostProcessingEffects();
    }

    public void OrganizeSettings() {
        // Pobieramy ustawienia z profilu Post Process Volume
        postProcessVolume.profile.TryGetSettings(out bloom);
        postProcessVolume.profile.TryGetSettings(out vignette);
        postProcessVolume.profile.TryGetSettings(out depthOfField);
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        postProcessVolume.profile.TryGetSettings(out colorGrading);

        // Zbieramy wszystkie efekty w liście
        effects = new List<PostProcessEffectSettings>
        {
            bloom,
            vignette,
            depthOfField,
            chromaticAberration,
            colorGrading
        };
    }

    public void ApplyRandomPostProcessingEffects()
    {
        // Wyłączamy wszystkie efekty
        foreach (var effect in effects)
        {
            effect.active = false;
        }

        // Losujemy dwa efekty do włączenia
        List<PostProcessEffectSettings> effectsToEnable = GetRandomEffects(2);

        // Włączamy losowo wybrane efekty
        foreach (var effect in effectsToEnable)
        {
            effect.active = true;

            // Losowe ustawienie wartości dla wybranego efektu
            if (effect is Bloom bloom)
            {
                bloom.intensity.value = Random.Range(bloomIntensityMin, bloomIntensityMax);
            }
            else if (effect is Vignette vignette)
            {
                vignette.intensity.value = Random.Range(vignetteIntensityMin, vignetteIntensityMax);
                vignette.smoothness.value = Random.Range(vignetteSmoothnessMin, vignetteSmoothnessMax);
            }
            else if (effect is DepthOfField dof)
            {
                dof.active = true;
                dof.focusDistance.value = Random.Range(dofFocusDistanceMin, dofFocusDistanceMax);
                //depthOfFieldEffect.aperture.value = Random.Range(0.1f, 10f);
                //depthOfFieldEffect.focalLength.value = Random.Range(20f, 70f);
            }
            else if (effect is ChromaticAberration chromatic)
            {
                chromatic.intensity.value = Random.Range(chromaticIntensityMin, chromaticIntensityMax);
            }
            else if (effect is ColorGrading grading)
            {
                //grading.postExposure.value = Random.Range(exposureMin, exposureMax);
                grading.saturation.value = Random.Range(saturationMin, saturationMax);
                grading.contrast.value = Random.Range(contrastMin, contrastMax);
            }
        }
    }

    List<PostProcessEffectSettings> GetRandomEffects(int count)
    {
        // Losujemy efekty
        List<PostProcessEffectSettings> selectedEffects = new List<PostProcessEffectSettings>();
        for (int i = 0; i < count; i++)
        {
            PostProcessEffectSettings randomEffect = effects[Random.Range(0, effects.Count)];
            // Aby nie dodać tego samego efektu wielokrotnie
            if (!selectedEffects.Contains(randomEffect))
            {
                selectedEffects.Add(randomEffect);
            }
            else
            {
                i--;  // Powtarzamy próbę, jeśli wybrany efekt już jest w liście
            }
        }
        return selectedEffects;
    }

    public void LoadSettingsFromManager(SettingsManager settingsManager)
    {
        bloomIntensityMin = settingsManager.bloomIntensityMin;
        bloomIntensityMax = settingsManager.bloomIntensityMax;

        vignetteIntensityMin = settingsManager.vignetteIntensityMin;
        vignetteIntensityMax = settingsManager.vignetteIntensityMax;
        vignetteSmoothnessMin = settingsManager.vignetteSmoothnessMin;
        vignetteSmoothnessMax = settingsManager.vignetteSmoothnessMax;

        dofFocusDistanceMin = settingsManager.dofFocusDistanceMin;
        dofFocusDistanceMax = settingsManager.dofFocusDistanceMax;

        chromaticIntensityMin = settingsManager.chromaticIntensityMin;
        chromaticIntensityMax = settingsManager.chromaticIntensityMax;
        
        saturationMin = settingsManager.saturationMin;
        saturationMax = settingsManager.saturationMax;
        contrastMin = settingsManager.contrastMin;
        contrastMax = settingsManager.contrastMax;
    }
}