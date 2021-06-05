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


    public void TileSelection(GameObject platform)
    {
        LeanTween.moveY(platform, 1, selectDurationTime).setEase(selectCurve);
        LeanTween.rotate(platform, new Vector3(90, 0, 0), selectRotTime).setEase(selectRotCurve);
    }

    public void TileDeselection(GameObject platform)
    {
        LeanTween.moveY(platform, platform.transform.position.y - 1, deselectTime).setEase(deselectCurve).setOnComplete(() => platform.transform.position = platform.transform.parent.transform.position);
        LeanTween.rotate(platform, new Vector3(-90, 0, 0), selectRotTime).setEase(deselectRotCurve);
    }
}
