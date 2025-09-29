using UnityEngine;

public class StageLight : MonoBehaviour
{
    [Header("�X�e�[�W���C�g�t���O(Inspector��Œ������邱��)")]
    [SerializeField] public bool isLitLight = false;

    [Header("�|�C���g���C�g(Prefab���A�^�b�`���邱��)")]
    [SerializeField] Light lightPrefab;

    [Header("�p�[�e�B�N���V�X�e��(Prefab���A�^�b�`���邱��)")]
    [SerializeField] ParticleSystem particleSystemPrefab;

    void Start()
    {
        //�X�e�[�W���C�g�t���O���I�t�̏ꍇ
        if (!isLitLight) 
        {
            //�X�e�[�W���C�g������������
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �X�e�[�W���̃��C�g��_��������
    /// </summary>
    public void LitStageLight() 
    {
        //�X�e�[�W���C�g�t���O���I�t�̏ꍇ
        if (!isLitLight)
        {
            //�X�e�[�W���C�g��_��������
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);

            //�t���O�l���I��
            isLitLight = true;
        }
    }
}
