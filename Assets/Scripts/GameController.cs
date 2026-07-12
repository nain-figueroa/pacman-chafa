using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Player pacman;
    [SerializeField] private List<Ghost> ghosts;

    void Start()
    {
        
    }

    void Update()
    {
        if (pacman.SuperPacman)
        {
            foreach (var ghost in ghosts)
            {
                if (!ghost.ScaredMode) ghost.SetScaredMode(true);
            }
        }
    }
}
