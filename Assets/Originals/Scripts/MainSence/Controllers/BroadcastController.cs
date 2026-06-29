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

        //ノイズ再生秒数が0の場合
        if (broadcastNoiseCount == 0) 
        {
            //放送スピーカーをランダムに抽出する
            ChooseRandomBroadcastNumber();
        }

        //ノイズ再生中ではない場合
        if (!isBroadcastNoiseFlag) 
        {
            //指定の放送スピーカーからノイズを流す
            PlayBroadcastNoise();
        }

        //ノイズ再生中の場合
        if (isBroadcastNoiseFlag) 
        {
            //ノイズが流れる秒数を加算する
            CountBroadcastNoise();
        }

        //ノイズ再生中の場合&&ノイズが流れる秒数が最大秒数を超えた場合
        if (isBroadcastNoiseFlag && broadcastNoiseCount > kMaxBroadcastNoiseCount) 
        {
            //指定の放送スピーカーのノイズを停止する
            StopBroadcastNoise();
        }
    }

    /// <summary>
    /// 放送スピーカーをランダムに抽出する処理
    /// </summary>
    private void ChooseRandomBroadcastNumber() 
    {
        //放送スピーカーをランダムに抽出する
        saveBroadcastSpeakerListNumber = Random.Range(0, broadcastSpeakerList.Count - 1);

        //ランダムに抽出した放送スピーカーが前回と同じ場合
        if (saveBroadcastSpeakerListNumber == lastSaveBroadcastSpeakerListNumber) 
        {
            //再度ランダムに抽出する
            ChooseRandomBroadcastNumber();
        }

        //前回保存した放送スピーカーリスト関連番号を更新する
        lastSaveBroadcastSpeakerListNumber = saveBroadcastSpeakerListNumber;
    }

    /// <summary>
    /// 指定の放送スピーカーからノイズを流す処理
    /// </summary>
    private void PlayBroadcastNoise() 
    {
        //放送スピーカーを取得する
        broadcastSpeaker = broadcastSpeakerList[saveBroadcastSpeakerListNumber].GetComponent<BroadcastSpeaker>();

        //取得した放送スピーカーのノイズを再生する
        broadcastSpeaker.SetIsBroadcastNoise(true);

        //ノイズ再生中フラグをtrueにする
        isBroadcastNoiseFlag = true;
    }

    /// <summary>
    /// ノイズが流れる秒数を加算する処理
    /// </summary>
    private void CountBroadcastNoise() 
    {
        //ノイズが流れる秒数が最大秒数以下の場合
        if (broadcastNoiseCount <= kMaxBroadcastNoiseCount) 
        {
            //ノイズが流れる秒数を加算する
            broadcastNoiseCount += Time.deltaTime;
        }
    }

    /// <summary>
    /// 指定の放送スピーカーのノイズを停止する処理
    /// </summary>
    private void StopBroadcastNoise() 
    {
        //放送中のスピーカーを取得する
        broadcastSpeaker = broadcastSpeakerList[saveBroadcastSpeakerListNumber].GetComponent<BroadcastSpeaker>();

        //取得した放送スピーカーのノイズを停止する
        broadcastSpeaker.SetIsBroadcastNoise(false);

        //ノイズ再生秒数を0にする
        broadcastNoiseCount = 0.0f;

        //ノイズ再生中フラグをfalseにする
        isBroadcastNoiseFlag = false;
    }
}
