using UnityEngine;

public class Drawer : MonoBehaviour
{
    [Header("�����o���̊J�̔���")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("�����o���̌˂̃��b�V������")]
    [SerializeField] public Transform drawerMeshTransform;

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

    [Header("�����o����������̃A�C�e���̈ʒu")]
    [SerializeField] private Vector3 closeItemPosition;


    [Header("�ړ����x")]
    [SerializeField] private float moveSpeed = 1.0f;

    // �ڕW�n�_
    private Vector3 targetPosition;

    [Header("BoxCollider")]
    [SerializeField] private BoxCollider boxCollider;

    // �T�E���h�֘A
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;


    void Start()
    {
        isOpenDrawer = false;
        boxCollider.enabled = true;

        //���b�V��������Transform��localPosition��������
        if (drawerMeshTransform != null)
        {
            drawerMeshTransform.localPosition = closePosition;
        }
        else
        {
            Debug.LogError($"{gameObject.name} �� drawerMeshTransform �� null �ł��I");
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
            //itemTransform.localRotation = Quaternion.identity;

            //�A�C�e���̃��[�J���X�P�[�������Z�b�g
            //itemTransform.localScale = Vector3.one;

            //�����o���{�̂̌����Ɠ����ɂ���
            //itemTransform.localRotation = this.transform.localRotation;

            //�A�C�e���̃��[�J���|�W�V����
            itemTransform.localPosition = new Vector3(0.15f, 0, -0.15f);


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
        boxCollider.enabled = false;
        DrawerSE(true);
    }

    public void CloseDrawer()
    {
        targetPosition = closePosition;
        isOpenDrawer = false;
        boxCollider.enabled = true;
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