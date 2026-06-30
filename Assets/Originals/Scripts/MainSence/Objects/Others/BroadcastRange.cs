using UnityEngine;

/// <summary>
/// このEmptyオブジェクトの範囲内にプレイヤーが存在する場合、特定の放送をBroadcastから流すクラス
/// </summary>
public class BroadcastRange : MonoBehaviour
{
    /// <summary>
    /// 放送スピーカー
    /// </summary>
    [SerializeField] private BroadcastSpeaker broadcastSpeaker;

    private void OnTriggerEnter(Collider collider)
    {
        //プレイヤーが範囲内に存在し、かつ放送ノイズが流れている場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag())
            && broadcastSpeaker.GetIsBroadcastNoise())
        {
            //プレイヤーが放送ノイズを聞いている状態にする
            broadcastSpeaker.SetIsListeningBroadcast(true);
        }
    }

    private void OnTriggerStay(Collider collider) 
    {
        //プレイヤーが範囲内に存在し、かつ放送ノイズが流れている場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) 
            && broadcastSpeaker.GetIsBroadcastNoise()) 
        {
            //プレイヤーが放送ノイズを聞いている状態にする
            broadcastSpeaker.SetIsListeningBroadcast(true);
        }
    }

    private void OnTriggerExit(Collider collider) 
    {
        //プレイヤーが範囲外に出た場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag())) 
        {
            //プレイヤーが放送ノイズを聞いていない状態にする
            broadcastSpeaker.SetIsListeningBroadcast(false);
        }
    }
}
