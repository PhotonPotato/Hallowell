using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/Items/MaterialItem")]
public class MaterialItem : ItemData
{
    public void init(string name, Sprite icon, int id, int quantity, int maxQuantity, bool stackable, bool empty = true, int itemType = 0)
    {
        this.itemName = name;
        this.itemIcon = icon;
        this.itemType = itemType;
        this.id = id;
        this.quantity = quantity;
        this.maxQuantity = maxQuantity;
        this.stackable = stackable;
        this.empty = empty;
    }

    public MaterialItem getDeepCopy()
    {
        MaterialItem temp = CreateInstance("MaterialItem") as MaterialItem;
        temp.init(itemName, itemIcon, id, quantity, maxQuantity, stackable, empty);
        return temp;
    }
}
