using ICVR;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private float yPos;
    private float bounceSpeed = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float sinValue = Mathf.Sin(Time.time * bounceSpeed);

        yPos = Mathf.Lerp(-0.066f, 0.066f, Mathf.Abs((1.0f + sinValue) / 2.0f));
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);

        Vector3 direction = transform.position - (BodyController.Instance.transform.position + Vector3.down * 0.5f);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
