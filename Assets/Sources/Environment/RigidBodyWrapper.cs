using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyWrapper : MonoBehaviour
{
    public Rigidbody body { get; private set; }
    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventSystem.RaiseEvent(WorldEvent.OnCollisionEnter, new object[1] {this});
        }
    }

    public void AddForce(Vector3 velocity)
    {
        body.AddForce(velocity, ForceMode.Impulse);
    }

    public void SetVelocity(Vector3 velocity)
    {
        body.velocity = velocity;
    }
    
}
