using UnityEngine;
using UnityEngine.SceneManagement;

public class Drawer : MonoBehaviour
{
    [Header("引き出しの開閉の判定")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("引き出しの戸のメッシュ部分")]
    [SerializeField] public Transform drawerMeshTransform;

    [Header("引き出しに入れるアイテム")]
    [SerializeField] public Transform drawerItemTransform;

    [Header("引き出しを開いた時の位置")]
    [SerializeField] private Vector3 openPosition;

    [Header("引き出しを閉じた時の位置")]
    [SerializeField] private Vector3 closePosition;

    // 新しいTransformを用意して、アイテムを格納する場所を明確にする
    // このTransformをdrawerMeshTransformの子に配置し、アイテムの基準点とする
    [Header("アイテム配置の基準点")]
    [SerializeField] private Transform itemPlacementPoint;

    [Header("引き出しを閉じた時のアイテムの位置")]
    [SerializeField] private Vector3 closeItemPosition;


    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 1.0f;

    // 目標地点
    private Vector3 targetPosition;

    [Header("BoxCollider")]
    [SerializeField] private BoxCollider boxCollider;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    // サウンド関連
    private AudioSource audioSourceSE;
    private readonly int openSEid = 11; // 引き出しを開けるSEのID
    private readonly int closeSEid = 10; // 引き出しを閉めるSEのID

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定するためのイベント登録解除
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }
    }

    void Start()
    {
        isOpenDrawer = false;
        boxCollider.enabled = true;

        //メッシュ部分のTransformのlocalPositionを初期化
        if (drawerMeshTransform != null)
        {
            drawerMeshTransform.localPosition = closePosition;
        }
        else
        {
            Debug.LogError($"{gameObject.name} の drawerMeshTransform が null です！");
        }

        targetPosition = closePosition;


        InitializeAudioSource();
    }


    
    public void SetItemTransform(Transform itemTransform)
    {
        if (itemPlacementPoint != null)
        {
            // アイテムの親をitemPlacementPointに設定
            itemTransform.SetParent(itemPlacementPoint);
            // itemPlacementPointの原点に配置
            itemTransform.localPosition = Vector3.zero;
            // 必要に応じて、rotationやscaleをリセット
            //itemTransform.localRotation = Quaternion.identity;

            //アイテムのローカルスケールをリセット
            //itemTransform.localScale = Vector3.one;

            //引き出し本体の向きと同じにする
            //itemTransform.localRotation = this.transform.localRotation;

            //アイテムのローカルポジション
            itemTransform.localPosition = new Vector3(0.15f, 0, -0.15f);


            // drawerItemTransformにアタッチ
            drawerItemTransform = itemTransform;
        }
        else
        {
            Debug.LogError("itemPlacementPointが設定されていません。");
        }
    }

    void Update()
    {
        //drawerMeshTransformのlocalPositionを移動させる
        if (drawerMeshTransform != null)
        {
            drawerMeshTransform.localPosition = Vector3.MoveTowards(drawerMeshTransform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            //drawerMeshTransformのlocalPositionで到着判定を行う
            if (Vector3.Distance(drawerMeshTransform.localPosition, targetPosition) < 0.01f)
            {
                if (isOpenDrawer && targetPosition == openPosition)
                {
                    isOpenDrawer = true;
                }
                else if (!isOpenDrawer && targetPosition == closePosition)
                {
                    isOpenDrawer = false;
                }
            }
        }
    }

    /// <summary>
    /// 引き出しの開閉
    /// </summary>
    public void DrawerSystem()
    {
        if (isOpenDrawer)
        {
            CloseDrawer();
        }
        else
        {
            OpenDrawer();
        }
    }

    /// <summary>
    /// 引き出しを開ける
    /// </summary>
    public void OpenDrawer()
    {
        targetPosition = openPosition;
        isOpenDrawer = true;
        boxCollider.enabled = false;
        DrawerSE(true);
    }

    /// <summary>
    /// 引き出しを閉じる
    /// </summary>
    public void CloseDrawer()
    {
        targetPosition = closePosition;
        isOpenDrawer = false;
        boxCollider.enabled = true;
        DrawerSE(false);
    }

    /// <summary>
    /// 引き出し効果音
    /// </summary>
    /// <param name="opening"></param>
    void DrawerSE(bool opening)
    {
        AudioClip currentSE = opening ? sO_SE.GetSEClip(openSEid) : sO_SE.GetSEClip(closeSEid);
        if (audioSourceSE != null && currentSE != null)
        {
            audioSourceSE.PlayOneShot(currentSE);
        }
    }
}