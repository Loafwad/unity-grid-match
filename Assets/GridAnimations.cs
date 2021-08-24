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

    void Start()
    {
        LTSeq seq = LeanTween.sequence();
    }

    public void TileSelection(GameObject platform)
    {
        LeanTween.moveY(platform, 1, selectDurationTime).setEase(selectCurve);
        LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), 180, selectRotTime).setEase(selectRotCurve).setOnComplete(() =>
        {
            platform.transform.rotation = new Quaternion(0, 0, 0, 0);
        });
    }

    public void TileDeselection(GameObject platform)
    {
        LeanTween.moveY(platform, new Vector3(0, 0, 0).y, deselectTime).setEase(deselectCurve);
        LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), 180, selectRotTime).setEase(selectRotCurve).setOnComplete(() =>
        {
            platform.transform.rotation = new Quaternion(0, 0, 0, 0);
        });
    }

    public void EnterHover(GameObject platform)
    {
        LeanTween.moveY(platform, platform.transform.position.y + hoverHeight, hoverEnterTime).setEase(hoverCurve);
    }

    public void ExitHover(GameObject platform)
    {
        LeanTween.moveY(platform, 0, hoverExitTime).setEase(hoverCurve);
    }

    public LTDescr IntroduceNewTile(GameObject platform, Vector3 newPosition, int zSize)
    {
        platform.transform.position = new Vector3(newPosition.x, introduceStartPosY + newPosition.y, zSize + introduceStartPosZ);

        float newRot = platform.transform.eulerAngles.z - (introduceStartPosY);

        //LeanTween.rotateAroundLocal(platform, new Vector3(1, 0, 0), newRot, introduceRotationTime).setEase(introduceRotationCurve);
        return (
        /* LeanTween.moveY(platform, newPosition.y, introduceTimeY).setEase(introduceCurveY) */
        LeanTween.moveZ(platform, newPosition.z, introduceTimeZ).setEase(introduceCurveZ)
        );
    }
}
