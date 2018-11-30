using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleManager : MonoBehaviour {

    public bool isRunning = false;
    public float tickRate = 1f;
    public int gridWidth = 20;
    public int gridHeight = 20;

    bool[,] cells;
    GameObject[,] cubes;

    // Having these bools are not necessary
    //  but makes setting the state more obvious
    bool alive = true;
    bool dead = false;

	void Start () {
        cells = new bool[gridWidth, gridHeight];
        cubes = new GameObject[gridWidth, gridHeight];

        AdjustCamera();
        GenerateGrid();
        StartCoroutine(Tick());
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ToggleCellState();
        }

        if (Input.GetButtonDown("Jump"))
        {
            isRunning = !isRunning;
        }
    }

    /// <summary>
    /// Runs a world tick to update the cells based on the rules
    /// </summary>
    /// <returns></returns>
    IEnumerator Tick()
    {
        while (true)
        {
            if (isRunning)
                UpdateGrid(RunGeneration());
            yield return new WaitForSeconds(tickRate);
        }
    }

    /// <summary>
    /// Toggles the clicked cell between alive and dead
    /// </summary>
    void ToggleCellState()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            int x = (int)Mathf.Floor(hit.point.x);
            int y = (int)Mathf.Floor(hit.point.y);
            cells[x, y] = !cells[x, y];
            cubes[x, y].GetComponent<MeshRenderer>().enabled = cells[x, y];
        }
    }

    /// <summary>
    /// Position the camera center of the grid and expand larger than the grid
    /// </summary>
    void AdjustCamera()
    {
        Camera cam = GetComponent<Camera>();
        cam.transform.position = new Vector3(gridWidth / 2, gridHeight / 2);
        cam.orthographicSize = gridHeight / 2 + 5;
    }
    
    /// <summary>
    /// Creates a grid with cubes to represent each of the cells
    /// </summary>
    void GenerateGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                CreateCube(x, y);
            }
        }
    }

    /// <summary>
    /// Creates a cube to be displayed on the grid at the designated cell
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void CreateCube(int x, int y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x + 0.5f, y + 0.5f, 1);
        cube.GetComponent<MeshRenderer>().enabled = false;
        cubes[x, y] = cube;
    }

    /// <summary>
    /// Handles the logic for setting the cells states in the next generation
    /// </summary>
    /// <returns></returns>
    bool[,] RunGeneration()
    {
        bool[,] nextGenCells = new bool[gridWidth, gridHeight];

        for (int y = 1; y < gridHeight - 1; y++)
        {
            for (int x = 1; x < gridWidth - 1; x++)
            {
                int neighbors = GetNumOfNeighbors(x, y);

                if (IsUnderpopulated(neighbors) || IsOverpopulated(neighbors))
                {
                    nextGenCells[x, y] = dead;
                }
                else
                {
                    if (CanReproduce(neighbors, x, y))
                    {
                        nextGenCells[x, y] = alive;
                    }
                    else if (!cells[x, y])
                    {
                        nextGenCells[x, y] = dead;
                    }
                    else
                    {
                        nextGenCells[x, y] = alive;
                    }
                }
            }
        }

        return nextGenCells;
    }

    /// <summary>
    /// Updates each cell within the grid with the new cell states
    /// </summary>
    /// <param name="nextGenCells"></param>
    void UpdateGrid(bool[,] nextGenCells)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                cells[x, y] = nextGenCells[x, y];
                cubes[x, y].GetComponent<MeshRenderer>().enabled = cells[x, y];
            }
        }
    }

    /// <summary>
    /// Checks all the eight direct neighbors of the cell and determines the total
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    int GetNumOfNeighbors(int x, int y)
    {
        int neighbors = 0;

        if (cells[x - 1, y + 1])    // Top Left
            neighbors++;
        if (cells[x, y + 1])        // Top Middle
            neighbors++;
        if (cells[x + 1, y + 1])    // Top Right
            neighbors++;
        if (cells[x + 1, y])        // Middle Right
            neighbors++;
        if (cells[x + 1, y - 1])    // Bottom Right
            neighbors++;
        if (cells[x, y - 1])        // Bottom Middle
            neighbors++;
        if (cells[x - 1, y - 1])    // Bottom Left
            neighbors++;
        if (cells[x - 1, y])        // Middle Left  
            neighbors++;

        return neighbors;
    }

    /// <summary>
    /// Any live cell with fewer than two live neighbors dies, as if by underpopulation
    /// </summary>
    /// <param name="neighbors"></param>
    /// <returns></returns>
    bool IsUnderpopulated(int neighbors)
    {
        return neighbors < 2;
    }

    /// <summary>
    /// Any live cell with more than three live neighbors dies, as if by overpopulation
    /// </summary>
    /// <param name="neighbors"></param>
    /// <returns></returns>
    bool IsOverpopulated(int neighbors)
    {
        return neighbors > 3;
    }

    /// <summary>
    /// Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction
    /// </summary>
    /// <param name="neighbors"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    bool CanReproduce(int neighbors, int x, int y)
    {
        return !cells[x, y] && neighbors == 3;
    }

}
