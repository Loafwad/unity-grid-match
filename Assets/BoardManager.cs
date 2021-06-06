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
    [Header("Board Appearance")]
    [SerializeField] private Vector2 offset = new Vector2(0.1f, 0.1f);
    [SerializeField] private bool inheritPrefabOffset = false;

    [Header("Delay Adjustment")]

    [SerializeField] float TileShuffleDelay = 0.5f;
    [SerializeField] float TileClearDelay = 0.5f;

    [Header("Debug Tools")]

    [SerializeField] private bool enableClearSkip = false;
    void Start()
    {
        instance = GetComponent<BoardManager>();

        LeanTween.init(5000);

        MeshRenderer prefabTileMesh = tile.GetComponent<MeshRenderer>();
        Vector2 prefabOffset = new Vector2(prefabTileMesh.bounds.size.x, prefabTileMesh.bounds.size.z);
        Vector2 addativeOffset;
        if (inheritPrefabOffset)
            addativeOffset = prefabOffset;
        else
            addativeOffset = offset * prefabOffset;

        CreateBoard(addativeOffset.x, addativeOffset.y);
    }

    public void ToggleShiftDirection()
    {
        toggle = !toggle;
        if (toggle)
        {
            shiftDirection = new Vector2(0, 1);
        }
        else
        {
            shiftDirection = new Vector2(0, -1);
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
                if (enableClearSkip)
                {
                    if (x % 2 == 0 && z % 2 != 0 || x % 2 != 0 && z % 2 == 0)
                    {
                        Debug.Log("odd number found");
                        continue;
                    }
                }
                GameObject gridTile = Instantiate(tile, new Vector3(startX + (xOffset * x), 0, startZ + (zOffset * z)), tile.transform.rotation);
                gridTile.name = "x = " + x + " || " + " z = " + z;
                grid[x, z] = gridTile;
                gridTile.transform.parent = this.transform;
                gridTile.GetComponent<Tile>().objectGridPosition = new Vector2(x, z);

                List<GameObject> possibleCharacters = new List<GameObject>();
                possibleCharacters.AddRange(characters);

                possibleCharacters.Remove(previousLeft[z]);
                possibleCharacters.Remove(previousBelow);

                GameObject allowedTile = characters[Random.Range(0, possibleCharacters.Count)];
                gridTile.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = allowedTile.GetComponent<MeshRenderer>().sharedMaterial;
            }
        }
    }
    [SerializeField] public bool isShifting;

    [SerializeField] List<Transform> tempListOfTiles = new List<Transform>();

    private List<Tile> FindEmptyGridTiles(int column)
    {
        List<Tile> listOfNullTiles = new List<Tile>();
        for (int z = 0; z < zSize; z++)
        {
            Tile currentGridTile = grid[column, z].GetComponent<Tile>();
            if (currentGridTile.platform.GetComponent<MeshRenderer>().enabled == false)
            {
                listOfNullTiles.Add(currentGridTile);
            }
        }
        Debug.Log("Found: " + listOfNullTiles.Count + "null tiles in column: " + column);
        return listOfNullTiles;
    }

    private int LowestYPosInlist(int column, List<GameObject> listOfTiles)
    {
        int lowest = xSize;
        foreach (GameObject tile in listOfTiles)
        {
            if ((int)tile.transform.position.y <= lowest)
            {
                lowest = (int)tile.transform.position.y;
            }
        }
        Debug.Log("Found lowest tile at: " + lowest);
        return lowest;
    }

    int lowestNullTile;

    public void CreateGroup(int column, List<GameObject> platformChain)
    {
        GameObject columnObject = new GameObject();
        columnObject.transform.parent = this.transform;
        columnObject.transform.position = new Vector2(column, LowestYPosInlist(column, platformChain));

        foreach (GameObject platform in platformChain)
        {
            platform.transform.parent = columnObject.transform.parent;
        }
    }

    private List<GameObject> FindChain(int column)
    {
        List<GameObject> platformChain = new List<GameObject>();
        for (int z = 0; z < zSize; z++)
        {
            GameObject _platform = grid[column, z].GetComponent<Tile>().platform;
            if (_platform.GetComponent<MeshRenderer>().enabled == true)
            {
                platformChain.Add(_platform);
            }
        }
        return platformChain;
    }

    //called after initial local match
    public void ShiftBoard()
    {
        for (int x = 0; x < xSize; x++)
        {
            CreateGroup(x, FindChain(x));
            for (int z = 0; z < zSize; z++)
            {
            }
        }
    }
    /* 
        public void FindNullTiles()
        {
            LTSeq seq = LeanTween.sequence();

            for (int x = 0; x < xSize; x++)
            {
                lowestNullTile = LowestNullTileInList(x, FindNullTilesInColumn(x));

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
                            Debug.Log("x: " + x + " z: " + z + " tried to go outside bounds");
                            continue;
                        }

                        GameObject childAbove = grid[upix, upiz].transform.GetChild(0).gameObject;
                        Transform parentAbove = grid[upix, upiz].transform;
                        GameObject thisChild = grid[x, z].transform.GetChild(0).gameObject;

                        if (childAbove.GetComponent<MeshRenderer>().enabled != true && parentAbove != null)
                        {
                            //skips iteration on tile if
                            // - child above does not have a color
                            // - parent above is not empty
                            continue;
                        }
                        if (thisChild.GetComponent<MeshRenderer>().enabled == true && thisChild.transform.parent == null)
                        {
                            continue;
                        }

                        grid[x, z].transform.GetChild(0).parent = parentAbove;
                        childAbove.transform.parent = thisParent;
                        Tile tile = parentAbove.GetComponent<Tile>();

                        //error: object reference not set to instance of object
                        //note: LTseq.addOn;
                        if (!tempListOfTiles.Contains(tile.transform))
                        {
                            seq.append(() => tile.isShifting = true);
                            seq.append(() => tile.triedToMove = true); //for debug
                            seq.append(() => tempListOfTiles.Add(tile.transform));
                        }
                        seq.append(TileShuffleDelay);
                        Debug.Log("tweens runnings" + LeanTween.tweensRunning);

                        //Terrible solution with terrible performance.
                        //LeanTween.move(childAbove, grid[x, z].transform.position, shiftSpeed).setEase(shitAnimCurve).setOnComplete(() => tile.isShifting = false);
                        FindNullTiles();
                        seq.append(TileClearDelay);
                        seq.append(() => ClearTilesIfIdle(seq));
                    }
                }
            }
        }
    */
    private void ClearTilesIfIdle(LTSeq seq)
    {
        for (int i = 0; i < tempListOfTiles.Count; i++)
        {
            if (tempListOfTiles[i].gameObject.GetComponent<Tile>().isShifting == false)
            {
                //ToggleShiftDirection();
                Debug.LogWarning("Clearing Tiles");
                tempListOfTiles.Clear();
                seq.append(TileShuffleDelay);
                //seq.append(FindNullTiles);
            }
            else
            {
                Debug.LogWarning("Tiles are still shifting, be patient");
            }
        }
    }
}
