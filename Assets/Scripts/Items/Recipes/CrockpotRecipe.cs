using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/Items/CrockpotRecipe")]
public class CrockpotRecipe : ScriptableObject
{
    [System.Serializable]
    public struct materialItemArray
    {
        public MaterialItem[] row;
    }

    //Get a2D array in the editor :(
    public materialItemArray[] slots;

    public bool ignoreOrientation = false;

    public MaterialItem output;

    public int getNumberItems()
    {
        int numberItems = 0;

        //Go through all of the slots to get the amount of actual items in teh recipe
        foreach (materialItemArray array in slots)
        {
            foreach (MaterialItem item in array.row)
            {
                if(item != null) numberItems++;
            }
        }

        return numberItems;
    }
}
