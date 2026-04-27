using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool RotateX = false;
    public bool RotateY = false;
    public bool RotateZ = false;
    [Space]
    public float XRotationSpeed = 1.0f;
    public float YRotationSpeed = 1.0f;
    public float ZRotationSpeed = 1.0f;
    [Space]
    public bool RotationOn = true;

    private void Update()
    {
        if (!RotationOn) return;

        if (RotateX)
        {
            RotateObjectX(XRotationSpeed * 10f);
        }

        if (RotateY)
        {
            RotateObjectY(YRotationSpeed * 10f);
        }

        if (RotateZ)
        {
            RotateObjectZ(ZRotationSpeed * 10f);
        }
    }

    public void RotateObjectX(float XRotSpeed)
    {
        gameObject.transform.Rotate(XRotSpeed * Time.deltaTime, 0f, 0f);
    }
    public void RotateObjectY(float YRotSpeed)
    {
        gameObject.transform.Rotate(0f, YRotSpeed * Time.deltaTime, 0f);
    }
    public void RotateObjectZ(float ZRotSpeed)
    {
        gameObject.transform.Rotate(0f, 0f, ZRotSpeed * Time.deltaTime);
    }
}
