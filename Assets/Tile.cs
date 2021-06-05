using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    private static Tile previousSelected = null;

    private MeshRenderer render;
    private bool isSelected = false;
    public string color;
    BoardManager board = BoardManager.instance;
    private GridAnimations anim;
    [SerializeField] public GameObject platform;

    [Header("Switch Tile Animation")]
    public AnimationCurve animSwitchCurve;
    public float animSwitchDuration = 1f;

    [Header("Rotation On Select Animation")]
    [SerializeField] private AnimationCurve animRotateCurve;
    [SerializeField] private float rotationTime = 1f;

    [Header("Position Info")]
    [SerializeField] public Vector2 objectGridPosition;
    [SerializeField] private Vector3 objectPosition;
    [SerializeField] public bool isShifting;
    [SerializeField] public bool triedToMove;

    [SerializeField]
    private List<GameObject> adjacentTiles;

    #region Awake/Start/Update
    void Awake()
    {
        render = platform.GetComponent<MeshRenderer>();
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();
    }
    void Start()
    {
        objectPosition = new Vector3(transform.position.x, 0, transform.position.z);
        color = this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.name;
    }
    void Update()
    {
        platform = gameObject.transform.GetChild(0).gameObject;
    }
    #endregion
    private void Select()
    {
        Hover(true);
        isSelected = true;
        previousSelected = gameObject.GetComponent<Tile>();
    }

    private void Deselect()
    {
        Hover(false);
        isSelected = false;
        previousSelected = null;
    }
    void OnMouseDown()
    {
        if (isSelected)
        {
            Deselect();
        }
        else
        {
            if (previousSelected == null)
            {
                Select();
            }
            else
            {
                if (GetAllAdjacentTiles(objectGridPosition).Contains(previousSelected.gameObject))
                {
                    SwitchPosition();
                }
                else
                {
                    previousSelected.GetComponent<Tile>().Deselect();
                    Select();
                }
            }
        }
    }

    void FloodFill(int x, int z)
    {
        if (x >= 0 && x < board.xSize && z >= 0 && z < board.zSize)
        {
            if (board.grid[x, z] != null
            && board.grid[x, z].transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial == this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial
            && !board.matchingTiles.Contains(board.grid[x, z].gameObject))
            {
                board.matchingTiles.Add(board.grid[x, z].gameObject);
                FloodFill(x + 1, z);
                FloodFill(x - 1, z);
                FloodFill(x, z + 1);
                FloodFill(x, z - 1);
            }
        }
    }
    public GameObject ObjecAtPosition(int x, int z)
    {
        return board.grid[x, z].gameObject;
    }


    public List<GameObject> GetAllAdjacentTiles(Vector2 position)
    {
        adjacentTiles = new List<GameObject>();
        adjacentTiles.Clear();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }
                //Debug.Log("index is " + x + "-" + z);
                //int ix = (int)position.x + x;
                //int iz = (int)position.y + z;

                int ix = (int)objectGridPosition.x + x;
                int iz = (int)objectGridPosition.y + z;
                if (iz < 0 || ix < 0 || ix >= board.xSize || iz >= board.zSize)
                {
                    continue;
                }
                if (x == -1 && z == 1 || x == 1 && z == 1 || x == -1 && z == -1 || x == 1 && z == -1)
                {
                    continue;
                }
                if (board.grid[ix, iz] != null)
                {
                    Debug.Log("Added adjacent tile");
                    adjacentTiles.Add(board.grid[ix, iz].gameObject);
                }
            }
        }
        return adjacentTiles;
    }

    //world position is always twice the grid position (dont know why lol)
    public void ClearAllMatches()
    {
        board.matchingTiles.Clear();
        Vector3 i;
        //i.x = Mathf.Round(transform.position.x / 2);
        i.x = objectGridPosition.x;
        i.z = objectGridPosition.y;
        //i.z = Mathf.Round(transform.position.z / 2);
        FloodFill((int)i.x, (int)i.z);
        //Instantiate(red, new Vector3(i.x * 2, 0, i.z * 2), Quaternion.identity);
        if (board.matchingTiles.Count >= 3)
        {
            for (int j = 0; j < board.matchingTiles.Count; j++)
            {
                Debug.Log("Removed " + board.matchingTiles.Count + " tiles");
                color = "null";
                board.matchingTiles[j].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void CheckMove()
    {
        previousSelected.ClearAllMatches();
        previousSelected.Deselect();
        ClearAllMatches();
        board.FindNullTiles();
    }

    public void SwitchPosition()
    {
        LeanTween.move(platform, previousSelected.transform.position, animSwitchDuration).setEase(animSwitchCurve).setOnComplete(CheckMove);
        LeanTween.move(previousSelected.platform, gameObject.transform.position, animSwitchDuration).setEase(animSwitchCurve);
        platform.transform.parent = previousSelected.transform;
        previousSelected.platform.transform.parent = this.transform;
    }

    public void Hover(bool selected)
    {
        if (selected)
        {
            anim.TileSelection(platform);
        }
        else
        {
            anim.TileDeselection(platform);
        }
    }

}


