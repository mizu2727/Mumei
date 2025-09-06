using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{

    [Header("�h�A�̊J�̔���")]
    public bool isOpenDoor = false;

    [Header("�����|�����Ă��邩�𔻒�")]
    [SerializeField] public bool isNeedKeyDoor = false;

    [Header("�h�A�J���̉�]�p�x")]
    [SerializeField] float openDirenctionValue = 90.0f;//�h�A���J����p�x
    [SerializeField] float closeDirenctionValue = 0.0f;//�h�A�����p�x

    //�v���C���[
    private Player player;

    [Header("�h�A�ԂƂ̋����𑪒肵�����I�u�W�F�N�g���A�^�b�`(�q�G�����L�[��̃v���C���[���A�^�b�`���邱��)")]
    [SerializeField] public Transform targetPoint;

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("�T�E���h�֘A")]
    private AudioSource audioSourceSE;//Door��p��AudioSource
    private readonly int openSEid = 6; // �h�A���J����SE��ID
    private readonly int closeSEid = 5; // �h�A��߂�SE��ID

    [Header("�T�E���h�̋����֘A(�v����)")]
    [SerializeField] private float maxSoundDistance = 10f; // ���ʂ��ő�ɂȂ鋗��
    [SerializeField] private float minSoundDistance = 20f; // ���ʂ��ŏ��ɂȂ鋗��
    [SerializeField] private float maxVolume = 1.0f; // �ő剹��
    [SerializeField] private float minVolume = 0.0f; // �ŏ�����

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ肷�邽�߂̃C�x���g�o�^����
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

    private void Start()
    {
        // AudioSource�̏�����
        InitializeAudioSource();

        //Player�V���O���g������Transform���擾
        //(�V�[���J�ڂ�����Ƀv���C���[��transform��null�ɂȂ�G���[��h�~����p)
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError("Player.instance��������܂���B�V�[����Player�I�u�W�F�N�g�����݂��邱�Ƃ��m�F���Ă��������B");
        }
    }

    /// <summary>
    /// �h�A�̊J�V�X�e��
    /// </summary>
    public void DoorSystem() 
    {
        if (isOpenDoor)
        {
            CloseDoor();
        }
        else 
        {
            //�����K�v�ȏꍇ
            if (isNeedKeyDoor && player.isHoldKey)
            {
                Debug.Log("�������܂����B");
                player.isHoldKey = false;
                isNeedKeyDoor = false;
            }
            else if (!isNeedKeyDoor)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("�{������Ă���B");
            }
        }
    }

    /// <summary>
    /// �h�A���J����
    /// </summary>
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
        DoorSE();
    }

    /// <summary>
    /// �h�A��߂�
    /// </summary>
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
        DoorSE();
    }


    /// <summary>
    /// �h�A�̊J�̌��ʉ�
    /// </summary>
    void DoorSE() 
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("targetPoint��null�ł��B�v���C���[�I�u�W�F�N�g�𐳂����ݒ肵�Ă��������B");
            return;
        }

        // ���ʉ�����
        AudioClip currentSE = (isOpenDoor) ? sO_SE.GetSEClip(openSEid) : sO_SE.GetSEClip(closeSEid);

        // �v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // �����Ɋ�Â����ʌv�Z
        float volume = CalculateVolumeBasedOnDistance(distance);

        // PlayOneShot���g�p���āA�ړ����Ƌ������Ȃ��悤�ɒP���Đ�
        audioSourceSE.PlayOneShot(currentSE, volume);

        //���ʂ�ݒ�
        audioSourceSE.volume = volume;
    }


    /// <summary>
    /// �����Ɋ�Â����ʂ��v�Z���郁�\�b�h
    /// </summary>
    /// <param name="distance">�ΏۃI�u�W�F�N�g�̋���</param>
    /// <returns></returns>
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            // �ő剹��
            return maxVolume; 
        }
        else if (distance >= minSoundDistance)
        {
            // �ŏ�����
            return minVolume; 
        }
        else
        {
            // �����Ɋ�Â��Đ��`���
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}
