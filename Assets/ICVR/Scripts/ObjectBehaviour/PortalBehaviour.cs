using System.Runtime.InteropServices;
using UnityEngine;

public class PortalBehaviour : MonoBehaviour
{
    [SerializeField] private string linkUrl;
    [SerializeField] private GameObject characterCollider;

    [DllImport("__Internal")]
    private static extern void OpenURL(string url);

    void OnTriggerEnter(Collider col)
    {
        ActivatePortal(col.gameObject);
    }

    private void ActivatePortal(GameObject cg)
    {
        if (cg.name == characterCollider.name)
        {
            OpenURL(linkUrl);
        }
    }
}