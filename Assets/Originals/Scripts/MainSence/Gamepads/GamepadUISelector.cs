using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ゲームパッドでUIを操作するため、パネル表示時に最初のボタンを自動選択するクラス
/// ポーズパネル・アイテム確認パネル・オプションパネル等、ボタンを持つパネルにアタッチする
/// </summary>
[DisallowMultipleComponent]
public class GamepadUISelector : MonoBehaviour
{
    [Header("最初に選択するボタン(未設定の場合は子オブジェクトから最初に見つかった操作可能なボタン)")]
    [SerializeField] private Selectable firstSelected;

    /// <summary>
    /// 表示中のGamepadUISelector(後から表示されたものほど末尾)
    /// </summary>
    private static readonly List<GamepadUISelector> activeSelectors = new List<GamepadUISelector>();

    private void OnEnable()
    {
        //表示中リストの末尾(最前面扱い)に追加
        activeSelectors.Remove(this);
        activeSelectors.Add(this);

        //ゲームパッド接続時のみ最初のボタンを選択(マウス操作時に選択枠が出ないようにする)
        if (GamepadInput.IsConnected)
        {
            SelectFirst();
        }
    }

    private void OnDisable()
    {
        activeSelectors.Remove(this);

        //このパネル内のボタンが選択されたまま非表示になった場合、選択を解除する
        //(ゲーム中にA / ×を押した際、非表示のボタンが押されてしまうのを防ぐため)
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null
            && eventSystem.currentSelectedGameObject.transform.IsChildOf(transform))
        {
            eventSystem.SetSelectedGameObject(null);
        }

        //1つ前に表示していたパネルがあれば、そちらのボタンを選択し直す
        if (GamepadInput.IsConnected && activeSelectors.Count > 0)
        {
            activeSelectors[activeSelectors.Count - 1].SelectFirst();
        }
    }

    private void Update()
    {
        //最前面のパネルのみ処理する
        if (activeSelectors.Count == 0 || activeSelectors[activeSelectors.Count - 1] != this)
        {
            return;
        }

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return;
        }

        //選択が外れている状態(マウスで何もない所をクリックした等)でゲームパッドを操作した場合、選択し直す
        GameObject current = eventSystem.currentSelectedGameObject;
        if ((current == null || !current.activeInHierarchy) && GamepadInput.HasNavigationInput())
        {
            SelectFirst();
        }
    }

    /// <summary>
    /// 最初のボタンを選択する
    /// </summary>
    public void SelectFirst()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return;
        }

        Selectable target = firstSelected;

        //指定ボタンが選択できない場合、子オブジェクトから操作可能なボタンを探す
        if (target == null || !target.IsInteractable() || !target.gameObject.activeInHierarchy)
        {
            target = null;
            foreach (Selectable selectable in GetComponentsInChildren<Selectable>())
            {
                if (selectable.IsInteractable() && selectable.gameObject.activeInHierarchy)
                {
                    target = selectable;
                    break;
                }
            }
        }

        if (target != null)
        {
            eventSystem.SetSelectedGameObject(target.gameObject);
        }
    }
}
