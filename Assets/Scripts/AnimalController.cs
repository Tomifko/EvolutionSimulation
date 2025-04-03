using System;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    public float Speed = 1f;
    public float RotationScale = 10f;

    void Start()
    {
        // Change direction of animal to random direction
        InvokeRepeating(nameof(ChangeDirection), 1f, 1f);
    }

    void Update()
    {
        transform.position += Speed * Time.deltaTime * transform.forward;
    }

    private void ChangeDirection()
    {
        var randomRotation = UnityEngine.Random.rotation;
        transform.eulerAngles += new Vector3(0, randomRotation.y * RotationScale, 0);
    }
}
