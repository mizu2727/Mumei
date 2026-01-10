using UnityEngine;
using UnityEngine.SceneManagement;

public class Drawer : MonoBehaviour
{
    /// <summary>
    /// 引き出しの外枠
    /// </summary>
    OutsideOfDrawer outsideOfDrawer;

    /// <summary>
    /// 引き出しの開閉フラグ
    /// </summary>
    private bool isOpenDrawer = false;

    [Header("引き出しの戸のメッシュ部分")]
    [SerializeField] private Transform drawerMeshTransform;

    [Header("引き出しに入れるアイテム")]
    [SerializeField] public Transform drawerItemTransform;

    [Header("引き出しを開いた時の位置")]
    [SerializeField] private Vector3 openPosition;

    [Header("引き出しを閉じた時の位置")]
    [SerializeField] private Vector3 closePosition;

    /// <summary>
    /// 新しいTransformを用意して、アイテムを格納する場所を明確にする
    /// このTransformをdrawerMeshTransformの子に配置し、アイテムの基準点とする
    /// </summary>
    [Header("アイテム配置の基準点")]
    [SerializeField] private Transform itemPlacementPoint;

    /// <summary>
    /// 引き出しを閉じた時のアイテムのローカルポジション
    /// </summary>
    private Vector3 closeItemPosition = new Vector3(-0.03f, 0.0f, -0.23f);


    /// <summary>
    /// 回転角度90度
    /// </summary>
    private const float rotation90 = 90.0f;

    /// <summary>
    /// 回転角度180度
    /// </summary>
    private const float rotation180 = 180.0f;

    /// <summary>
    /// 回転角度270度
    /// </summary>
    private const float rotation270 = 270.0f;


    /// <summary>
    /// 引き出しの移動速度
    /// </summary>
    private const float kMoveSpeed = 1.0f;

    /// <summary>
    /// 引き出しの目標地点
    /// </summary>
    private Vector3 targetPosition;

    [Header("引き出しの戸のBoxCollider")]
    [SerializeField] private BoxCollider boxCollider;


    /// <summary>
    /// タグ："Untagged"
    /// </summary>
    private const string stringUntaggedTag = "Untagged";


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// 引き出しを開けるSEのID
    /// </summary>
    private readonly int openSEid = 11;


    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時にAudioSourceを再設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
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

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
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

        //AudioSourceの初期化
        InitializeAudioSource();
    }


    /// <summary>
    /// 引き出し内にアイテムを設定
    /// </summary>
    /// <param name="itemTransform">アイテムの位置</param>
    public void SetItemTransform(Transform itemTransform)
    {
        //nullチェック
        if (itemPlacementPoint != null)
        {
            // アイテムの親をitemPlacementPointに設定
            itemTransform.SetParent(itemPlacementPoint);

            // itemPlacementPointの原点に配置
            itemTransform.localPosition = Vector3.zero;

            //アイテムのローカルポジション
            itemTransform.localPosition = closeItemPosition;

            // drawerItemTransformにアタッチ
            drawerItemTransform = itemTransform;


            //引き出し本体(外側部分)を取得
            Transform parentTransform = transform.parent;            

            //引き出しの外枠が90度か270度の場合
            if (parentTransform.eulerAngles.y == rotation90 || parentTransform.eulerAngles.y == rotation270)
            {
                //アイテムのY軸角度が0度か180度以外の場合
                if (drawerItemTransform.eulerAngles.y != 0 || drawerItemTransform.eulerAngles.y != rotation180)
                {
                    //X軸のローカル回転角度が0度以外の場合||X軸のローカル回転角度が180度以外の場合
                    if (drawerItemTransform.eulerAngles.x != 0 || drawerItemTransform.eulerAngles.x != rotation180)
                    {
                        //処理をスキップ
                        return;
                    }

                    //Z軸のローカル回転角度が0度以外の場合||X軸のローカル回転角度が180度以外の場合
                    if (drawerItemTransform.eulerAngles.z != 0 || drawerItemTransform.eulerAngles.z != rotation180)
                    {
                        //処理をスキップ
                        return;
                    }

                    //Y軸のローカル回転角度を0度に設定
                    drawerItemTransform.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
            }
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
            drawerMeshTransform.localPosition = Vector3.MoveTowards(drawerMeshTransform.localPosition, targetPosition, kMoveSpeed * Time.deltaTime);

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
        if (!isOpenDrawer)
        {
            //引き出しを開ける
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

        //タグをUntaggedに変更(開けた引き出しのoutlineを非表示にするため)
        this.gameObject.tag = stringUntaggedTag;
    }

    /// <summary>
    /// 引き出し効果音
    /// </summary>
    /// <param name="opening"></param>
    void DrawerSE(bool opening)
    {
        //audioSourceSEが存在する&&引き出しを開けるSEが存在する場合
        if (audioSourceSE != null && sO_SE.GetSEClip(openSEid) != null)
        {
            //引き出しを開けるSEを再生
            audioSourceSE.PlayOneShot(sO_SE.GetSEClip(openSEid));
        }
    }
}