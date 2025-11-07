using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedeemerBehavior : MonoBehaviour
{
    [Header("Refs")]
    Animator RedeemerAnimator;

    public RedeemerPriestBehavior redeemerPriestBehavior;

    public ParticleSystem SwordParticles;

    public RedeemerShockWaveController ShockwaveController;

    [Header("Boss Trackers")]
    public RedeemerBossState bossState = RedeemerBossState.Idle;

    [Header("Adversary Trackers")]
    public Transform AdversaryTransform;

    public float adversaryXDistance;

    public AnimationCurve AdversaryDistanceVsSwordSwingBlend;

    // Start is called before the first frame update
    void Start()
    {
        // Get the animator and other important components
        RedeemerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AdversaryTransform != null)
        {
            switch (bossState)
            {
                case RedeemerBossState.MakingSword:
                    // Anything special here
                    break;

                case RedeemerBossState.Idle:
                    // What the bost should do while idle
                    break;

                case RedeemerBossState.TargetedSwordSwingInit:
                    // Calculate the swing blend
                    RedeemerAnimator.SetFloat("SwordSwingBlend", GetSwordSwingBlendValue());

                    // Set trigger for sword swing
                    RedeemerAnimator.ResetTrigger("SwingSword");
                    RedeemerAnimator.SetTrigger("SwingSword");

                    bossState = RedeemerBossState.TargetedSwordSwing;
                    break;

                case RedeemerBossState.TargetedSwordSwing:
                    break;

                case RedeemerBossState.EndTargetedSwordSwing:
                    break;
            }
        }
    }

    /// <summary>
    /// Brings statue to life, initiating its make sword animation.
    /// </summary>
    public void Activate(Transform target)
    {
        // Set up the adversary
        AdversaryTransform = target;

        // Update this boss state
        bossState = RedeemerBossState.MakingSword;

        // Set active bool in animator to start sword making anim
        RedeemerAnimator.SetBool("Active", true);
    }

    /// <summary>
    /// Gets a value used to blend between the near and far sword swings based on the adversary's position.
    /// </summary>
    /// <returns>
    /// Defaults to 0 (near swing) if there is no adversary.
    /// Otherwise returns a float as described above.
    /// </returns>
    float GetSwordSwingBlendValue()
    {
        // Catch calls happening without an adversary present
        if (AdversaryTransform == null) return 0;

        // Get the x difference
        adversaryXDistance = Mathf.Abs(AdversaryTransform.position.x - transform.position.x);

        // Run that through a curve to remap the distance to a blend val
        return AdversaryDistanceVsSwordSwingBlend.Evaluate(adversaryXDistance);
    }

    /// <summary>
    /// Sets animation bool to init a low stab
    /// </summary>
    public void InitiateLowStab()
    {
        RedeemerAnimator.SetBool("LowStabbing", true);
    }

    public void EndLowStab()
    {
        RedeemerAnimator.SetBool("LowStabbing", false);
    }

    public void LowStabFullyExtended()
    {
        redeemerPriestBehavior.bossState = PriestBossState.PriestVulnerable;

        // Update the current boss state of this thang
        bossState = RedeemerBossState.LowStabExtended;
    }

    public void LowStabPullbackOver()
    {
        redeemerPriestBehavior.bossState = PriestBossState.Idle;

        // Update the current boss state of this thang
        bossState = RedeemerBossState.LowStabEnd;
    }

    public void InitiateTargetedSwing()
    {
        bossState = RedeemerBossState.TargetedSwordSwingInit;
    }

    public void EndTargetedSwordSwing()
    {
        // End the priests attacking state.
        redeemerPriestBehavior.bossState = PriestBossState.EndAttacking;
    }


    public void OnInitSlashShockwave()
    {
        ShockwaveController.InitNewShockWave(SwordParticles.transform.position, 40, .3f);
    }
}

public enum RedeemerBossState
{
    Inactive,
    MakingSword,
    Idle,
    TargetedSwordSwingInit,
    TargetedSwordSwing,
    EndTargetedSwordSwing,
    LowStabInit,
    LowStabExtended,
    LowStabEnd
}