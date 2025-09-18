using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SoundData[] musicSounds;
    [SerializeField] private SoundData[] sfxSounds;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private Dictionary<string, AudioClip> musicClips;
    private Dictionary<string, AudioClip> sfxClips;

    public float masterVolume = 1f;
    public float defaultMusicVolume = 0.5f;
    public float defaultSfxVolume = 0.5f;
    private Tween fadeTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicClips = new Dictionary<string, AudioClip>();
        foreach (var sound in musicSounds)
            musicClips[sound.nameSound] = sound.clip;

        sfxClips = new Dictionary<string, AudioClip>();
        foreach (var sound in sfxSounds)
            sfxClips[sound.nameSound] = sound.clip;
    }

    private void Start()
    {
        UpdateVolumes();
        PlayMusic("Esmeralda");
    }

    private void UpdateVolumes()
    {
        musicSource.volume = defaultMusicVolume * masterVolume;
        sfxSource.volume = defaultSfxVolume * masterVolume;
    }

    public void PlayMusic(string name, float fadeDuration = 1f)
    {
        if (!musicClips.TryGetValue(name, out AudioClip newClip))
        {
            Debug.LogWarning($"[AudioManager] Música no encontrada: {name}");
            return;
        }

        fadeTween?.Kill();

        if (musicSource.isPlaying)
        {
            fadeTween = musicSource.DOFade(0f, fadeDuration / 2).OnComplete(() =>
            {
                musicSource.clip = newClip;
                musicSource.Play();
                musicSource.DOFade(defaultMusicVolume * masterVolume, fadeDuration / 2);
            });
        }
        else
        {
            musicSource.clip = newClip;
            musicSource.volume = 0f;
            musicSource.Play();
            musicSource.DOFade(defaultMusicVolume * masterVolume, fadeDuration);
        }
    }

    public void PlaySFX(string name)
    {
        if (!sfxClips.TryGetValue(name, out AudioClip clip))
        {
            Debug.LogWarning($"[AudioManager] SFX no encontrado: {name}");
            return;
        }
        sfxSource.PlayOneShot(clip, masterVolume); // Puedes aplicar master volume aquí también
    }

    public void ToggleMusic() => musicSource.mute = !musicSource.mute;

    public void ToggleSFX() => sfxSource.mute = !sfxSource.mute;

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        defaultMusicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        defaultSfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void PauseAll(bool pause)
    {
        if (pause)
        {
            musicSource.Pause();
            sfxSource.Pause();
        }
        else
        {
            musicSource.UnPause();
            sfxSource.UnPause();
        }
    }
    
    [Serializable]
    public class SoundData
    {
        public string nameSound;
        public AudioClip clip;
    }
}

