using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiBehaviour : MonoBehaviour
{
    public bool NeedsKeys { get; private set; }

    [SerializeField] private Image settingsTab;
    [SerializeField] private GameObject settingsPane;

    [SerializeField] private Image chatTab;
    [SerializeField] private GameObject chatPane;

    private float settXPos = 0f;
    private float chatXPos = 0f;

    public bool isChatOpen { get; set; }
    public bool settingsOpen { get; set; }

    private ChatController chatController;


    private void Start()
    {
        chatController = chatPane.GetComponent<ChatController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ToggleChat();
        }
    }

    public void ToggleSettings()
    {
        float startXPos = settXPos;

        if (!settingsOpen)
        {
            if (isChatOpen)
            {
                ToggleChat();
            }

            StartCoroutine(FadeTab(settingsTab, true, 0.5f, 0.0f));
            settXPos = 280f;
        }
        else
        {
            StartCoroutine(FadeTab(settingsTab, false, 0.5f, 0.37f));
            settXPos = 0f;
        }

        Vector3 goPos = new Vector3(startXPos, -90f, 0f);
        Vector3 targetPos = new Vector3(settXPos, -90f, 0f);

        // slidy movement delay (0.4s when opening menu == fade out tab)
        float menuDelay = !settingsOpen ? 0.37f : 0.0f;
        settingsOpen = !settingsOpen;

        StartCoroutine(MoveTab(settingsPane, goPos, targetPos, 0.7f, menuDelay));
    }


    public void ToggleChat()
    {
        float startXPos = chatXPos;
        NeedsKeys = !NeedsKeys;

        if (!isChatOpen) 
        {
            if (settingsOpen)
            {
                ToggleSettings();
            }

            chatXPos = 280f;
            chatController.GetFocus();

            StartCoroutine(FadeTab(chatTab, true, 0.5f, 0.0f));

        }
        else 
        {
            chatXPos = 0f;
            chatController.LoseFocus();
            StartCoroutine(FadeTab(chatTab, false, 0.5f, 0.37f));
        }

        Vector3 goPos = new Vector3(startXPos, -150f, 0f);
        Vector3 targetPos = new Vector3(chatXPos, -150f, 0f);

        float menuDelay = !isChatOpen ? 0.37f : 0.0f;
        isChatOpen = !isChatOpen;

        StartCoroutine(MoveTab(chatPane, goPos, targetPos, 0.7f, menuDelay));
    }

    public void ToggleVR()
    {
        WebXR.WebXRManager.Instance.ToggleVR();
    }


    private IEnumerator FadeTab(Image imgComp, bool fadeOut,  float duration, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float targetAlpha = fadeOut ? 0.0f : 0.62f;
        Color targetColour = new Color(1.0f, 1.0f, 1.0f, targetAlpha);
        imgComp.CrossFadeColor(targetColour, duration, false, true);
    }


    private IEnumerator MoveTab(GameObject menuObject, Vector3 startPos, Vector3 endPos, 
        float speed, float delay = 0f, bool showFirst = false, bool hideAfter = false)
    {
        yield return new WaitForSeconds(delay);

        // warm up
        if (showFirst) menuObject.SetActive(true);

        // move
        float rate = 1.0f / Vector3.Distance(startPos, endPos) * speed * 1000f;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += Time.deltaTime * rate;
            menuObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }

        // cool down
        if (hideAfter) menuObject.SetActive(false);  
    }



}
