using UnityEngine;
using static GameController;

public class HomeController : MonoBehaviour
{
    void Awake()
    {
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
    }
}
