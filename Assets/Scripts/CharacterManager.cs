using System.Collections;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Active Actor")]
    public Animator activeAnimator;

    [Header("Stage Positioning")]
    public Vector3 stagePosition = new Vector3(0f, -0.085f, 0f);
    public Quaternion stageRotation = Quaternion.Euler(0f, 180f, 0f);
    public bool lockToStage = true;

    private void Start()
    {
        if (activeAnimator == null)
        {
            var animators = FindObjectsByType<Animator>(FindObjectsInactive.Exclude);
            if (animators.Length > 0)
            {
                SetActiveActor(animators[0]);
            }
        }
    }

    private void LateUpdate()
    {
        // Keeps the active character on the center mark during actions
        if (lockToStage && activeAnimator != null)
        {
            activeAnimator.transform.position = stagePosition;
            activeAnimator.transform.rotation = stageRotation;
        }
    }

    public void SetActiveActor(Animator newAnimator)
    {
        activeAnimator = newAnimator;
        if (activeAnimator != null)
        {
            activeAnimator.applyRootMotion = false;
            activeAnimator.transform.position = stagePosition;
            activeAnimator.transform.rotation = stageRotation;
        }
    }

    public void ToggleWalk(bool walkState)
    {
        if (activeAnimator != null)
        {
            activeAnimator.applyRootMotion = false;
            activeAnimator.SetBool("isWalking", walkState);
        }
    }

    public void TriggerTalk()
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetTrigger("talkTrigger");
        }
    }

    public void TriggerReact(int reactIndex)
    {
        if (activeAnimator != null)
        {
            activeAnimator.SetInteger("reactionType", reactIndex);
            StartCoroutine(ResetReactionRoutine(activeAnimator));
        }
    }

    private IEnumerator ResetReactionRoutine(Animator anim)
    {
        // Reset reaction parameter so the state machine returns to idle after playing
        yield return null;
        yield return null;
        if (anim != null)
        {
            anim.SetInteger("reactionType", 0);
        }
    }
}
