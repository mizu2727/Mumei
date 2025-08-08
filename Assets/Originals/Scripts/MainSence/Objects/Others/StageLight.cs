using UnityEngine;

public class StageLight : MonoBehaviour
{
    [Header("�X�e�[�W���C�g�������Ă��邩�𔻒�(Inspector��Œ������邱��)")]
    [SerializeField] public bool isLitLight = false;

    [Header("�|�C���g���C�g(Prefab���A�^�b�`���邱��)")]
    [SerializeField] Light lightPrefab;

    [Header("�p�[�e�B�N���V�X�e��(Prefab���A�^�b�`���邱��)")]
    [SerializeField] ParticleSystem particleSystemPrefab;

    void Start()
    {
        if (!isLitLight) 
        {
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///     �X�e�[�W���̃��C�g��_��������
    /// </summary>
    public void LitStageLight() 
    {
        if (!isLitLight)
        {
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);
            isLitLight = true;
        }
    }
}
