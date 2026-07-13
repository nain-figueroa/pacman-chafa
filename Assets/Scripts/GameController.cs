using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Player pacman;
    [SerializeField] private List<Ghost> ghosts;
    [SerializeField] private float superPacmanTime = 5f;

    private Coroutine superPacmanTimer;

    #region UnityMethods
    void Start()
    {
        pacman.OnSuperPacmanMode += SuperPacman;
    }

    void Update()
    {

    }
    #endregion

    private void SuperPacman()
    {
        foreach (var ghost in ghosts)
        {
            if (ghost.ActualState != GhostState.Eaten && ghost.ActualState != GhostState.Idle) ghost.SetFrightenedMode(true);
        }
        
        if (superPacmanTimer !=  null)
        {
            StopCoroutine(superPacmanTimer);
        }
        superPacmanTimer = StartCoroutine(SuperPacmanTimer());
    }
    private IEnumerator SuperPacmanTimer()
    {
        yield return new WaitForSeconds(superPacmanTime);
        
        foreach (var ghost in ghosts)
        {
            if (ghost.ActualState == GhostState.Frightened) ghost.SetFrightenedMode(false);
        }
        pacman.ChangeSuperPacmanState(false);
        superPacmanTimer = null;
    }
}
