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
}
