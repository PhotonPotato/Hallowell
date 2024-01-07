using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeaponObjectAnimationFeedback : MonoBehaviour
{
    public WeaponAnimationHandler animationHandler;

    //Set the animationHandler's "attackDealingDamage" to true
    public void WeaponDamageActive()
    {
        if (animationHandler == null) return;

        animationHandler.attackDealingDamage = true;
    }

    //Set the animationHandler's "attackDealingDamage" to false
    public void WeaponDamageInactive()
    {
        if (animationHandler == null) return;

        animationHandler.attackDealingDamage = false;
    }

    //Tells other code to reset all the tracked objs damaged during a swing
    public void ResetDamagedEnemyCache()
    {
        animationHandler.ResetCombatManagerSwingCache();
    }
}
