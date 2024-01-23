using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidfangManager : MonoBehaviour
{
    public ProceduralQuadropedAnimation animationScript;
    public enemyHealthContainer m_healthContainer;

    public bool dead = false;

    private void Start()
    {
        animationScript = GetComponent<ProceduralQuadropedAnimation>();
        m_healthContainer = GetComponent<enemyHealthContainer>();
    }

    public void Update()
    {
        if (dead) return;

        //Check for death
        if(m_healthContainer.currentHealth <= 0)
        {
            dead = true;

            animationScript.entityDead = true;

            //Disable boid update in enemy
            foreach (BasicBoidBehavior man in animationScript.boidBehaviorScripts)
            {
                man.update = false;
            }

            //Sprite rend
            GetComponent<SpriteRenderer>().color = Color.red;

            this.gameObject.AddComponent<Rigidbody2D>();

            m_healthContainer.enabled = false;
            Destroy(m_healthContainer.barObj);
        }
    }
}
