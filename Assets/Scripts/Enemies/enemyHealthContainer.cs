using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyHealthContainer : HealthContainer
{
    //This is just a universal container to store health in the game.
    public int objID;
    public bool destroyOnNoHealth = false;

    public bool respawnWithTime = false;
    [NonSerialized] public bool inactive = false;
    public float respawnTime = 10;
    [NonSerialized] public float respawnTimer = 0;

    //Health bar slider vars
    public bool showHealthBarOnDealDamage = false;
    [NonSerialized] public bool healthBarCreated = false;
    public GameObject canvasWithHealthBar;
    [NonSerialized] public GameObject barObj;

    public Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

    public float damageDealt;

    //CLEAN THIS PLEASE
    [Header("Extra Settings")]
    public bool DamageCallbackToParent = false;

    public bool SetDamageTriggerInAnimator = false;

    public bool SendDamageCallbackToThisObject = false;

    private void Update()
    {
        if (healthBarCreated) barObj.GetComponentInChildren<Slider>().value = currentHealth;

        if (respawnTimer > 0) respawnTimer -= Time.deltaTime;
        else if (respawnWithTime && inactive)
        {
            //Unhide the enemy (and add effects
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            GetComponent<Rigidbody2D>().simulated = true;

            //Hide the health bar
            inactive = false;

            currentHealth = maxHealth;

            ///SPAWN EFFECTS HERE
        }
    }

    //A public mehtod to deal damage
    public override void DealDamage(float damage, AttackDirection direction, Vector2 inheritedVelocity = default)
    {
        currentHealth -= damage;

        if (SetDamageTriggerInAnimator) GetComponent<Animator>().SetTrigger("EnemyHit");

        if (respawnWithTime && currentHealth <= 0)
        {
            //Hide the enemy and heatlh bar for reset
            //Turn off the renderer
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;

            //Hide the health bar
            if (barObj != null) barObj.SetActive(false);
            inactive = true;
            respawnTimer = respawnTime;
        }
        else if (destroyOnNoHealth && currentHealth <= 0)
        {
            //Destroy the current objecta and health bar and remove from
            if (healthBarCreated) Destroy(barObj);

            Destroy(this.transform.parent.gameObject);
        }
        else if (currentHealth > 0)
        {
            //If the option to create a health bar when damage is dealt then instantiate a worldspace canvas (with health bar) and update boolean
            if (showHealthBarOnDealDamage && !healthBarCreated)
            {
                barObj = Instantiate(canvasWithHealthBar, transform.position + new Vector3(Random.Range(-10, 10), 0, 0), Quaternion.identity, this.gameObject.transform);
                barObj.transform.localScale = healthBarScale;

                //This vector 3 is the OFFSET  
                barObj.transform.position = transform.position + new Vector3(0, 4, 0);

                barObj.GetComponentInChildren<Slider>().maxValue = maxHealth;

                barObj.GetComponent<Canvas>().worldCamera = Camera.main;

                healthBarCreated = true;
            }
            else if (healthBarCreated)
            {
                barObj.SetActive(true);
            }
        }

        // Call any functions on this object that can handle the directional damage
        if (SendDamageCallbackToThisObject) SendMessage("OnDamageReceived", new EnemyDamageInfo()
                                                                                {
                                                                                    amount = damage,
                                                                                    direction = direction,
                                                                                    inheritedVelocity = inheritedVelocity
                                                                                });
    }
}

public struct EnemyDamageInfo
{
    public float amount;
    public AttackDirection direction;
    public Vector2 inheritedVelocity;
}
