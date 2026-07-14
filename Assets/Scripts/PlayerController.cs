using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 8f;
    public float arrivalDistance = 1f;

    private List<Transform> signsToVisit = new List<Transform>();
    private int currentSignIndex = 0;
    private bool[,] mazeGrid;
    private int gridWidth;
    private int gridHeight;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int currentPathIndex = 0;

    private readonly Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public void SetMaze(bool[,] grid, int width, int height)
    {
        mazeGrid = grid;
        gridWidth = width;
        gridHeight = height;
    }

    public void SetSigns(List<Transform> signs)
    {
        signsToVisit = signs ?? new List<Transform>();
        currentSignIndex = 0;
        currentPath.Clear();
        currentPathIndex = 0;
    }

    void Update()
    {
        if (signsToVisit.Count == 0 || currentSignIndex >= signsToVisit.Count)
            return;

        Transform targetSign = signsToVisit[currentSignIndex];
        if (targetSign == null)
        {
            AdvanceToNextSign();
            return;
        }

        if (HasReachedTarget(targetSign))
        {
            AdvanceToNextSign();
            return;
        }

        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            Vector2Int startCell = WorldToGrid(transform.position);
            Vector2Int targetCell = WorldToGrid(targetSign.position);
            currentPath = FindPath(startCell, targetCell);
            currentPathIndex = 0;
        }

        if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            AdvanceToNextSign();
            return;
        }

        Vector3 targetPosition = GridToWorld(currentPath[currentPathIndex]);
        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            currentPathIndex++;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private bool HasReachedTarget(Transform targetSign)
    {
        if (targetSign == null)
            return true;

        Vector3 targetPosition = targetSign.position;
        targetPosition.y = transform.position.y;
        return (transform.position - targetPosition).sqrMagnitude <= arrivalDistance * arrivalDistance;
    }

    private void AdvanceToNextSign()
    {
        if (signsToVisit.Count == 0)
        {
            currentSignIndex = 0;
            currentPath.Clear();
            currentPathIndex = 0;
            return;
        }

        currentSignIndex = (currentSignIndex + 1) % signsToVisit.Count;
        currentPath.Clear();
        currentPathIndex = 0;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        if (mazeGrid == null || mazeGrid.GetLength(0) != gridWidth || mazeGrid.GetLength(1) != gridHeight)
            return new List<Vector2Int>();

        if (!IsWalkable(start) || !IsWalkable(target))
            return new List<Vector2Int>();

        if (start == target)
            return new List<Vector2Int> { start };

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        frontier.Enqueue(start);
        visited.Add(start);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;
                if (!IsWalkable(next) || visited.Contains(next))
                    continue;

                visited.Add(next);
                parents[next] = current;

                if (next == target)
                {
                    return ReconstructPath(parents, target);
                }

                frontier.Enqueue(next);
            }
        }

        return new List<Vector2Int>();
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parents, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = target;

        while (parents.ContainsKey(current))
        {
            path.Add(current);
            current = parents[current];
        }

        path.Add(current);
        path.Reverse();
        return path;
    }

    private bool IsWalkable(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight && mazeGrid[cell.x, cell.y];
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x / 2f), Mathf.RoundToInt(worldPosition.z / 2f));
    }

    private Vector3 GridToWorld(Vector2Int gridCell)
    {
        return new Vector3(gridCell.x * 2f, 1f, gridCell.y * 2f);
    }
}
