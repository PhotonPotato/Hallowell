using UnityEngine;

public class BloodLeechPrepareState : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BloodLeechBehavior behavior = animator.GetComponent<BloodLeechBehavior>();

        behavior.timeOfLastLunge = Time.time;
        behavior.animatorLunging = true;
        behavior.LungeAtPlayer();
    }
}