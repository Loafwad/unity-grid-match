using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public int xSize, zSize;
    [SerializeField] public List<GameObject> characters = new List<GameObject>();
    [SerializeField] private GameObject prefabTile;
    private GridAnimations anim;

    public GameObject[,] grid;

    [Header("Board Shift Animations")]
    [SerializeField] private float introduceNewTileTime;

    [Header("Board Shift Animations")]
    [SerializeField] private float shiftSpeed = 1f;
    [SerializeField] private AnimationCurve shitAnimCurve;


    [Header("Board Appearance")]
    [SerializeField] private Vector2 offset = new Vector2(0.1f, 0.1f);
    [SerializeField] private bool inheritPrefabOffset = false;
    private Vector3 addativeOffset;

    [Header("Debug Tools")]

    [SerializeField] private bool enableClearSkip = false;
    [SerializeField] private List<GameObject> listOfColumns;

    [Range(0f, 1f)]
    [SerializeField] private float rndRemoveAmount;

    public List<GameObject> matchingTiles;
    public bool enableText;

    private void Awake()
    {
        LeanTween.init((xSize * zSize) + 1000);
    }
    private void Start()
    {

        instance = GetComponent<BoardManager>();
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();


        MeshRenderer prefabTileMesh = prefabTile.GetComponent<MeshRenderer>();
        Vector2 prefabOffset = new Vector2(prefabTileMesh.bounds.size.x, prefabTileMesh.bounds.size.z);
        if (inheritPrefabOffset)
            addativeOffset = prefabOffset;
        else
            addativeOffset = offset * prefabOffset;


        CreateBoard(addativeOffset.x, addativeOffset.y);

    }

    public void RemoveRandomTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                if (Random.value < rndRemoveAmount)
                {
                    grid[x, z].GetComponent<Tile>().DisableTile();
                }
            }
        }
        ShiftBoard();
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
                tempParent.name = "ColumnObject: " + x;
                gridTile.transform.SetParent(tempParent.transform);
                gridTile.GetComponent<Tile>().objectGridPosition = new Vector2(x, z);

                List<GameObject> possibleCharacters = new List<GameObject>();
                possibleCharacters.AddRange(characters);

                possibleCharacters.Remove(previousLeft[z]);
                possibleCharacters.Remove(previousBelow);

                GameObject allowedTile = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
                gridTile.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = allowedTile.GetComponent<MeshFilter>().sharedMesh;
                gridTile.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = allowedTile.GetComponent<MeshRenderer>().sharedMaterial;

                previousLeft[z] = allowedTile;
                previousBelow = allowedTile;
            }
        }
    }

    private int LowestGridPos(int column, List<GameObject> list)
    {
        int _lowest = zSize - 1;

        foreach (GameObject tile in list)
        {
            int _tilePos = (int)GridPosFromWorldPos(tile.transform.position).z;

            if (_tilePos <= _lowest)
            {
                _lowest = _tilePos;
            }
        }
        return _lowest;
    }

    public (int x, int z) GridPosFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / addativeOffset.x) * (int)addativeOffset.x / 2;
        int z = Mathf.RoundToInt(worldPos.z / addativeOffset.y) * (int)addativeOffset.y / 2;

        return (x, z);
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
        animCounter = 0;
        StopAllCoroutines();
        StartCoroutine(Sequence());
    }
    public IEnumerator Sequence()
    {
        yield return new WaitForSeconds(sequenceDelay);
        yield return StartCoroutine(ClearAllMatches());
        for (int x = 0; x < xSize; x++)
        {
            AnimateColumn(x);
        }
    }
    public float sequenceDelay;
    public float boardDelay;
    public IEnumerator ShiftBoardDelay()
    {
        yield return new WaitForSeconds(boardDelay);
        ShiftBoard();
    }

    public IEnumerator ClearAllMatches()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                grid[x, z].GetComponent<Tile>().ClearMatch();
            }
        }
        yield return new WaitForSeconds(sequenceDelay);
    }

    private void AnimateColumn(int x)
    {
        List<GameObject> chain = FindChain(x, false);
        int lowestTile = LowestGridPos(x, chain);
        int nextAvailablePos = NextTilePos(x, lowestTile);
        if (lowestTile == nextAvailablePos || chain.Count == 0)
        {
            StartCoroutine(IntroduceNewTile(FindChain(x, true).Count, x));
            return;
        }

        chain.Reverse();
        float distance = lowestTile - nextAvailablePos;
        float time = distance / shiftSpeed;

        for (int z = 0; z < chain.Count; z++)
        {
            int nextPos = nextAvailablePos + z;
            LeanTween.moveZ(chain[z], grid[x, nextPos].transform.position.z, time).setEase(shitAnimCurve);
            SwapTile(x, GridPosFromWorldPos(chain[z].transform.position).z, nextPos);
        }
        StartCoroutine(NullTileDelay(x, time));
    }

    private IEnumerator NullTileDelay(int x, float time)
    {
        yield return new WaitForSeconds(time);
        AnimateColumn(x);
    }

    void SwapTile(int x, int currentPos, int newPos)
    {
        Tile currentTile = grid[x, currentPos].GetComponent<Tile>();
        Tile nextTile = grid[x, newPos].GetComponent<Tile>();

        GameObject tempPlatform = currentTile.platform;

        currentTile.platform = nextTile.platform;
        nextTile.platform = tempPlatform;

        currentTile.UpdateTileInfo();
        nextTile.UpdateTileInfo();
    }
    public int animCounter = 0;
    private IEnumerator IntroduceNewTile(int amount, int x)
    {
        WaitForSeconds wait = new WaitForSeconds(introduceNewTileTime);
        for (int z = 1; z <= amount; z++)
        {
            int _newZPos = (zSize - 1) - amount + z;
            if (_newZPos > zSize || _newZPos < 0)
            {
                continue;
            }
            GameObject gridTile = grid[x, _newZPos];
            GameObject platform = gridTile.GetComponent<Tile>().platform;
            GameObject allowedTile = characters[Random.Range(0, characters.Count)];
            platform.GetComponent<MeshRenderer>().sharedMaterial = allowedTile.GetComponent<MeshRenderer>().sharedMaterial;
            platform.GetComponent<MeshFilter>().sharedMesh = allowedTile.GetComponent<MeshFilter>().sharedMesh;

            anim.IntroduceNewTile(platform, gridTile.transform.position, (int)grid[zSize - 1, zSize - 1].transform.position.z).setOnComplete(() =>
                {
                    if (amount == z || amount == 0)
                    {
                        if (animCounter >= zSize)
                        {
                            /*  Debug.Log("Anim is done");
                             animCounter = 0;
                             ShiftBoard(); */
                        }
                    }
                });

            platform.GetComponent<MeshRenderer>().enabled = true;
            gridTile.GetComponent<Tile>().UpdateTileInfo();
            yield return wait;
        }
        animCounter++;
        if (animCounter == xSize)
        {
            animCounter = 0;
            StartCoroutine(ShiftBoardDelay());
        }
    }
}
