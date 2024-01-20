using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
    const float nullValue = float.NaN;
    static float argOrDefault(float arg, float fallback) => float.IsNaN(arg) ? fallback : arg;

    /// <summary>
    /// Copies the old vector and replaces the given values.
    /// </summary>
    public static Vector3 With(this Vector3 old, float x = nullValue, float y = nullValue, float z = nullValue)
    {
        return new Vector3(argOrDefault(x, old.x), argOrDefault(y, old.y), argOrDefault(z, old.z));
    }

    /// <summary>
    /// Copies the old vector and replaces the given values.
    /// </summary>
    public static Vector2 With(this Vector2 old, float x = nullValue, float y = nullValue)
    {
        return new Vector2(argOrDefault(x, old.x), argOrDefault(y, old.y));
    }

    /// <summary>
    /// Copies the old vector and adds the given values.
    /// </summary>
    public static Vector3 Add(this Vector3 old, float x = 0, float y = 0, float z = 0)
    {
        return new Vector3(old.x + x, old.y + y, old.z + z);
    }

    /// <summary>
    /// Copies the old vector and adds the given values.
    /// </summary>
    public static Vector2 Add(this Vector2 old, float x = 0, float y = 0)
    {
        return new Vector2(old.x + x, old.y + y);
    }

    /// <summary>
    /// Returns a random Element from the array. Uses Random.Range
    /// </summary>
    public static T RandomElement<T>(this T[] array)
    {
        if (array.Length == 0)
        {
            return default;
        }

        return array[Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }

    /// <summary>
    /// An actual Log function taking everything as parameters
    /// </summary>
    public static void Log(params object[] objects)
    {
        Debug.Log(string.Join(", ", objects));
    }

    public static void ClearChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }
}