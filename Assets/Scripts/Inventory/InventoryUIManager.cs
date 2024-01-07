using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [System.NonSerialized]
    public PlayerInventory playerInventory;
    public PlayerManager playerMan;
    public Transform playerPos;

    public GameObject hotBarPanel;

    public GameObject InventoryPanel;
    public GameObject InventorySlotObject;
    public Transform inventorySlotsParent;
    public MaterialItemSlot[] materialItemSlots;

    public InventoryMouseManager mouseMan;

    public LayerMask itemLayer;
    public float attractionRadius;
    public float attractionForce;
    public float pickupRadius;

    public bool inventoryPanelOpen = false;
    [System.NonSerialized] public bool crockpotPanelOpen = false;

    Vector3 defaultHealthBarPos;

    void Start()
    {
        //materialItemSlots = inventorySlotsParent.GetComponentsInChildren<MaterialItemSlot>();
        playerInventory = new PlayerInventory(this);
        playerMan = FindObjectOfType<PlayerManager>();

        defaultHealthBarPos = playerMan.healthBarSlider.gameObject.transform.position;
    }

    void Update()
    {
        ItemDetectionHandler();

        playerMan.healthBarSlider.gameObject.transform.position = Camera.main.WorldToScreenPoint(Vector3.Scale(Camera.main.ScreenToWorldPoint(defaultHealthBarPos), new Vector3(1, inventoryPanelOpen ? -1 : 1, 1)));
    }

    public void OpenInventoryPanel()
    {
        Debug.Log("received signal");
        RefreshMaterialInventory();

        InventoryPanel.SetActive(true);
        hotBarPanel.SetActive(false);

        inventoryPanelOpen = true;

        //Move health bar down to not block items
    }

    public void CloseInventoryPanel()
    {
        InventoryPanel.SetActive(false);
        hotBarPanel.SetActive(true);
        mouseMan.mouseLabel.gameObject.SetActive(false);

        if (crockpotPanelOpen) playerMan.currentInteractionObject.GetComponent<CrockPotBehavior>().closeCrockPot(false);

        inventoryPanelOpen = false;

        mouseMan.OnInventoryClose();

        //Return health bar to original position

    }

    public void RefreshMaterialInventory()
    {
        materialItemSlots = inventorySlotsParent.GetComponentsInChildren<MaterialItemSlot>();

        Debug.Log("Len: " + materialItemSlots.Length + " player shitter: " + playerInventory.playerMaterialInventory.Count);

        //Check for the correct amount of slots.
        for (int i = GetSlotsLength(); i < playerInventory.playerMaterialInventory.Count; i++)
        {
            GameObject obj = Instantiate(InventorySlotObject, inventorySlotsParent);

            //Initialize a listener for the mouse hovering over the object trigger (i think)
            mouseMan.initListener(obj.GetComponentsInChildren<Transform>()[1].gameObject);
        }

        //Update list of item slots
        materialItemSlots = inventorySlotsParent.GetComponentsInChildren<MaterialItemSlot>();
    }

    public void UpdateMaterialInventorySlots()
    {
        for (int i = 0; i < GetSlotsLength(); i++)
        {
            materialItemSlots[i].AddIcon(playerInventory.playerMaterialInventory[i].getDeepCopy());
        }
    }

    public int GetSlotsLength()
    {
        return materialItemSlots.Length;
    }

    public void ItemDetectionHandler()
    {
        //Make items drift towards you in a certain radius.
        Collider2D[] itemsInAttraction = Physics2D.OverlapCircleAll(playerPos.position, attractionRadius, itemLayer);

        if (itemsInAttraction.Length == 0) return;

        foreach (Collider2D col in itemsInAttraction)
        {
            Rigidbody2D itemRigid;

            if (col.gameObject.TryGetComponent<Rigidbody2D>(out itemRigid))
            {
                //Make it able to move.
                if (itemRigid.bodyType == RigidbodyType2D.Static) itemRigid.bodyType = RigidbodyType2D.Dynamic;

                //Make items float toward player with an attraction force.
                Vector2 forceToApply = playerPos.position - col.gameObject.transform.position;
                forceToApply *= attractionForce;
                //itemRigid.angularDrag = 0;
                itemRigid.velocity += forceToApply;
            }
        }

        //Pickup detected items close to you.
        Collider2D[] itemsInRadius = Physics2D.OverlapCircleAll(playerPos.position, pickupRadius, itemLayer);

        if (itemsInRadius.Length == 0) return;
        
        foreach(Collider2D col in itemsInRadius)
        {
            //Filter for objects of the right tag
            if (col.gameObject.CompareTag("CollectableMaterialItem"))
            {
                //Add item to inventory list
                playerInventory.addItem(col.gameObject.GetComponent<MaterialItemSlot>().getItem());

                //Refresh the inventory so that the list of material item slots is updated.
                RefreshMaterialInventory();

                //Updates the icons and data of the actual gameObjects
                UpdateMaterialInventorySlots();

                //Destroys the Collectable item
                Destroy(col.gameObject);
                return;
            }
        }
    }
}
