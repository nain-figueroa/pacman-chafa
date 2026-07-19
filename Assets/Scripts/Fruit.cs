using UnityEngine;

public class Fruit : MonoBehaviour
{
    private int _id;
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
}
