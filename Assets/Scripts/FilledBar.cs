using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilledBar : MonoBehaviour
{
    [SerializeField]
    private Transform fill;
    [SerializeField]
    private float initialScale;

#if UNITY_EDITOR
    [SerializeField]
    [Range(0, 1)]
    private float value;

    private void OnValidate()
    {
        if (fill != null)
        {
            initialScale = fill.localScale.x;
            SetValue(value);
        }
    }
#endif

    private void Start()
    {
        initialScale = fill.localScale.x;
    }

    public void SetValue(float percent)
    {
        fill.localScale = fill.localScale.With(x: initialScale * percent);
    }
}
