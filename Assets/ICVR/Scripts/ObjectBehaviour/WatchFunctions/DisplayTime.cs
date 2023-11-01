using UnityEngine;

public class DisplayTime : MonoBehaviour
{
    public TextMesh text;
    private System.DateTime currentTime;
    private float lastCheck = 0;
    private float rate = 0.5f;

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (Time.time >= lastCheck + rate)
        {
            currentTime = System.DateTime.UtcNow;
            lastCheck = Time.time;
            text.text = currentTime.ToString("HH:mm");
        }
    }
}