using UnityEngine;
using static GameController;

public class HomeController : MonoBehaviour
{
    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    [Header("�T�E���h�֘A")]
    public AudioSource audioSourceBGM;
    private readonly int homeSceneBGMid = 1; // �z�[���V�[��BGM��ID

    void Awake()
    {
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);

        //�S�Ă�BGM�̏�Ԃ�Stop�ɕύX
        sO_BGM.StopAllBGM();

        //audioSourceBGM��ݒ�TODO(2�Ԗڂ̗v�f�ɒǉ�����H)
        //audioSourceBGM = MusicController.Instance.GetAudioSource();

        //�z�[���V�[��BGM���Đ�TODO
        //MusicController.Instance.PlayBGM(homeSceneBGMid);
    }
}
