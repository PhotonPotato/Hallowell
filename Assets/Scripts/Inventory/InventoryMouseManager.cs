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
    GameObject closestSlot;

    private void Update()
    {
        mousePos = Input.mousePosition + labelPosOffset;

        mouseLabel.gameObject.transform.position = mousePos;

        if (onSlot)
        {
            Vector3[] positions = new Vector3[2];

            positions[0] = Camera.main.ScreenToWorldPoint(mousePos);
            positions[0].x -= .03f;
            positions[0].y += .38f;
            if (closestSlot != null) positions[1] = Camera.main.ScreenToWorldPoint(closestSlot.gameObject.transform.position);

            mouseLine.SetPositions(positions);
        }
    }


    public void initListener(GameObject obj)
    {
        //This script will only work if the object has a button.
        if (obj.GetComponent<Button>() == null) return;

        EventTrigger.Entry mousePointerEnter = new EventTrigger.Entry();
        mousePointerEnter.eventID = EventTriggerType.PointerEnter;
        //make it so that when this event triggers, it will send the "updatePointerText" finction a string containing the name of the item.
        mousePointerEnter.callback.AddListener((eventData) => { mouseOnSlot(obj); });

        EventTrigger.Entry mousePointerExit = new EventTrigger.Entry();
        mousePointerExit.eventID = EventTriggerType.PointerExit;
        //make it so that when this event triggers, it will send the "updatePointerText" finction a string containing the name of the item.
        mousePointerExit.callback.AddListener((eventData) => { mouseOffSlot(obj); });

        obj.AddComponent<EventTrigger>();
        obj.GetComponent<EventTrigger>().triggers.Add(mousePointerEnter);
        obj.GetComponent<EventTrigger>().triggers.Add(mousePointerExit);
    }

    public void mouseOnSlot(GameObject obj) 
    {
        string text = obj.GetComponentInParent<MaterialItemSlot>().item.getItemName();

        if (text == null) return;

        mouseLabel.text = text;
        mouseLabel.gameObject.SetActive(true);

        closestSlot = obj;
        onSlot = true;
        mouseLine.gameObject.SetActive(true);
    }

    public void mouseOffSlot(GameObject obj)
    {
        string text = obj.GetComponentInParent<MaterialItemSlot>().item.getItemName();

        if (text == null) return;

        mouseLabel.gameObject.SetActive(false);

        closestSlot = obj;
        onSlot = false;
        mouseLine.gameObject.SetActive(false);
    }
}
