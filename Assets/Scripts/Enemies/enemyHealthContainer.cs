using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyHealthContainer : MonoBehaviour
{
    //This is just a universal container to store health in the game.
    public int objID;
    public bool destroyOnNoHealth = false;

    public bool respawnWithTime = false;
    [System.NonSerialized] public bool inactive = false;
    public float respawnTime = 10;
    [System.NonSerialized] public float respawnTimer = 0;

    public float maxHealth;
    public float currentHealth;

    //Health bar slider vars
    public bool showHealthBarOnDealDamage = false;
    [System.NonSerialized] public bool healthBarCreated = false;
    public GameObject canvasWithHealthBar;
    [System.NonSerialized] public GameObject barObj;

    public Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

    public float damageDealt;

    //CLEAN THIS PLEASE
    public bool DamageCallbackToParent = false;

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
    public virtual void DealDamage(float damage)
    {
        currentHealth -= damage;

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
    }
}
