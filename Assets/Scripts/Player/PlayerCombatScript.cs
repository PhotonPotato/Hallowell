using System;
using System.Collections.Generic;
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

    [NonSerialized] public List<EnemyHealthContainer> healthContainerCache;

    private AttackDirection lastAttackDirection;

    private void Start()
    {
        PlayerManager = FindObjectOfType<PlayerManager>();

        healthContainerCache = new List<EnemyHealthContainer>();
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
                    lastAttackDirection = AttackDirection.Regular;

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
                EnemyHealthContainer enemyHealthContainer;

                if (col.gameObject.TryGetComponent<EnemyHealthContainer>(out enemyHealthContainer))
                {
                    //Make sure this collider hasn't been damaged before
                    if (healthContainerCache.Contains(enemyHealthContainer)) return;

                    //Change this later btw MAKE THE DAMAGE AMT MORE NUANCED
                    enemyHealthContainer.DealDamage(weaponAttackDamage);

                    //Save it in the cache for later
                    healthContainerCache.Add(enemyHealthContainer);

                    //Lastly send the slash feedback to the player movement to add player recoil
                    PlayerManager.playerMovement.EnactAttackRecoil(lastAttackDirection);
                }
            }
        }
    }

    public void ResetCombatEnemyCache()
    {
        //Just clear the cache :)
        healthContainerCache.Clear();
    }

    /// <summary>
    /// Turns on dealing damage.
    /// Called midway through attack animation.
    /// </summary>
    public void OnTurnOnAttackDamage()
    {
        dealingAttackDamage = true;
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
    }

    public void OnDrawGizmosSelected()
    {
        if (!dealingAttackDamage) return;

        Gizmos.DrawSphere(AttackPoint.position, weaponAttackRange);
    }

    /*
    [Header("References")]
    public Transform attackPoint;
    [NonSerialized] public PlayerManager playerManager;

    public WeaponAnimationHandler animationHandler;
    public WeaponSlotManager slotManager;

    [Header("Settings")]
    public LayerMask enemyLayer;
    public float attackRange;

    public WeaponItem defaultStartItem;


    public void Start()
    {
        //Set up refs
        playerManager = FindObjectOfType<PlayerManager>();

        if (animationHandler == null) return;

        slotManager.LoadWeaponModel(defaultStartItem);

        //Update the slot manager witha  reference to this script
        slotManager.combatManager = this;
        animationHandler.combatManager = this;

        healthContainerCache = new List<EnemyHealthContainer>();
    }

    public void Update()
    {
        //Make sure the inventory isnt open
        if (playerManager.playerInventoryManager.inventoryPanelOpen == false)
        {
            //Check for combat button ("left-click") input
            if (Input.GetMouseButtonDown(0))
            {
                //Actually try to attack
                animationHandler.TryAttack(AttackType.Light01, AttackDirection.Regular);
            }
        }

        //Update movement restriction in 

        //Check for weapon active in swing
        if (animationHandler.attackDealingDamage)
        {
            //Run an overlapp collision circle, filtering for enemy layer
            Collider2D[] cols = Physics2D.OverlapCircleAll(slotManager.damageOriginPoint.position, slotManager.activeWeaponItem.attackRange, enemyLayer);

            //Check for valid colliders and their objects
            foreach (Collider2D col in cols)
            {
                EnemyHealthContainer enemyHealthContainer;

                if (col.gameObject.TryGetComponent<EnemyHealthContainer>(out enemyHealthContainer))
                {
                    //Make sure this collider hasn't been damaged before
                    if (healthContainerCache.Contains(enemyHealthContainer)) return;

                    //Change this later btw MAKE THE DAMAGE AMT MORE NUANCED
                    enemyHealthContainer.DealDamage(slotManager.activeWeaponItem.baseAttackDamage);

                    //Save it in the cache for later
                    healthContainerCache.Add(enemyHealthContainer);
                }
            }
        }
    }



    */
}
