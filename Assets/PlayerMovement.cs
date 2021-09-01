using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public BoardManager board;
    void Start()
    {
    }


    // Update is called once per frame
    int posX = 0;
    int posZ = 0;
    void Update()
    {
        this.transform.position = board.grid[posX, posZ].gameObject.transform.position;
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (posX + 1 < board.xSize) { posX++; }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (posX - 1 >= 0) { posX--; }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (posZ + 1 < board.zSize) { posZ++; }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (posZ - 1 >= 0) { posZ--; }
        }
    }
}
