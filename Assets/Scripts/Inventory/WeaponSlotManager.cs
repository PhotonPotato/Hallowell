using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponParent;
    public WeaponAnimationHandler weaponAnimationHandler;
    [NonSerialized] public PlayerCombatScript combatManager;

    [Header("Containers")]
    public WeaponItem activeWeaponItem;
    public GameObject currentWeaponSprite;

    [NonSerialized] 
    public Transform damageOriginPoint;

    private void Start()
    {
        weaponAnimationHandler = GetComponent<WeaponAnimationHandler>();
    }

    public void UnloadWeapon()
    {
        if (currentWeaponSprite != null)
        {
            currentWeaponSprite.SetActive(false);
        }
    }

    public void UnloadWeaponAndDestroy()
    {
        if (currentWeaponSprite != null)
        {
            Destroy(currentWeaponSprite);
        }
    }

    public void LoadWeaponModel(WeaponItem weaponItem)
    {
        UnloadWeaponAndDestroy();

        if (weaponItem == null)
        {
            UnloadWeapon();
            return;
        }

        activeWeaponItem = weaponItem;
        
        //PLEASE FUTURE TY CHANGE THE NAME AWAY FROM "WEAOPNSPIRITE" SPRITE DONT SOUND RIGHT
        GameObject weaponSprite = Instantiate(weaponItem.spritePrefab) as GameObject;
        if (weaponSprite != null)
        {
            if (weaponParent != null)
            {
                weaponSprite.transform.parent = weaponParent;
            }
            else
            {
                weaponSprite.transform.parent = transform;
            }

            //Make sure the scaling is correct
            weaponSprite.transform.localPosition = Vector3.zero;
            weaponSprite.transform.localRotation = Quaternion.identity;
            weaponSprite.transform.localScale = Vector3.one;
        }

        //Make sure to initialize the damage origin point
        damageOriginPoint = weaponSprite.GetComponentsInChildren<Transform>()[1];
        //CLEAN LATER SO THAT DAMAGE ORIGIN DOESNT HAVE TO BE FIRST CHILD
        if (damageOriginPoint.name != "DamageOrigin") damageOriginPoint = null;

        //Set up the weapons feedback script
        weaponSprite.GetComponent<WeaponObjectAnimationFeedback>().animationHandler = weaponAnimationHandler;

        currentWeaponSprite = weaponSprite;

        Debug.Log("Loaded");

        //Update the handler as well
        weaponAnimationHandler.InitNewWeapon();
    }

    public GameObject GetWeaponObject()
    {
        return currentWeaponSprite;
    }

    public WeaponItem GetWeaponItem()
    {
        return activeWeaponItem;
    }
}
