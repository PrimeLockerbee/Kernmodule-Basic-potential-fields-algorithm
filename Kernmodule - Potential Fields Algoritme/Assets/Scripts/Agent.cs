using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float speed = 5.0f;
    public float repulsionRadius = 1.5f;
    public float repulsionStrengthFactor = 4.0f;

    public Transform goal;
    public GridManager gridManager;

    private void Start()
    {
        goal = gridManager.goal.transform;
    }

    void Update()
    {
        Vector3 totalForce = CalculateForces();
        UpdateVelocityAndPosition(totalForce);

        float goalProximityThreshold = 1f;
        float distanceToGoal = Vector3.Distance(transform.position, goal.position);

        if (distanceToGoal < goalProximityThreshold)
        {
            Disappear();
        }
    }

    void Disappear()
    {
        Destroy(gameObject);
    }

    Vector3 CalculateForces()
    {
        Vector3 attractiveForce = CalculateAttractiveForce();
        Vector3 repulsiveForce = CalculateRepulsiveForce();

        Vector3 totalForce = attractiveForce + repulsiveForce;

        return totalForce;
    }

    Vector3 CalculateAttractiveForce()
    {
        Vector3 agentToGoal = goal.position - transform.position;
        return agentToGoal.normalized; // Normalize the force
    }

    Vector3 CalculateRepulsiveForce()
    {
        Vector3 repulsiveForce = Vector3.zero;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repulsionRadius);
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject != gameObject)
            {
                Vector3 agentToObstacle = transform.position - collider.transform.position;

                float distanceToObstacle = agentToObstacle.magnitude;
                float additionalRepulsion = 1.0f;

                if (distanceToObstacle < repulsionRadius)
                {
                    additionalRepulsion *= 1.0f - (distanceToObstacle / repulsionRadius);
                }

                repulsiveForce += agentToObstacle.normalized * additionalRepulsion;
            }
        }

        return repulsiveForce.normalized;
    }

    //Vector3 CalculateRepulsiveForce()
    //{
    //    Vector3 repulsiveForce = Vector3.zero;

    //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, repulsionRadius);
    //    foreach (Collider collider in hitColliders)
    //    {
    //        if (collider.gameObject != gameObject)
    //        {
    //            Vector3 agentToObstacle = transform.position - collider.transform.position;
    //            float distanceToObstacle = agentToObstacle.magnitude;

    //            float additionalRepulsion = 1.0f - (distanceToObstacle / repulsionRadius);
    //            repulsiveForce += agentToObstacle.normalized * additionalRepulsion;
    //        }
    //    }

    //    return repulsiveForce.normalized;
    //}

    void UpdateVelocityAndPosition(Vector3 force)
    {
        Vector3 newVelocity = force * speed;

        newVelocity.z = 0;

        Vector3 newPosition = transform.position + newVelocity * Time.deltaTime;

        float minX = 0.0f;
        float maxX = (gridManager.gridSizeX - 1) * gridManager.cellSize;
        float minY = 0.0f;
        float maxY = (gridManager.gridSizeY - 1) * gridManager.cellSize;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        AvoidAgentCollisions(ref newPosition);

        transform.position = newPosition;
    }

    void AvoidAgentCollisions(ref Vector3 newPosition)
    {
        float avoidanceRadius = 1.0f;
        float avoidanceForceFactor = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(newPosition, avoidanceRadius);
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Agent"))
            {
                Vector3 agentToOtherAgent = newPosition - collider.transform.position;
                float distance = agentToOtherAgent.magnitude;

                if (distance < avoidanceRadius)
                {
                    float avoidanceStrength = 1.0f - (distance / avoidanceRadius);
                    avoidanceForce += agentToOtherAgent.normalized * avoidanceStrength;
                }
            }
        }

        newPosition += avoidanceForce * avoidanceForceFactor;
    }
}