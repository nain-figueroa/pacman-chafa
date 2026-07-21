using System;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    [SerializeField]private int value;
    [SerializeField] private bool superPoint = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite superSprite;

    void Start()
    {
        if (superPoint) spriteRenderer.sprite = superSprite;
    }

    public int Value => superPoint ? value * 2 : value;
    public bool SuperPoint => superPoint;
}
