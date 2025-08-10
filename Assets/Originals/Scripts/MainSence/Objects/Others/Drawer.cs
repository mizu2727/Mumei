using UnityEngine;

public class Drawer : MonoBehaviour
{
    [Header("�����o���̊J�̔���")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("�����o���̃��b�V������")]
    [SerializeField] private Transform drawerMeshTransform;

    [Header("�����o���ɓ����A�C�e��")]
    [SerializeField] public Transform drawerItemTransform;

    [Header("�����o�����J�������̈ʒu")]
    [SerializeField] private Vector3 openPosition;

    [Header("�����o����������̈ʒu")]
    [SerializeField] private Vector3 closePosition;

    // �V����Transform��p�ӂ��āA�A�C�e�����i�[����ꏊ�𖾊m�ɂ���
    // ����Transform��drawerMeshTransform�̎q�ɔz�u���A�A�C�e���̊�_�Ƃ���
    [Header("�A�C�e���z�u�̊�_")]
    [SerializeField] private Transform itemPlacementPoint;


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



    public void SetItemTransform(Transform itemTransform)
    {
        if (itemPlacementPoint != null)
        {
            // �A�C�e���̐e��itemPlacementPoint�ɐݒ�
            itemTransform.SetParent(itemPlacementPoint);
            // itemPlacementPoint�̌��_�ɔz�u
            itemTransform.localPosition = Vector3.zero;
            // �K�v�ɉ����āArotation��scale�����Z�b�g
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;

            // drawerItemTransform�ɃA�^�b�`
            drawerItemTransform = itemTransform;
        }
        else
        {
            Debug.LogError("itemPlacementPoint���ݒ肳��Ă��܂���B");
        }
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