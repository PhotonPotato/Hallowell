using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [System.NonSerialized]
    public PlayerInventory playerInventory;
    public Transform playerPos;

    public GameObject hotBarPanel;

    public GameObject InventoryPanel;
    public GameObject InventorySlotObject;
    public Transform inventorySlotsParent;
    MaterialItemSlot[] materialItemSlots;

    public InventoryMouseManager mouseMan;

    public LayerMask itemLayer;
    public float attractionRadius;
    public float attractionForce;
    public float pickupRadius;

    void Start()
    {
        materialItemSlots = inventorySlotsParent.GetComponentsInChildren<MaterialItemSlot>();
        playerInventory = new PlayerInventory(this);
    }

    void Update()
    {
        itemDetectionHandler();
    }

    public void openInventoryPanel()
    {
        refreshMaterialInventory();

        InventoryPanel.SetActive(true);
        hotBarPanel.SetActive(false);
    }

    public void closeInventoryPanel()
    {
        InventoryPanel.SetActive(false);
        hotBarPanel.SetActive(true);
        mouseMan.mouseLabel.gameObject.SetActive(false);
    }

    public void refreshMaterialInventory()
    {
        //Check for the correct amount of slots.
        for (int i = getSlotsLength(); i < playerInventory.playerMaterialInventory.Count; i++)
        {
            GameObject obj = Instantiate(InventorySlotObject, inventorySlotsParent);

            //Initialize a listener for the mouse hovering over the object trigger (i think)
            mouseMan.initListener(obj.GetComponentsInChildren<Transform>()[1].gameObject);
        }

        materialItemSlots = inventorySlotsParent.GetComponentsInChildren<MaterialItemSlot>();
    }

    public void updateMaterialInventorySlots()
    {
        for (int i = 0; i < getSlotsLength(); i++)
        {
            materialItemSlots[i].AddIcon(playerInventory.playerMaterialInventory[i]);
        }
    }

    public int getSlotsLength()
    {
        return materialItemSlots.Length;
    }

    public void itemDetectionHandler()
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
            if (col.gameObject.tag == "CollectableMaterialItem")
            {
                playerInventory.addItem(col.gameObject.GetComponent<MaterialItemSlot>().item);
                refreshMaterialInventory();
                updateMaterialInventorySlots();
                Destroy(col.gameObject);
                return;
            }
        }
    }
}
