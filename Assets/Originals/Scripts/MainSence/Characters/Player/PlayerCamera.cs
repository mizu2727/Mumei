using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PlayerCamera instance;

    /// <summary>
    /// マウス/ゲームパッドの右スティックの感度を保存
    /// </summary>
    private float keepMouseSensitivitySlider;

    /// <summary>
    /// マウスの横移動
    /// </summary>
    float lookX;

    /// <summary>
    /// マウスの縦移動
    /// </summary>
    float lookY;

    /// <summary>
    /// ゲームパッドの右スティックの横移動
    /// </summary>
    float lookX2;

    /// <summary>
    /// ゲームパッドの右スティックの縦移動
    /// </summary>
    float lookY2;

    [Header("ゲームパッド右スティックの視点回転速度(感度スライダーの値と掛け合わせる。大きいほど速い)")]
    [SerializeField] private float gamepadLookSpeed = 60.0f;

    /// <summary>
    /// 90度X軸回転ベクトル
    /// </summary>
    private Vector3 rotateX90 = new Vector3(90.0f, 0, 0);

    /// <summary>
    /// 90度Y軸回転ベクトル
    /// </summary>
    private Vector3 rotateY90 = new Vector3(0, 90.0f, 0);

    /// <summary>
    /// カメラのX軸回転角度
    /// </summary>
    private float xRotation = 0.0f;

    /// <summary>
    /// カメラのX軸回転範囲(上側)
    /// </summary>
    private const float kXRotationUpRange = 45.0f;

    /// <summary>
    /// 現在のカメラのX軸回転範囲(下側)
    /// </summary>
    private float currentXRotationDownRange;

    /// <summary>
    /// 通常時のカメラのX軸回転範囲(下側)
    /// </summary>
    private const float kXRotationDownRange = 80.0f;

    /// <summary>
    /// 隠れている時のカメラのX軸回転範囲(下側)
    /// </summary>
    private const float kHiddenXRotationDownRange = 45.0f;

    /// <summary>
    /// 回転速度倍率1
    /// </summary>
    private const float kRotationSpeedMagnification1 = 1.0f;

    /// <summary>
    /// 前フレームの後ろを向くフラグ
    /// </summary>
    private bool wasTrunLastFrame = false;

    /// <summary>
    /// X軸回転リセットフラグ
    /// </summary>
    private bool isResetXRotate = false;

    /// <summary>
    /// Y軸回転リセットフラグ
    /// </summary>
    private bool isResetYRotate = false;

    /// <summary>
    /// プレイヤーのTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// X軸回転リセットフラグを取得
    /// </summary>
    /// <returns>X軸回転リセットフラグ</returns>
    public bool GetIsResetXRotate()
    {
        return isResetXRotate;
    }

    /// <summary>
    /// X軸回転リセットフラグを取得
    /// </summary>
    /// <param name="isResetXRotateValue">X軸回転リセットフラグ</param>
    public void SetIsResetXRotate(bool isResetXRotateValue)
    {
        isResetXRotate = isResetXRotateValue;
    }

    /// <summary>
    /// Y軸回転リセットフラグを取得
    /// </summary>
    /// <returns>X軸回転リセットフラグ</returns>
    public bool GetIsResetYRotate()
    {
        return isResetYRotate;
    }

    /// <summary>
    /// Y軸回転リセットフラグを取得
    /// </summary>
    /// <param name="isResetYRotateValue">Y軸回転リセットフラグ</param>
    public void SetIsResetYRotate(bool isResetYRotateValue)
    {
        isResetYRotate = isResetYRotateValue;
    }

    /*---------------------------------------------
     * うつ伏せ(ベッドの下)の視点関連
     --------------------------------------------*/

    [Header("うつ伏せ時のカメラのローカル位置(プレイヤー基準)")]
    [Header("※うつ伏せ中はプレイヤーのY軸(緑)が頭の方向、Z軸(青)が床方向になる")]
    [Header("  Y:大きいほど頭側へ前進  Z:マイナスほど目線が高くなる")]
    [SerializeField] private Vector3 proneCameraLocalPosition = new Vector3(0f, 1.7f, -0.08f);

    [Header("うつ伏せ時に左右を見渡せる角度(度)")]
    [SerializeField] private float proneYawRange = 70.0f;

    [Header("うつ伏せ時に上を見上げられる角度(度)")]
    [SerializeField] private float proneUpRange = 20.0f;

    [Header("うつ伏せ時に下を見下ろせる角度(度)")]
    [SerializeField] private float proneDownRange = 25.0f;

    [Header("うつ伏せ時に注視対象(敵)へ視点を向ける速さ")]
    [SerializeField] private float proneLookTargetSpeed = 6.0f;

    /// <summary>
    /// うつ伏せモードフラグ
    /// </summary>
    private bool isProneMode = false;

    /// <summary>
    /// うつ伏せ時の左右回転角度(頭の向きを0度とする)
    /// </summary>
    private float proneYaw = 0f;

    /// <summary>
    /// うつ伏せ時の上下回転角度(水平を0度とする。+で下向き)
    /// </summary>
    private float pronePitch = 0f;

    /// <summary>
    /// うつ伏せになる前のカメラのローカル位置
    /// </summary>
    private Vector3 saveCameraLocalPosition;

    /// <summary>
    /// うつ伏せ時に自動で視点を向ける対象(nullの場合は向けない)
    /// </summary>
    private Transform proneLookTarget;

    /// <summary>
    /// うつ伏せモードフラグを取得
    /// </summary>
    /// <returns>うつ伏せモードフラグ</returns>
    public bool GetIsProneMode()
    {
        return isProneMode;
    }

    /// <summary>
    /// うつ伏せモードを開始する
    /// (プレイヤーをうつ伏せの姿勢にした後に呼ぶこと)
    /// </summary>
    public void EnterProneMode()
    {
        //既にうつ伏せモードの場合
        if (isProneMode)
        {
            return;
        }

        isProneMode = true;

        //カメラのローカル位置を保存し、うつ伏せ用の位置へ移動
        saveCameraLocalPosition = transform.localPosition;
        transform.localPosition = proneCameraLocalPosition;

        //視点を頭の方向(正面・水平)へ初期化
        proneYaw = 0f;
        pronePitch = 0f;
        proneLookTarget = null;

        //回転リセット処理を止める
        isResetXRotate = false;
        isResetYRotate = false;

        //カメラの向きを反映
        ApplyProneRotation();
    }

    /// <summary>
    /// うつ伏せモードを終了する
    /// (プレイヤーの姿勢を元に戻した後に呼ぶこと)
    /// </summary>
    public void ExitProneMode()
    {
        //うつ伏せモードでない場合
        if (!isProneMode)
        {
            return;
        }

        isProneMode = false;
        proneLookTarget = null;

        //カメラのローカル位置を元に戻す
        transform.localPosition = saveCameraLocalPosition;

        //上下回転をリセットして正面を向かせる
        xRotation = 0f;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// うつ伏せ時に自動で視点を向ける対象を設定する(敵に発見された演出用。nullで解除)
    /// </summary>
    /// <param name="target">視点を向ける対象</param>
    public void SetProneLookTarget(Transform target)
    {
        proneLookTarget = target;
    }

    /// <summary>
    /// うつ伏せ時のカメラの向きを反映する
    /// </summary>
    private void ApplyProneRotation()
    {
        Quaternion baseRotation = GetProneBaseRotation();

        //頭の方向を基準に、左右回転(Y) → 上下回転(X)の順で回す
        transform.rotation = baseRotation * Quaternion.Euler(pronePitch, proneYaw, 0f);
    }

    /// <summary>
    /// うつ伏せ時の基準の向き(頭の方向・水平)を取得する
    /// </summary>
    /// <returns>基準の向き</returns>
    private Quaternion GetProneBaseRotation()
    {
        Transform player = Player.instance != null ? Player.instance.transform : playerTransform;
        if (player == null)
        {
            return Quaternion.identity;
        }

        //うつ伏せ中はプレイヤーのY軸(up)が頭の方向になる
        Vector3 headDirection = Vector3.ProjectOnPlane(player.up, Vector3.up);

        //頭の方向が取れない場合(念のため)はプレイヤーの前方を使う
        if (headDirection.sqrMagnitude < 0.0001f)
        {
            headDirection = Vector3.ProjectOnPlane(player.forward, Vector3.up);
        }

        if (headDirection.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(headDirection.normalized, Vector3.up);
    }

    /// <summary>
    /// アニメーション適用後に、注視対象の方へ視点を向ける
    /// </summary>
    private void LateUpdate()
    {
        //うつ伏せモードでない場合||注視対象がない場合
        if (!isProneMode || proneLookTarget == null)
        {
            //処理をスキップ
            return;
        }

        //基準の向きから見た、注視対象への方向
        Quaternion baseRotation = GetProneBaseRotation();
        Vector3 localDirection = Quaternion.Inverse(baseRotation) * (proneLookTarget.position - transform.position);

        if (localDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        //目標の左右・上下角度を計算
        float targetYaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float horizontalLength = new Vector2(localDirection.x, localDirection.z).magnitude;
        float targetPitch = -Mathf.Atan2(localDirection.y, horizontalLength) * Mathf.Rad2Deg;

        //なめらかに向ける(演出中は見渡し範囲の制限を受けない)
        float t = 1f - Mathf.Exp(-proneLookTargetSpeed * Time.deltaTime);
        proneYaw = Mathf.LerpAngle(proneYaw, targetYaw, t);
        pronePitch = Mathf.LerpAngle(pronePitch, targetPitch, t);

        //カメラの向きを反映
        ApplyProneRotation();
    }


    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時に設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //うつ伏せ状態のままシーン遷移した場合(ベッドの下で死亡した場合など)は通常の視点に戻す
        if (isProneMode)
        {
            ExitProneMode();
        }
    }

    private void Awake()
    {
        //シングルトンの設定
        if (instance == null)
        {
            instance = this;

            //シーン遷移時に破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //すでにインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //プレイヤーのTransformを取得
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;

        //カメラの下回転範囲を通常に戻す
        currentXRotationDownRange = kXRotationDownRange;

        //X軸回転リセットフラグを初期化
        isResetXRotate = false;
    }

    private void Update()
    {
        //isResetXRotateフラグがtrueの場合(うつ伏せ中はカメラの向きを独自に管理するため行わない)
        if (isResetXRotate && !isProneMode)
        {
            //X軸回転をリセットする
            ResetXRotate();
        }

        //通常のプレイ以外の場合
        if (Player.instance == null || Player.instance.GetIsFallDown()
            || Time.timeScale == 0) 
        {
            //処理をスキップ
            return; 
        }

        

        //プレイヤーが隠れている場合
        if (Player.instance.GetIsPlayerHidden())
        {
            //マウスカーソルを非表示にし、位置を固定
            //(ポーズ解除後隠れている最中にマウスカーソルが表示されてしまうバグを防ぐため)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Ctrl押下で視点が後ろを向く
        if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame 
            && Player.instance.playerIsBackRotate && !Player.instance.GetIsPlayerHidden()) 
        {
            if (!wasTrunLastFrame)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を後方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, -0.25f);
                
                // カメラを即座に180度回転（プレイヤーの背後）
                transform.rotation = Quaternion.LookRotation(-playerTransform.forward, Vector3.up);

                //現在のマウス感度を保存
                keepMouseSensitivitySlider = GameController.lookSensitivity;

                wasTrunLastFrame = true;
            }
        }
        else if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame 
            && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
        {
            // カメラをプレイヤーの前方に戻す
            transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            if (GameController.instance.mouseSensitivitySlider)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を前方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, 0.1f);
            }
            wasTrunLastFrame = false;
        }

        if ( 0 < GameController.lookSensitivity && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame 
            && !Player.instance.PlayerIsBackRotate()) 
        {
            //マウスの移動
            lookX = Input.GetAxis("Mouse X") * GameController.lookSensitivity;
            lookY = Input.GetAxis("Mouse Y") * GameController.lookSensitivity;

            //ゲームパッドの右スティックの移動
            //Input SystemのGamepadから取得(Xbox / PS共通)
            Vector2 gamepadLook = GamepadInput.GetLook();
            lookX2 = gamepadLook.x * GameController.lookSensitivity * gamepadLookSpeed * Time.deltaTime;

            //スティック上方向で上を向く
            lookY2 = gamepadLook.y * GameController.lookSensitivity * gamepadLookSpeed * Time.deltaTime;

            //うつ伏せ(ベッドの下)の場合
            if (isProneMode)
            {
                //左右・上下の見渡し(プレイヤー本体は回転させず、カメラだけを回す)
                proneYaw += (lookX + lookX2);
                pronePitch -= (lookY + lookY2);

                //見渡せる範囲を制限
                proneYaw = Mathf.Clamp(proneYaw, -proneYawRange, proneYawRange);
                pronePitch = Mathf.Clamp(pronePitch, -proneUpRange, proneDownRange);

                //カメラの向きを反映
                ApplyProneRotation();

                //通常の視点処理(プレイヤー本体の左右回転)は行わない
                return;
            }

            //プレイヤーが隠れている場合
            if (Player.instance.GetIsPlayerHidden())
            {
                //カメラの下回転範囲を狭くする
                currentXRotationDownRange = kHiddenXRotationDownRange;
            }
            else 
            {
                //カメラの下回転範囲を通常に戻す
                currentXRotationDownRange = kXRotationDownRange;
            }

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -kXRotationUpRange, currentXRotationDownRange);

            //カメラの上下回転
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            //プレイヤーの左右回転（カメラの親オブジェクト）
            playerTransform.Rotate(Vector3.up * (lookX + lookX2));
        }
    }

    /// <summary>
    /// カメラの回転をリセットする
    /// </summary>
    public void ResetCameraRotation()
    {
        //ストーリーモードの場合
        if (GameController.instance.GetGameModeStatus() == GameModeStatus.Story) 
        {
            //上下回転をリセット
            xRotation = 0f;

            //カメラのローカル回転を初期状態に戻す
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            //カメラのローカル回転を初期状態に戻す
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        //通常プレイモードの場合
        if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame)
        {
            //上下回転をリセット
            xRotation = 0f;

            //左右回転をリセット
            lookY = 0f;

            //カメラのローカル回転を初期状態に戻す
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            //カメラのローカル回転を初期状態に戻す
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    /// <summary>
    /// カメラのX軸回転をリセットする
    /// </summary>
    public void ResetXRotate()
    {
        //カメラのX軸の向きを0度になるように回転させる
        if (transform.rotation.x < 0)
        {
            transform.Rotate(-rotateX90 * (Time.deltaTime * kRotationSpeedMagnification1));
        }
        else if (transform.rotation.x > 0)
        {
            transform.Rotate(rotateX90 * (Time.deltaTime * kRotationSpeedMagnification1));
        }
        else
        {
            //回転終了
            isResetXRotate = false;
        }
    }

    /// <summary>
    /// カメラのY軸回転をリセットする
    /// </summary>
    public void ResetYRotate()
    {
        //カメラのX軸の向きを0度になるように回転させる
        if (transform.rotation.y < 0)
        {
            transform.Rotate(-rotateY90 * (Time.deltaTime * kRotationSpeedMagnification1));
        }
        else if (transform.rotation.y > 0)
        {
            transform.Rotate(rotateY90 * (Time.deltaTime * kRotationSpeedMagnification1));
        }
        else
        {
            //回転終了
            isResetYRotate = false;
        }
    }
}
