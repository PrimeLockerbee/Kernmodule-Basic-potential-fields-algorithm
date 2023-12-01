using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridSizeX = 25;
    public int gridSizeY = 25;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;
    public GameObject obstaclePrefab;
    public GameObject agentPrefab;
    public GameObject goalPrefab;
    public GameObject goal;
    public float obstacleProbability = 0.1f;
    public int numAgents = 10;

    public Material obstacleMaterial;

    [SerializeField] private float[,] potentialField;

    void Start()
    {
        GenerateGrid();
        CalculatePotentialField();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, y * cellSize, 0f);
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                cell.transform.position = cellPosition;
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                cell.transform.parent = transform;

                if (Random.value < (float)numAgents / (gridSizeX * gridSizeY))
                {
                    Vector3 agentPosition = new Vector3(x * cellSize, y * cellSize, -1f);
                    GameObject agent = Instantiate(agentPrefab, agentPosition, Quaternion.identity);
                    Agent agentScript = agent.GetComponent<Agent>();
                    agentScript.gridManager = this;
                    agent.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                }

                if (Random.value < obstacleProbability)
                {
                    Vector3 obstaclePosition = new Vector3(x * cellSize, y * cellSize, -1f);
                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    obstacle.transform.position = obstaclePosition;
                    obstacle.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    obstacle.transform.parent = transform;

                    BoxCollider boxCollider = obstacle.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(cellSize, cellSize, cellSize);
                    Renderer renderer = obstacle.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = obstacleMaterial;
                    }
                }
            }
        }

        Vector3 goalPosition = Vector3.zero;
        goalPosition.z = -1f;
        goal = Instantiate(goalPrefab, goalPosition, Quaternion.identity);
    }

    void CalculatePotentialField()
    {
        potentialField = new float[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, y * cellSize, 0f);
                float distanceToGoal = Vector3.Distance(cellPosition, goal.transform.position);
                potentialField[x, y] = distanceToGoal;
            }
        }
    }

    public float GetPotentialFieldValue(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.y / cellSize);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return potentialField[x, y];
    }
}