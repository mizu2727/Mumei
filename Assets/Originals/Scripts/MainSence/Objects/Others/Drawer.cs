using UnityEngine;
using UnityEngine.SceneManagement;

public class Drawer : MonoBehaviour
{
    [Header("�����o���̊J�t���O")]
    [SerializeField] public bool isOpenDrawer = false;

    [Header("�����o���̌˂̃��b�V������")]
    [SerializeField] public Transform drawerMeshTransform;

    [Header("�����o���ɓ����A�C�e��")]
    [SerializeField] public Transform drawerItemTransform;

    [Header("�����o�����J�������̈ʒu")]
    [SerializeField] private Vector3 openPosition;

    [Header("�����o����������̈ʒu")]
    [SerializeField] private Vector3 closePosition;

    /// <summary>
    /// �V����Transform��p�ӂ��āA�A�C�e�����i�[����ꏊ�𖾊m�ɂ���
    /// ����Transform��drawerMeshTransform�̎q�ɔz�u���A�A�C�e���̊�_�Ƃ���
    /// </summary>
    [Header("�A�C�e���z�u�̊�_")]
    [SerializeField] private Transform itemPlacementPoint;

    [Header("�����o����������̃A�C�e���̈ʒu")]
    [SerializeField] private Vector3 closeItemPosition;

    [Header("�����o���̈ړ����x")]
    [SerializeField] private float moveSpeed = 1.0f;

    /// <summary>
    /// �����o���̖ڕW�n�_
    /// </summary>
    private Vector3 targetPosition;

    [Header("BoxCollider")]
    [SerializeField] private BoxCollider boxCollider;

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// �����o�����J����SE��ID
    /// </summary>
    private readonly int openSEid = 11;

    /// <summary>
    /// �����o����߂�SE��ID
    /// </summary>
    private readonly int closeSEid = 10;

    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //�V�[���J�ڎ���AudioSource���Đݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSource�̏�����
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSource�̏�����
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

        //AudioSource�̏�����
        InitializeAudioSource();
    }


    /// <summary>
    /// �����o�����ɃA�C�e����ݒ�
    /// </summary>
    /// <param name="itemTransform">�A�C�e���̈ʒu</param>
    public void SetItemTransform(Transform itemTransform)
    {
        //null�`�F�b�N
        if (itemPlacementPoint != null)
        {
            // �A�C�e���̐e��itemPlacementPoint�ɐݒ�
            itemTransform.SetParent(itemPlacementPoint);

            // itemPlacementPoint�̌��_�ɔz�u
            itemTransform.localPosition = Vector3.zero;

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

    /// <summary>
    /// �����o���̊J��
    /// </summary>
    public void DrawerSystem()
    {
        if (isOpenDrawer)
        {
            //�����o�������
            CloseDrawer();
        }
        else
        {
            //�����o�����J����
            OpenDrawer();
        }
    }

    /// <summary>
    /// �����o�����J����
    /// </summary>
    public void OpenDrawer()
    {
        targetPosition = openPosition;
        isOpenDrawer = true;
        boxCollider.enabled = false;
        DrawerSE(true);
    }

    /// <summary>
    /// �����o�������
    /// </summary>
    public void CloseDrawer()
    {
        targetPosition = closePosition;
        isOpenDrawer = false;
        boxCollider.enabled = true;
        DrawerSE(false);
    }

    /// <summary>
    /// �����o�����ʉ�
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