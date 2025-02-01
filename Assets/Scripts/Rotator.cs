using UnityEngine;
using UnityEngine.UIElements;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 50f;
    private float currentYRotation = 0f;

    void Update()
    {
        currentYRotation += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(-37, currentYRotation, 0); 
    }
}
