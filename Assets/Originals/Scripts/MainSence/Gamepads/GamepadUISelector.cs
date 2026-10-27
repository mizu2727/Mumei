using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

    [Header("EventSystemの設定ミスをConsoleに警告表示する(Inspector内での編集禁止)")]
    [SerializeField] private bool isLogWarning = true;

    /// <summary>
    /// 表示中のGamepadUISelector(後から表示されたものほど末尾)
    /// </summary>
    private static readonly List<GamepadUISelector> activeSelectors = new List<GamepadUISelector>();

    private void OnEnable()
    {
        //表示中リストの末尾(最前面扱い)に追加
        activeSelectors.Remove(this);
        activeSelectors.Add(this);

        //EventSystemより先にOnEnableが呼ばれるとEventSystem.currentがnullで選択できないため、1フレーム待ってから選択する
        //(yield return nullはTime.timeScale=0でも進むため、ポーズ中でも動作する)
        StartCoroutine(SelectNextFrame());
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

    /// <summary>
    /// 1フレーム待ってから最初のボタンを選択する
    /// </summary>
    private IEnumerator SelectNextFrame()
    {
        yield return null;

        //EventSystemの設定を確認
        if (isLogWarning)
        {
            CheckEventSystem();
        }

        //ゲームパッド接続時のみ選択(マウス操作時に選択枠が出ないようにする)
        if (GamepadInput.IsConnected && IsTopSelector())
        {
            SelectFirst();
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

        //選択が外れている状態(マウスで何もない所をクリックした・途中でコントローラーを接続した等)で
        //ゲームパッドを操作した場合、選択し直す
        GameObject current = eventSystem.currentSelectedGameObject;
        if ((current == null || !current.activeInHierarchy) && GamepadInput.HasNavigationInput())
        {
            SelectFirst();
        }
    }

    /// <summary>
    /// 最前面のGamepadUISelectorか
    /// </summary>
    private bool IsTopSelector()
    {
        return activeSelectors.Count > 0 && activeSelectors[activeSelectors.Count - 1] == this;
    }

    /// <summary>
    /// 最初のボタンを選択する
    /// </summary>
    public void SelectFirst()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            if (isLogWarning) Debug.LogWarning("[GamepadUISelector] EventSystemがシーンに存在しません");
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

        if (target == null)
        {
            if (isLogWarning) Debug.LogWarning($"[GamepadUISelector] {name} 内に選択できるボタンがありません(非表示・Interactableオフの可能性)");
            return;
        }

        //Navigationが「None」だと十字キー・スティックで移動できないため警告
        if (isLogWarning && target.navigation.mode == Navigation.Mode.None)
        {
            Debug.LogWarning($"[GamepadUISelector] {target.name} のNavigationがNoneです。Automaticにしてください");
        }

        eventSystem.SetSelectedGameObject(target.gameObject);
    }

    /// <summary>
    /// EventSystemの設定ミスを確認してConsoleに警告を出す
    /// </summary>
    private void CheckEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
#else
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
#endif
        //EventSystemが複数ある場合(DontDestroyOnLoadのCanvas等に別のEventSystemが入っていると、そちらが使われることがある)
        if (eventSystems.Length > 1)
        {
            string names = "";
            foreach (EventSystem es in eventSystems) names += $" [{es.name} / {es.gameObject.scene.name}]";
            Debug.LogWarning($"[GamepadUISelector] EventSystemが{eventSystems.Length}個あります。1つにしてください:{names}");
        }

        //使われているEventSystemがInputSystemUIInputModuleでない場合
        EventSystem current = EventSystem.current;
        if (current != null && current.GetComponent<InputSystemUIInputModule>() == null)
        {
            Debug.LogWarning($"[GamepadUISelector] 使用中のEventSystem({current.name})にInputSystemUIInputModuleがありません");
        }

        //ゲームパッドが認識されていない場合
        if (!GamepadInput.IsConnected)
        {
            Debug.LogWarning("[GamepadUISelector] ゲームパッドが認識されていません(Window > Analysis > Input Debuggerで確認できます)");
        }
    }
}
