using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleManager : MonoBehaviour {

    public float tickRate = 1f;
    public int gridWidth = 20;
    public int gridHeight = 20;

    bool[,] cells;
    GameObject[,] cubes;
    bool isRunning = false;

	void Start () {
        cells = new bool[gridWidth, gridHeight];
        cubes = new GameObject[gridWidth, gridHeight];

        AdjustCamera();
        GenerateGrid();
	}

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
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

        if (Input.GetButtonDown("Jump"))
        {
            if (!isRunning)
            {
                isRunning = true;
                StartCoroutine(Tick());
            }
        }
    }

    void AdjustCamera()
    {
        Camera cam = GetComponent<Camera>();
        cam.transform.position = new Vector3(gridWidth / 2, gridHeight / 2);
        cam.orthographicSize = gridHeight / 2 + 5;
    }
    
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

    void CreateCube(int x, int y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x + 0.5f, y + 0.5f, 1);
        cube.GetComponent<MeshRenderer>().enabled = false;
        cubes[x, y] = cube;
    }

    IEnumerator Tick()
    {
        bool[,] nextGenCells = new bool[gridWidth, gridHeight];
        int neighbors = 0;

        while (true)
        {
            for (int y = 1; y < gridHeight - 1; y++)
            {
                for (int x = 1; x < gridWidth - 1; x++)
                {
                    neighbors = 0;

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

                    // If cell has too little or too many neighbors
                    if (neighbors < 2 || neighbors > 3)
                    {
                        // Die
                        nextGenCells[x, y] = false;
                    }
                    else
                    {
                        // If cell is dead but has the right amount of neighbors
                        if (!cells[x, y] && neighbors == 3)
                        {
                            // Reproduce
                            nextGenCells[x, y] = true;
                        }
                        else if (!cells[x, y])
                        {
                            nextGenCells[x, y] = false;
                        }
                        else
                        {
                            nextGenCells[x, y] = true;
                        }         
                    }
                }
            }

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    cells[x, y] = nextGenCells[x, y];
                    cubes[x, y].GetComponent<MeshRenderer>().enabled = cells[x, y];
                }
            }

            yield return new WaitForSeconds(tickRate);
        }
    }

}
