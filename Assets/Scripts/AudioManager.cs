using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    public AudioSource footstepSource;

    [Header("Music")]
    public AudioClip bgmMusic;

    [Header("Footsteps")]
    public AudioClip[] floorSteps;
    public float stepInterval = 0.48f;

    [Header("Voice Clips")]
    public AudioClip voiceGreetings;
    public AudioClip voiceWhatDoYouWant;
    public AudioClip voiceSpeakQuickly;
    public AudioClip voiceWantToTalk;
    public AudioClip voicePatience;
    public AudioClip voiceDontScareMe;
    public AudioClip voiceLaugh;

    [Header("UI Click")]
    public AudioClip selectSound;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool isMuted = false;

    private Coroutine footstepCoroutine;
    private int currentStepIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
        }

        ApplyVolumes();
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmSource != null && bgmMusic != null)
        {
            bgmSource.clip = bgmMusic;
            if (!bgmSource.isPlaying && !isMuted)
            {
                bgmSource.Play();
            }
        }
    }

    public void StartFootsteps()
    {
        if (footstepCoroutine != null) StopCoroutine(footstepCoroutine);
        footstepCoroutine = StartCoroutine(FootstepLoop());
    }

    public void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        if (footstepSource != null) footstepSource.Stop();
    }

    private IEnumerator FootstepLoop()
    {
        while (true)
        {
            if (floorSteps != null && floorSteps.Length > 0 && !isMuted && footstepSource != null)
            {
                var clip = floorSteps[currentStepIndex % floorSteps.Length];
                currentStepIndex++;
                footstepSource.clip = clip;
                footstepSource.pitch = Random.Range(0.95f, 1.05f);
                footstepSource.volume = isMuted ? 0f : (sfxVolume * masterVolume * 0.95f);
                footstepSource.Play();
            }
            yield return new WaitForSeconds(stepInterval);
        }
    }

    public void PlayCharacterVoice(string characterName, string actionType)
    {
        if (voiceSource == null || isMuted) return;

        // Stop prior clip before starting a new one
        voiceSource.Stop();

        AudioClip clipToPlay = null;
        float pitch = 1.0f;

        switch (characterName)
        {
            case "The Boss":
                pitch = 0.92f;
                if (actionType == "talk") clipToPlay = voiceSpeakQuickly != null ? voiceSpeakQuickly : voiceWhatDoYouWant;
                else if (actionType == "celebrate") clipToPlay = voiceLaugh;
                else if (actionType == "special") clipToPlay = voiceGreetings;
                else if (actionType == "react") clipToPlay = voicePatience;
                break;

            case "Remy":
                pitch = 1.22f;
                if (actionType == "talk") clipToPlay = voiceWantToTalk != null ? voiceWantToTalk : voiceGreetings;
                else if (actionType == "celebrate") clipToPlay = voiceLaugh;
                else if (actionType == "special") clipToPlay = voiceDontScareMe;
                else if (actionType == "react") clipToPlay = voiceDontScareMe;
                break;

            case "Peasant Girl":
                pitch = 1.45f;
                if (actionType == "talk") clipToPlay = voiceGreetings != null ? voiceGreetings : voiceWantToTalk;
                else if (actionType == "celebrate") clipToPlay = voiceLaugh;
                else if (actionType == "special") clipToPlay = voiceGreetings;
                else if (actionType == "react") clipToPlay = voicePatience;
                break;

            default:
                if (actionType == "talk") clipToPlay = voiceWantToTalk;
                else if (actionType == "celebrate") clipToPlay = voiceLaugh;
                break;
        }

        if (clipToPlay != null)
        {
            voiceSource.clip = clipToPlay;
            voiceSource.pitch = pitch;
            voiceSource.volume = isMuted ? 0f : (sfxVolume * masterVolume * 1.15f);
            voiceSource.Play();
        }
    }

    public void PlayActionSound(string actionName, string characterName = "The Boss")
    {
        string act = actionName.ToLower();
        if (act == "select")
        {
            if (sfxSource != null && selectSound != null && !isMuted)
            {
                sfxSource.clip = selectSound;
                sfxSource.pitch = 1f;
                sfxSource.volume = isMuted ? 0f : (sfxVolume * masterVolume * 0.8f);
                sfxSource.Play();
            }
        }
        else if (act == "walk_start")
        {
            StartFootsteps();
        }
        else if (act == "walk_stop")
        {
            StopFootsteps();
        }
        else
        {
            PlayCharacterVoice(characterName, act);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = isMuted ? 0f : masterVolume;
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : masterVolume;
        if (bgmSource != null)
        {
            if (isMuted) bgmSource.Pause();
            else bgmSource.UnPause();
        }
        if (isMuted) StopFootsteps();
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = isMuted ? 0f : (musicVolume * masterVolume);
        if (sfxSource != null)
            sfxSource.volume = isMuted ? 0f : (sfxVolume * masterVolume);
        if (voiceSource != null)
            voiceSource.volume = isMuted ? 0f : (sfxVolume * masterVolume);
        if (footstepSource != null)
            footstepSource.volume = isMuted ? 0f : (sfxVolume * masterVolume);
    }
}
