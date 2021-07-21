using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAnimations : MonoBehaviour
{
    [Header("Tile Select")]
    [SerializeField] private AnimationCurve selectCurve;
    [SerializeField] private AnimationCurve selectRotCurve;
    [SerializeField] private float selectRotTime;
    [SerializeField] private float selectDurationTime;

    [Header("Tile Deselect")]

    [SerializeField] private AnimationCurve deselectCurve;
    [SerializeField] private AnimationCurve deselectRotCurve;
    [SerializeField] private float deselectRotTime;
    [SerializeField] private float deselectTime;

    [Header("Tile Hover")]

    [SerializeField] private AnimationCurve hoverCurve;
    [SerializeField] private float hoverEnterTime;
    [SerializeField] private float hoverExitTime;
    [SerializeField] private float hoverHeight;

    [Header("Tile Load")]

    [SerializeField] private AnimationCurve loadRaiseCurve;
    [SerializeField] private AnimationCurve loadRotationCurve;

    [SerializeField] private float loadRaiseTime;
    [SerializeField] private float loadFlipRotationTime;
    [SerializeField] public float loadTileDelay;

    [SerializeField] private int lrheight;

    [Header("Introduce Tile")]

    [SerializeField] private AnimationCurve introduceCurveY;
    [SerializeField] private float introduceTimeY;
    [SerializeField] int introduceStartPosY;

    [SerializeField] private AnimationCurve introduceCurveZ;
    [SerializeField] private float introduceTimeZ;
    [SerializeField] int introduceStartPosZ;

    [SerializeField] private AnimationCurve introduceRotationCurve;
    [SerializeField] float introduceRotationTime;
    [SerializeField] float introduceDelay;


    LTSeq seq;

    void Start()
    {
        seq = LeanTween.sequence();
    }
    public void TileLoad(GameObject platform, bool toggle)
    {
        platform.transform.position = new Vector3(platform.transform.position.x, 0, platform.transform.position.z);
        platform.transform.eulerAngles = new Vector3(-90, 0, 0);
        LeanTween.moveY(platform, lrheight, loadRaiseTime).setEase(loadRaiseCurve);

        LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), 180, loadFlipRotationTime).setEase(loadRotationCurve);
    }
    public void TileSelection(GameObject platform)
    {
        LeanTween.moveY(platform, 1, selectDurationTime).setEase(selectCurve);
        LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), 180, selectRotTime).setEase(selectRotCurve);
    }

    public void TileDeselection(GameObject platform)
    {
        LeanTween.moveY(platform, new Vector3(0, 0, 0).y, deselectTime).setEase(deselectCurve);
        LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), 180, selectRotTime).setEase(selectRotCurve);
    }

    public void EnterHover(GameObject platform)
    {
        LeanTween.moveY(platform, platform.transform.position.y + hoverHeight, hoverEnterTime).setEase(hoverCurve);
    }

    public void ExitHover(GameObject platform)
    {
        LeanTween.moveY(platform, 0, hoverExitTime).setEase(hoverCurve);
    }

    public void IntroduceNewTile(GameObject platform, Vector3 newPosition, int zSize)
    {
        //seq.append(introduceDelay);

        platform.transform.position = new Vector3(newPosition.x, introduceStartPosY + newPosition.y, newPosition.z + introduceStartPosZ);

        float newRot = platform.transform.eulerAngles.z - (introduceStartPosY);

        //LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), newRot, introduceRotationTime).setEase(introduceRotationCurve);

        LeanTween.moveY(platform, newPosition.y, introduceTimeY).setEase(introduceCurveY);
        LeanTween.moveZ(platform, newPosition.z, introduceTimeZ).setEase(introduceCurveZ);
    }
}
