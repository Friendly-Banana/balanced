using System;
using UnityEngine;

public class PositionUtil : MonoBehaviour
{
    public enum Position
    {
        Center,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }

    [SerializeField] private Position position;
    [SerializeField] private Vector3 viewportOffset;
    [SerializeField] private Vector3 worldOffset;
    [SerializeField] private bool spanCrossAxis;
    [SerializeField] private float scaleOffset;
    [SerializeField] private bool inset;

#if UNITY_EDITOR
    private void OnValidate()
    {
        Awake();
    }
#endif

    public void Awake()
    {
        transform.position = WorldPosition(position, viewportOffset, worldOffset);
        if (spanCrossAxis)
        {
            if (position is Position.Left or Position.Right)
            {
                var height = 2 * WorldPosition(Position.Top).y;
                transform.localScale = transform.localScale.With(y: height + scaleOffset);
            }
            else if (position is Position.Bottom or Position.Top)
            {
                var width = 2 * WorldPosition(Position.Right).x;
                transform.localScale = transform.localScale.With(x: width + scaleOffset);
            }
        }

        if (inset)
        {
            transform.position += Vector3.Scale(transform.localScale, position switch
            {
                Position.Center => Vector3.zero,
                Position.Left => new Vector3(0.5f, 0, 0),
                Position.TopLeft => new Vector3(0.5f, -0.5f, 0),
                Position.Top => new Vector3(0, -0.5f, 0),
                Position.TopRight => new Vector3(-0.5f, -0.5f, 0),
                Position.Right => new Vector3(-0.5f, 0, 0),
                Position.BottomRight => new Vector3(-0.5f, 0.5f, 0),
                Position.Bottom => new Vector3(0, 0.5f, 0),
                Position.BottomLeft => new Vector3(0.5f, 0.5f, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
            });
        }
    }

    public static Vector3 WorldPosition(Position position) => WorldPosition(position, Vector3.zero, Vector3.zero);

    public static Vector3 WorldPosition(Position position, Vector3 viewportOffset, Vector3 worldOffset)
    {
        Vector3 pos = position switch
        {
            Position.Center => new Vector3(0.5f, 0.5f, 0),
            Position.Left => new Vector3(0, 0.5f, 0),
            Position.TopLeft => new Vector3(0, 1, 0),
            Position.Top => new Vector3(0.5f, 1, 0),
            Position.TopRight => new Vector3(1, 1, 0),
            Position.Right => new Vector3(1, 0.5f, 0),
            Position.BottomRight => new Vector3(1, 0, 0),
            Position.Bottom => new Vector3(0.5f, 0, 0),
            Position.BottomLeft => Vector3.zero,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
        Vector3 v = Camera.main.ViewportToWorldPoint(pos + viewportOffset) + worldOffset;
        v.z = 0;
        return v;
    }
}