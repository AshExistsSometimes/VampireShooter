using UnityEngine;

public class VampireBase : EnemyBase, IVampire
{
    public enum VampireState
    {
        Waiting,
        Stalking,
        Hunting,
        Attacking,
        Searching,
        Fleeing,
        KillingHuman
    }

    public VampireState currentState;

    public bool InUVLight = false;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetInUVLight(bool state) // Called by UV zones
    {
        InUVLight = state;
    }

    public void SetCrossZone(Vector3 center, float radius, bool state) // Called by cross zones
    {

    }

    public void DrainHuman(IHuman target)
    {

    }
}