using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS CLASS STORES DATA ABOUT THE WEILDER TO DETERMINE SPECIAL ANIMATIONS
public class WeaponHolderState
{
    public Vector2 velocity;

    //Maybe add "Effects" enum. Then:
    //public List<Effects> activeEffects;

    bool jumping;
    float timeOfLastJump;

    public int attackDirection = 0;

    
}
