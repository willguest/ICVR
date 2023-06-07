using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MediaInterface : MonoBehaviour
{
    [SerializeField] private Text TrackInfo;
    [SerializeField] private Text TrackNo;

    [SerializeField] private MediaController mediaController;

    [SerializeField] private GameObject needleObject;
    [SerializeField] private GameObject recordObject;

    public bool IsInterfaceBusy { get; private set; }

    void Start()
    {
        mediaController.OnAudioUpdated += UpdateScreenInfo;
        mediaController.OnTrackChange += ChangeRecord;
    }

    private void OnDestroy()
    {
        mediaController.OnAudioUpdated -= UpdateScreenInfo;
        mediaController.OnTrackChange -= ChangeRecord;
    }

    private void UpdateScreenInfo(string trackInfo, int trackNo)
    {
        TrackInfo.text = trackInfo;
        TrackNo.text = trackNo.ToString();
    }

    public void ChangeRecord()
    {
        TrackInfo.text = "Loading\n--------\nPlease Wait";
        TrackNo.text = "...";
        IsInterfaceBusy = true;

        StartCoroutine(ChangeRecordSequence(ResumeOperation));
    }

    private IEnumerator ChangeRecordSequence(System.Action completedCallback)
    {
        Vector3 needlePosition = needleObject.transform.position;
        Quaternion needleOnRot = needleObject.transform.rotation;
        Quaternion needleOffRot = needleObject.transform.rotation * Quaternion.Euler(0f, -20f, 0f);
        Vector3 recordOnPos = recordObject.transform.position;
        Vector3 recordOffPos = recordObject.transform.position + Vector3.up * 0.25f;
        Quaternion recordSide1Rot = recordObject.transform.rotation;
        Quaternion recordSide2Rot = recordObject.transform.rotation * Quaternion.Euler(180f, 0f, 0f);
        
        recordObject.GetComponentInChildren<CCRotateDirect>().ToggleRotation();     // stop record
        yield return AdjustPart(needleObject, needlePosition, needleOffRot, 0.7f);  // arm off
        yield return AdjustPart(recordObject, recordOffPos, recordSide1Rot, 1f);    // lift record
        yield return AdjustPart(recordObject, recordOffPos, recordSide2Rot, 2f);    // flip record
        yield return AdjustPart(recordObject, recordOnPos, recordSide2Rot, 2f);     // drop record
        recordObject.GetComponentInChildren<CCRotateDirect>().ToggleRotation();     // start record
        yield return AdjustPart(needleObject, needlePosition, needleOnRot, 0.7f);   // arm on

        completedCallback();
    }

    private void ResumeOperation()
    {
        IsInterfaceBusy = false;
        mediaController.UnblockOperation();
    }

    private IEnumerator AdjustPart(GameObject movingPart, Vector3 endPosition, Quaternion endRotation, float duration)
    {
        yield return new WaitForEndOfFrame();

        Vector3 startPos = movingPart.transform.position;
        Quaternion startQuat = movingPart.transform.rotation;

        float time = 0f;
        float ratio = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            ratio = time / duration;

            movingPart.transform.position = Vector3.Lerp(startPos, endPosition, Mathf.SmoothStep(0.0f, 1.0f, ratio));
            movingPart.transform.rotation = Quaternion.Slerp(startQuat, endRotation, ratio);

            yield return null;
        }
    }

}
