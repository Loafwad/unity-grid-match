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
    [SerializeField] bool affectNeighbours;

    [Header("Tile Load")]

    [SerializeField] private AnimationCurve loadRaiseCurve;
    [SerializeField] private AnimationCurve loadRotationCurve;

    [SerializeField] private float loadRaiseTime;
    [SerializeField] private float loadFlipRotationTime;
    [SerializeField] public float loadTileDelay;

    public void TileLoad(GameObject platform, bool toggle)
    {
        if (toggle)
        {
            LeanTween.moveY(platform, platform.transform.parent.position.y + 5, loadRaiseTime).setEase(loadRaiseCurve).setOnComplete(() =>
                    {
                        LeanTween.moveY(platform, platform.transform.position.y - 5, loadRaiseTime).setEase(loadRaiseCurve);
                    });
            LeanTween.rotate(platform, platform.transform.parent.eulerAngles + (platform.transform.eulerAngles * 2), loadFlipRotationTime).setEase(loadRotationCurve);
        }
        else
        {
            LeanTween.moveY(platform, platform.transform.parent.position.y + 5, loadRaiseTime).setEase(loadRaiseCurve).setOnComplete(() =>
                     {
                         LeanTween.moveY(platform, platform.transform.position.y - 5, loadRaiseTime).setEase(loadRaiseCurve);
                     });
            LeanTween.rotate(platform, platform.transform.parent.eulerAngles, loadFlipRotationTime).setEase(loadRotationCurve);
        }

    }
    public void TileSelection(GameObject platform)
    {
        LeanTween.moveY(platform, 1, selectDurationTime).setEase(selectCurve);
        LeanTween.rotate(platform, new Vector3(90, 0, 0), selectRotTime).setEase(selectRotCurve);

    }

    public void TileDeselection(GameObject platform)
    {
        LeanTween.rotate(platform, platform.transform.parent.transform.eulerAngles, deselectRotTime).setEase(deselectRotCurve);
        LeanTween.moveY(platform, platform.transform.position.y - 1, deselectTime).setEase(deselectCurve).setOnComplete(() =>
        {
            platform.transform.position = platform.transform.parent.transform.position;
        });

    }

    public void EnterHover(GameObject platform)
    {
        if (!affectNeighbours)
        {
            LeanTween.moveY(platform, platform.transform.position.y + hoverHeight, hoverEnterTime).setEase(hoverCurve);
        }
        else
        {
            foreach (GameObject item in platform.transform.parent.GetComponent<Tile>().AnimAdjacent(false))
            {
                LeanTween.moveY(item, item.transform.position.y + hoverHeight, hoverEnterTime).setEase(hoverCurve);
            }
        }
    }

    public void ExitHover(GameObject platform)
    {
        if (!affectNeighbours)
        {
            LeanTween.moveY(platform, platform.transform.parent.transform.position.y, hoverExitTime).setEase(hoverCurve);
        }
        else
        {
            foreach (GameObject item in platform.transform.parent.GetComponent<Tile>().AnimAdjacent(false))
            {
                LeanTween.moveY(item, item.transform.parent.transform.position.y, hoverExitTime).setEase(hoverCurve);
            }
        }
    }

}
