using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Items/WeaponItem")]
public class WeaponItem : ItemData
{
    public GameObject spritePrefab;
    public bool isUnarmed;
    public float attackRange = 2f;

    public float baseAttackDamage = 15f;

    public List<WeaponTag> WeaponTags;
    public List<AttackType> WeaponAttacks;

    ///Game plan:
    ///SpritePrefab holds a whole ass gameobject with an animated weaopn (with swing and allat)
    ///Instantiate that prefab when using selected weapon
    ///Call THAT prefabs anuimator
    ///boom, different weapons from items
}
