using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class proceduralQuadropedHealthContainer : enemyHealthContainer
{
    //Number of boids to destroy per hit point
    public float boidToHealthRatio = 0;
    public ProceduralQuadropedAnimation parentQuadropedScript;

    //NOTE: in this case the obj id stores the sibling index

    public void Start()
    {
        parentQuadropedScript = transform.parent.parent.GetComponentInChildren<ProceduralQuadropedAnimation>();
        boidToHealthRatio = parentQuadropedScript.boidBehaviorScripts[transform.GetSiblingIndex()].numBoids / maxHealth;
    }

    public override void DealDamage(float damageAmount)
    {
        //SAME AS INHERETED **Maybe find a way to optimize this**
        currentHealth -= damageAmount;

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

                barObj.GetComponentInChildren<Slider>().maxValue = maxHealth;

                barObj.GetComponent<Canvas>().worldCamera = Camera.main;

                healthBarCreated = true;
            }
            else if (healthBarCreated)
            {
                barObj.SetActive(true);
            }
        }

        //BELOW IS CUSTOM SCRIPT for this class

        if (currentHealth > 0)
        {
            if (currentHealth < damageAmount)
            {
                //If its trying to damage more than we have then just damage with the current health
                damageAmount = currentHealth;
            }

            transform.parent.parent.GetComponentInChildren<enemyHealthContainer>().DealDamage(damageAmount);

            //Now to destroy boids
            parentQuadropedScript.boidBehaviorScripts[objID].DestroyBoids((int) (damageAmount * boidToHealthRatio));
        }
    }
}
