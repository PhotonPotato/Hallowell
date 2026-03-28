using UnityEngine;

public class BloodLeechLungingState : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BloodLeechBehavior behavior = animator.GetComponent<BloodLeechBehavior>();
    }
}
