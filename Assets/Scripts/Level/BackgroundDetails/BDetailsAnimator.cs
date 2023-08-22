using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BDetailsAnimator : MonoBehaviour
{
    public SpriteRenderer sprite;

    public List<Sprite> idle;
    public List<Sprite> touched;
    public List<Sprite> cut;

    public float idleSpeed;
    public float touchedSpeed;
    public float cutSpeed;
    public bool justTouched;
    public bool detailCut;

    int state;
    int animFrame;
    /// States!!!
    /// 0 = idle
    /// 1 = touched
    /// 2 = cutting
    /// 3 = cut

    float nextFrameTimer;
    
    private void Start()
    {
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (nextFrameTimer > 0) nextFrameTimer -= Time.deltaTime;

        switch (state)
        {
            case 0:
                if (nextFrameTimer <= 0)
                {
                    if (animFrame < idle.Count) sprite.sprite = idle[animFrame];
                    nextFrameTimer = idleSpeed;
                    animFrame++;
                    if (animFrame >= idle.Count) frameReset();
                }
                break;

            case 1:
                if (nextFrameTimer <= 0)
                {
                    if (animFrame < touched.Count) sprite.sprite = touched[animFrame];
                    nextFrameTimer = touchedSpeed;
                    animFrame++;
                    if (animFrame >= touched.Count)
                    {
                        frameReset();
                        state = 0;

                        justTouched = false;
                    }
                }
                break;

            case 2:
                if (animFrame < cut.Count) sprite.sprite = cut[animFrame];

                if (nextFrameTimer <= 0)
                {
                    if (cut.Count - 1 <= animFrame)
                    {
                        animFrame++;
                        nextFrameTimer = cutSpeed;
                    }
                    else
                    {
                        state = 3;
                        frameReset();

                        detailCut = false;
                    }
                }

                break;

            case 3:
                sprite.sprite = cut[cut.Count - 1];
                break;

        }

        //DEBUG TIMMMMEEEEE!!!!!!!!
        if (detailCut && state != 2 && state != 3) playerCut();

        if (justTouched && state != 1) playerTouched();
    }

    public void frameReset()
    {
        animFrame = 0;
    }

    public void playerTouched()
    {
        if (state == 1) return;

        nextFrameTimer = 0;
        //frameReset();
        state = 1;
    }

    public void playerCut()
    {
        if (state == 2 && state == 3) return;

        nextFrameTimer = 0;
        frameReset();
        state = 2;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerTouched();
        }
    }
}
