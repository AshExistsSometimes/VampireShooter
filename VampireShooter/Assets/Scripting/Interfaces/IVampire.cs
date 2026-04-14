using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVampire
{
    void SetInUVLight(bool state);
    void SetCrossZone(Vector3 center, float radius, bool state);

    void DrainHuman(IHuman target);

}