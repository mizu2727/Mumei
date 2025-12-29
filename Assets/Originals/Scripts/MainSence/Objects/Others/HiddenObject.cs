using UnityEngine;
using UnityEngine.SceneManagement;

public class HiddenObject : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの位置
    /// </summary>
    private Transform player;

    /// <summary>
    /// プレイヤー縮小用ローカルスケール
    /// </summary>
    private Vector3 smallPlayerLocalScale = new Vector3(0.25f, 0.25f, 0.25f);

    /// <summary>
    /// 保存用プレイヤーのローカルスケール
    /// </summary>
    private Vector3 savePlayerLocalScale;

    /// <summary>
    /// 保存用プレイヤーのローカルポジション
    /// </summary>
    private Vector3 savePlayerLocalPosition;


    /// <summary>
    /// Stage01
    /// </summary>
    private const string stage01 = "Stage01";

    /// <summary>
    /// Stage02
    /// </summary>
    private const string stage02 = "Stage02";


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// 隠れるSEの現在のID
    /// </summary>
    private int currentHiddenPlayerSEid;

    /// <summary>
    /// チェストに隠れる時のSEのID
    /// </summary>
    private readonly int chestSEid = 13;

    /// <summary>
    /// ゴミ箱に隠れる時のSEのID
    /// </summary>
    private readonly int trashCanSEid = 14;


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

        //SEのIDを現在のStageSceneによって切り替えて設定する
        switch (SceneManager.GetActiveScene().name) 
        {
            case stage01:
                currentHiddenPlayerSEid = chestSEid;
                break;

            case stage02:
                currentHiddenPlayerSEid = trashCanSEid;
                break;
        }
    }


    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// プレイヤーが隠れる処理
    /// 
    /// 処理の主な流れ…
    /// プレイヤーの親オブジェクトを隠れるオブジェクトに設定
    /// → プレイヤーのスケールを小さくする
    /// → 隠れるオブジェクトの位置にプレイヤーのワールド座標を移動
    /// → プレイヤーのローカルポジションとローカル回転を初期化
    /// </summary>
    public void HiddenPlayer()
    {
        //プレイヤーのTransformを取得
        player = Player.instance.transform;

        //プレイヤーが隠れている状態にする
        Player.instance.SetIsPlayerHidden(true);

        //プレイヤーの移動を停止
        Player.instance.IsMove = false;

        //プレイヤーの移動音を消す
        Player.instance.audioSourceSE.Stop();

        //プレイヤーのローカルスケールを保存する
        savePlayerLocalScale = player.localScale;

        //プレイヤーのローカルポジションを保存する
        savePlayerLocalPosition = player.localPosition;


        //CharacterControllerを取得
        CharacterController characterController = player.GetComponent<CharacterController>();

        //nullチェック
        if (characterController != null)
        {
            //物理判定を一時的に無効化
            characterController.enabled = false;
        }
        

        //playerの親を隠れるオブジェクトに設定
        player.SetParent(transform);

        //playerのスケールを小さくする
        player.localScale = smallPlayerLocalScale;

        //隠れるオブジェクトの位置にplayerを移動
        player.position = transform.position;

        //playerのローカルポジションを初期化
        player.localPosition = Vector3.zero;

        //playerのローカル回転を初期化
        player.localRotation = Quaternion.identity;


        //nullチェック
        if (characterController != null)
        {
            //物理判定を有効化
            characterController.enabled = true;
        }


        //プレイヤーが隠れるアニメーションを再生
        Player.instance.PlayOrStopKneelingAnimation();

        //隠れたときのSE
        audioSourceSE.clip = sO_SE.GetSEClip(currentHiddenPlayerSEid);
        audioSourceSE.PlayOneShot(audioSourceSE.clip);
    }

    /// <summary>
    /// プレイヤーの姿を現す処理
    /// </summary>
    public void ShowThePlayer() 
    {
        //プレイヤーの親をnullに設定
        player.SetParent(null);

        //CharacterControllerを取得
        CharacterController characterController = player.GetComponent<CharacterController>();

        //nullチェック
        if (characterController != null) 
        {
            //物理判定を一時的に無効化
            characterController.enabled = false;
        }

        //姿を現す位置へ移動
        player.position = transform.position + transform.forward * 1.5f;

        //nullチェック
        if (characterController != null)
        {
            //物理判定を有効化
            characterController.enabled = true;
        }

        //プレイヤーが隠れている状態をオフにする
        Player.instance.SetIsPlayerHidden(false);

        //プレイヤーのローカルスケールを元に戻す
        player.localScale = savePlayerLocalScale;

        //プレイヤーのローカルポジションを保存していた値に戻す
        player.localPosition = savePlayerLocalPosition;

        //プレイヤーが隠れるアニメーションを停止
        Player.instance.PlayOrStopKneelingAnimation();

        //プレイヤーが姿を現すときのSE
        audioSourceSE.clip = sO_SE.GetSEClip(currentHiddenPlayerSEid);
        audioSourceSE.PlayOneShot(audioSourceSE.clip);
    }
}
