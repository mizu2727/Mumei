using Unity.VisualScripting;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [Header("�v���C���[�J����(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Transform cameraTransform;

    [Header("���C�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    public GameObject playerHasLight;

    private void Start()
    {
        playerHasLight.SetActive(false);
        Player.instance.IsLight = false;
    }

    void Update()
    {
        TranceCamera();

        TurnOnAndOfLight();


    }

    //�E�N���b�N�Ń��C�g�؂�ւ�
    bool PlayerIsLight()
    {
        return Input.GetMouseButtonDown(1);
    }

    void TranceCamera()
    {
        //���W�Ǐ]
        this.transform.position = cameraTransform.position;
        //�p�x�Ǐ]
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraTransform.rotation, 0.5f); 
    }

    void TurnOnAndOfLight() 
    {
        if (PlayerIsLight() && !Player.instance.IsLight)
        {
            // ���C�g���A�N�e�B�u��Ԃɂ���E�E�E�����C�g���_��
            playerHasLight.SetActive(true);
            Player.instance.IsLight = true;
            Debug.Log("���C�g�_��");
        }
        else if ((PlayerIsLight() && Player.instance.IsLight))
        {
            // ���C�g���m���E�A�N�e�B�u��Ԃɂ���E�E�E�����C�g��������
            playerHasLight.SetActive(false);
            Player.instance.IsLight = false;
            Debug.Log("���C�g����");
        }
    }
}


