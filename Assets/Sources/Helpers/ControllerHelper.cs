using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerHelper
{

    public static Vector3 CalculateNormalForce(Vector3 velocity, Vector3 normal)
    {
        float dotProduct = Vector3.Dot(velocity, normal);
        if (dotProduct > 0f)
        {
            dotProduct = 0f;
        }
        Vector3 projection = dotProduct * normal;
        return -projection;
    }

    public static Vector2 CalculateProjection(Vector2 vectorBeingProjected, Vector2 vectorProjectedOn)
    {
        return Vector2.Dot(vectorBeingProjected, vectorProjectedOn) * vectorProjectedOn;
    }
}
