using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    public WeaponSlotManager slotManager;
    [NonSerialized] public Animator animator;
    [NonSerialized] public PlayerCombatScript combatManager;
    WeaponItem weaponItem;

    //This determines if the weapon is able to deal damage at the moment
    public bool attackDealingDamage = false;

    public void Start()
    {
        slotManager = GetComponent<WeaponSlotManager>();

        animator = slotManager.GetWeaponObject().GetComponent<Animator>();
    }

    public void InitNewWeapon()
    {
        if (slotManager == null) return;

        weaponItem = slotManager.GetWeaponItem();
    }

    //TRY ATTACK()
    public void TryAttack(AttackType attackType, AttackDirection direction)
    {
        if (weaponItem == null) return;

        //Check if the weapon handles directions
        if (weaponItem.WeaponTags.Contains(WeaponTag.Directional)) animator.SetInteger("Direction", (int) direction);

        //Check if weapon can handle this type of attack
        if (!weaponItem.WeaponAttacks.Contains(attackType)) return;

        switch (attackType)
        {
            //Handle light attacks
            case AttackType.Light01:
                animator.SetTrigger("Light01");
                break;
        }
    }

    //TRY INTERUPT ATTACK()

    //TRY DEAL DAMAGE

    //TRY STUN

    //Reset cached list of damaged enemies in a single swing
    //(So that an enemy can't be damaged twice)
    public void ResetCombatManagerSwingCache()
    {
        //Just pass the signal up the line to the next manager :) lol
        combatManager.ResetCombatEnemyCache();
    }
}
