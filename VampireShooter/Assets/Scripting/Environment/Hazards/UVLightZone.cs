using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVLightZone : MonoBehaviour
{
    public bool GizmosOn = true;
    public Color GizmoColour = Color.blue;
    private void OnTriggerEnter(Collider other) // When something enters UV light
    {
        IVampire vampire = other.GetComponent<IVampire>();

        if (vampire != null)
        {
            vampire.SetInUVLight(true);
        }
    }

    private void OnTriggerExit(Collider other) // When something leaves UV light
    {
        IVampire vampire = other.GetComponent<IVampire>();

        if (vampire != null)
        {
            vampire.SetInUVLight(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (!GizmosOn) { return; }

        Gizmos.color = GizmoColour;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}