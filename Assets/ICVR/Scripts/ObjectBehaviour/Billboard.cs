using ICVR;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        Vector3 direction = transform.position - (BodyController.Instance.transform.position + Vector3.down * 0.5f);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
