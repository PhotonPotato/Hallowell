using UnityEngine;

public class BloodLeechLungingState : StateMachineBehaviour
{
    BloodLeechBehavior behavior;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        behavior = animator.GetComponent<BloodLeechBehavior>();

        behavior.CheckForPlayerCol();
    }
}
