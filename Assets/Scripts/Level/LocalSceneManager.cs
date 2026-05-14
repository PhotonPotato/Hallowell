using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSceneManager : MonoBehaviour
{
    public CameraContraints cameraContraints;

    public Vector3[] spawnPositions;

    public void OnPlayerRoomEnter(int entranceIndex)
    {
        FindFirstObjectByType<PlayerManager>().transform.position = spawnPositions[entranceIndex];
    }
}
