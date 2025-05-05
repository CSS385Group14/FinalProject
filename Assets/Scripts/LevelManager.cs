using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public Transform startPoint;
    public Transform[] path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        main = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gameOver() {
        Debug.Log("Game over");
    }
}
