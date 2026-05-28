using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    
}

public enum ItemType
{
    MaterialItem,
    WeaponItem,
    HealthItem
}

public enum AttackType
{
    Light01
}

//For when swinging in different directions
public enum AttackDirection
{
    Regular,
    Right,
    Left,
    Up,
    Down
}

public enum WeaponTag
{
    Directional,
    LightAttack,
    HeavyAttack
}

// TODO: just delete this bullshit, clean it up
public enum HealthContainerType
{
    Player,
    Enemy,
    ProceduralQuadropedLeg
}

public enum EnemyTag
{
    Light,
    Staggerable,
    Etc
}

public enum ScreenShakeType
{
    Default,
    Hit,
    Explosion
}