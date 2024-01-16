using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/Items/HealthItem")]
public class HealthItem : ItemData
{
    //Effects ->

    public float healthHealed = 10;
}
