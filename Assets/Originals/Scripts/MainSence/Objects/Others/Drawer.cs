using UnityEngine;

public class Drawer : MonoBehaviour
{
    [Header("引き出しの開閉の判定")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("引き出しのメッシュ部分")]
    [SerializeField] private Transform drawerMeshTransform;

    [Header("引き出しを開いた時の位置")]
    [SerializeField] private Vector3 openPosition;

    [Header("引き出しを閉じた時の位置")]
    [SerializeField] private Vector3 closePosition;

    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 1.0f;

    // 目標地点
    private Vector3 targetPosition;

    // サウンド関連
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;


    void Start()
    {
        isOpenDrawer = false;

        //メッシュ部分のTransformのlocalPositionを初期化
        if (drawerMeshTransform != null)
        {
            drawerMeshTransform.localPosition = closePosition;
        }
        targetPosition = closePosition;


        audioSourceSE = GetComponent<AudioSource>();
        audioSourceSE = MusicController.Instance.GetAudioSource();
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

    public void OpenDrawer()
    {
        targetPosition = openPosition;
        isOpenDrawer = true;
        DrawerSE(true);
    }

    public void CloseDrawer()
    {
        targetPosition = closePosition;
        isOpenDrawer = false;
        DrawerSE(false);
    }

    void DrawerSE(bool opening)
    {
        AudioClip currentSE = opening ? openSE : closeSE;
        if (audioSourceSE != null && currentSE != null)
        {
            audioSourceSE.PlayOneShot(currentSE);
        }
    }
}