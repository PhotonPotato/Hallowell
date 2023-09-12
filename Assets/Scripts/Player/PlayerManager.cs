using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    //Script attached to player character
    public PlayerMovementScript playerMovement;
    public PlayerCameraController playerCameraController;

    Rigidbody2D rb;

    public float invincibilityTimer = 0;
    public float invincibilityTime;

    [System.NonSerialized]
    public playerHealthContainer playerHealth;

    //Effects
    public GameObject invincibilityAura;
    public float auraPulseFrequency = .5f;

    //Playerhealth slider
    public Slider healthBarSlider;
    public Slider healthBarSmoothing;
    public float smoothingValue = 10;

    void Start()
    {
        playerMovement= GetComponent<PlayerMovementScript>();
        playerCameraController= GetComponent<PlayerCameraController>();

        rb = GetComponent<Rigidbody2D>();

        playerHealth = GetComponent<playerHealthContainer>();
    }

    void Update()
    {
        updateHealthBar();

        //Update Timers
        if (invincibilityTimer > 0) invincibilityTimer -= Time.deltaTime;

        //Reset position for debug when R is pressed
        if (Input.GetKey("r"))
        {
            playerMovement.resetPlayerInLevel();
        }

        //If the player is touching anything.
        Collider2D[] cols = new Collider2D[10];
        if (rb.GetContacts(cols) != 0)
        {
            //Run through all of the collisions
            foreach(Collider2D col in cols)
            {
                if (col == null) continue;

                if (col.gameObject.tag == "Spikes")
                {
                    //The player is touching spikes.

                    //This value is adjustable
                    dealDamage(20, false);

                    playerMovement.resetPlayerInLevel();
                }
            }
        }

        //Update invincibility effects
        Color invincibilityAuraColor = invincibilityAura.GetComponent<SpriteRenderer>().color;

        if (invincibilityTimer <= 0 && invincibilityAuraColor.a > 0)
        {
            invincibilityAuraColor.a -= auraPulseFrequency * Time.deltaTime;
        }else if (invincibilityTimer > 0)
        {
            //The aura should pulse using a sin function for the alpha value that oscillates between 0-1
            invincibilityAuraColor.a = .25f * Mathf.Sin(Time.time * auraPulseFrequency) + .75f;
            invincibilityAura.SetActive(true);
        }
        else
        {
            invincibilityAura.SetActive(false);
        }

        invincibilityAura.GetComponent<SpriteRenderer>().color = invincibilityAuraColor;
    }

    public void initializeHealthBar()
    {
        healthBarSlider.maxValue = playerHealth.maxHealth;
        healthBarSmoothing.maxValue = playerHealth.maxHealth;
    }

    public void updateHealthBar()
    {
        float currentPlayerHealth = playerHealth.getHealth();

        healthBarSlider.value = currentPlayerHealth;

        float currentSmoothingBarValue = healthBarSmoothing.value;

        healthBarSmoothing.value = currentSmoothingBarValue + ((currentPlayerHealth - currentSmoothingBarValue) / smoothingValue);
    }

    public void dealDamage(GameObject obj)
    {
        if (invincibilityTimer > 0) return;

        float damageAmount = obj.GetComponentInChildren<enemyHealthContainer>().damageDealt;

        playerHealth.dealDamage(damageAmount);

        //Add iFrames and timer
        invincibilityTimer = invincibilityTime;
    }

    public void dealDamage(float damageAmount, bool forceDamage = false)
    {
        if (invincibilityTimer > 0 && !forceDamage) return;

        playerHealth.dealDamage(damageAmount);

        //Add iFrames and timer
        invincibilityTimer = invincibilityTime;
    }
}
