using UnityEngine;
using static GameController;

public partial class Inventory
{
    /// <summary>
    /// Q or O or Y/△が押されているか
    /// </summary>
    /// <returns>Q or O押下中はtrue</returns>
    private bool IsHoldDropItemKey()
    {
        return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.O) || GamepadInput.IsDropItemHeld();
    }

    /// <summary>
    /// Q or Oの長押しを監視し、一定時間押し続けたらアイテムを破棄する
    /// </summary>
    private void UpdateDropItemHold()
    {
        //キーが離されている場合は長押し状態をリセット
        if (!IsHoldDropItemKey())
        {
            dropItemHoldTime = 0.0f;
            isWaitDropKeyRelease = false;
            return;
        }

        //操作可能な状態か(アイテム使用と同じ条件)
        bool canOperate = !PauseController.instance.GetIsPause() && Time.timeScale != 0
            && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && !Player.instance.GetIsPlayerHidden();

        //操作不可・破棄直後でキー押しっぱなし・アイテム未所持の場合はカウントしない
        if (!canOperate || isWaitDropKeyRelease || keepItemId == CommonController.instance.GetKNoneItemId())
        {
            dropItemHoldTime = 0.0f;
            return;
        }

        //長押し時間を加算
        dropItemHoldTime += Time.deltaTime;

        //規定時間に達したらアイテムを破棄
        if (kDropItemHoldSeconds <= dropItemHoldTime)
        {
            DropItem();

            //キーを離すまで再度破棄しない
            dropItemHoldTime = 0.0f;
            isWaitDropKeyRelease = true;
        }
    }

    /// <summary>
    /// 所持アイテムを捨てる(インベントリをリセット)
    /// </summary>
    public void DropItem()
    {
        //アイテムを所持していない場合は処理をスキップ
        if (keepItemId == CommonController.instance.GetKNoneItemId())
        {
            return;
        }

        //アイテムDB側の所持数も0にする
        sO_Item.ReduceUseItem(keepItemId, kMinKeepItemCount);

        //インベントリをリセット
        //(使用中のスタミナ増強剤の効果は継続させるため、EndStaminaEffectは呼ばない)
        ResetInventoryItem();
    }
}
