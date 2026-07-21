using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Player pacman;
    [SerializeField] private Fruit fruit;
    [SerializeField] private float superPacmanTime = 7f;
    [SerializeField] private TextMeshProUGUI scoreText, hiScoreText;
    [SerializeField] private AnimationClip pacmanDeadAnimation;
    [SerializeField] private List<Ghost> ghosts;
    [SerializeField] private List<Image> pacmanLifes;
    [SerializeField] private List<Image> fruitsCounters;
    [SerializeField] private GameObject points;

    private int _highScore, _level;
    private Coroutine _superPacmanTimer;
    private Point[] _pointsArray;
    private string _dataPath, _fullSavePath,_dataJson = "/gameData.json";

    #region UnityMethods

    private void Awake()
    {
        _dataPath = Application.persistentDataPath;
        _fullSavePath = _dataPath + _dataJson;
        if (!File.Exists(_fullSavePath))
        {
            File.Create(_fullSavePath);
        }
        LoadData();
        hiScoreText.text = $"{_highScore}";
    }

    void Start()
    {
        pacman.OnSuperPacmanMode += SuperPacman;
        pacman.OnGhostCollision += PacmanLifes;
        pacman.OnDied += PacmanDied;
        pacman.OnFruitEaten += FruitEaten;

        _highScore = int.Parse(hiScoreText.text);

        _level = 1;
        _pointsArray = points.GetComponentsInChildren<Point>();
        StartCoroutine(PauseTime(1.5f));
        foreach (var fruitCounter in fruitsCounters)
        {
            fruitCounter.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateScore();
        if (pacman.Score > _highScore)
        {
            hiScoreText.text = $"{pacman.Score}";
            _highScore = pacman.Score;
        }

        if (IsAllPointsDisabled())
        {
            StartCoroutine(ResetWorld());
        }

        if (pacman.EatenPoints == 70 || pacman.EatenPoints == 170)
        {
            fruit.ActivateFruit(_level);
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

        StartCoroutine(PauseTime(0.5f));
        _superPacmanTimer = StartCoroutine(SuperPacmanTimer());
    }

    private void PacmanLifes()
    {
        ChangeLifeCounter();
        StartCoroutine(StopEntitys(pacmanDeadAnimation.length + 1f, true));
        pacman.animator.SetBool("isDead", true);
    }

    private void PacmanDied()
    {
        ChangeLifeCounter();
        StopAllCoroutines();
        StartCoroutine(StopEntitys(pacmanDeadAnimation.length + 1f));
        pacman.animator.SetBool("isDead", true);
        StartCoroutine(ChangeToGameOverScene());
    }

    private void FruitEaten(int fruitId)
    {
        if (!fruitsCounters[fruitId].gameObject.activeSelf)
        {
            fruitsCounters[fruitId].gameObject.SetActive(true);
            return;
        }

        var amountTextMesh = fruitsCounters[fruitId].GetComponentInChildren<TextMeshProUGUI>();
        string amountText = amountTextMesh.text;
        amountText = amountText.Remove(0, 1);

        int amountInt = int.Parse(amountText);
        amountTextMesh.text = $"x{amountInt + 1}";
    }
    #endregion

    #region Coroutines
    private IEnumerator StopEntitys(float time, bool resetEntitys = false)
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

    private IEnumerator ChangeToGameOverScene()
    {
        SaveData();
        yield return new WaitForSeconds(pacmanDeadAnimation.length);
        SceneManager.LoadScene("GameOver");
    }
    private IEnumerator ResetWorld()
    {
        yield return StopEntitys(1f, true);
        foreach (var point in _pointsArray)
        {
            point.gameObject.SetActive(true);
        }

        foreach (var life in pacmanLifes)
        {
            life.gameObject.SetActive(true);
        }
        
        _level += 1;
        StopAllCoroutines();
        StartCoroutine(PauseTime(1.5f));
    }
    #endregion

    private void SaveData()
    {
        GameData gameData = new GameData() { highScore = _highScore};
        string dataJson = JsonUtility.ToJson(gameData);
        File.WriteAllText(_fullSavePath, dataJson);
    }

    private void LoadData()
    {
        GameData gameData = new();
        string dataText = File.ReadAllText(_fullSavePath);

        if (dataText == "")
        {
            _highScore = 0;
            return;
        }
        gameData = JsonUtility.FromJson<GameData>(dataText);

        _highScore = gameData.highScore;
    }
    
    private void ResetEntitys()
    {
        pacman.ResetState();
        foreach (var ghost in ghosts)
        {
            ghost.ResetState();
        }
    }


    private void ChangeLifeCounter()
    {
        pacmanLifes[pacman.Lifes].gameObject.SetActive(false);
    }

    private void UpdateScore()
    {
        scoreText.text = $"{pacman.Score}";
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
