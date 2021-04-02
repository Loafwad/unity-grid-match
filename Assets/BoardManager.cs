using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<GameObject> characters = new List<GameObject>();
    public GameObject tile;
    public int xSize, zSize;

    public GameObject[,] grid;

    [SerializeField] private Vector2 shiftDirection;
    [SerializeField] private float shiftSpeed = 1f;
    [SerializeField] private AnimationCurve shitAnimCurve;

    public List<GameObject> matchingTiles;
    private bool toggle = false;
    void Start()
    {
        instance = GetComponent<BoardManager>();

        Vector3 offset = tile.GetComponent<MeshRenderer>().bounds.size;
        CreateBoard(offset.x, offset.z);
        //InvokeRepeating("FindNullTiles", 1.0f, 0.3f);
    }

    public void ToggleShiftDirection()
    {
        toggle = !toggle;
        if (toggle)
        {
            shiftDirection = new Vector2(1, 0);
        }
        else
        {
            shiftDirection = new Vector2(-1, 0);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindNullTiles();
        }
    }
    private void CreateBoard(float xOffset, float zOffset)
    {
        grid = new GameObject[xSize, zSize];

        float startX = transform.position.x;
        float startZ = transform.position.z;

        GameObject[] previousLeft = new GameObject[zSize];
        GameObject previousBelow = null;
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), 0, startZ + (zOffset * z)), tile.transform.rotation);
                newTile.name = "x = " + x + " || " + " z = " + z;
                grid[x, z] = newTile;
                newTile.transform.parent = transform;
                newTile.GetComponent<Tile>().objectGridPosition = new Vector2(x, z);

                List<GameObject> possibleCharacters = new List<GameObject>();
                possibleCharacters.AddRange(characters);

                possibleCharacters.Remove(previousLeft[z]);
                possibleCharacters.Remove(previousBelow);

                GameObject allowedTile = characters[Random.Range(0, possibleCharacters.Count)];
                newTile.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = allowedTile.GetComponent<MeshRenderer>().sharedMaterial;
            }
        }
    }
    [SerializeField] public bool isShifting;
    public void FindNullTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                Transform thisParent = grid[x, z].transform;
                if (grid[x, z].transform.GetChild(0).GetComponent<MeshRenderer>().enabled == false)
                {
                    //current position + shift direction
                    var upix = x + (int)shiftDirection.x;
                    var upiz = z + (int)shiftDirection.y;

                    if (upiz < 0 || upix < 0 || upix >= xSize || upiz >= zSize)
                    {
                        Debug.Log("FindNullTiles(); tried to go outside bounds");
                        continue;
                    }
                    GameObject childAbove = grid[upix, upiz].transform.GetChild(0).gameObject;
                    Transform parentAbove = childAbove.transform.parent;

                    if (childAbove.GetComponent<MeshRenderer>().enabled == false && childAbove != null)
                    {
                        continue;
                    }
                    LeanTween.move(childAbove, grid[x, z].transform.position, shiftSpeed).setEase(shitAnimCurve).setOnComplete(DoBoth);

                    childAbove.transform.parent = thisParent;
                    grid[x, z].transform.GetChild(0).parent = parentAbove;
                }
            }
        }
    }

    void DoBoth()
    {
        ClearMatchesFinalPass();
        FindNullTiles();
    }

    void ClearMatchesFinalPass()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                var downix = x - (int)shiftDirection.x;
                var downiz = z - (int)shiftDirection.y;

                if (downiz < 0 || downix < 0 || downix >= xSize || downiz >= zSize)
                {
                    Debug.Log("FindNullTiles(); tried to go outside bounds");
                    continue;
                }
                GameObject childBelow = grid[downix, downiz].transform.GetChild(0).gameObject;
                if (childBelow.GetComponent<MeshRenderer>().enabled == true)
                {
                    childBelow.transform.parent.GetComponent<Tile>().ClearAllMatches();
                }
            }
        }
    }
}
