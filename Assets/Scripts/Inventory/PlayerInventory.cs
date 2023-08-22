using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    public List<MaterialItem> playerMaterialInventory;
    public InventoryUIManager UIManager;

    public PlayerInventory(InventoryUIManager UIMan)
    {
        UIManager = UIMan;

        //Init inventory
        playerMaterialInventory = new List<MaterialItem>();
    }

    public void addItem(MaterialItem item)
    {
        playerMaterialInventory.Add(item.getDeepCopy());
    }
}
