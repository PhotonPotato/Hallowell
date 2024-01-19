using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public Rigidbody2D playerRB;
    public Transform playerTransform;
    public PlayerManager playerManager;
    SpriteRenderer playerRenderer;
    Animator playerAnimator;
    private Vector3 resetPos;

    [Space] 
    public float playerSpeed;
    public float effectedPlayerSpeed;
    public float xInput;
    public float yInput;
    public float xInputMin;
    public float xVel;
    public float jumpForce;
    public float gravityForce;
    public float yVel;
    public float minYVel;
    public float gravityMult;
    public float drag;
    public float jumpTimer;
    public float jumpCooldownTime;
    public bool jumpButtonReset = false;
    float jumpInputBuffer = 0;
    public float inputBufferTime = .1f;
    [Space]

    //Player hitboxes and stomp detection vars
    public BoxCollider2D hitbox;
    public BoxCollider2D stompHitbox;
    LayerMask enemyLayer;

    //Ground detection vars
    public LayerMask groundMask;
    public float detectionOffsetX;
    public float detectionOffsetY;
    public float detectionWidth;
    public float detectionHeight;
    public bool onGround;
    public bool isJumping;
    public float cayoteTime = .1f;
    float cayoteTimer;
    [Space]

    //Wall jump vars.
    public bool onWall;
    public int wallDir;
    public float gravityMultOnWall;
    public float wallDetectionRange;
    public float pushOffForce;
    public float pushOffTimer;
    public float pushOffDuration;
    float timePushedOff;
    public float postPushOffDampeningOffset = 0.1f;
    public float wallYVelocityDecay = .8f;
    public bool jumpFromWallBase;
    [Space]

    //Graphics
    public bool showEffects;
    ParticleSystem landingEffects;
    ParticleSystem runningEffects;
    ParticleSystem jumpEffect;
    ParticleSystem stompBloodEffect;
    [Space]

    //Enemy stomp detection (and stomp slash/jump mechanic)
    public float stompDetectionWidth = 1;
    public float stompDetectionHeight = 1;
    public Vector2 stompDetectionOffset;
    public LayerMask stompLayers;
    public float bounceSpeedBoostTime;
    float bounceSpeedBoostTimer = 0;
    float stompCayoteTimer = 0;

    //Stomp power controls
    public float stompBounceForce = 5;
    public float stompBounceSpeedBoost = 1;

    public void Start()
    {
        resetPos = transform.position;

        playerRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        playerTransform = GetComponent<Transform>();
        playerManager = GetComponent<PlayerManager>();

        landingEffects = GetComponentsInChildren<ParticleSystem>()[0];
        runningEffects = GetComponentsInChildren<ParticleSystem>()[1];
        jumpEffect = GetComponentsInChildren<ParticleSystem>()[2];
        stompBloodEffect = GetComponentsInChildren<ParticleSystem>()[3];

        isJumping = false;

        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void Update()
    {
        //Timers.
        if (!(onGround & isJumping && jumpTimer <= 0)) jumpTimer -= Time.deltaTime;
        if (pushOffTimer > 0) pushOffTimer -= Time.deltaTime;

        //Update cayote-time timer
        if (cayoteTimer > 0) cayoteTimer -= Time.deltaTime;
        if (stompCayoteTimer > 0) stompCayoteTimer -= Time.deltaTime;
    }

    public void FixedUpdate()
    {
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        //Run some good ol ground detection.
        onGround = detectGround();

        if (onGround) onWall = false;

        //Slowly bring back input if there is a wall jump in action (with a slight time offset to start the dampeneing midway into the pushoff)
        if (Time.time - timePushedOff < 1)
        {
            xInput *= 1 - Mathf.Pow(.345f, (Time.time - timePushedOff) * 9.4f);
        }

        if (Mathf.Abs(xInput) > xInputMin)
        {
            xVel = xInput * effectedPlayerSpeed;
        }
        else
        {
            //playerRB.velocity *= drag;
            xVel *= drag;
        }

        //make sure to reset x velocity if we are on the wall (unless the xvel is away from the wall)
        if (onWall && xVel * wallDir > 0) xVel = 0;

        //Jumping
        //Some jankey getbuttondown type logic b/c get button down was not super responsive.
        //*EDIT* "jumpButtonReset" pretty much just detects when you release the jump button so that we know when it gets pressed down (not held)bh
        bool jumpButton = Input.GetButton("Jump");
        if (!jumpButton)
        {
            jumpButtonReset = true;
        }

        //Update jump buffer
        if (jumpButton && (jumpButtonReset || (Time.time - jumpInputBuffer < 0.05 && Time.time - jumpInputBuffer != Mathf.NegativeInfinity))) jumpInputBuffer = Time.time;

        //Detect jump condition
        if ((jumpButton || Time.time - jumpInputBuffer < inputBufferTime) && (onGround || onWall) && (!isJumping || jumpFromWallBase) && jumpButtonReset)
        {
            yVel = jumpForce;
            isJumping = true;
            playerAnimator.SetTrigger("Jump");
            jumpTimer = jumpCooldownTime;

            if (detectGround() && detectWall())
            {
                jumpFromWallBase = true;

                //jumpTimer = .05f;
            }

            jumpButtonReset = false;

            if (onWall)
            {
                //Add a value to push off wall.
                xVel = pushOffDuration * wallDir;

                //Start push off timer.
                pushOffTimer = pushOffDuration;
                playerAnimator.SetTrigger("Jump");

                //Set the time pushed off
                timePushedOff = Time.time;
            }
            else
            {
                jumpEffect.Play();
            }

            jumpInputBuffer = Mathf.NegativeInfinity;
            cayoteTimer = 0;
        }
        else
        {
            //FIX: need to fix how gavity constantly pushing player down causes friction between ground and player and slows down player L/R movement.
            //Gravity
            yVel -= gravityForce * gravityMult;

            if (yVel < minYVel)
            {
                yVel = minYVel;
            }

            //Watch out for logic bugs here

            ///JUMP BUG
            ///For future Ty
            ///-This line made it so that when you jumped against a wall, the player velocity ate the jump
            ///-This is because isJumping got set to false somehow because you are touching the wall
            ///
            /// ISSUE
            /// -isJumping does not get reset when you jump already touching a wall
            /// 
            ///FIX
            ///-Set the if then near the jumpButtonReset() line
            /// to only reset isJumping to false if the player is not on the wall
            ///-Added new condition "jumpFromWallBase" that only is set to true in these scenarios
            /// and is dependent on the jump timer
            ///-Jump timer is too short (.2secs) so reset isJumping to false
            /// 
            ///-This if below is crutial to reset the velocity of the player when running off edges.
            if (onGround && !isJumping) yVel = 0;

            if (onWall && !jumpFromWallBase) yVel *= wallYVelocityDecay;
            playerAnimator.ResetTrigger("Jump");
        }

        //Update jump timers.
        if (pushOffTimer > 0)
        {
            //Calculate the exponential decay based on time since push off
            float exponentialDecayVal = Mathf.Pow(.345f, (Time.time - timePushedOff) * 9.4f);

            xVel += pushOffForce * exponentialDecayVal * wallDir * -1;

            //Input.ResetInputAxes();
        }

        //Update Animator to trigger run/idle/other animations.
        playerAnimator.SetFloat("PlayerAbsXVel", Mathf.Abs(xVel));
        playerAnimator.SetFloat("PlayerYVel", yVel);
        playerAnimator.SetBool("onGround", onGround);
        playerAnimator.SetBool("onWall", onWall);
        playerAnimator.SetBool("isJumping", isJumping);

        //Check for stomp and bounce of the player.
        detectEnemyStomp();

        playerRB.velocity = new Vector2(xVel, yVel);

        if (xVel > 0)
        {
            //Moving right
            //Face character right.
            playerRenderer.flipX = false;
            playerManager.playerFacingRight = true;
        }
        else if (xVel < 0)
        {
            //Moving left
            //Face character left.
            playerRenderer.flipX = true;
            playerManager.playerFacingRight = false;
        }
        else
        {
            //No velocity

        }

        //On a wall
        if (onWall)
        {
            playerRenderer.flipX = (wallDir > 0);
            if (!isJumping) gravityMult = gravityMultOnWall;
            else gravityMult = 1;
        }
        else
        {
            gravityMult = 1;
        }

        //Add a running effect if on the ground and running.
        if (Mathf.Abs(xVel) >= .1 && onGround) runningEffects.Play();

        if (onGround & isJumping && jumpTimer <= 0)
        {
            //If just landed
            justLanded();

            ///Adding the if on wall here fixes the jump bug against the wall that was described above.
            isJumping = false;
        }
        if (jumpTimer <= 0) jumpFromWallBase = false;

        if (onWall && jumpFromWallBase) isJumping = false;

        //Run some wall detection for jump resets.
        if (detectJumpReset())
        {
            //This returns true when it goes from not on wall to on wall (once every time you touch a wall).
            ///This also helps to hix the jump against the wall bug described above
            if (!detectGround()) isJumping = false;
        }
    }

    public bool detectGround()
    {
        Collider2D[] groundCols = Physics2D.OverlapAreaAll(new Vector2(detectionOffsetX + playerTransform.position.x - (detectionWidth / 2), detectionOffsetX + playerTransform.position.y - detectionOffsetY + (detectionHeight / 2)), new Vector2(playerTransform.position.x + (detectionWidth / 2), playerTransform.position.y - (detectionOffsetY) - (detectionHeight / 2)));

        if (groundCols == null && cayoteTimer <= 0) return false;

        for (int i = 0; i < groundCols.Length; i++)
        {
            if (groundCols[i].gameObject.tag == "Ground")
            {
                cayoteTimer = cayoteTime;
                return true;
            }
        }

        if (cayoteTimer <= 0) return false;
        else return true;
    }

    void justLanded()
    {
        if (showEffects)
        {
            landingEffects.Play();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(new Vector3(playerTransform.position.x, playerTransform.position.y - detectionOffsetY - (detectionHeight / 2), 1), new Vector3(detectionWidth, detectionHeight, 1));
    }

    private bool detectWall()
    {
        if (pushOffTimer > 0) return false;

        RaycastHit2D hit1 = Physics2D.Raycast(playerTransform.position, Vector2.right, wallDetectionRange, LayerMask.GetMask("Wall"));
        bool detection = false;
        if (hit1.collider != null)
        {
            detection = true;
            wallDir = 1;
        }

        RaycastHit2D hit2 = Physics2D.Raycast(playerTransform.position, Vector2.left, wallDetectionRange, LayerMask.GetMask("Wall"));
        if (hit2.collider != null)
        {
            detection = true;
            wallDir = -1;
        }

        return detection;
    }

    private bool detectJumpReset()
    {
        bool returnVal = false;

        if (detectWall() && !onWall)
        {
            returnVal = true;    
        }

        onWall = detectWall();

        return returnVal;
    }

    public void detectEnemyStomp()
    {
        //Keep track off some speed boost timers
        if (bounceSpeedBoostTimer > 0) bounceSpeedBoostTimer -= Time.deltaTime;
        else
        {
            effectedPlayerSpeed = playerSpeed;
        }

        //Get the bounds that for where we will search for enemy colliders
        Vector2 originPoint = (Vector2)playerTransform.position + stompDetectionOffset;
        Vector2 detectionBoxSize = new Vector2(stompDetectionWidth / 2,stompDetectionHeight / 2);

        //Vector2 UL = new Vector2(transform.position.x + stompDetectionOffset.x - (stompDetectionWidth / 2), transform.position.y + stompDetectionOffset.y + (stompDetectionHeight / 2));
        //Vector2 BR = new Vector2(transform.position.x + stompDetectionOffset.x + (stompDetectionWidth / 2), transform.position.y + stompDetectionOffset.y - (stompDetectionHeight / 2));

        /// **NOTE** This will only work if put after the clamping of the player velocity as a speed boost may exeed the clamp.
        /// May want to fix this by adding a temporary boost timer or smthn of the sort in the future, currently too llazy to care.

        //Cleanup start
        //Detect stomp Hitbox
        bool stompDetected = false;
        List<Collider2D> stompHitboxCols = new List<Collider2D>();
        if (stompHitbox.GetContacts(stompHitboxCols) != 0)
        {
            //Look through the colliders within the hitbox for enemies.
            foreach (Collider2D col in stompHitboxCols)
            {
                if (col.gameObject.layer == enemyLayer && col.gameObject.tag == "BouncableEnemy")
                {
                    //Contact point exists on top of object.
                    enemyHealthContainer container = col.gameObject.GetComponent<enemyHealthContainer>();
                    container.DealDamage(5);

                    //Detect stomp jump
                    if (Input.GetButton("Jump") && jumpButtonReset)
                    {
                        //Change the effected speed of the player for a specific time
                        effectedPlayerSpeed = playerSpeed + stompBounceSpeedBoost;
                        bounceSpeedBoostTimer = bounceSpeedBoostTime;

                        //Add a jump to the player
                        yVel = stompBounceForce;

                        //Play the jump particle effect
                        jumpEffect.Play();
                    }
                    else
                    {
                        //If not pressed, just give the player a slight upwards jelocity to cushion falls
                        yVel = 10;
                    }

                    //Add quick blood effect for the stomp
                    stompBloodEffect.Play();

                    stompDetected = true;
                }
            }
        }

        if (!stompDetected)
        {
            //Detect player hitbox before the stomp hitbox.
            List<Collider2D> playerHitboxCols = new List<Collider2D>();

            //Use a mask that filters for enemies
            ContactFilter2D playerHitboxFilter = new ContactFilter2D();
            playerHitboxFilter.useTriggers = true;
            playerHitboxFilter.useLayerMask = true;
            //Bitshift the mask
            playerHitboxFilter.layerMask = 1 << LayerMask.NameToLayer("Enemy"); ;

            if (hitbox.GetContacts(playerHitboxFilter, playerHitboxCols) != 0)
            {
                //Look through the colliders within the hitbox for enemies.
                foreach (Collider2D col in playerHitboxCols)
                {
                    //Reference the player manager to deal damage.
                    playerManager.DealDamage(col.gameObject);
                }
            }
        }
    }

    public void resetPlayerInLevel()
    {
        //Reset Position
        transform.position = resetPos;

        //Reset Velocity
        xVel = 0;
        yVel = 0;
        playerRB.velocity = Vector2.zero;
    }
}
