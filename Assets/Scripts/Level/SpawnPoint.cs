using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] string id;
    [SerializeField] bool isDefault;

    public string Id => id;
    public bool IsDefault => isDefault;

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.35f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, string.IsNullOrEmpty(id) ? "(ID Not Set)" : id);
#endif
    }
}
