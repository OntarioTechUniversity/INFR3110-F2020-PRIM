using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [Header("Maze Attributes")]
    public GameObject quad;
    public Vector3 size;
    private List<Tile> grid;

    private List<GameObject> horizontalWalls;
    private List<GameObject> verticalWalls;
    private int tileSize = 2;

    [Header("Prim's Algorithm")]
    private HashSet<Tile> visitedCells;
    private Tile currentCell;
    private HashSet<Tile> frontierCells;
    private Tile randomFrontierCell;

    public GameObject player;
    public Vector3 playerStart;
    public Tile distinationTile;


    // Start is called before the first frame update
    void Start()
    {
        //initialize
        grid =  new List<Tile>();
        horizontalWalls = new List<GameObject>();
        verticalWalls = new List<GameObject>();
        visitedCells = new HashSet<Tile>();
        frontierCells = new HashSet<Tile>();

        Regenerate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Regenerate();
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Quit();
        }
    }

    /// <summary>
    /// This method builds the ground tiles out and adds each tile to the grid 
    /// </summary>
    private void BuildGround()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var groundTile = Instantiate(quad, new Vector3(tileSize * x, 0.0f , tileSize * z), Quaternion.Euler(90f, 0.0f, 0.0f));
                groundTile.transform.parent = transform;
                groundTile.name = "Ground";
                grid.Add(new Tile(groundTile));
            }
        }
    }

    /// <summary>
    /// This method instantiates each wall and adds the wall to the respective List structure
    /// </summary>
    private void BuildWalls()
    {
        var tileIndex = 0;
        for (int x = 0; x < size.x + 1; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var verticalWall = Instantiate(quad, new Vector3(tileSize * x - (tileSize * 0.5f), tileSize * 0.5f, tileSize * z), Quaternion.Euler(0.0f, -90.0f, 0.0f));
                verticalWall.transform.parent = transform;
                verticalWall.name = "VerticalWall";
                verticalWalls.Add(verticalWall);
            }
        }

        tileIndex = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z + 1; z++)
            {
                var horizontalWall = Instantiate(quad, new Vector3(tileSize * x, tileSize * 0.5f, tileSize * z - (tileSize * 0.5f)), Quaternion.Euler(0.0f, 0.0f, 0.0f));
                horizontalWall.transform.parent = transform;
                horizontalWall.name = "HorizontalWall";
                horizontalWalls.Add(horizontalWall);
            }
        }
    }

    /// <summary>
    /// This method builds all grid references. Each tile has references to each of its walls an neighbours
    /// </summary>
    private void BuildGrid()
    {
        var sizeX = (int)size.x;

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var groundPosition = grid[(x * sizeX) + z].ground.transform.position;
                var bottomPosition = groundPosition + new Vector3(0, 1, -1);
                var topPosition = groundPosition + new Vector3(0, 1, 1);
                var leftPosition = groundPosition + new Vector3(-1, 1, 0);
                var rightPosition = groundPosition + new Vector3(1, 1, 0);
                var left = groundPosition - new Vector3(2, 0, 0);
                var right = groundPosition + new Vector3(2, 0, 0);
                var top = groundPosition + new Vector3(0, 0, 2);
                var bottom = groundPosition - new Vector3(0, 0, 2);

                // build references to walls
                grid[(x * sizeX) + z].topWall = horizontalWalls.Where(wall => wall.transform.position == topPosition).Select(wall => wall).ToList().First();
                grid[(x * sizeX) + z].bottomWall = horizontalWalls.Where(wall => wall.transform.position == bottomPosition).Select(wall => wall).ToList().First();
                grid[(x * sizeX) + z].leftWall = verticalWalls.Where(wall => wall.transform.position == leftPosition).Select(wall => wall).ToList().First();
                grid[(x * sizeX) + z].rightWall = verticalWalls.Where(wall => wall.transform.position == rightPosition).Select(wall => wall).ToList().First();

                if (grid[(x * sizeX) + z].topWall != null)
                {
                    grid[(x * sizeX) + z].wallCount++;
                }

                if (grid[(x * sizeX) + z].bottomWall != null)
                {
                    grid[(x * sizeX) + z].wallCount++;
                }

                if (grid[(x * sizeX) + z].leftWall != null)
                {
                    grid[(x * sizeX) + z].wallCount++;
                }

                if (grid[(x * sizeX) + z].rightWall != null)
                {
                    grid[(x * sizeX) + z].wallCount++;
                }


                // build references to neighbours
                if (x > 0)
                {
                    grid[(x * sizeX) + z].left = grid.Where(tile => tile.ground.transform.position == left).Select(tile => tile).ToList().First();
                }

                if (x < size.x - 1)
                {
                    grid[(x * sizeX) + z].right = grid.Where(tile => tile.ground.transform.position == right).Select(tile => tile).ToList().First();
                }

                if (z > 0)
                {
                    grid[(x * sizeX) + z].bottom = grid.Where(tile => tile.ground.transform.position == bottom).Select(tile => tile).ToList().First();
                }

                if (z < size.z - 1)
                {
                    grid[(x * sizeX) + z].top = grid.Where(tile => tile.ground.transform.position == top).Select(tile => tile).ToList().First();
                }
            }
        }
    }

    /// <summary>
    /// Execute Prim's Algorithm and build the maze (non-recursive version)
    /// </summary>
    private void ExecutePrims()
    { 
        currentCell = grid[Random.Range(0, grid.Count)];
       
        while (visitedCells.Count < grid.Count)
        {
            visitedCells.Add(currentCell);
            if (currentCell.top != null)
            {
                frontierCells.Add(currentCell.top);
            }

            if (currentCell.bottom != null)
            {
                frontierCells.Add(currentCell.bottom);
            }

            if (currentCell.left != null)
            {
                frontierCells.Add(currentCell.left);
            }

            if (currentCell.right != null)
            {
                frontierCells.Add(currentCell.right);
            }

            randomFrontierCell = frontierCells.ToList()[Random.Range(0, frontierCells.Count)];

            foreach (var cell in visitedCells)
            {
                if (cell.wallCount > 2)
                {
                    if (randomFrontierCell.topWall == cell.bottomWall)
                    {
                        Destroy(randomFrontierCell.topWall);
                        randomFrontierCell.topWall = null;
                        cell.wallCount--;
                    }

                    else if (randomFrontierCell.bottomWall == cell.topWall)
                    {
                        Destroy(randomFrontierCell.bottomWall);
                        randomFrontierCell.bottomWall = null;
                        cell.wallCount--;
                    }

                    else if (randomFrontierCell.leftWall == cell.rightWall)
                    {
                        Destroy(randomFrontierCell.leftWall);
                        randomFrontierCell.bottomWall = null;
                        cell.wallCount--;
                    }

                    else if (randomFrontierCell.rightWall == cell.leftWall)
                    {
                        Destroy(randomFrontierCell.rightWall);
                        randomFrontierCell.bottomWall = null;
                        cell.wallCount--;
                    }
                }
            }

            currentCell = randomFrontierCell;
            frontierCells.Remove(currentCell);
        }

    }

    /// <summary>
    /// method to regenerate the maze, select player's start and select random destination tile
    /// </summary>
    private void Regenerate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        grid.Clear();
        horizontalWalls.Clear();
        verticalWalls.Clear();
        visitedCells.Clear();
        frontierCells.Clear();
        currentCell = null;
        randomFrontierCell = null;

        BuildGround();
        BuildWalls();
        BuildGrid();

        ExecutePrims();

        SelectPlayerStart();
        SelectDestination();
    }

    /// <summary>
    /// Select a random location for the player to start
    /// </summary>
    private void SelectPlayerStart()
    {
        var randomStartTile = grid[Random.Range(0, grid.Count)];
        var randomPlayerStart = randomStartTile.ground.transform.position + new Vector3(0, 10, 0);
        randomStartTile.ground.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = randomPlayerStart;
        player.GetComponent<CharacterController>().enabled = true;
        playerStart = randomStartTile.ground.transform.position;
    }

    /// <summary>
    /// Select a random destination for the player
    /// This function is recursive - it will call itself until the minimum distance criteria is met
    /// </summary>
    private void SelectDestination()
    {
        var randomDestinationTile = grid[Random.Range(0, grid.Count)];
        var randomDestination = randomDestinationTile.ground.transform.position;

        var distance = Vector3.Distance(playerStart, randomDestination);

        if (distance < Mathf.Max(size.x, size.z) - 0.5f)
        {
            SelectDestination();
        }
        else
        {
            distinationTile = randomDestinationTile;
            distinationTile.ground.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }
    }

    private void Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
