using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Tile : MonoBehaviour
{
    private static Tile previousSelected = null;
    private static Tile selected = null;

    private bool isSelected = false;
    public string color;
    BoardManager board = BoardManager.instance;
    private GridAnimations anim;
    [SerializeField] public GameObject platform;

    [Header("Switch Tile Animation")]
    public AnimationCurve animSwitchCurve;
    public float animSwitchDuration = 1f;

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

    public void DisableTile()
    {
        mesh.enabled = false;
        UpdateTileInfo();
    }

    public MeshRenderer UpdateTileInfo()
    {
        mesh = platform.GetComponent<MeshRenderer>();
        color = mesh.sharedMaterial.name;
        platformMesh = mesh.enabled;
        text = platform.GetComponentInChildren<TextMeshPro>();

        //Debug.Log("UPDATED MESH!");

        return mesh;
    }

    TextMeshPro text;

    void Update()
    {

        //UpdateMesh();

        if (!platformMesh || !board.enableText)
        {
            text.enabled = false;
        }
        else
        {
            int zPos = board.GridPosFromWorldPos(platform.transform.position).z;
            text.SetText(zPos.ToString());
            text.enabled = true;
        }
    }
    #endregion
    private void Select()
    {

    }

    private void Deselect()
    {

    }
    void OnMouseDown()
    {

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
                //board.matchingTiles[j].transform.GetComponent<Tile>().platform.GetComponent<MeshRenderer>().enabled = false;
                board.matchingTiles[j].GetComponent<Tile>().DisableTile();
            }
            board.ShiftBoard();
        }
    }

    public void CheckMove()
    {
        previousSelected.ClearAllMatches();
        previousSelected.Deselect();
        selected.Deselect();
        selected.ClearAllMatches();
    }

    public void SwitchPosition()
    {
        LeanTween.move(platform, previousSelected.transform.position, animSwitchDuration).setEase(animSwitchCurve);
        LeanTween.move(previousSelected.platform, selected.transform.position, animSwitchDuration).setEase(animSwitchCurve).setOnComplete(CheckMove);

        Tile tempSelected = selected;
        selected = previousSelected;
        previousSelected = tempSelected;

        previousSelected.UpdateTileInfo();
        selected.UpdateTileInfo();
    }

    public void SelectPlayAnim(bool selected)
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
    public void OnMouseEnter()
    {
        anim.EnterHover(platform);
    }
    public void OnMouseExit()
    {
        anim.ExitHover(platform);
    }
}



