using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [SerializeField] private TestMap01 map; // TestMap01Ç÷ÇÃéQè∆

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        Time.timeScale = 1;
    }


    private void Start()
    {

    }


}
