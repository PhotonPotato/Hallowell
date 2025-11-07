using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Temp refs")]
    public Transform RespawnPoint;

    //Script attached to player character
    public PlayerMovementScript playerMovement;
    public PlayerCombatScript playerCombat;
    public PlayerCameraController playerCameraController;
    [NonSerialized] public InventoryUIManager playerInventoryManager;

    Rigidbody2D rb;

    public float invincibilityTimer = 0;
    public float invincibilityTime;

    public bool cloaked = false;
    public float cloakingTime = 3;
    float timeOfCloaking = float.NegativeInfinity;

    [NonSerialized]
    public HealthContainer playerHealth;

    //Effects
    public GameObject invincibilityAura;
    public float auraPulseFrequency = .5f;

    //Playerhealth slider
    public Slider healthBarSlider;
    public Slider healthBarSmoothing;
    public float smoothingValue = 10;

    //Station interaction radius
    public float stationInteractionRadius = 5;
    public LayerMask stationInteractionMask;

    [NonSerialized] public GameObject currentInteractionObject;

    [NonSerialized] public bool playerFacingRight;
    public GameObject weaponPositionParent;

    private void Awake()
    {
        // Singleton boilerplate
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Start()
    {
        playerMovement= GetComponent<PlayerMovementScript>();
        playerCameraController = GetComponent<PlayerCameraController>();

        rb = GetComponent<Rigidbody2D>();

        playerHealth = GetComponent<HealthContainer>();

        playerInventoryManager = FindObjectOfType<InventoryUIManager>();
    }

    void Update()
    {
        UpdateHealthBar();

        //Update Timers
        if (invincibilityTimer > 0) invincibilityTimer -= Time.deltaTime;

        //Reset position for debug when R is pressed
        if (Input.GetKey("r"))
        {
            playerMovement.resetPlayerInLevel();
        }

        // Cloaking
        if (Input.GetKey("v"))
        {
            cloaked = true;
            timeOfCloaking = Time.time;
        }
        if (cloaked && Time.time - timeOfCloaking > cloakingTime) cloaked = false;

        //If the player is touching anything.
        Collider2D[] cols = new Collider2D[19];
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
                    DealDamage(20, false);

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


        //Check for nearby interactable stations
        if (Input.GetKeyDown("e"))
        {
            //Maybe play a sound or smthn
            CheckForInteraction();
        }

        if (playerInventoryManager.crockpotPanelOpen)
        {
            if (currentInteractionObject == null) return;

            //Might wanna optimize with this later
            if (Vector2.Distance(currentInteractionObject.transform.position, transform.position) >= stationInteractionRadius)
            {
                GameObject openCrockPot = GameObject.FindGameObjectWithTag("Crockpot");

                //Close crockpot without closing inventory panel
                openCrockPot.GetComponent<CrockPotBehavior>().closeCrockPot(false);
            }
        }

        //Update weapon orientation
        weaponPositionParent.transform.rotation = playerFacingRight ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
    }

    public void InitializeHealthBar()
    {
        healthBarSlider.maxValue = playerHealth.maxHealth;
        healthBarSmoothing.maxValue = playerHealth.maxHealth;
    }

    public void UpdateHealthBar()
    {
        float currentPlayerHealth = playerHealth.GetHealth();

        healthBarSlider.value = currentPlayerHealth;

        float currentSmoothingBarValue = healthBarSmoothing.value;

        healthBarSmoothing.value = currentSmoothingBarValue + ((currentPlayerHealth - currentSmoothingBarValue) / smoothingValue);
    }

    public void DealDamage(GameObject obj)
    {
        if (invincibilityTimer > 0) return;

        EnemyHealthContainer container = obj.GetComponentInChildren<EnemyHealthContainer>();

        if (container == null) return;

        float damageAmount = container.damageDealt;

        // TODO: this shit is disgusting asf
        if (container.type == HealthContainerType.ProceduralQuadropedLeg)
        {
            if ((container as proceduralQuadropedHealthContainer).performingLightAttack)
            {
                //Then deal light attack damage, not just touch damage
                damageAmount = (container as proceduralQuadropedHealthContainer).lightAttackDamage;
            }
        }

        playerHealth.DealDamage(damageAmount);

        //Add iFrames and timer
        invincibilityTimer = invincibilityTime;
    }

    public void DealDamage(float damageAmount, bool forceDamage = false)
    {
        if (invincibilityTimer > 0 && !forceDamage) return;

        playerHealth.DealDamage(damageAmount);

        //Add iFrames and timer
        invincibilityTimer = invincibilityTime;
    }

    /// <summary>
    /// Deals damager to the player and resets the players position to the last safe spot
    /// </summary>
    public void DealDamageWithReset(float damageAmount, bool forceDamage = false)
    {
        DealDamage(damageAmount, forceDamage);

        transform.position = RespawnPoint.position;
        playerMovement.xVel = 0;
        playerMovement.yVel = 0;
    }

    public void CheckForInteraction()
    {
        Collider2D[] interactableStationColliders = Physics2D.OverlapCircleAll((Vector2)transform.position, stationInteractionRadius, stationInteractionMask);

        //Check if there's nothing around the player
        if (interactableStationColliders.Length == 0)
        {
            return;
        }

        foreach (Collider2D col in interactableStationColliders)
        {
            if (col.gameObject.tag == "Crockpot")
            {
                col.GetComponent<CrockPotBehavior>().openCrockpot();
                playerInventoryManager.OpenInventoryPanel();
                currentInteractionObject = col.gameObject;
            }
        }
    }

    public void HealPlayer(float amount)
    {
        //Add health and clamp to max hp
        playerHealth.currentHealth = Mathf.Clamp(playerHealth.currentHealth + amount, 0, playerHealth.maxHealth);
    }
}
