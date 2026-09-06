using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputManager : MonoBehaviour
{
    public CharacterManager characterManager;

    private void Start()
    {
        if (characterManager == null)
        {
            characterManager = FindAnyObjectByType<CharacterManager>();
        }
    }

    private void Update()
    {
        if (characterManager == null) return;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            // Spacebar: Talk
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TriggerTalkAction();
            }

            // W key: Walk in place
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                ToggleWalkAction(true);
            }
            if (Keyboard.current.wKey.wasReleasedThisFrame)
            {
                ToggleWalkAction(false);
            }

            // Keys 1-3: Reactions
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                TriggerReactAction(1);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                TriggerReactAction(2);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                TriggerReactAction(3);
            }

            // Tab: Cycle characters
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                CycleActorAction();
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerTalkAction();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ToggleWalkAction(true);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            ToggleWalkAction(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            TriggerReactAction(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            TriggerReactAction(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            TriggerReactAction(3);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleActorAction();
        }
#endif
    }

    private void ToggleWalkAction(bool walk)
    {
        characterManager.ToggleWalk(walk);
        string actor = GetActiveActorName();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDirectorStatus(actor, walk ? "Walking" : "Idle");
            UIManager.Instance.LogInteraction(actor, walk ? "Walk Started (Key W)" : "Walk Stopped");
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound(walk ? "walk_start" : "walk_stop", actor);
        }
    }

    private void TriggerTalkAction()
    {
        characterManager.TriggerTalk();
        string actor = GetActiveActorName();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDirectorStatus(actor, "Talking");
            UIManager.Instance.LogInteraction(actor, "Talk Triggered (Spacebar)");
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound("talk", actor);
        }
    }

    private void TriggerReactAction(int index)
    {
        characterManager.TriggerReact(index);
        string actor = GetActiveActorName();
        string reactLabel = $"Reaction {index}";
        string voiceAction = "react";
        if (index == 2)
        {
            voiceAction = "celebrate";
            reactLabel = actor == "Remy" ? "Celebrate (Silly Dance)" : (actor == "The Boss" ? "Celebrate (Waving)" : "Celebrate (Pointing)");
        }
        else if (index == 3)
        {
            voiceAction = "special";
            reactLabel = actor == "The Boss" ? "Special (Hard Head Nod)" : "Special (React 3)";
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDirectorStatus(actor, reactLabel);
            UIManager.Instance.LogInteraction(actor, $"{reactLabel} (Key {index})");
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayActionSound(voiceAction, actor);
        }
    }

    private int currentActorCycle = 0;
    private void CycleActorAction()
    {
        currentActorCycle = (currentActorCycle + 1) % 3;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnSelectActor(currentActorCycle);
        }
    }

    private string GetActiveActorName()
    {
        if (characterManager != null && characterManager.activeAnimator != null)
            return characterManager.activeAnimator.gameObject.name;
        return "The Boss";
    }
}
