using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public GameObject ground;

    // walls
    public GameObject topWall;
    public GameObject bottomWall;
    public GameObject leftWall;
    public GameObject rightWall;

    public int wallCount;
    
    // neighbours
    public Tile top;
    public Tile bottom;
    public Tile left;
    public Tile right;

    public Tile(GameObject ground)
    {
        this.ground = ground;
        this.topWall = null;
        this.bottomWall = null;
        this.leftWall = null;
        this.rightWall = null;
        this.top = null;
        this.bottom = null;
        this.left = null;
        this.right = null;
        this.wallCount = 0;
    }
}
