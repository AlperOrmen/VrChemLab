using UnityEngine;
using UnityEngine.InputSystem;
public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;
    public Animator handAnimator;
    void Start()
    {
        
    }

    void Update()
    {
        float triggervalue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggervalue);

        float gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
    }
}
