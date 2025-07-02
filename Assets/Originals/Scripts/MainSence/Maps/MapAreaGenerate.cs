
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapAreaGenerate : MonoBehaviour
{
    [Header("�}�b�v�G���A���i�[����(�q�G�����L�[��̃}�b�v�G���A���A�^�b�`���邱��)")]
    [SerializeField] private List <GameObject> areaPrefabList;

    [Header("�����_���ɑI�΂ꂽ�}�b�v�G���A���i�[�����(�A�^�b�`�֎~)")]
    public List<GameObject> useList = new ();


    [Header("�}�b�v�G���A�����n�_��Transform�z��(�q�G�����L�[��̃}�b�v�G���A�����n�_���A�^�b�`���邱��)")]
    [SerializeField] private Transform[] areaPoint;


    void Start()
    {
        // areaPrefabList�̃R�s�[���쐬
        List<GameObject> shuffledPrefabList = new List<GameObject>(areaPrefabList);

        // areaPrefabList���V���b�t��
        ShuffleList(shuffledPrefabList);

        // �V���b�t�����ꂽ���X�g���i�[
        useList.AddRange(shuffledPrefabList);


        for (int i = 0; i < useList.Count; i++)
        {
            useList[i].transform.position = areaPoint[i].position;

            Debug.Log("GameObject " + i + ": " + useList[i].name);
        }
    }

    /// <summary>
    /// ���X�g�̗v�f�������_���ɃV���b�t�����郁�\�b�h
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
