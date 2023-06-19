using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace ICVR
{
    public class UiBehaviour : MonoBehaviour
    {
        public bool NeedsKeys { get; private set; }

        [SerializeField] private Image settingsTab;
        [SerializeField] private GameObject settingsPane;

        private float settYPos = 0.0f;
        private float settYScale = 0.0f;

        public bool settingsOpen { get; set; }

        public void ToggleSettings()
        {
            if (!settingsOpen)
            {
                StartCoroutine(FadeTab(settingsTab, true, 0.2f, 0.0f));
                settYPos = 2.0f;
                settYScale = 0.005f;
            }
            else
            {
                StartCoroutine(FadeTab(settingsTab, false, 0.2f, 0.37f));
                settYPos = 0.0f;
                settYScale = 0.0f;
            }


            Vector3 targetPos = new Vector3(0f, settYPos, 0f);
            Vector3 targetScale = new Vector3(0.005f, settYScale, 0.005f);
            settingsOpen = !settingsOpen;

            StartCoroutine(MoveAndScaleTab(targetPos, targetScale, 2.0f));
        }

        public void ToggleVR()
        {
            PlatformManager.Instance.StartVR();
        }


        private IEnumerator FadeTab(Image imgComp, bool fadeOut, float duration, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);

            float targetAlpha = fadeOut ? 0.0f : 0.62f;
            Color targetColour = new Color(1.0f, 1.0f, 1.0f, targetAlpha);
            imgComp.CrossFadeColor(targetColour, duration, false, true);
        }


        private IEnumerator MoveAndScaleTab(Vector3 targetPos, Vector3 targetScale, float speed)
        {
            if (settingsOpen)
            {
                settingsPane.SetActive(true);
            }

            RectTransform settingsRect = settingsPane.GetComponent<RectTransform>();

            float rate = 1.0f / Vector3.Distance(settingsRect.anchoredPosition, targetPos) * speed;
            float t = 0.0f;
            while (t < 1.0)
            {
                t += Time.deltaTime * rate;
                settingsRect.anchoredPosition = Vector3.Lerp(settingsRect.anchoredPosition, targetPos, Mathf.SmoothStep(0.0f, 1.0f, t));
                settingsRect.localScale = Vector3.Lerp(settingsRect.localScale, targetScale, Mathf.SmoothStep(0.0f, 1.0f, t));
                yield return null;
            }

            if (!settingsOpen)
            {
                settingsPane.SetActive(false);
            }
        }
    }
}