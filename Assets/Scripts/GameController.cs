using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private Player pacman;
    [SerializeField] private Fruit fruit;
    [SerializeField] private float superPacmanTime = 7f;
    [SerializeField] private TextMeshProUGUI lifesText, scoreText, hiScoreText, levelText;
    [SerializeField] private List<Ghost> ghosts;
    [SerializeField] private GameObject points;

    private int _highScore, _level;
    private Coroutine _superPacmanTimer;
    private Point[] _pointsArray;

    #region UnityMethods
    void Start()
    {
        pacman.OnSuperPacmanMode += SuperPacman;
        pacman.OnGhostCollision += PacmanLifes;
        pacman.OnDied += PacmanDied;

        _highScore = int.Parse(hiScoreText.text);

        _level = 1;
        _pointsArray = points.GetComponentsInChildren<Point>();
        StartCoroutine(PauseTime(1.5f));
    }

    void Update()
    {
        UpdateScore();
        if (pacman.Score > _highScore)
        {
            hiScoreText.text = $"{pacman.Score}";
        }

        if (IsAllPointsDisabled())
        {
            ResetWorld();
            _level += 1;
            UpdateLevel();
        }

        if (pacman.EatenPoints == 70 || pacman.EatenPoints == 170)
        {
            fruit.SetID(_level);
            fruit.gameObject.SetActive(true);
            StartCoroutine(DisableFruit());
        }
    }
    #endregion

    #region EventsSubscription
    private void SuperPacman()
    {
        foreach (var ghost in ghosts)
        {
            if (ghost.ActualState != GhostState.Eaten && ghost.ActualState != GhostState.Idle) ghost.SetFrightenedMode(true);
        }
        
        if (_superPacmanTimer !=  null)
        {
            StopCoroutine(_superPacmanTimer);
        }

        StartCoroutine(PauseTime(1.5f));
        _superPacmanTimer = StartCoroutine(SuperPacmanTimer());
    }

    private void PacmanLifes()
    {
        ChangeLifeCounter();
        StartCoroutine(StopEntitys(2f, true));
    }

    private void PacmanDied()
    {
        ChangeLifeCounter();
        StopAllCoroutines();
        StartCoroutine(StopEntitys(2f));
        SceneManager.LoadScene("GameOver");
    }

    #endregion

    #region Coroutines
    private IEnumerator StopEntitys(float time,bool resetEntitys = false)
    {
        pacman.speed = 0f;
        foreach (var ghost in ghosts)
        {
            ghost.speed = 0f;
        }
        yield return new WaitForSeconds(time);
        if (resetEntitys) ResetEntitys();
    }

    private IEnumerator SuperPacmanTimer()
    {
        yield return new WaitForSeconds(superPacmanTime);
        
        foreach (var ghost in ghosts)
        {
            if (ghost.ActualState == GhostState.Frightened) ghost.SetFrightenedMode(false);
        }
        pacman.ChangeSuperPacmanState(false);
        _superPacmanTimer = null;
    }

    private IEnumerator DisableFruit()
    {
        yield return new WaitForSeconds(9.5f);
        
        fruit.gameObject.SetActive(false);
    }

    private IEnumerator PauseTime(float time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }
    #endregion
    private void ResetEntitys()
    {
        pacman.ResetState();
        foreach (var ghost in ghosts)
        {
            ghost.ResetState();
        }
    }

    private void ResetWorld()
    {
        foreach (var point in _pointsArray)
        {
            point.gameObject.SetActive(true);
        }
        StopAllCoroutines();
        StartCoroutine(StopEntitys(2f, true));
    }

    private void ChangeLifeCounter()
    {
        lifesText.text = $"{pacman.Lifes}";
    }

    private void UpdateScore()
    {
        scoreText.text = $"{pacman.Score}";
    }

    private void UpdateLevel()
    {
        levelText.text = $"{_level}";
    }

    private bool IsAllPointsDisabled()
    {
        foreach (var point in _pointsArray)
        {
            if (point.gameObject.activeInHierarchy) return false;
        }
        return true;
    }
}
