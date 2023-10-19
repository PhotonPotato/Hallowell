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

    public float currentCamSpeed;
    public float lastCamSpeed;

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

        //Clamp BEFORE setting the position
        desiredPos.x = Mathf.Clamp(desiredPos.x, camConstraints.xMin, camConstraints.xMax);
        desiredPos.y = Mathf.Clamp(desiredPos.y, camConstraints.yMin, camConstraints.yMax);

        //Update the cam speed
        currentCamSpeed += (camConstraints.getSpeed() - currentCamSpeed) / 10;

        camPos += (desiredPos - camPos) / currentCamSpeed;

        //Add constraints if its not using desired posCam.

        mainCam.position = camPos;

        //Set the camera z to -10.
        mainCam.position += new Vector3(0, 0, -10);

        //Set the zoom
        Camera.main.orthographicSize += (camConstraints.zoom - Camera.main.orthographicSize) / camConstraints.zoomSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "CamConstraintZone") return;

            //Return if its the same thing
        if (collision.gameObject.GetComponent<CamConstraintHolder>().cameraConstraint == camConstraints) return;

        Debug.Log("enter");

    //Look for camera zones
        currentCamSpeed = camConstraints.getSpeed();
        camConstraints = collision.gameObject.GetComponent<CamConstraintHolder>().cameraConstraint;
            
        if (!camConstraints.useSpeedSmoothing || currentCamSpeed < camConstraints.getSpeed()) currentCamSpeed = camConstraints.getSpeed();
    }
}
