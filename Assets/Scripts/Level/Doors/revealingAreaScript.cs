using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class revealingAreaScript : MonoBehaviour
{
    public bool reHideOnPlayerExit = false;
    public float alphaChangeRate = 1;

    [Space]
    public bool playSoundOnReveal;
    public AudioSource revealAudio;

    SpriteRenderer renderer;
    Color maskColor;

    bool playerDetected;
    bool playerDetectedHistory;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        maskColor = renderer.color;
    }

    public void Update()
    {
        if (playerDetected)
        {
            if (maskColor.a > 0)
            {
                maskColor.a -= alphaChangeRate * Time.deltaTime;
            }
        }
        else if (reHideOnPlayerExit)
        {
            if (maskColor.a < 1)
            {
                maskColor.a += alphaChangeRate * Time.deltaTime;
            }
        }
        else if (playerDetectedHistory)
        {
            Destroy(this.gameObject);
        }

        renderer.color = maskColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerDetected = true;

            //Check if this is the first time of revealing the area
            if (!playerDetectedHistory)
            {
                //If not, play a sound
                if (playSoundOnReveal) revealAudio.Play();
            }
            else playerDetectedHistory = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerDetected = false;
        }
    }
}
