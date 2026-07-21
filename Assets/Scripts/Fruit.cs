using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> sprites;
    private int _id;

    private Dictionary<int, int> levelToIndex = new Dictionary<int, int>()
    {
        {1, 0}, {2, 1}, {3, 2}, {4, 2}, {5, 3}, {6, 3}, {7, 4}, {8, 4}, {9, 5}, {10, 5}, {11, 6}, {12, 6}
    };
    #region UnityMethods
    void Start()
    {
        gameObject.SetActive(false);   
    }
    
    void Update()
    {
        
    }
    #endregion

    public int ID => _id;

    public void SetID(int id)
    {
        _id = id;
    }

    public void ActivateFruit(int level)
    {
        _id = level >= 13 ? 7 : levelToIndex[level];
        gameObject.SetActive(true);
        spriteRenderer.sprite = sprites[_id];
    }
}
