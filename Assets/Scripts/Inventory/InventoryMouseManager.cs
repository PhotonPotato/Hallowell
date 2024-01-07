using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryMouseManager : MonoBehaviour
{
    public InventoryUIManager UIMan;

    Vector3 mousePos;
    public Vector3 labelPosOffset;
    public Text mouseLabel;
    public LineRenderer mouseLine;
    bool onSlot = false;
    bool onCrockpotSlot = false;
    //For slots that cannot have things placed in but only taken
    public bool onOutputSlot = false;
    GameObject closestSlot;

    public bool mouseContainerFull = false;
    public MaterialItemSlot mouseItemContainer;
    public Image mouseContainerImage;
    public GameObject dragBackButtonObject;

    private void Start()
    {
        mouseItemContainer = GetComponent<MaterialItemSlot>();

        mouseItemContainer.icon = mouseContainerImage;
    }

    private void Update()
    {
        //CLEAN
        //print(UIMan.playerInventory.playerMaterialInventory.Count);
        mousePos = Input.mousePosition + labelPosOffset;

        mouseLabel.gameObject.transform.position = mousePos;

        //Update where the image sits nad its visibility
        mouseContainerImage.gameObject.transform.position = mousePos;
        mouseContainerImage.gameObject.SetActive(mouseContainerFull && UIMan.inventoryPanelOpen);

        //SIMPLE HACK BC I GOT NO WIIF
        //CHANGE THIS TO MAKE THE BUTTON SEE THROUGH
        dragBackButtonObject.SetActive(mouseContainerFull && !onSlot);

        if (onSlot)
        {
            Vector3[] positions = new Vector3[2];

            positions[0] = Camera.main.ScreenToWorldPoint(mousePos);
            positions[0].x -= .03f;
            positions[0].y += .38f;
            if (closestSlot != null) positions[1] = Camera.main.ScreenToWorldPoint(closestSlot.gameObject.transform.position);

            mouseLine.SetPositions(positions);
        }

        //Look for clicks on the inventory objects
        if (onSlot && Input.GetMouseButtonDown(0))
        {
            /* REMOVE
            Debug.Log("Clicked");

            //DEBUG
            foreach (MaterialItem j in UIMan.playerInventory.playerMaterialInventory)
            {
                Debug.Log(j.itemName);
            }
            */
            //Initiate the slot movement
            MaterialItemSlot slot = closestSlot.GetComponentInParent<MaterialItemSlot>();

            //CHANGE THIS LATER TOO LAZY RN
            if (slot == null) return;

            if (slot.item != null) Debug.Log(slot.item.itemIcon != null);

            //First check if the slot item exist
            if (slot.item == null)
            {
                ///AddIcon() is an awful name for a function that effectively just resets a slot to a material item

                //Make sure you are not trying to place stuff into a strictly output slot
                if (mouseContainerFull && !onOutputSlot)
                {
                    //Drop the item in the container off
                    slot.AddIcon(mouseItemContainer.getItem());

                    //Reset the container
                    mouseItemContainer.ClearSlot(false);
                    mouseContainerFull = false;
                }
            }
            else
            {
                //Closest slot is full

                if (mouseContainerFull && !onOutputSlot)
                {
                    //Swap the item in the slot and the one in the container
                    //Do this by creating a temporary copy with no pointers to existing MatItemSlot vars. 
                    MaterialItem tempSwapItem = slot.getItem();

                    slot.AddIcon(mouseItemContainer.item.getDeepCopy());

                    mouseItemContainer.AddIcon(tempSwapItem);
                }
                else
                {
                    //Collect the item and put it into the mouseContainer
                    mouseItemContainer.AddIcon(slot.getItem());

                    slot.ClearSlot(false);

                    mouseLabel.gameObject.SetActive(false);
                    mouseLine.gameObject.SetActive(false);

                    if (!onCrockpotSlot & !onOutputSlot)
                    {
                        int slotIndex = slot.transform.GetSiblingIndex();

                        Debug.Log("removing at " + slotIndex);

                        //Remove item references from both the playerInventory (most reliable list of all players items)
                        //and the InventoryManagers 'materialItemSlots' list that holds all actual gameObject component
                        //references.The latter can be dont by deleteing the actual gameobject and then calling an update
                        //function to update the list to GetComponentsInChildren tyoe thing
                        UIMan.playerInventory.removeItem(slotIndex);

                        //Delete the existing gameobject slot
                        Destroy(slot.gameObject);

                        //Update variables and slot displays
                        UIMan.RefreshMaterialInventory();

                        //Create a new array deleting this item out of the material item slot array
                        MaterialItemSlot[] editedMaterialItemSlotArray = new MaterialItemSlot[UIMan.materialItemSlots.Length - 1];
                        for (int i = 0, j = 0; i < UIMan.materialItemSlots.Length - 1; i++)
                        {
                            if (i != slotIndex)
                            {
                                editedMaterialItemSlotArray[j] = UIMan.materialItemSlots[i];
                                j++;
                            }
                        }
                        UIMan.materialItemSlots = editedMaterialItemSlotArray;

                        //onMouseOff never updates if the slot is destroyed so we [effectively] do it for them (just what matters in the funciton)
                        onSlot = false;
                    }
                }

                mouseContainerFull = true;
            }
        }
    }


    public void initListener(GameObject obj, int type  = 0)
    {
        //This script will only work if the object has a button.
        if (obj.GetComponentInChildren<Button>() == null) return;

        EventTrigger.Entry mousePointerEnter = new EventTrigger.Entry();
        mousePointerEnter.eventID = EventTriggerType.PointerEnter;
        //make it so that when this event triggers, it will send the "updatePointerText" finction a string containing the name of the item.
        mousePointerEnter.callback.AddListener((eventData) => { mouseOnSlot(obj); });

        EventTrigger.Entry mousePointerExit = new EventTrigger.Entry();
        mousePointerExit.eventID = EventTriggerType.PointerExit;
        //make it so that when this event triggers, it will send the "updatePointerText" finction a string containing the name of the item.
        mousePointerExit.callback.AddListener((eventData) => { mouseOffSlot(obj); });

        EventTrigger trigger = obj.AddComponent<EventTrigger>();
        trigger.triggers.Add(mousePointerEnter);
        trigger.triggers.Add(mousePointerExit);
    }

    public void mouseOnSlot(GameObject obj) 
    {
        MaterialItemSlot slot = obj.GetComponentInParent<MaterialItemSlot>();
        MaterialItem item = slot.item;
        
        //Make sure its not just an empty crockpotslot
        if (obj.name.Contains("Crockpot Slot"))
        {
            onCrockpotSlot = true;
        }

        onOutputSlot = obj.tag == "UIOutputSlot";

        //Update the closest slot and indicator
        closestSlot = obj;
        onSlot = true;

        //Make sure the item exists
        if (item == null) return;
        if (item.getItemName() == null) return;

        mouseLabel.text = item.getItemName();
        mouseLabel.gameObject.SetActive(true);
        
        mouseLine.gameObject.SetActive(true);
    }

    public void mouseOffSlot(GameObject obj)
    {
        //Make sure its not just an empty crockpotslot
        if (obj.name.Contains("Crockpot Slot"))
        {
            onCrockpotSlot = false;
        }

        //Might be unnecessary to set these.
        onOutputSlot = false;

        //Update the closest slot and indicator
        closestSlot = obj;
        onSlot = false;

        //No need to check if item exists, just hide the label and line
        mouseLabel.gameObject.SetActive(false);
        mouseLine.gameObject.SetActive(false);
    }

    public void inventoryDragBackClick()
    {
        if (mouseContainerFull && !onSlot)
        {
            //Ass new item to the inventory
            Debug.Log("yes");
            UIMan.playerInventory.addItem(mouseItemContainer.item);

            UIMan.RefreshMaterialInventory();
            UIMan.UpdateMaterialInventorySlots();

            //Reset the container
            mouseItemContainer.ClearSlot(false);
            mouseContainerFull = false;
        }
    }

    public void OnInventoryClose()
    {
        if (mouseContainerFull)
        {
            UIMan.playerInventory.addItem(mouseItemContainer.item);

            UIMan.RefreshMaterialInventory();
            UIMan.UpdateMaterialInventorySlots();

            //Reset the container
            mouseItemContainer.ClearSlot(false);
            mouseContainerFull = false;
        }
    }
}
