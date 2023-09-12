using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyHealthContainer : MonoBehaviour
{
    //This is just a universal container to store health in the game.
    public int objID;
    public bool destroyOnNoHealth = false;
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
    }

    //A public mehtod to deal damage
    public void dealDamage(float damage)
    {
        currentHealth -= damage;

        if (destroyOnNoHealth && currentHealth <= 0)
        {
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
        }
    }
}
