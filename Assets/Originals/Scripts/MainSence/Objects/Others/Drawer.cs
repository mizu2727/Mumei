using UnityEngine;

public class Drawer : MonoBehaviour
{
    [Header("�����o���̊J�̔���")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("�����o���̃��b�V������")]
    [SerializeField] private Transform drawerMeshTransform;

    [Header("�����o�����J�������̈ʒu")]
    [SerializeField] private Vector3 openPosition;

    [Header("�����o����������̈ʒu")]
    [SerializeField] private Vector3 closePosition;

    [Header("�ړ����x")]
    [SerializeField] private float moveSpeed = 1.0f;

    // �ڕW�n�_
    private Vector3 targetPosition;

    // �T�E���h�֘A
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;


    void Start()
    {
        isOpenDrawer = false;

        //���b�V��������Transform��localPosition��������
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
        //drawerMeshTransform��localPosition���ړ�������
        if (drawerMeshTransform != null)
        {
            drawerMeshTransform.localPosition = Vector3.MoveTowards(drawerMeshTransform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            //drawerMeshTransform��localPosition�œ���������s��
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