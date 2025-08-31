using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;

    public static Player instance; 

    private void Awake()
    {
        if (instance == null) 
            instance = this;
    }

    //method for damage proceccing by 'Player'
    public virtual void GetDamage(int damage)   
    {
        Destruction();
    }    

    //'Player's' destruction procedure
    void Destruction()
    {
        if (destructionFX != null)
            Instantiate(destructionFX, transform.position, Quaternion.identity);
        else
            Debug.LogWarning("Player.destructionFX is null; skipping VFX.");

        Destroy(gameObject);
    }
}
















