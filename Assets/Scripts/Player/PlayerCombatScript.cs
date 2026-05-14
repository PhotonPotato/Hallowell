using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombatScript : MonoBehaviour
{
    [Header("Refs")]
    public PlayerManager PlayerManager;
    public Transform AttackPoint;
    public Animator WeaponAnimator;

    [Header("Overall Settings")]
    public LayerMask enemyLayer;

    [Header("Weapon Settings")]
    public float weaponAttackDamage = 30f;
    public float weaponAttackRange = 2f;

    [Header("Trackers")]
    public bool dealingAttackDamage = false;
    public bool isAttacking = false;

    [Header("Effects")]
    public TrailRenderer swordSwingTipTrail;

    [NonSerialized] public List<EnemyHealthContainer> healthContainerCache;
    public List<IDamagable> damagedEnemyCache = new List<IDamagable>();

    private AttackDirection lastAttackDirection;

    private void Start()
    {
        PlayerManager = FindFirstObjectByType<PlayerManager>();

        healthContainerCache = new List<EnemyHealthContainer>();

        swordSwingTipTrail.emitting = false;
    }

    private void Update()
    {
        //Flip the swing to the proper side
        transform.localScale = new Vector3(PlayerManager.playerFacingRight ? 1 : -1, 1, 1);

        //Check for left click
        if (Input.GetMouseButtonDown(0))
        {
            //Only allow the player to swing if the inventory is closed
            if (!PlayerManager.playerInventoryManager.inventoryPanelOpen)
            {
                //Attack!

                //Run player animation/stop movement

                //Check the attack direction cases, prioritize vertical attacks
                float verticalInputRaw = Input.GetAxisRaw("Vertical");
                if (verticalInputRaw > .1f)
                {
                    WeaponAnimator.SetTrigger("AttackUp");
                    lastAttackDirection = AttackDirection.Up;

                    //Player upslash
                }
                else if (verticalInputRaw < -.1f && !PlayerManager.playerMovement.isOnGroundRaw)
                {
                    WeaponAnimator.SetTrigger("AttackDown");
                    lastAttackDirection = AttackDirection.Down;

                    //Player downslash
                }
                else
                {
                    //Then it must be a horizontal slash
                    WeaponAnimator.SetTrigger("AttackHorizontal");

                    if (PlayerManager.playerFacingRight)
                        lastAttackDirection = AttackDirection.Right;
                    else
                        lastAttackDirection = AttackDirection.Left;

                    //Player regular slash
                }
            }
        }

        //Check for a current attack
        if (dealingAttackDamage)
        {
            //Run an overlapp collision circle, filtering for enemy layer
            Collider2D[] cols = Physics2D.OverlapCircleAll(AttackPoint.position, weaponAttackRange, enemyLayer);

            //Check for valid colliders and their objects
            foreach (Collider2D col in cols)
            {
                /* DEPRICATED old code for damageable stuff
                 * REMOVE WHENEVER */
                #region old shart
                if (col.gameObject.TryGetComponent<EnemyHealthContainer>(out EnemyHealthContainer enemyHealthContainer))
                {
                    //Make sure this collider hasn't been damaged before
                    if (healthContainerCache.Contains(enemyHealthContainer)) return;

                    //Change this later btw MAKE THE DAMAGE AMT MORE NUANCED
                    enemyHealthContainer.DealDamage(weaponAttackDamage, lastAttackDirection, PlayerManager.playerMovement.playerRB.velocity);

                    //Save it in the cache for later
                    healthContainerCache.Add(enemyHealthContainer);

                    //Lastly send the slash feedback to the player movement to add player recoil
                    PlayerManager.playerMovement.EnactAttackRecoil(lastAttackDirection);

                    HitStopManager.Instance?.InitDefaultEasedHitStop();
                    ScreenShakeManager.Instance?.InitiateDefaultSinShake();
                }
                #endregion

                if (col.gameObject.TryGetComponent(out IDamagable damagableEnemy))
                {
                    //Make sure this collider hasn't been damaged before
                    if (damagedEnemyCache.Contains(damagableEnemy)) return;

                    DamageInfo damageInfo = new DamageInfo
                    {
                        amount = weaponAttackDamage,
                        direction = lastAttackDirection,
                        attacker = this.gameObject,
                        contactPoint = col.ClosestPoint(AttackPoint.position),
                        inheritedVelocity = PlayerManager.playerMovement.playerRB.velocity
                    };

                    damagableEnemy.DealDamage(damageInfo);

                    //Save it in the cache for later
                    damagedEnemyCache.Add(damagableEnemy);

                    //Lastly send the slash feedback to the player movement to add player recoil
                    PlayerManager.playerMovement.EnactAttackRecoil(lastAttackDirection);

                    HitStopManager.Instance?.InitDefaultEasedHitStop();
                    ScreenShakeManager.Instance?.InitiateDefaultSinShake();
                }
            }
        }

        // Keep track of if the current animation state is attacking
        isAttacking = !WeaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("WeaponIdle");
    }

    public void ResetCombatEnemyCache()
    {
        //Just clear the cache :)
        healthContainerCache.Clear();

        damagedEnemyCache.Clear();
    }

    /// <summary>
    /// Turns on dealing damage.
    /// Called midway through attack animation.
    /// </summary>
    public void OnTurnOnAttackDamage()
    {
        dealingAttackDamage = true;

        // Call the start of attack effects
        swordSwingTipTrail.emitting = true;
    }

    /// <summary>
    /// Called at end of attack animation.
    /// Turns off dealing attack damage and resets triggers.
    /// Should also turn off any slowness given to player.
    /// Clears cache of damaged colliders.
    /// </summary>
    public void OnTurnOffAttackDamage()
    {
        dealingAttackDamage = false;

        ResetCombatEnemyCache();

        WeaponAnimator.ResetTrigger("AttackUp");
        WeaponAnimator.ResetTrigger("AttackDown");
        WeaponAnimator.ResetTrigger("AttackHorizontal");

        // Turn off the attack effects
        swordSwingTipTrail.emitting = false;
    }

    public void OnDrawGizmosSelected()
    {
        if (!dealingAttackDamage) return;

        Gizmos.DrawSphere(AttackPoint.position, weaponAttackRange);
    }
}
