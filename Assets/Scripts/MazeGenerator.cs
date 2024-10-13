using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10; // Width of the maze
    public int height = 10; // Height of the maze
    public int signs = 7; // Number of signs in the maze
    public GameObject wallPrefab; // Prefab for walls
    public GameObject floorPrefab; // Prefab for floor

    public GameObject playerPrefab;
    public GameObject signsPrefab;

    private bool[,] visited; // 2D array to track visited cells
    private System.Random rand = new System.Random();

    Vector2Int[] directions = { // Define possible directions (Right, Left, Up, Down)
            new Vector2Int(1, 0), 
            new Vector2Int(-1, 0), 
            new Vector2Int(0, 1), 
            new Vector2Int(0, -1) 
        };

    void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        visited = new bool[width, height]; // Initialize the visited array

        //Find mid of maze
        var midX = Mathf.RoundToInt(width/2);
        var midY = Mathf.RoundToInt(height/2);

        CreateMaze(midX, midY); // Start the maze generation from mid

        DrawMaze(); // Draw the maze based on the visited cells

        if (SetupPlayerAndSigns(midX, midY)){
            //Start Game
        }
        else {
            //Draw another maze
            Debug.Log("Failed to set all signs!!!");
        }


    }

     bool SetupPlayerAndSigns(int midX, int midY)
    {
        Instantiate(playerPrefab, new Vector3(midX*2f, 1f, midY*2f), Quaternion.identity); //Instantiate player in the middle of maze

        int signsSet = 0;
        bool[,] outPos = new bool[width, height];;

        int attemped = 0;
        while (signsSet < signs || attemped >= 10000){
            int x = Random.Range(1, width);
            int y = Random.Range(1, height);
            if (visited[x, y] && !outPos[x,y] && IsInCorner(x,y)){
                GameObject s = Instantiate(signsPrefab, new Vector3(x*2f, 0.5f, y*2f), Quaternion.identity); //Instantiate sign at random position
                RotateTowardsClearPath(x , y, s);
                outPos[x,y] = true;
                signsSet++;
                attemped = 0;
            }
            else
                attemped++; //fail safe
        }

        return signsSet == signs;
    }

    void CreateMaze(int x, int y)
    {
        visited[x, y] = true; // Mark the current cell as visited

        // Shuffle directions for randomness
        Shuffle(directions);

        foreach (var direction in directions)
        {
            int nx = x + direction.x * 2; // Calculate new x position
            int ny = y + direction.y * 2; // Calculate new y position

            // Check if the neighbor is within bounds and not visited
            if (IsInBounds(nx, ny) && !visited[nx, ny])
            {
                // Remove the wall between the current cell and the neighbor
                visited[x + direction.x,y + direction.y] = true; // Mark wall removal (neighbor cell)
                CreateMaze(nx, ny); // Recursively create the maze from the new cell
            }
        }
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y]) // If not visited, place a wall
                {
                    Instantiate(wallPrefab, new Vector3(x * 2, 1, y * 2), Quaternion.identity);
                }
                else // If visited, place a floor
                {
                    Instantiate(floorPrefab, new Vector3(x * 2, 0, y * 2), Quaternion.identity);
                }
            }
        }
    }

    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width && y > 0 && y < height; // Check bounds
    }

    void Shuffle(Vector2Int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1); // Random index
            Vector2Int temp = array[i];
            array[i] = array[j];
            array[j] = temp; // Swap
        }
    }

    bool IsInCorner(int x, int y){

        int paths = 0;
        foreach (Vector2Int v in directions){
            if (visited[x + v.x, y + v.y])
                paths++;
        }

        return paths == 1;
    }

    void RotateTowardsClearPath(int x, int y, GameObject s){
        foreach (Vector2Int v in directions){
            if (visited[x + v.x, y + v.y])
                if (v.x == 1) //Right
                    s.transform.Rotate(0f, 90f, 0f);
                else if (v.x == -1) //Left
                    s.transform.Rotate(0f, -90f, 0f);
                else if (v.y == -1) //Down
                    s.transform.Rotate(0f, 180f, 0f);
        }
            
    }
}