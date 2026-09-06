using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 100f;
    
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float vertical = 0f;
        float horizontal = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        // Keyboard triggers for actions
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Space))
        {
            TriggerTalk();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            TriggerReaction(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            TriggerReaction(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            TriggerReaction(3);
        }
#else
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;

            if (Keyboard.current.tKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TriggerTalk();
            }
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                TriggerReaction(1);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                TriggerReaction(2);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                TriggerReaction(3);
            }
        }
#endif

        // Move forward / backward
        Vector3 move = transform.forward * vertical * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Rotate left / right
        transform.Rotate(Vector3.up * horizontal * turnSpeed * Time.deltaTime);

        // Update Animator parameter isWalking
        if (animator != null)
        {
            bool isWalking = Mathf.Abs(vertical) > 0.05f;
            animator.SetBool("isWalking", isWalking);
        }
    }

    public void TriggerTalk()
    {
        if (animator != null)
        {
            animator.SetTrigger("talkTrigger");
        }
    }

    public void TriggerReaction(int reactionIndex)
    {
        if (animator != null)
        {
            animator.SetInteger("reactionType", reactionIndex);
        }
    }
}
