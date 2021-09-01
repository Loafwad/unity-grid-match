using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Tile : MonoBehaviour
{
    private BoardManager board = BoardManager.instance;
    private CurrentSelection selectionSquare;
    private PreviousSelection prevSelectionSquare;

    public GameObject floodFillCube;
    private GridAnimations anim;


    public static Tile CurrentSelected;
    public static Tile PreviousSelected;

    [Header("Switch Tile Animation")]
    public AnimationCurve animSwitchCurve;
    public float animSwitchDuration = 1f;
    [SerializeField] private string color;
    [SerializeField] public GameObject platform;

    [Header("Position Info")]
    [SerializeField] public Vector2 objectGridPosition;
    [SerializeField] private Vector3 objectPosition;
    [SerializeField] public bool isShifting;

    [Header("State info")]
    public bool platformMesh;


    [SerializeField]
    private List<GameObject> adjacentTiles;
    MeshRenderer mesh;

    #region Awake/Start/Update

    void Awake()
    {
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();
    }
    void Start()
    {
        prevSelectionSquare = GameObject.Find("PreviousSelected").GetComponent<PreviousSelection>();
        selectionSquare = GameObject.Find("CurrentSelected").GetComponent<CurrentSelection>();
        mesh = UpdateTileInfo();

        int x = (int)board.GridPosFromWorldPos(this.transform.position).z;
        int z = (int)board.GridPosFromWorldPos(this.transform.position).z;
        objectPosition = new Vector3(transform.position.x, 0, transform.position.z);
    }

    #endregion

    public void DisableTile()
    {
        mesh.enabled = false;
        UpdateTileInfo();
    }
    void OnMouseEnter()
    {
        if (CurrentSelected != null)
        {
            prevSelectionSquare.SetPosition(this.transform.position);
        }
        if (CurrentSelected != this)
        {
            anim.EnterHover(this.platform);
        }
    }

    void OnMouseExit()
    {
        if (CurrentSelected != this)
        {
            anim.ExitHover(this.platform);
        }
    }

    void OnMouseDown()
    {
        if (CurrentSelected == null)
        {
            Select();
            selectionSquare.SetPosition(this.transform.position);

            prevSelectionSquare.SetPosition(new Vector3(-100f, -100f, 0));


            CurrentSelected = this.GetComponent<Tile>();
        }
        else if (CurrentSelected == this)
        {
            Deselect();

            selectionSquare.SetPosition(new Vector3(-100f, -100f, 0));

            prevSelectionSquare.SetPosition(CurrentSelected.transform.position);
            PreviousSelected = CurrentSelected;
            prevSelectionSquare.SetPosition(new Vector3(-100f, -100f, 0));

            CurrentSelected = null;
        }
        else
        {
            prevSelectionSquare.SetPosition(CurrentSelected.transform.position);
            PreviousSelected = CurrentSelected;
            selectionSquare.SetPosition(this.transform.position);
            CurrentSelected = this.GetComponent<Tile>();
            SwitchPlatforms(CurrentSelected.gameObject, PreviousSelected.gameObject);

        }
    }

    public MeshRenderer UpdateTileInfo()
    {
        mesh = platform.GetComponent<MeshRenderer>();
        color = mesh.sharedMaterial.name;
        platformMesh = mesh.enabled;

        return mesh;
    }
    int completed = 0;

    public void SwitchPlatforms(GameObject objectA, GameObject objectB)
    {
        GameObject platformA = objectA.GetComponent<Tile>().platform;
        GameObject platformB = objectB.GetComponent<Tile>().platform;

        LeanTween.move(platformA, platformB.transform.position, animSwitchDuration).setEase(animSwitchCurve).setOnComplete(() => completed++);
        LeanTween.move(platformB, platformA.transform.position, animSwitchDuration).setEase(animSwitchCurve).setOnComplete(() => completed++);



        //seperate this top function later
        objectA.GetComponent<Tile>().Deselect();
        selectionSquare.SetPosition(new Vector3(-100f, -100f, 0));
        CurrentSelected = null;

        objectB.GetComponent<Tile>().Deselect();
        prevSelectionSquare.SetPosition(new Vector3(-100f, -100f, 0));
        PreviousSelected = null;

        GameObject tempPlatform = objectA.GetComponent<Tile>().platform;
        objectA.GetComponent<Tile>().platform = objectB.GetComponent<Tile>().platform;
        objectB.GetComponent<Tile>().platform = tempPlatform;

        objectA.GetComponent<Tile>().UpdateTileInfo();
        objectB.GetComponent<Tile>().UpdateTileInfo();

        StartCoroutine(board.ShiftBoardDelay());
    }

    void FloodFill(int x, int z)
    {
        if (x >= 0 && x < board.xSize && z >= 0 && z < board.zSize)
        {
            Material GetMaterial(GameObject gridTile)
            {
                return gridTile.GetComponent<Tile>().platform.GetComponent<MeshRenderer>().sharedMaterial;
            }
            UpdateTileInfo();
            if (board.grid[x, z] != null
            && GetMaterial(board.grid[x, z]) == mesh.sharedMaterial
            && !board.matchingTiles.Contains(board.grid[x, z]))
            {
                board.matchingTiles.Add(board.grid[x, z]);

                for (int i = 0; i < board.matchingTiles.Count; i++)
                {
                    board.matchingTiles[i].GetComponent<Tile>().floodFillCube.SetActive(true);
                }
                FloodFill(x + 1, z);
                FloodFill(x - 1, z);
                FloodFill(x, z + 1);
                FloodFill(x, z - 1);
            }
        }
    }

    public List<GameObject> GetAllAdjacentTiles(Vector2 position)
    {
        adjacentTiles = new List<GameObject>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }
                int _x = (int)objectGridPosition.x + x;
                int _z = (int)objectGridPosition.y + z;
                if (_z < 0 || _x < 0 || _x >= board.xSize || _z >= board.zSize)
                {
                    continue;
                }
                if (x == -1 && z == 1 || x == 1 && z == 1 || x == -1 && z == -1 || x == 1 && z == -1)
                {
                    continue;
                }
                if (board.grid[_x, _z] != null)
                {
                    adjacentTiles.Add(board.grid[_x, _z].gameObject);
                }
            }
        }
        return adjacentTiles;
    }

    public void ClearMatch()
    {
        for (int i = 0; i < board.matchingTiles.Count; i++)
        {
            board.matchingTiles[i].GetComponent<Tile>().floodFillCube.SetActive(false);
        }
        board.matchingTiles.Clear();

        int _x = (int)objectGridPosition.x;
        int _z = (int)objectGridPosition.y;
        FloodFill(_x, _z);
        if (board.matchingTiles.Count >= 3)
        {
            for (int j = 0; j < board.matchingTiles.Count; j++)
            {
                //Debug.Log("Removed " + board.matchingTiles.Count + " tiles");
                board.matchingTiles[j].GetComponent<Tile>().DisableTile();
                board.matchingTiles[j].GetComponent<Tile>().UpdateTileInfo();
            }
        }
    }

    void Select()
    {
        anim.TileSelection(platform);
    }

    void Deselect()
    {
        anim.TileDeselection(platform);
    }
}
