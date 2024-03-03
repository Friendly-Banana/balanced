using UnityEngine;

public class FilledBar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    public void SetValue(float percent)
    {
        fill.localScale = fill.localScale.With(x: percent);
    }
}