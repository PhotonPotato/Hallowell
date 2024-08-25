using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraConstraint", menuName = "ScriptableObjects/CameraContraint", order = 1)]
public class CameraContraints : ScriptableObject
{
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;

    public bool useSpeedSmoothing;

    public float zoom = 21.25774f;
    public float zPos = -15.2f;
    public float zoomSpeed = 10;

    public float speed = 10;

    public float getSpeed()
    {
        return speed;
    }
}
