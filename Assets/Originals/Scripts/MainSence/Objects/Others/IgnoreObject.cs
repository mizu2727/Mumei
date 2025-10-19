using UnityEngine;

public class IgnoreObject : MonoBehaviour
{
    /// <summary>
    /// MeshCollider
    /// </summary>
    private MeshCollider meshCollider;

    /// <summary>
    /// MeshCollider‚ðŽæ“¾‚·‚é
    /// </summary>
    /// <returns>MeshCollider</returns>
    public MeshCollider GetMeshCollider()
    {
        return meshCollider;
    }


    private void Start()
    {
        //MeshCollider‚ðŽæ“¾
        meshCollider = GetComponent<MeshCollider>();
    }
}
