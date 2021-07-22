using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Tile : MonoBehaviour
{
    private BoardManager board = BoardManager.instance;
    private CurrentSelection selectionSquare = CurrentSelection.selection;
    private PreviousSelection prevSelectionSquare = PreviousSelection.selection;
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
    private List<GameObject> _adjacentTiles;
    MeshRenderer mesh;

    #region Awake/Start/Update

    void Awake()
    {
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();
    }
    void Start()
    {
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
    void OnMouseDown()
    {
        if (CurrentSelected == null)
        {
            selectionSquare.SetPosition(this.transform.position);
            prevSelectionSquare.SetPosition(new Vector3(-100f, -100f, 0));
            Select();
            CurrentSelected = this.GetComponent<Tile>();
        }
        else if (CurrentSelected == this)
        {
            Deselect();
            selectionSquare.SetPosition(this.transform.position);
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

    public void SwitchPlatforms(GameObject objectA, GameObject objectB)
    {
        Debug.Log("switching platforms");
        GameObject platformA = objectA.GetComponent<Tile>().platform;
        GameObject platformB = objectB.GetComponent<Tile>().platform;

        LeanTween.move(platformA, platformB.transform.position, animSwitchDuration).setEase(animSwitchCurve);
        LeanTween.move(platformB, platformA.transform.position, animSwitchDuration).setEase(animSwitchCurve);

        GameObject tempPlatform = platformA;
        objectA.GetComponent<Tile>().platform = platformB;
        objectB.GetComponent<Tile>().platform = tempPlatform;

        //seperate this top function later
        objectA.GetComponent<Tile>().Deselect();
        selectionSquare.SetPosition(new Vector3(-100f, -100f, 0));
        CurrentSelected = null;

        objectB.GetComponent<Tile>().Deselect();
        prevSelectionSquare.SetPosition(new Vector3(-100f, -100f, 0));
        PreviousSelected = null;

        objectA.GetComponent<Tile>().UpdateTileInfo();
        objectB.GetComponent<Tile>().UpdateTileInfo();
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
                FloodFill(x + 1, z);
                FloodFill(x - 1, z);
                FloodFill(x, z + 1);
                FloodFill(x, z - 1);
            }
        }
    }

    public List<GameObject> GetAllAdjacentTiles(Vector2 position)
    {
        _adjacentTiles = new List<GameObject>();
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
                    _adjacentTiles.Add(board.grid[_x, _z].gameObject);
                }
            }
        }
        return _adjacentTiles;
    }

    public void ClearAllMatches()
    {
        board.matchingTiles.Clear();
        int _x = (int)objectGridPosition.x;
        int _z = (int)objectGridPosition.y;
        FloodFill(_x, _z);
        if (board.matchingTiles.Count >= 3)
        {
            for (int j = 0; j < board.matchingTiles.Count; j++)
            {
                Debug.Log("Removed " + board.matchingTiles.Count + " tiles");
                board.matchingTiles[j].GetComponent<Tile>().UpdateTileInfo();
                board.matchingTiles[j].GetComponent<Tile>().DisableTile();
            }
            board.ShiftBoard();
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
