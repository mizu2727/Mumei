using UnityEngine;

public class IgnoreObject : MonoBehaviour
{
    /// <summary>
    /// MeshCollider
    /// </summary>
    private MeshCollider meshCollider;

    /// <summary>
    /// MeshCollider���擾����
    /// </summary>
    /// <returns>MeshCollider</returns>
    public MeshCollider GetMeshCollider()
    {
        return meshCollider;
    }


    private void Start()
    {
        //MeshCollider���擾
        meshCollider = GetComponent<MeshCollider>();
    }
}
