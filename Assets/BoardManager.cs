using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    private GridAnimations anim;
    public List<GameObject> characters = new List<GameObject>();
    public GameObject prefabTile;
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
    private Vector3 addativeOffset;

    [SerializeField] private List<GameObject> listOfColumns;

    public bool enableText;
    private void Start()
    {

        instance = GetComponent<BoardManager>();
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();


        LeanTween.init(5000);

        MeshRenderer prefabTileMesh = prefabTile.GetComponent<MeshRenderer>();
        Vector2 prefabOffset = new Vector2(prefabTileMesh.bounds.size.x, prefabTileMesh.bounds.size.z);
        if (inheritPrefabOffset)
            addativeOffset = prefabOffset;
        else
            addativeOffset = offset * prefabOffset;


        CreateBoard(addativeOffset.x, addativeOffset.y);

        for (int x = 0; x < xSize; x++)
        {
            CreateGroup(FindChain(x, false), x);
        }

        //InvokeRepeating("ShiftBoard", 0.5f, 0.5f);
    }


    public void FlipAllTiles()
    {
        List<Tile> listTile = new List<Tile>();
        reverse = !reverse;
        for (int x = 0; x < xSize; x++)
        {
            StartCoroutine(DelayAnimation(listTile, x));
        }
    }
    bool reverse;
    IEnumerator DelayAnimation(List<Tile> listTile, int x)
    {
        WaitForSeconds wait = new WaitForSeconds(anim.loadTileDelay);
        for (int z = 0; z < zSize; z++)
        {
            anim.TileLoad(grid[x, z].GetComponent<Tile>().platform, reverse);
            yield return wait;
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
                    //grid[x, z].GetComponent<Tile>().platform.GetComponent<MeshRenderer>().enabled = false;
                    grid[x, z].GetComponent<Tile>().DisableTile();
                }
            }
        }
        ShiftBoard();
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
                GameObject gridTile = Instantiate(prefabTile, new Vector3(startX + (xOffset * x), 0, startZ + (zOffset * z)), prefabTile.transform.rotation);
                gridTile.name = "x = " + x + " || " + " z = " + z;
                grid[x, z] = gridTile;
                tempParent.transform.position = grid[x, 0].transform.position;
                gridTile.transform.SetParent(tempParent.transform);
                gridTile.GetComponent<Tile>().objectGridPosition = new Vector2(x, z);

                List<GameObject> possibleCharacters = new List<GameObject>();
                possibleCharacters.AddRange(characters);

                possibleCharacters.Remove(previousLeft[z]);
                possibleCharacters.Remove(previousBelow);

                GameObject allowedTile = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
                gridTile.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = allowedTile.GetComponent<MeshRenderer>().sharedMaterial;

                previousLeft[z] = allowedTile;
                previousBelow = allowedTile;
            }
        }
    }

    private int LowestGridPos(int column, List<GameObject> list)
    {
        int lowest = xSize - 1;

        foreach (GameObject tile in list)
        {
            int tilePos = (int)GridPosFromWorldPos(tile.transform.position).z;

            if (tilePos <= lowest)
            {
                lowest = tilePos;
            }
        }
        return lowest;
    }

    public (int x, int z) GridPosFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / addativeOffset.x) * (int)addativeOffset.x / 2;
        int z = Mathf.RoundToInt(worldPos.z / addativeOffset.y) * (int)addativeOffset.y / 2;

        return (x, z);
    }

    private List<GameObject> CreateGroup(List<GameObject> chain, int x)
    {
        GameObject columnObject = new GameObject();
        columnObject.transform.parent = this.transform;
        columnObject.transform.position = grid[x, LowestGridPos(x, chain)].transform.position;
        columnObject.transform.name = "ColumnObject: " + x;
        listOfColumns.Add(columnObject);

        return listOfColumns;
    }

    private GameObject PoolGroup(List<GameObject> chain, int x, int columnPos)
    {
        //major performance issue!!
        //note: deatching all children shouldn't be necessary as some objects in the list of chains should already be parented to the columnObject. They just need to be reorganized by possibly using SetSiblingIndex and only parenting the objects of child if they are not already parented to it.

        GameObject columnObject = listOfColumns[x];
        columnObject.transform.DetachChildren();

        columnObject.transform.position = grid[x, columnPos].transform.position;

        for (int j = 0; j < chain.Count; j++)
        {
            chain[j].transform.SetParent(columnObject.transform, true);
        }

        return columnObject;
    }

    private List<GameObject> FindChain(int column, bool filled)
    {
        List<GameObject> chain = new List<GameObject>();
        bool _firstTile = new bool();
        filled = !filled;
        for (int z = zSize - 1; z >= 0; z--)
        {
            Tile tile = grid[column, z].GetComponent<Tile>();

            if (tile.platformMesh == filled)
            {
                _firstTile = filled;
                chain.Add(tile.platform);
            }
            else if (_firstTile)
            {
                return chain;
            }
            else { continue; }
        }
        _firstTile = false;
        return chain;
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

    public void ShiftBoard()
    {
        LTSeq seq = LeanTween.sequence();
        seq.append(() =>
        {
            for (int x = 0; x < xSize; x++)
            {
                AnimateColumn(x);
            }
        });

    }

    void AnimateColumn(int x)
    {
        LTSeq seq = LeanTween.sequence();
        List<GameObject> currentChain = FindChain(x, false);
        int lowestChainPos = LowestGridPos(x, currentChain);
        GameObject currentColumn = PoolGroup(currentChain, x, lowestChainPos);
        int nextTilePos = NextTilePos(x, lowestChainPos);
        if (lowestChainPos == nextTilePos)
        {
            IntroduceNewTile(FindChain(x, true).Count, x);
            return;
        }

        if (lowestChainPos <= 0 || nextTilePos < 0)
        {
            //introduce new tile.
            return;
        }


        LeanTween.moveZ(currentColumn, grid[x, nextTilePos].transform.position.z, shiftSpeed * (lowestChainPos - nextTilePos)).setEase(shitAnimCurve).setOnComplete(() =>
                      {
                          seq.append(() =>
                          {

                              int distanceMoved = lowestChainPos - nextTilePos;

                              for (int z = 0; z < currentChain.Count; z++)
                              {

                                  int currentTilePos = lowestChainPos + z;
                                  int nextPos = lowestChainPos - distanceMoved + z;

                                  Tile currentTile = grid[x, currentTilePos].GetComponent<Tile>();
                                  Tile nextTile = grid[x, nextPos].GetComponent<Tile>();

                                  GameObject tempPlatorm = currentTile.platform;

                                  currentTile.platform = nextTile.platform;
                                  nextTile.platform = tempPlatorm;

                                  nextTile.UpdateTileInfo();
                                  currentTile.UpdateTileInfo();
                              }
                          });

                          seq.append(() =>
                          {
                              AnimateColumn(x);
                          });
                      });
    }

    [SerializeField] List<GameObject> newPos = new List<GameObject>();
    void IntroduceNewTile(int amount, int x)
    {
        //bug-note: does not currently replace a removed tile if,
        //the removed tile was at the top of the chain (column).

        for (int i = 1; i <= amount; i++)
        {
            int _newZPos = (xSize - 1) - amount + i;
            if (_newZPos > zSize || _newZPos < 0)
            {
                continue;
            }
            GameObject gridTile = grid[x, _newZPos];
            GameObject platform = gridTile.GetComponent<Tile>().platform;

            //platform.transform.position = gridTile.transform.position;

            anim.IntroduceNewTile(platform, gridTile.transform.position, zSize, i).append(() =>
            {
                platform.GetComponent<MeshRenderer>().enabled = true;
                gridTile.GetComponent<Tile>().UpdateTileInfo();
            });

        }
    }
}
