using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform mainCam;
    public Transform playerPos;

    public PlayerMovementScript movementScript;
    public Vector2 playerVel;

    public float camFollowDelay = 10f;
    public float camLookAheadMult = 1.5f;

    Vector2 desiredPos;
    CameraContraints camConstraints;

    private void Start()
    {
        camConstraints = FindObjectOfType<SceneManager>().cameraContraints;
    }

    public void FixedUpdate()
    {
        Vector2 camPos = new Vector2(mainCam.position.x, mainCam.position.y);
        Vector2 pPos = new Vector2(playerPos.position.x, playerPos.position.y);

        playerVel.x = movementScript.xVel;
        playerVel.y = movementScript.yVel;

        desiredPos = pPos + (playerVel * camLookAheadMult);

        camPos += (desiredPos - camPos) / camFollowDelay;

        //Add constraints.
        camPos.x = Mathf.Clamp(camPos.x, camConstraints.xMin, camConstraints.xMax);
        camPos.y = Mathf.Clamp(camPos.y, camConstraints.yMin, camConstraints.yMax);

        mainCam.position = camPos;

        //Set the camera z to -10.
        mainCam.position += new Vector3(0, 0, -10);
    }
}
