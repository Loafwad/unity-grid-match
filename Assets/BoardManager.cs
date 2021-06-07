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
    Vector3 addativeOffset;
    private void Start()
    {
        instance = GetComponent<BoardManager>();

        LeanTween.init(5000);

        MeshRenderer prefabTileMesh = tile.GetComponent<MeshRenderer>();
        Vector2 prefabOffset = new Vector2(prefabTileMesh.bounds.size.x, prefabTileMesh.bounds.size.z);
        if (inheritPrefabOffset)
            addativeOffset = prefabOffset;
        else
            addativeOffset = offset * prefabOffset;

        CreateBoard(addativeOffset.x, addativeOffset.y);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShiftBoard();
        }
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

    private void CreateBoard(float xOffset, float zOffset)
    {
        grid = new GameObject[xSize, zSize];

        float startX = transform.position.x;
        float startZ = transform.position.z;

        GameObject[] previousLeft = new GameObject[zSize];
        GameObject previousBelow = null;
        for (int x = 0; x < xSize; x++)
        {
            GameObject tempParent = new GameObject();
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
                gridTile.transform.parent = tempParent.transform;
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

    private List<GameObject> FindEmptyGridTiles(int column)
    {
        List<GameObject> listOfNullTiles = new List<GameObject>();
        for (int z = zSize - 1; z >= 0; z--)
        {
            if (grid[column, z].GetComponent<Tile>().platform.GetComponent<MeshRenderer>().enabled == false)
            {
                listOfNullTiles.Add(grid[column, z]);
            }
        }
        Debug.Log("Found: " + listOfNullTiles.Count + " null tiles in column: " + column);
        return listOfNullTiles;
    }

    private int LowestGridPos(int column, List<GameObject> list)
    {
        int lowest = xSize - 1;
        foreach (GameObject tile in list)
        {
            if (GridTileFromWorldPos(tile.transform.position).z <= lowest)
            {
                lowest = GridTileFromWorldPos(tile.transform.position).z;
            }
            else
            {
                return lowest;
            }
        }
        if (list.Count == 0)
        {
            return 0;
        }
        Debug.Log("Found lowest tile at: " + lowest);
        return lowest;
    }

    public (int x, int z) GridTileFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / addativeOffset.x) * (int)addativeOffset.x / 2;
        int z = Mathf.RoundToInt(worldPos.z / addativeOffset.y) * (int)addativeOffset.y / 2;

        return (x, z);
    }

    int lowestNullTile;

    private GameObject CreateGroup(int column, List<GameObject> chain)
    {
        GameObject columnObject = new GameObject();
        columnObject.transform.parent = this.transform;
        columnObject.transform.position = grid[column, LowestGridPos(column, chain)].transform.position;
        columnObject.transform.name = "ColumnObject: " + column;

        foreach (GameObject platform in chain)
        {
            platform.transform.parent = columnObject.transform;
        }
        return columnObject;
    }

    private void DestroyGroup(GameObject group)
    {
        //group.transform.DetachChildren();
        //Destroy(group);
    }

    private List<GameObject> FindChain(int column)
    {
        List<GameObject> chain = new List<GameObject>();
        for (int z = zSize - 1; z >= 0; z--)
        {
            MeshRenderer platformMesh = grid[column, z].GetComponent<Tile>().platform.GetComponent<MeshRenderer>();

            if (platformMesh.enabled == true)
            {
                chain.Add(grid[column, z].GetComponent<Tile>().platform);
            }
            else
            {
                Debug.Log("Found: " + chain.Count + " in current chain: " + column);
                return chain;
            }
        }
        Debug.Log("Found: " + chain.Count + " in current chain: " + column);
        return chain;
    }

    //called after initial local match
    public void ShiftBoard()
    {
        LTSeq seq = LeanTween.sequence();
        for (int x = 0; x < xSize; x++)
        {
            List<GameObject> nullTileList = FindEmptyGridTiles(x);

            List<GameObject> currentChain = FindChain(x);

            int lowestChainPos = LowestGridPos(x, currentChain);
            int lowestNullPos = LowestGridPos(x, nullTileList);

            GameObject currentColumn = CreateGroup(x, currentChain);

            for (int z = 0; z < zSize; z++)
            {

            }
            seq.append(LeanTween.move(currentColumn, grid[x, lowestNullPos].transform.position, shiftSpeed).setEase(shitAnimCurve).setOnComplete(() => DestroyGroup(currentColumn)));
        }
    }
    /* 
        public void FindNullTiles()
        {
            c

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
