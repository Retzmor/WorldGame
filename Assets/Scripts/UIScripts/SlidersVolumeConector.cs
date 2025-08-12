using UnityEngine;
using UnityEngine.UI;

public class SlidersVolumeConector : MonoBehaviour
{
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSfx;

    private bool isInitializing = false;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager no encontrado en la escena.");
            return;
        }

        isInitializing = true;

        // Inicializa sliders con los valores actuales
        sliderMaster.value = AudioManager.Instance.masterVolume;
        sliderMusic.value = AudioManager.Instance.defaultMusicVolume;
        sliderSfx.value = AudioManager.Instance.defaultSfxVolume;

        // Suscribe eventos
        sliderMaster.onValueChanged.AddListener(OnMasterVolumeChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicVolumeChanged);
        sliderSfx.onValueChanged.AddListener(OnSfxVolumeChanged);

        isInitializing = false;
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (isInitializing) return;
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (isInitializing) return;
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (isInitializing) return;
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnDestroy()
    {
        if (sliderMaster != null)
            sliderMaster.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (sliderMusic != null)
            sliderMusic.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sliderSfx != null)
            sliderSfx.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }
}
