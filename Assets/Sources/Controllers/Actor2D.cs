using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor2D : MonoBehaviour
{
    Vector2 target = Vector2.zero;
    Vector2 originalPosition = Vector2.zero;
    Vector2 targetPosition = Vector2.zero;
    bool flipTarget = false;
    float roundTime = 10f;
    Vector2 movementDirection = Vector2.zero;
    public Vector3 velocity { get; protected set; }
    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        targetPosition = transform.position + new Vector3(3f, 0f, 0f);
        target = targetPosition;
        movementDirection = Vector2.right;
    }

    // Update is called once per frame
    void Update()
    {

        if(Vector2.Distance(transform.position, target) <= 0.01f)
        {
            flipTarget = true;  
        }
        if(flipTarget)
        {
            target = Vector2.Distance(transform.position, originalPosition) < 0.1f ? targetPosition : originalPosition;
            movementDirection = target == targetPosition ? Vector2.right : Vector2.left;
            flipTarget = false;
        }
        velocity = (Vector3)movementDirection;
        transform.position += velocity * Time.deltaTime;
    }

}
