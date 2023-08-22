using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{/*
    public int rows;
    public int columns;

    public MaterialItemSlot[,] inventory;

    void Start()
    {
        //Initialize the inventory.
        inventory = new MaterialItemSlot[rows, columns];

        //Initialize the individual slots.
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                inventory[i, j] = new MaterialItemSlot("", -1, 0, 0, false);
            }
        }
    }

    void Update()
    {
        
    }

    public bool placeInOpenSlot(MaterialItemSlot itemToAdd)
    {
        MaterialItemSlot tempSlot = itemToAdd.getDeepCopy();

        //Commence the search for simmilar items
        if (tempSlot.stackable)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    //Same id's
                    if (inventory[i, j].id == tempSlot.id)
                    {
                        //If there is space in the stack.
                        if (inventory[i, j].quantity < inventory[i, j].maxQuantity)
                        {
                            int spaceInStack = inventory[i, j].maxQuantity - inventory[i, j].quantity;

                            if (spaceInStack >= tempSlot.quantity)
                            {
                                //There is enough space.
                                inventory[i, j].quantity += tempSlot.quantity;
                                return true;
                            }
                            else
                            {
                                //There is not enough space.
                                //Now add that to the inventory and take it away from the tempSlot;
                                inventory[i, j].quantity += spaceInStack;
                                tempSlot.quantity -= spaceInStack;
                            }
                        }
                    }
                }
            }
        }

        //Now just search the qhole inventory for a empty slot.
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (inventory[i, j].empty)
                {
                    inventory[i, j] = tempSlot.getDeepCopy();
                    return true;
                }
            }
        }

        //If it gets to here, the inventory must be full.
        return false;
    }*/
}
