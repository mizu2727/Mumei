using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

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


    [Header("�T�E���h�֘A")]
    private AudioSource audioSourceSE;//Door��p��AudioSource
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;

    [Header("�T�E���h�̋����֘A(�v����)")]
    [SerializeField] private float maxSoundDistance = 10f; // ���ʂ��ő�ɂȂ鋗��
    [SerializeField] private float minSoundDistance = 20f; // ���ʂ��ŏ��ɂȂ鋗��
    [SerializeField] private float maxVolume = 1.0f; // �ő剹��
    [SerializeField] private float minVolume = 0.0f; // �ŏ�����

    private void Start()
    {
        audioSourceSE = MusicController.Instance.GetAudioSource();
    }

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

    //�h�A���J����
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
        DoorSE();
    }

    //�h�A��߂�
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
        DoorSE();
    }


    //�h�A�̊J�̌��ʉ�
    void DoorSE() 
    {
        // ���ʉ�����
        AudioClip currentSE = (isOpenDoor) ? openSE : closeSE;

        // �v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // �����Ɋ�Â����ʌv�Z
        float volume = CalculateVolumeBasedOnDistance(distance);

        MusicController.Instance.PlayAudioSE(audioSourceSE, currentSE);

        //���ʂ�ݒ�
        audioSourceSE.volume = volume;
    }


    // �����Ɋ�Â����ʂ��v�Z���郁�\�b�h
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
