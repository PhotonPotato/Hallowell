using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchRoomDoor : MonoBehaviour
{
    public BoxCollider2D entryTrigger;
    public Animator doorAnimator;

    public bool doorUp = false;
    bool statesComplete = false;

    public List<GameObject> enemies;

    // Start is called before the first frame update
    void Start()
    {
        entryTrigger = GetComponentsInChildren<BoxCollider2D>()[0];
        doorAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (statesComplete) return;

        //Weed out empty gameobjects from the enemy list (might wanna clean this up in the morning)
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
                i--;
            }
        }

        if (!doorUp)
        {
            //Check for players trying to walk through arena and shut the door on them.
            if (isPlayerInTrigger(entryTrigger))
            {
                onDoorClose();
                doorUp = true;
            }
        }
        else
        {
            //If there are no enemies in the arena then open the exit.
            if (enemies.Count == 0)
            {
                onDoorOpen();
                doorUp = false;
                statesComplete= true;
            }
        }
    }

    bool isPlayerInTrigger(BoxCollider2D triggerZone)
    {
        List<Collider2D> colsInTrigger = new List<Collider2D>();

        if (triggerZone.GetContacts(colsInTrigger) != 0)
        {
            foreach (Collider2D col in colsInTrigger)
            {
                if (col.gameObject.tag == "Player")
                {
                    return true;
                }
            }
        }

        return false;
    }

    void onDoorClose()
    {
        doorAnimator.SetTrigger("Entry");

        //This is where you play sounds and shit
    }

    void onDoorOpen()
    {
        doorAnimator.SetTrigger("Exit");

        //this is where victory music plays
    }
}
