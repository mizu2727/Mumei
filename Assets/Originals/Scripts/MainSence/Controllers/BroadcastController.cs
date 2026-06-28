using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 放送を管理するコントローラー
/// </summary>
public class BroadcastController : MonoBehaviour
{
    public static BroadcastController instance;

    /// <summary>
    /// 放送スピーカー
    /// </summary>
    private BroadcastSpeaker broadcastSpeaker;

    [Header("放送スピーカーリスト")]
    [SerializeField] private List<GameObject> broadcastSpeakerList;

    /// <summary>
    /// 放送スピーカーの位置のリスト
    /// </summary>
    private List<Transform> broadcastSpeakerTransformList = new();

    /// <summary>
    /// 保存用放送スピーカーリスト関連番号
    /// </summary>
    private int saveBroadcastSpeakerListNumber;

    /// <summary>
    /// 前回保存した放送スピーカーリスト関連番号
    /// </summary>
    private int lastSaveBroadcastSpeakerListNumber;

    /// <summary>
    /// ノイズを流す秒数
    /// </summary>
    private float broadcastNoiseCount;

    /// <summary>
    /// ノイズを流す最大秒数
    /// </summary>
    private const float kMaxBroadcastNoiseCount = 5.0f;

    /// <summary>
    /// ノイズ再生中フラグ
    /// </summary>
    private bool isBroadcastNoiseFlag;


    private void Awake()
    {
        //シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //変数の初期化
        lastSaveBroadcastSpeakerListNumber = 99999;
        isBroadcastNoiseFlag = false;

        //放送スピーカーの位置のリストを作成
        foreach (GameObject obj in broadcastSpeakerList) 
        { 
            broadcastSpeakerTransformList.Add(obj.transform); 
        }
    }

    private void Update()
    {
        //ポーズ中の場合
        if (PauseController.instance.isPause || Time.timeScale == 0) 
        {
            //処理をスキップ
            return;
        }

        //TODO
    }

    /// <summary>
    /// 放送スピーカーをランダムに抽出する処理
    /// </summary>
    private void ChooseRandomBroadcastNumber() 
    {
        //TODO
    }
}
