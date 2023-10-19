using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int itemType;
    public Sprite itemIcon;
    public int id;
    public int quantity;
    public int maxQuantity;
    public bool stackable;
    public bool empty;

    public string getItemName()
    {
        return itemName;
    }

    public int getItemType()
    {
        return itemType;
    }
}
