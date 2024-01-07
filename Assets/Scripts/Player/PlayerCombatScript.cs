using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatScript : MonoBehaviour
{
    [Header("References")]
    public Transform attackPoint;
    [NonSerialized] public PlayerManager playerManager;

    public WeaponAnimationHandler animationHandler;
    public WeaponSlotManager slotManager;

    [Header("Settings")]
    public LayerMask enemyLayer;
    public float attackRange;

    public WeaponItem defaultStartItem;

    [NonSerialized] public List<enemyHealthContainer> healthContainerCache;

    public void Start()
    {
        //Set up refs
        playerManager = FindObjectOfType<PlayerManager>();

        if (animationHandler == null) return;

        slotManager.LoadWeaponModel(defaultStartItem);

        //Update the slot manager witha  reference to this script
        slotManager.combatManager = this;
        animationHandler.combatManager = this;

        healthContainerCache = new List<enemyHealthContainer>();
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

        //Check for weapon active in swing
        if (animationHandler.attackDealingDamage)
        {
            //Run an overlapp collision circle, filtering for enemy layer
            Collider2D[] cols = Physics2D.OverlapCircleAll(slotManager.damageOriginPoint.position, slotManager.activeWeaponItem.attackRange, enemyLayer);

            //Check for valid colliders and their objects
            foreach (Collider2D col in cols)
            {
                enemyHealthContainer enemyHealthContainer;

                if (col.gameObject.TryGetComponent<enemyHealthContainer>(out enemyHealthContainer))
                {
                    //Make sure this collider hasn't been damaged before
                    if (healthContainerCache.Contains(enemyHealthContainer)) return;

                    //Change this later btw MAKE THE DAMAGE AMT MORE NUANCED
                    enemyHealthContainer.dealDamage(slotManager.activeWeaponItem.baseAttackDamage);

                    //Save it in the cache for later
                    healthContainerCache.Add(enemyHealthContainer);
                }
            }
        }
    }

    public void ResetCombatEnemyCache()
    {
        //Just clear the cache :)
        healthContainerCache.Clear();
    }

    public void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position , attackRange);
    }
}
