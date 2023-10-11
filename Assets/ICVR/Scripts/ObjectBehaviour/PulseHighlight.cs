using UnityEngine;

public class PulseHighlight : MonoBehaviour
{
    private Color baseColour;
    private Color highlightColour;
    private bool is_highlighted = true;
    private Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        baseColour = new Color(0.33f, 0.53f, 0.56f, 0.3f);
        highlightColour = new Color(0.62f, 0.58f, 0.32f, 0.5f);
    }

    public void ToggleHighlight()
    {
        is_highlighted = !is_highlighted;
        if (!is_highlighted) material.color = highlightColour;
    }

    void Update()
    {
        if (is_highlighted)
        {
            var ratio = Mathf.Abs(Mathf.Sin(Time.time));
            material.color = Color.Lerp(baseColour, highlightColour, ratio);
        }
    }
}
