using System;
using UnityEngine;

public class Point : MonoBehaviour
{
    [SerializeField]private int value;
    [SerializeField] private bool superPoint = false;

    void Start()
    {
        if (superPoint) GetComponent<SpriteRenderer>().color = Color.aquamarine;
    }

    public int Value => superPoint ? value * 2 : value;
    public bool SuperPoint => superPoint;
}
