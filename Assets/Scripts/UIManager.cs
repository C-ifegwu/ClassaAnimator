using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Panels")]
    public CanvasGroup mainMenuPanel;
    public CanvasGroup directorStudioPanel;
    public CanvasGroup settingsPanel;

    [Header("UI RectTransforms for Tweening")]
    public RectTransform topBarRT;
    public RectTransform actorPanelRT;
    public RectTransform logPanelRT;
    public RectTransform cmdPanelRT;
    public RectTransform menuCardRT;
    public RectTransform settingsCardRT;

    [Header("Status and Logs")]
    public TextMeshProUGUI directorStatusText;
    public TextMeshProUGUI sceneStateText;
    public TextMeshProUGUI interactionHistoryText;

    [Header("Audio Settings UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI muteButtonText;

    [Header("Actor GameObjects")]
    public GameObject bossActor;
    public GameObject remyActor;
    public GameObject peasantActor;

    [Header("Transitions")]
    [SerializeField] private float transitionDuration = 0.35f;

    [Header("Manager References")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private AudioManager audioManager;

    private CanvasGroup currentActivePanel;
    private bool isWalkingState = false;
    private string currentActorName = "The Boss";
    private readonly List<string> historyLog = new List<string>();
    private const int MaxHistoryLines = 6;
    private int sceneTakeCounter = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DOTween.Init();
    }

    private void Start()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();
        if (audioManager == null)
            audioManager = FindAnyObjectByType<AudioManager>();

        if (bossActor == null || remyActor == null || peasantActor == null)
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name == "The Boss") bossActor = r;
                if (r.name == "Remy") remyActor = r;
                if (r.name == "Peasant Girl") peasantActor = r;
            }
        }

        ResolveHUDTransforms();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterVolumeSlider.value = AudioListener.volume;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = 0.8f;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxVolumeSlider.value = 1f;
        }

        InitializeScreens();

        UpdateDirectorStatus("The Boss", "Idle");
        UpdateSceneState("Rehearsal Stage");
        LogInteraction("The Boss", "Scene Initialized (Take 1)");
    }

    private void ResolveHUDTransforms()
    {
        if (directorStudioPanel != null)
        {
            if (topBarRT == null) topBarRT = directorStudioPanel.transform.Find("TopBar") as RectTransform;
            if (actorPanelRT == null) actorPanelRT = directorStudioPanel.transform.Find("ActorSelectPanel") as RectTransform;
            if (logPanelRT == null) logPanelRT = directorStudioPanel.transform.Find("DirectorLogPanel") as RectTransform;
            if (cmdPanelRT == null) cmdPanelRT = directorStudioPanel.transform.Find("ActionControlsPanel") as RectTransform;
        }
        if (mainMenuPanel != null && menuCardRT == null)
        {
            menuCardRT = mainMenuPanel.transform.Find("MenuCard") as RectTransform;
        }
        if (settingsPanel != null && settingsCardRT == null)
        {
            settingsCardRT = settingsPanel.transform.Find("SettingsCard") as RectTransform;
        }
    }

    private void InitializeScreens()
    {
        SetPanelImmediate(mainMenuPanel, true);
        SetPanelImmediate(directorStudioPanel, false);
        SetPanelImmediate(settingsPanel, false);

        currentActivePanel = mainMenuPanel;
    }

    private void SetPanelImmediate(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        group.gameObject.SetActive(visible);
    }

    #region Screen Navigation

    public void ShowMainMenu()
    {
        TransitionToScreen(mainMenuPanel);
        if (GameManager.Instance != null)
            GameManager.Instance.currentGameState = GameManager.GameState.MainMenu;
        UpdateSceneState("Main Menu");
    }

    public void ShowDirectorStudio()
    {
        TransitionToScreen(directorStudioPanel);
        if (GameManager.Instance != null)
            GameManager.Instance.currentGameState = GameManager.GameState.DirectorStudio;
        UpdateSceneState($"Live Studio (Take {sceneTakeCounter})");
    }

    public void ShowSettings()
    {
        TransitionToScreen(settingsPanel);
        if (GameManager.Instance != null)
            GameManager.Instance.currentGameState = GameManager.GameState.Settings;
    }

    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameManager.GameState.Settings)
        {
            ShowDirectorStudio();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void TransitionToScreen(CanvasGroup targetPanel)
    {
        if (targetPanel == null || targetPanel == currentActivePanel) return;

        var fromPanel = currentActivePanel;
        currentActivePanel = targetPanel;

        // Fade out previous screen
        if (fromPanel != null)
        {
            fromPanel.interactable = false;
            fromPanel.blocksRaycasts = false;
            fromPanel.DOKill();
            fromPanel.DOFade(0f, transitionDuration * 0.7f).SetUpdate(true).OnComplete(() =>
            {
                fromPanel.gameObject.SetActive(false);
            });
        }

        // Fade in new screen
        targetPanel.gameObject.SetActive(true);
        targetPanel.alpha = 0f;
        targetPanel.interactable = false;
        targetPanel.blocksRaycasts = false;
        targetPanel.DOKill();

        targetPanel.DOFade(1f, transitionDuration).SetUpdate(true).OnComplete(() =>
        {
            targetPanel.interactable = true;
            targetPanel.blocksRaycasts = true;
        });

        // Entrance motion
        if (targetPanel == directorStudioPanel)
        {
            AnimateDirectorStudioEntrance();
        }
        else if (targetPanel == mainMenuPanel)
        {
            AnimateCardEntrance(menuCardRT);
        }
        else if (targetPanel == settingsPanel)
        {
            AnimateCardEntrance(settingsCardRT);
        }
    }

    private void AnimateDirectorStudioEntrance()
    {
        if (topBarRT != null)
        {
            topBarRT.DOKill();
            topBarRT.anchoredPosition = new Vector2(0f, 54f);
            topBarRT.DOAnchorPosY(0f, transitionDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        if (actorPanelRT != null)
        {
            actorPanelRT.DOKill();
            actorPanelRT.anchoredPosition = new Vector2(-220f, 20f);
            actorPanelRT.DOAnchorPosX(20f, transitionDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        if (logPanelRT != null)
        {
            logPanelRT.DOKill();
            logPanelRT.anchoredPosition = new Vector2(280f, 20f);
            logPanelRT.DOAnchorPosX(-20f, transitionDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        if (cmdPanelRT != null)
        {
            cmdPanelRT.DOKill();
            cmdPanelRT.anchoredPosition = new Vector2(0f, -80f);
            cmdPanelRT.DOAnchorPosY(18f, transitionDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }
    }

    private void AnimateCardEntrance(RectTransform cardRT)
    {
        if (cardRT != null)
        {
            cardRT.DOKill();
            cardRT.localScale = Vector3.one * 0.90f;
            cardRT.DOScale(1f, transitionDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void AnimateButtonPunch(Transform btnTransform)
    {
        if (btnTransform != null)
        {
            btnTransform.DOKill(true);
            btnTransform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.18f, 6, 1f).SetUpdate(true);
        }
    }

    #endregion

    #region Status Display and Log

    public void UpdateDirectorStatus(string actorName, string currentAction)
    {
        currentActorName = actorName;
        if (directorStatusText != null)
        {
            directorStatusText.text = $"<color=#FFD700>Currently Directing:</color> <b>{actorName}</b>  |  <color=#00E5FF>Status:</color> <i>{currentAction}</i>";

            directorStatusText.transform.DOKill(true);
            directorStatusText.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.22f, 5, 0.5f).SetUpdate(true);
        }
    }

    public void UpdateSceneState(string state)
    {
        if (sceneStateText != null)
        {
            sceneStateText.text = $"<color=#AAAAAA>Scene State:</color> <color=#39FF14><b>{state}</b></color>";
        }
    }

    public void LogInteraction(string actor, string action)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = $"<color=#888>[{timestamp}]</color> <color=#FFCC00>{actor}:</color> {action}";
        historyLog.Add(entry);

        if (historyLog.Count > MaxHistoryLines)
        {
            historyLog.RemoveAt(0);
        }

        if (interactionHistoryText != null)
        {
            interactionHistoryText.text = string.Join("\n", historyLog);

            interactionHistoryText.transform.DOKill(true);
            interactionHistoryText.transform.DOPunchScale(new Vector3(0.03f, 0.03f, 0f), 0.15f, 4, 0.5f).SetUpdate(true);
        }
    }

    #endregion

    #region Studio Controls

    public void OnSelectActor(int actorIndex)
    {
        if (bossActor != null) bossActor.SetActive(actorIndex == 0);
        if (remyActor != null) remyActor.SetActive(actorIndex == 1);
        if (peasantActor != null) peasantActor.SetActive(actorIndex == 2);

        GameObject active = null;
        if (actorIndex == 0) { active = bossActor; currentActorName = "The Boss"; }
        else if (actorIndex == 1) { active = remyActor; currentActorName = "Remy"; }
        else if (actorIndex == 2) { active = peasantActor; currentActorName = "Peasant Girl"; }

        if (active != null)
        {
            var anim = active.GetComponent<Animator>();
            if (characterManager != null)
            {
                characterManager.SetActiveActor(anim);
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.activeActor = active;
            }
        }

        isWalkingState = false;
        UpdateDirectorStatus(currentActorName, "Idle");
        LogInteraction(currentActorName, "Switched Actor (On Stage)");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopFootsteps();
            AudioManager.Instance.PlayActionSound("select", currentActorName);
        }

        if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject != null)
        {
            AnimateButtonPunch(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform);
        }
    }

    public void OnActionButtonWalk()
    {
        isWalkingState = !isWalkingState;
        if (characterManager != null)
        {
            characterManager.ToggleWalk(isWalkingState);
        }
        string actionStr = isWalkingState ? "Walking" : "Idle";
        UpdateDirectorStatus(currentActorName, actionStr);
        LogInteraction(currentActorName, actionStr);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound(isWalkingState ? "walk_start" : "walk_stop", currentActorName);
        }

        if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject != null)
        {
            AnimateButtonPunch(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform);
        }
    }

    public void OnActionButtonTalk()
    {
        if (characterManager != null)
        {
            characterManager.TriggerTalk();
        }
        UpdateDirectorStatus(currentActorName, "Talking");
        LogInteraction(currentActorName, "Performed Talk");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound("talk", currentActorName);
        }

        if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject != null)
        {
            AnimateButtonPunch(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform);
        }
    }

    public void OnActionButtonReact(int reactionIndex)
    {
        if (characterManager != null)
        {
            characterManager.TriggerReact(reactionIndex);
        }

        string reactName = $"React {reactionIndex}";
        string voiceAction = "react";
        if (reactionIndex == 2)
        {
            voiceAction = "celebrate";
            if (currentActorName == "Remy") reactName = "Celebrate (Silly Dance)";
            else if (currentActorName == "The Boss") reactName = "Celebrate (Waving)";
            else if (currentActorName == "Peasant Girl") reactName = "Celebrate (Pointing)";
        }
        else if (reactionIndex == 3)
        {
            voiceAction = "special";
            if (currentActorName == "The Boss") reactName = "Special (Hard Head Nod)";
            else if (currentActorName == "Remy") reactName = "Special (React 3)";
            else if (currentActorName == "Peasant Girl") reactName = "Special (React 3)";
        }

        UpdateDirectorStatus(currentActorName, reactName);
        LogInteraction(currentActorName, reactName);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound(voiceAction, currentActorName);
        }

        if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject != null)
        {
            AnimateButtonPunch(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform);
        }
    }

    #endregion

    #region Settings Callbacks

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
        else
            AudioListener.volume = value;
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    public void OnMuteToggle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            if (muteButtonText != null)
            {
                muteButtonText.text = AudioManager.Instance.isMuted ? "UNMUTE AUDIO" : "MUTE AUDIO";
            }
        }

        if (UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject != null)
        {
            AnimateButtonPunch(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform);
        }
    }

    #endregion
}
