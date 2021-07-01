using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Tile : MonoBehaviour
{
    private static Tile previousSelected = null;

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
    private List<GameObject> adjacentTiles;
    MeshRenderer mesh;

    #region Awake/Start/Update
    void Awake()
    {
        anim = GameObject.Find("BoardAnimator").GetComponent<GridAnimations>();
    }
    void Start()
    {
        UpdateTileInfo();

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
        SelectTile(true);
        isSelected = true;
        previousSelected = gameObject.GetComponent<Tile>();
    }

    private void Deselect()
    {
        SelectTile(false);
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
            && board.grid[x, z].GetComponent<Tile>().platform.GetComponent<MeshRenderer>().sharedMaterial == this.platform.GetComponent<MeshRenderer>().sharedMaterial
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

    public List<GameObject> AnimAdjacent(bool includeCorners)
    {
        adjacentTiles = new List<GameObject>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                int ix = (int)objectGridPosition.x + x;
                int iz = (int)objectGridPosition.y + z;
                if (iz < 0 || ix < 0 || ix >= board.xSize || iz >= board.zSize)
                {
                    continue;
                }
                if (includeCorners)
                {
                    if (x == -1 && z == 1 || x == 1 && z == 1 || x == -1 && z == -1 || x == 1 && z == -1)
                    {
                        continue;
                    }
                }
                if (board.grid[ix, iz] != null)
                {
                    if (adjacentTiles.Contains(board.grid[ix, iz].gameObject))
                    {
                        continue;
                    }
                    Debug.Log("Added adjacent tile");
                    adjacentTiles.Add(board.grid[ix, iz].gameObject);
                }
            }
        }
        return adjacentTiles;
    }

    public void ClearAllMatches()
    {
        board.matchingTiles.Clear();
        Vector3 i;
        i.x = objectGridPosition.x;
        i.z = objectGridPosition.y;
        FloodFill((int)i.x, (int)i.z);
        if (board.matchingTiles.Count >= 3)
        {
            for (int j = 0; j < board.matchingTiles.Count; j++)
            {
                Debug.Log("Removed " + board.matchingTiles.Count + " tiles");
                board.matchingTiles[j].transform.GetComponent<Tile>().platform.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void CheckMove()
    {
        previousSelected.ClearAllMatches();
        previousSelected.Deselect();
        ClearAllMatches();
    }

    public void SwitchPosition()
    {
        LeanTween.move(platform, previousSelected.transform.position, animSwitchDuration).setEase(animSwitchCurve).setOnComplete(CheckMove);
        LeanTween.move(previousSelected.platform, gameObject.transform.position, animSwitchDuration).setEase(animSwitchCurve);
        platform.transform.parent = previousSelected.transform;
        previousSelected.platform.transform.parent = this.transform;

        GameObject tempThisPlatform = this.platform;
        this.platform = previousSelected.platform;
        previousSelected.platform = tempThisPlatform;


    }

    public void SelectTile(bool selected)
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
        //anim.EnterHover(platform);
        anim.TileSelection(platform);

    }

    public void OnMouseExit()
    {
        anim.ExitHover(platform);
    }
}



