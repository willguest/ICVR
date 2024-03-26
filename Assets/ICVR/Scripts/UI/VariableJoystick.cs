using ICVR;
using UnityEngine;
using UnityEngine.EventSystems;
using WebXR;

public class VariableJoystick : JoystickBase
{
    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1;
    [SerializeField] private JoystickTypeB joystickType = JoystickTypeB.Fixed;

    protected override void Start()
    {
        base.Start();
        UpdateJoystickVisibility();
    }

    public void UpdateJoystickVisibility()
    {
        bool showJS = (PlatformManager.Instance.IsMobile && WebXRManager.Instance.XRState == WebXRState.NORMAL);
        background.gameObject.SetActive(showJS);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(joystickType != JoystickTypeB.Fixed)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.gameObject.SetActive(true);
        }
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(joystickType != JoystickTypeB.Fixed)
            background.gameObject.SetActive(false);

        base.OnPointerUp(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (joystickType == JoystickTypeB.Dynamic && magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}

public enum JoystickTypeB { Fixed, Floating, Dynamic }