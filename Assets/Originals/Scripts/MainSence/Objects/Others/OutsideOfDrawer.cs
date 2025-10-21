using UnityEngine;

public class OutsideOfDrawer : MonoBehaviour
{
    /// <summary>
    /// �I�C���[�p
    /// </summary>
    private Vector3 worldEulerAngles;

    /// <summary>
    /// �I�C���[�p���擾���郁�\�b�h
    /// </summary>
    /// <returns>�I�C���[�p</returns>
    public Vector3 GetWorldEulerAngles() 
    {
        return worldEulerAngles;
    }

    private void Awake()
    {
        //�I�C���[�p���擾���ĕۑ�����
        worldEulerAngles = transform.rotation.eulerAngles;
    }

}
