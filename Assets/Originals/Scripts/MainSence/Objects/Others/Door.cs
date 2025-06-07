using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{

    [Header("ドアの開閉の判定")]
    public bool isOpenDoor = false;

    [Header("鍵が掛かっているかを判定")]
    [SerializeField] public bool isNeedKeyDoor = false;

    [Header("ドア開閉時の回転角度")]
    [SerializeField] float openDirenctionValue = 90.0f;//ドアを開ける角度
    [SerializeField] float closeDirenctionValue = 0.0f;//ドアを閉じる角度

    //プレイヤー
    private Player player;

    [Header("ドア間との距離を測定したいオブジェクトをアタッチ(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;


    [Header("サウンド関連")]
    private AudioSource audioSourceSE;//Door専用のAudioSource
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;

    [Header("サウンドの距離関連(要調整)")]
    [SerializeField] private float maxSoundDistance = 10f; // 音量が最大になる距離
    [SerializeField] private float minSoundDistance = 20f; // 音量が最小になる距離
    [SerializeField] private float maxVolume = 1.0f; // 最大音量
    [SerializeField] private float minVolume = 0.0f; // 最小音量

    private void Start()
    {
        audioSourceSE = MusicController.Instance.GetAudioSource();
    }

    public void DoorSystem() 
    {
        if (isOpenDoor)
        {
            CloseDoor();
        }
        else 
        {
            //鍵が必要な場合
            if (isNeedKeyDoor && player.isHoldKey)
            {
                Debug.Log("解錠しました。");
                player.isHoldKey = false;
                isNeedKeyDoor = false;
            }
            else if (!isNeedKeyDoor)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("施錠されている。");
            }
        }
    }

    //ドアを開ける
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
        DoorSE();
    }

    //ドアを閉める
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
        DoorSE();
    }


    //ドアの開閉の効果音
    void DoorSE() 
    {
        // 効果音制御
        AudioClip currentSE = (isOpenDoor) ? openSE : closeSE;

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // 距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        MusicController.Instance.PlayAudioSE(audioSourceSE, currentSE);

        //音量を設定
        audioSourceSE.volume = volume;
    }


    // 距離に基づく音量を計算するメソッド
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            // 最大音量
            return maxVolume; 
        }
        else if (distance >= minSoundDistance)
        {
            // 最小音量
            return minVolume; 
        }
        else
        {
            // 距離に基づいて線形補間
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}
