using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class DebugTimeRotator : MonoBehaviour
{
    public float rotationSpeed = 180f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}