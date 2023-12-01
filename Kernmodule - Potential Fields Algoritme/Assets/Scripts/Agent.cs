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
        Vector3 avoidanceForce = AvoidAgentCollisions();
        Vector3 potentialFieldForce = CalculatePotentialFieldForce();

        Vector3 totalForce = attractiveForce + repulsiveForce + avoidanceForce + potentialFieldForce;

        return totalForce;
    }

    Vector3 CalculateAttractiveForce()
    {
        Vector3 agentToGoal = goal.position - transform.position;
        return agentToGoal.normalized;
    }

    Vector3 CalculateRepulsiveForce()
    {
        Vector3 repulsiveForce = Vector3.zero;

        int numRays = 8;
        for (int i = 0; i < numRays; i++)
        {
            float angle = i * 2 * Mathf.PI / numRays;
            Vector3 rayDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, repulsionRadius))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    Vector3 agentToObstacle = transform.position - hit.point;
                    float additionalRepulsion = 1.0f - (hit.distance / repulsionRadius);
                    repulsiveForce += agentToObstacle.normalized * additionalRepulsion * repulsionStrengthFactor;
                }
            }
        }

        return repulsiveForce.normalized;
    }

    Vector3 AvoidAgentCollisions()
    {
        float avoidanceRadius = 1.0f;
        float avoidanceForceFactor = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, avoidanceRadius);
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Agent"))
            {
                Vector3 agentToOtherAgent = transform.position - collider.transform.position;
                float distance = agentToOtherAgent.magnitude;

                if (distance < avoidanceRadius)
                {
                    float avoidanceStrength = 1.0f - (distance / avoidanceRadius);
                    avoidanceForce += agentToOtherAgent.normalized * avoidanceStrength;
                }
            }
        }

        return avoidanceForce.normalized * avoidanceForceFactor;
    }

    Vector3 CalculatePotentialFieldForce()
    {
        float potentialFieldValue = gridManager.GetPotentialFieldValue(transform.position);

        Vector3 potentialFieldForce = -CalculateGradient(potentialFieldValue).normalized;

        return potentialFieldForce;
    }

    Vector3 CalculateGradient(float potentialFieldValue)
    {
        float epsilon = 0.01f;

        float xPlus = gridManager.GetPotentialFieldValue(transform.position + new Vector3(epsilon, 0, 0));
        float xMinus = gridManager.GetPotentialFieldValue(transform.position - new Vector3(epsilon, 0, 0));

        float yPlus = gridManager.GetPotentialFieldValue(transform.position + new Vector3(0, epsilon, 0));
        float yMinus = gridManager.GetPotentialFieldValue(transform.position - new Vector3(0, epsilon, 0));

        float gradientX = (xPlus - xMinus) / (2 * epsilon);
        float gradientY = (yPlus - yMinus) / (2 * epsilon);

        return new Vector3(gradientX, gradientY, 0f);
    }

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

        transform.position = newPosition;
    }
}