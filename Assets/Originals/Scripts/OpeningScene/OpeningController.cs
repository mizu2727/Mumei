using UnityEngine;
using static GameController;

public class OpeningController : MonoBehaviour
{
    void Awake()
    {
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
    }
}
