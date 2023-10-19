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
    bool inactive = false;
    public float respawnTime = 10;
    float respawnTimer = 0;

    public float maxHealth;
    public float currentHealth;

    //Health bar slider vars
    public bool showHealthBarOnDealDamage = false;
    bool healthBarCreated = false;
    public GameObject canvasWithHealthBar;
    GameObject barObj;

    public float damageDealt;

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
    public void dealDamage(float damage)
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
            barObj.SetActive(false);
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
                barObj = Instantiate(canvasWithHealthBar, transform.position, Quaternion.identity, this.gameObject.transform);
                barObj.transform.localScale = new Vector3(.05f, .05f, .05f);

                barObj.GetComponentInChildren<Slider>().maxValue = maxHealth;

                healthBarCreated = true;
            }
            else if (healthBarCreated)
            {
                barObj.SetActive(true);
            }
        }
    }
}
