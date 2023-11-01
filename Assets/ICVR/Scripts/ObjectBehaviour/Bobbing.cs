using UnityEngine;

public class Bobbing : MonoBehaviour
{
    private float yPos;
    private float bounceSpeed = 0.3f;

    void Update()
    {
        float sinValue = Mathf.Sin(Time.time * bounceSpeed);
        yPos = Mathf.Lerp(-0.066f, 0.066f, Mathf.Abs((1.0f + sinValue) / 2.0f));
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
    }
}
