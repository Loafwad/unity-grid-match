using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private BoardManager board = BoardManager.instance;

    public static GameObject selected;
    public static GameObject previousSelected;
    // Start is called before the first frame update

    void Select(GameObject platform)
    {
        selected = platform;
    }

    void Deselect()
    {
        previousSelected = selected;
        selected = null;
    }
}
