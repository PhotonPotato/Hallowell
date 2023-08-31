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

    SpriteRenderer maskRenderer;
    Color maskColor;

    bool playerDetected;
    bool playerDetectedHistory;

    private void Start()
    {
        maskRenderer = GetComponent<SpriteRenderer>();
        maskColor = maskRenderer.color;
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

        maskRenderer.color = maskColor;
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
