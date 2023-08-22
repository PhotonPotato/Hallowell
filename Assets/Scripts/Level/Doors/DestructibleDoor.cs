using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class DestructibleDoor : MonoBehaviour
{
    public BoxCollider2D triggerZone;

    private void Start()
    {
        triggerZone = GetComponentsInChildren<BoxCollider2D>()[1];
    }

    public void Update()
    {
        List<Collider2D> colsInTrigger = new List<Collider2D>();

        if (triggerZone.GetContacts(colsInTrigger) != 0)
        {
            foreach (Collider2D col in colsInTrigger)
            {
                if (col.gameObject.tag == "Player")
                {
                    openDoor();
                }
            }
        }
    }

    void openDoor()
    {
        ///NOTE
        ///Add here later
        ///-Particles
        ///-Sound
        ///Dakine lol
        
        Destroy(this.gameObject);
    }
}
