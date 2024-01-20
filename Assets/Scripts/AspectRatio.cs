using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatio : MonoBehaviour
{
    public float aspect = 16f / 9;

    void Start()
    {
        GetComponent<Camera>().aspect = aspect;
    }
}