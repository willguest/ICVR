using UnityEngine;
using WebXR;

namespace ICVR
{
    public class CursorManager : MonoBehaviour
    {
        // Singleton
        private static CursorManager _instance;
        public static CursorManager Instance { get { return _instance; } }

        public bool isGameMode { get; private set; }

        // Inspector Objects
        [SerializeField] private Texture2D cursorForScene;
        [SerializeField] private Texture2D cursorForObjects;
        [SerializeField] private Texture2D cursorForControls;

        [SerializeField] private ICVRCrosshair crosshair;

        [SerializeField] private bool DebugMouseInteraction;


        // Private Variables
        private Vector2 hotspot = new Vector2(10, 5);
        private readonly CursorMode cMode = CursorMode.ForceSoftware;
        private int pLayer = 0;

        
        private GameObject focusedObject;
        private WebXRState xrState;

        private void Awake()
        {
            _instance = this;
            xrState = WebXRState.NORMAL;
        }

        private void Start()
        {
            SetCrosshairVisibility(isGameMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleGameMode();
            }
        }

        void OnGUI()
        {
            if (xrState != WebXRState.NORMAL) { return; }

            if (!Application.isEditor || DebugMouseInteraction)
            {
                SetCursorImage();
            }
        }

        private void ToggleGameMode()
        {
            isGameMode = !isGameMode;
            DesktopController.Instance.IsGameMode = isGameMode;

            SetCrosshairVisibility(isGameMode);
            SetCursorParameters(xrState);
        }

        public void SetCrosshairVisibility(bool visibility)
        {
            isGameMode = visibility;
            int isActive = (isGameMode ? 1 : 0) * 255;
            crosshair.SetColor(CrosshairColorChannel.ALPHA, isActive, true);
        }

        public void SetFocusedObject(GameObject inFocus)
        {
            focusedObject = inFocus;
        }

        public void SetCursorImage()
        {
            if (focusedObject != null)
            {
                SetCursorImageFromLayer(focusedObject.layer);
            }
            else
            {
                SetDefaultCursor();
            }
        }

        public void SetCursorParameters(WebXRState state)
        {
            xrState = state;

            if (xrState != WebXRState.NORMAL)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = isGameMode ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isGameMode;
            }
        }

        private void SetDefaultCursor()
        {
            Cursor.SetCursor(cursorForScene, hotspot, cMode);
            if (isGameMode)
            {   
                crosshair.SetSize(14, true);
                crosshair.SetThickness(1, true);
                crosshair.SetGap(6, true);
            }
        }

        private void SetCursorImageFromLayer(int objectLayer)
        {
            if (objectLayer == pLayer)
            {
                return;
            }

            pLayer = objectLayer;

            // ui and buttons
            if (objectLayer == 12)
            {
                Cursor.SetCursor(cursorForControls, hotspot, cMode);
                if (isGameMode)
                {   // a thin square
                    crosshair.SetSize(1, true);
                    crosshair.SetThickness(16, true);
                    crosshair.SetGap(8, true);
                }
            }
            // interactables (object and tools)
            else if (objectLayer == 10 || objectLayer == 15)
            {
                Cursor.SetCursor(cursorForObjects, hotspot, cMode);
                if (isGameMode)
                {   // wide crosshair
                    crosshair.SetSize(14, true);
                    crosshair.SetThickness(1, true);
                    crosshair.SetGap(18, true);
                }
            }
            // controllables (furniture and wearables)
            else if (objectLayer == 9 || objectLayer == 14)
            {
                Cursor.SetCursor(cursorForControls, hotspot, cMode);
                if (isGameMode)
                {   // narrow, thicker crosshair
                    crosshair.SetSize(6, true);
                    crosshair.SetThickness(2, true);
                    crosshair.SetGap(6, true);
                }
            }
            // default (scene) cursor
            else
            {
                SetDefaultCursor();
            }
        }
    }
}