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
        // Calculate forces and update position
        Vector3 totalForce = CalculateForces();
        UpdateVelocityAndPosition(totalForce);

        // Check proximity to the goal
        float goalProximityThreshold = 1f; // Adjust as needed
        float distanceToGoal = Vector3.Distance(transform.position, goal.position);

        if (distanceToGoal < goalProximityThreshold)
        {
            // Agent is close to the goal
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
        // This force guides the agent towards the goal
        Vector3 agentToGoal = goal.position - transform.position;
        return agentToGoal.normalized; // Normalize the force
    }

    Vector3 CalculateRepulsiveForce()
    {
        Vector3 repulsiveForce = Vector3.zero;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repulsionRadius);
        foreach (Collider collider in hitColliders)
        {
            // Exclude the agent itself from the repulsion
            if (collider.gameObject != gameObject)
            {
                Vector3 agentToObstacle = transform.position - collider.transform.position;
                float distance = agentToObstacle.magnitude;

                // Increase repulsion strength
                float strength = Mathf.Clamp01(1.0f - distance / repulsionRadius);
                repulsiveForce += agentToObstacle.normalized * strength * repulsionStrengthFactor;
            }
        }

        return repulsiveForce.normalized;
    }

    void UpdateVelocityAndPosition(Vector3 force)
    {
        // Apply the force to the agent's velocity
        Vector3 newVelocity = force * speed;

        // Restrict movement to the x and y axes
        newVelocity.z = 0;

        // Move the agent based on the updated velocity
        Vector3 newPosition = transform.position + newVelocity * Time.deltaTime;

        // Ensure the agent stays within the grid bounds
        float minX = 0.0f;
        float maxX = (gridManager.gridSizeX - 1) * gridManager.cellSize;
        float minY = 0.0f;
        float maxY = (gridManager.gridSizeY - 1) * gridManager.cellSize;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Avoid overlapping with other agents
        AvoidAgentCollisions(ref newPosition);

        transform.position = newPosition;
    }

    void AvoidAgentCollisions(ref Vector3 newPosition)
    {
        float avoidanceRadius = 1.0f; // Adjust as needed

        Collider[] hitColliders = Physics.OverlapSphere(newPosition, avoidanceRadius);
        foreach (Collider collider in hitColliders)
        {
            // Exclude the agent itself from the collision avoidance
            if (collider.gameObject != gameObject && collider.CompareTag("Agent"))
            {
                Vector3 agentToOtherAgent = newPosition - collider.transform.position;
                float distance = agentToOtherAgent.magnitude;

                // If too close to another agent, adjust position to avoid collision
                if (distance < avoidanceRadius)
                {
                    Vector3 avoidanceDirection = agentToOtherAgent.normalized;
                    newPosition += avoidanceDirection * (avoidanceRadius - distance);
                }
            }
        }
    }
}