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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveRandomTiles();
        }
    }

    public void RemoveRandomTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                if (Random.value < 0.1f)
                {
                    grid[x, z].GetComponent<Tile>().platform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
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

    private List<GameObject> FindAllEmptyGridTiles(int column)
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
                continue;
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
        Debug.Log("Found lowest tile at: " + lowest + "in column: " + column);
        return lowest;
    }

    public (int x, int z) GridTileFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / addativeOffset.x) * (int)addativeOffset.x / 2;
        int z = Mathf.RoundToInt(worldPos.z / addativeOffset.y) * (int)addativeOffset.y / 2;

        return (x, z);
    }

    int lowestNullTile;

    private GameObject CreateGroup(List<GameObject> chain, int x, int z)
    {
        GameObject columnObject = new GameObject();
        columnObject.transform.parent = this.transform;
        columnObject.transform.position = grid[x, z].transform.position;
        columnObject.transform.name = "ColumnObject: " + x;

        foreach (GameObject platform in chain)
        {
            platform.transform.SetParent(columnObject.transform, true);
        }
        return columnObject;
    }

    private void DestroyGroup(GameObject group)
    {
        //group.transform.DetachChildren();
        //Destroy(group);
    }

    public void UpdatePlatformReference()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                int _z = GridTileFromWorldPos(grid[x, z].GetComponent<Tile>().platform.transform.position).z;
                int _x = GridTileFromWorldPos(grid[x, z].GetComponent<Tile>().platform.transform.position).x;

                grid[_x, _z].GetComponent<Tile>().platform = grid[x, z].GetComponent<Tile>().platform;
            }
        }
    }
    private List<GameObject> FindChain(int column)
    {
        List<GameObject> _chain = new List<GameObject>();
        bool _firstTile = new bool();
        for (int z = zSize - 1; z >= 0; z--)
        {
            Tile tile = grid[column, z].GetComponent<Tile>();

            if (tile.platformMesh == true)
            {
                _firstTile = true;
                _chain.Add(grid[column, z].GetComponent<Tile>().platform);
            }
            else if (_firstTile)
            {
                Debug.Log("Found: " + _chain.Count + " in current chain: " + column);
                _firstTile = false;
                return _chain;
            }
            else { continue; }

        }
        //Debug.Log("Found: " + _chain.Count + " in current chain: " + column);
        return _chain;
    }

    private int NextTilePos(int column, int currentPos)
    {
        for (int z = currentPos; z >= 0; z--)
        {
            bool platformMesh = grid[column, z].GetComponent<Tile>().platformMesh;
            if (platformMesh == false)
            {
                continue;
            }
            else if (z + 1 <= currentPos)
            {
                return z + 1;
            }
        }
        return 0;
    }

    //called after initial local match
    public void ShiftBoard()
    {
        LTSeq seq = LeanTween.sequence();
        for (int x = 0; x < xSize; x++)
        {
            AnimateColumn(x);
        }
    }

    void AnimateColumn(int x)
    {
        UpdatePlatformReference();
        List<GameObject> currentChain = FindChain(x);
        int lowestChainPos = LowestGridPos(x, currentChain);
        GameObject currentColumn = CreateGroup(currentChain, x, lowestChainPos);
        int nextTilePos = NextTilePos(x, lowestChainPos);

        LeanTween.move(currentColumn, grid[x, nextTilePos].transform.position, shiftSpeed).setEase(shitAnimCurve).setOnComplete(() =>
                    {

                        //currentChain = FindChain(x);
                        UpdatePlatformReference();
                        DestroyGroup(currentColumn);
                        //currentColumn = CreateGroup(x, currentChain);

                        if (nextTilePos > 0)
                        {
                            //Debug.Log("Column: " + x + " has a grid pos of: " + GridTileFromWorldPos(currentColumn.transform.position).z);
                            //AnimateColumn(x);

                        }
                    });
    }

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
