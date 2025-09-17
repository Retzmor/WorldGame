using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PostProcessingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume volume; // Arrastra tu Volume de la escena
    private Vignette vignette;
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        if (volume == null)
        {
            Debug.LogError("No se asignó ningún Volume al PostProcessingManager");
            return;
        }

        // Buscar efectos dentro del Volume
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out colorAdjustments);
    }

    #region Bloom
    public void EnableBloom(bool enable)
    {
        if (bloom != null)
            bloom.active = enable;
    }

    public void SetBloomIntensity(float intensity)
    {
        if (bloom != null)
            bloom.intensity.value = intensity;
    }
    #endregion

    #region Vignette
    public void EnableVignette(bool enable, float targetIntensity, Color targetColor, float fadeInTime = 0.15f, float fadeOutTime = 0.3f, float holdTime = 1f)
    {
        if (vignette == null) return;

        vignette.active = enable;

        if (enable)
        {
            vignette.intensity.value = 0f; // empieza invisible
            vignette.color.value = targetColor;

            Sequence seq = DOTween.Sequence();

            // Fade In con DOFloat
            seq.Append(DOTween.To(() => vignette.intensity.value,
                                  x => vignette.intensity.value = x,
                                  targetIntensity, fadeInTime));

            // Mantener el efecto un tiempo
            seq.AppendInterval(holdTime);

            // Fade Out con DOFloat
            seq.Append(DOTween.To(() => vignette.intensity.value,
                                  x => vignette.intensity.value = x,
                                  0f, fadeOutTime));

            // Al terminar, desactiva el efecto
            seq.OnComplete(() => vignette.active = false);
        }
        else
        {
            vignette.active = false;
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (vignette != null)
            vignette.intensity.value = intensity;
    }
    #endregion

    #region Color Adjustments
    public void SetSaturation(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = value;
    }

    public void SetContrast(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.contrast.value = value;
    }
    #endregion
}
