using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousSelection : MonoBehaviour
{
    [SerializeField] private float scaleTime;
    [SerializeField] private AnimationCurve scaleCurve;
    public static PreviousSelection instance;

    void Start()
    {
        instance = this.GetComponent<PreviousSelection>();
        Animate();
    }
    public void SetPosition(Vector3 newPos)
    {
        this.transform.position = newPos;
    }

    void Animate()
    {
        LeanTween.scale(this.gameObject, new Vector3(1.2f, 1.2f, 1), scaleTime).setEase(scaleCurve).setOnComplete(() =>
        {
            LeanTween.scale(this.gameObject, new Vector3(1, 1, 1), scaleTime).setEase(scaleCurve).setOnComplete(() => Animate());

        });
    }
}
