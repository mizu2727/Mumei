using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SampleScene01";
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartMapGenerationAndWarpAsync().Forget();
    }

    private async UniTask StartMapGenerationAndWarpAsync()
    {
        // 1. �v���C���[�̃V���O���g���Q�Ƃ��m�F
        if (Player.instance == null)
        {
            Debug.LogError("[ChangeScene] Player.instance ��������܂���BSampleScene02 �̃v���C���[�� Player �X�N���v�g���A�^�b�`����Ă��邱�Ƃ��m�F���Ă��������B");
            return;
        }
        Debug.Log("[ChangeScene] Player.instance ���擾���܂����B");

        // 1.5 SampleScene02 �̃I�u�W�F�N�g���A�N�e�B�u�ɂ���
        Scene currentScene = SceneManager.GetSceneByName("SampleScene02");
        if (currentScene.IsValid())
        {
            foreach (var rootObj in currentScene.GetRootGameObjects())
            {
                if (rootObj != Player.instance.gameObject) // �v���C���[�ȊO���A�N�e�B�u��
                {
                    rootObj.SetActive(false);
                }
            }
            Debug.Log("[ChangeScene] SampleScene02 �̃I�u�W�F�N�g���A�N�e�B�u�ɂ��܂����B");
        }

        // 2. SampleScene01 ��񓯊��Ń��[�h�iAdditive���[�h�j
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }
        Debug.Log("[ChangeScene] SampleScene01 �̃��[�h���������܂����B");

        

        

        // 5. �f�o�b�O���O��\��
        Debug.Log("[ChangeScene] �X�y�[�X�L�[���������Ă��������B");

        // 6. �X�y�[�X�L�[���͑҂�
        bool spacePressed = false;
        while (!spacePressed)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacePressed = true;
                Debug.Log("[ChangeScene] �X�y�[�X�L�[��������܂����B");
            }
            await UniTask.Yield();
        }

        // 7. �v���C���[�� SampleScene01 �Ƀ��[�v�i�V�[���؂�ւ������j
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (!targetScene.IsValid())
        {
            Debug.LogError("[ChangeScene] SampleScene01 �V�[���������ł��B");
            return;
        }

        // �V�[���ړ��O�Ƀv���C���[��NavMeshAgent�𖳌���
        NavMeshAgent playerAgent = Player.instance.GetComponent<NavMeshAgent>();
        if (playerAgent != null && playerAgent.enabled)
        {
            playerAgent.enabled = false;
            Debug.Log("[ChangeScene] �v���C���[��NavMeshAgent�𖳌������܂����B");
        }

        // �v���C���[�� SampleScene01 �Ɉړ�
        Debug.Log($"[ChangeScene] �v���C���[�ړ��O: �ʒu={Player.instance.transform.position}, �V�[��={Player.instance.gameObject.scene.name}");
        SceneManager.MoveGameObjectToScene(Player.instance.gameObject, targetScene);
        Debug.Log($"[ChangeScene] �v���C���[�ړ���: �ʒu={Player.instance.transform.position}, �V�[��={Player.instance.gameObject.scene.name}");

        

        // 9. SampleScene02 ���A�����[�h
        currentScene = SceneManager.GetSceneByName("SampleScene02");
        if (currentScene.IsValid())
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
            while (!asyncUnload.isDone)
            {
                await UniTask.Yield();
            }
            Debug.Log("[ChangeScene] SampleScene02 ���A�����[�h���܂����B");
        }

        // 10. SampleScene01 ���A�N�e�B�u�V�[���ɐݒ�
        SceneManager.SetActiveScene(targetScene);
        Debug.Log("[ChangeScene] SampleScene01 ���A�N�e�B�u�V�[���ɐݒ肵�܂����B");

        
    }
}