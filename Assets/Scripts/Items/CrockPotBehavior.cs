using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrockPotBehavior : MonoBehaviour
{
    public GameObject inventoryManagerObject;
    InventoryUIManager playerInventoryUIMan;
    InventoryMouseManager mouseManager;
    public bool panelEnabled;

    public int rows = 3;
    public int columns = 3;
    public MaterialItem[,] slots;

    public GameObject crockpotUIParent;
    public GameObject crockpotSlotGridParent;
    GameObject openButtonObject;
    MaterialItemSlot[] slotObjs;

    public GameObject outputSlot;

    public MaterialItem testitem;

    public CrockpotRecipe testRecipe;
    
    private void Start()
    {
        mouseManager = inventoryManagerObject.GetComponent<InventoryMouseManager>();
        playerInventoryUIMan = inventoryManagerObject.GetComponent<InventoryUIManager>();

        openButtonObject = GetComponentInChildren<Button>().gameObject;

        init();
    }

    
    private void Update()
    {
        //Place for future optimizations
        for (int i = 0; i < slotObjs.Length; i++)
        {
            if (slotObjs[i].item == null)
            {
                slots[(int)Mathf.Ceil(i / rows), i % rows] = null;
                //Debug.Log(new Vector2((int)Mathf.Ceil(i / rows), i % rows));
                continue;
            }

            slots[(int)Mathf.Ceil(i / rows), i % rows] = slotObjs[i].getItem();
        }
    }

    public void init()
    {
        //Initailize the new 2D array of slots for the crocpot
        slots = new MaterialItem[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                slots[i, j] = null;
            }
        }

        //Get all the objs
        slotObjs = crockpotSlotGridParent.GetComponentsInChildren<MaterialItemSlot>();

        foreach (MaterialItemSlot item in slotObjs)
        {
            mouseManager.initListener(item.gameObject);
        }

        //Init the output
        mouseManager.initListener(outputSlot);
        //NOT WORKING outputSlot.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnCookButtonClicked(); } );
    }

    public bool addToSlot(MaterialItem item, int row = -1, int column = -1)
    {
        if (row == -1 || column == -1)
        {
            //Look for the first open slot
            Vector2 openSlot = findNearestOpenSlot();
            //Return if there is no open slot for new items
            if (openSlot == Vector2.negativeInfinity) return false;

            row = (int)openSlot.x;
            column = (int)openSlot.y;
        }

        slots[row, column] = item;
        return true;
    }

    public Vector2 findNearestOpenSlot()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (slots[i, j] == null)
                {
                    return new Vector2(i, j);
                }
            }
        }

        return Vector2.negativeInfinity;
    }

    public bool compareToRecipe(CrockpotRecipe recipe)
    {
        int numItemsInRecipe = recipe.getNumberItems();

        //Cache already used indices of the recipe
        List<Vector2Int> iteratedIndices = new List<Vector2Int>();
        int numItemsInSlots = 0;
        int numMatchingItems = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                //Search each slot in the slots.

                if (slots[i, j] == null) continue;
                else numItemsInSlots++;

                if (recipe.ignoreOrientation)
                {
                    //Just check that the right items are here then

                    //For each slot, look at all of the recipe slots
                    for (int k = 0; k < recipe.slots.Length; k++)
                    {
                        for (int l = 0; l < recipe.slots[k].row.Length; l++)
                        {
                            //Check if this index is in the list or if its an empty slot
                            if (iteratedIndices.Contains(new Vector2Int(k, l)) || recipe.slots[k].row[l] == null) continue;

                            //Check if the item matches
                            if (slots[i, j].compareTo(recipe.slots[k].row[l]))
                            {
                                Debug.Log("Item Match! at " + k + ", " + l);
                                //Add this index to the iterated indicies
                                iteratedIndices.Add(new Vector2Int(k, l));
                                numMatchingItems++;
                            }
                        }
                    }
                }
                else
                {
                    //Check if the corresponding slot is the same
                    if (slots[i, j].compareTo(recipe.slots[i].row[j]))
                    {
                        Debug.Log("Item Match! at " + i + ", " + j);
                        numMatchingItems++;
                    }
                }
            }
        }

        //If they were all matching and there are no extra items.
        if (numItemsInSlots == numItemsInRecipe && numMatchingItems == numItemsInRecipe)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void OnCookButtonClicked()
    {
        Debug.Log("clicked as");

        if (outputSlot.GetComponent<MaterialItemSlot>().item != null) return;

        if (compareToRecipe(testRecipe))
        {
            Debug.Log("true");
            outputSlot.GetComponent<MaterialItemSlot>().AddIcon(testRecipe.output);

            clearAllCrockpotSlots();
        }
    }

    public void clearAllCrockpotSlots()
    {
        foreach (MaterialItemSlot slot in slotObjs)
        {
            slot.ClearSlot(false);
        }
    }

    public void openCrockpot(bool openInventoryPanel = true)
    {
        crockpotUIParent.SetActive(true);

        openButtonObject.SetActive(false);

        if (openInventoryPanel) playerInventoryUIMan.openInventoryPanel();
        playerInventoryUIMan.crockpotPanelOpen = true;
        FindObjectOfType<PlayerManager>().currentInteractionObject = this.gameObject;

        //Play sound/animation for polish
    }

    public void closeCrockPot(bool closeInventoryPanel = true)
    {
        //Put all remaining items back into the inventory dand refresh lists and displays
        foreach (MaterialItemSlot slot in slotObjs)
        {
            //Weed out empty slots
            if (slot.item == null) continue;

            playerInventoryUIMan.playerInventory.addItem(slot.item);

            slot.ClearSlot(false);
        }

        MaterialItemSlot outputItemSlot = outputSlot.GetComponent<MaterialItemSlot>();
        if (outputItemSlot.item != null)
        {
            playerInventoryUIMan.playerInventory.addItem(outputItemSlot.item);

            outputItemSlot.ClearSlot(false);
        }

        playerInventoryUIMan.refreshMaterialInventory();
        playerInventoryUIMan.updateMaterialInventorySlots();

        crockpotUIParent.SetActive(false);

        openButtonObject.SetActive(true);

        if (closeInventoryPanel) playerInventoryUIMan.closeInventoryPanel();
        playerInventoryUIMan.crockpotPanelOpen = false;

        //Play sound/animation for polish
    }
}
