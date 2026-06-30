using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Teleport otherTeleport;

    public Teleport OtherTelepor => otherTeleport;


}
