using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
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

    public ItemType getItemType()
    {
        return itemType;
    }

    public bool compareTo(ItemData item)
    {
        if (item == null) return false;

        return (itemName == item.itemName && itemType == item.itemType && id == item.id);
    }
}
