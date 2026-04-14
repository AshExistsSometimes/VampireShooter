using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CrossZone : MonoBehaviour
{
    public bool GizmosOn = true;
    [Space]
    public float radius = 5f;


    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        GetComponent<SphereCollider>().radius = radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        IVampire vampire = other.GetComponent<IVampire>();

        if (vampire != null)
        {
            vampire.SetCrossZone(transform.position, radius, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IVampire vampire = other.GetComponent<IVampire>();

        if (vampire != null)
        {
            vampire.SetCrossZone(transform.position, radius, false);
        }
    }

    private void OnDrawGizmos()
    {
        if (!GizmosOn) { return; }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}