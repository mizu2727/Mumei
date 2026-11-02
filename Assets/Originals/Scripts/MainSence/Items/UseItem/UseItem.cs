using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public partial class Inventory
{
    /// <summary>
    /// 右クリック or U or E or RT/R2でインベントリアイテム使用する関数
    /// </summary>
    /// <returns>右クリック or U or Eでtrue</returns>
    private bool UseInventoryItem()
    {
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.U)
            || GamepadInput.IsUseItemPressed();
    }

    /// <summary>
    /// アイテムを使用する
    /// </summary>
    public void UseItem()
    {
        //アイテム所持数が0より大きい場合
        if (kMinKeepItemCount < keepItemCount)
        {
            //アイテム使用時の効果を適用する
            ActivationUseItem(keepItemId);

            //アイテム数が0になった場合
            if (keepItemCount == kMinKeepItemCount)
            {
                //インベントリをリセット
                ResetInventoryItem();
            }
        }
        else
        {
            Debug.Log("使用できるアイテムitemがないようだ");
        }
    }

    /// <summary>
    /// 使用したアイテムのIDによって、それぞれの処理を行う
    /// </summary>
    /// <param name="keepItemId">アイテムID</param>
    async void ActivationUseItem(int keepItemId)
    {
        //使用するアイテムIDによって処理を分岐
        switch (keepItemId)
        {
            //スタミナ増強剤
            case kStaminaEnhancerId:
                //スタミナ増強剤SEを再生
                audioSourceInventorySE.clip = sO_SE.GetSEClip(useStaminaEnhancerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //スタミナ増強剤を使用中ではない場合
                if (!isUseStaminaItem)
                {
                    //スタミナゲージの元の色を保存
                    keepStaminaColor = Player.instance.staminaSlider.fillRect.GetComponent<Image>().color;
                }

                //スタミナ消費率を12.5%に変更
                Player.instance.SetStaminaConsumeRatio(kSpecifiedStaminaConsumeRatio);

                //スタミナ回復値を30.0に変更
                Player.instance.SetStaminaRecoveryRatio(kSpecifiedStaminaRecoveryRatio);

                //スタミナゲージの色を緑色に変更
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = greenColor;

                //スタミナ増強剤使用フラグをtrueに設定
                isUseStaminaItem = true;

                //スタミナ増強剤使用時間を加算
                staminaUseTime += keepItemEffectValue;
                break;

            //クラッカー
            case kCrackerId:

                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //クラッカーSEを再生
                audioSourceInventorySE.clip = sO_SE.GetSEClip(crackerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //クラッカー使用フラグをオン
                isUseCrackerItem = true;

                //クラッカー関連処理
                CrackerRelatedProcessing();
                break;

            //テスト用使用アイテム①
            case 995:
                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //ローカル座標をワールド座標に変換
                Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
                Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

                //Addressablesを使用してプレハブをステージ上に非同期生成
                await Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
                break;
        }
    }
}
