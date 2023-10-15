using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchController : MonoBehaviour
{
    [SerializeField] private GameObject InventoryContainer;
    [SerializeField] private List<GameObject> modeCanvases;

    [SerializeField] private GameObject characterRoot;
    //[SerializeField] private UnityEngine.UI.Button faceButton;

    private int currentMode = 0;
    
    private GameObject myStuff = null;
    private bool inventoryIsOpen = false;

    private Vector3 invOrigin = Vector3.zero;
    private float distanceMoved = 0.0f;

    private void Start()
    {
        
    }

    private void SetMode(int newMode)
    {
        foreach (GameObject g in modeCanvases)
        {
            g.SetActive(modeCanvases.IndexOf(g) == newMode);
        }
    }

    void Update()
    {
        if (inventoryIsOpen)
        {
            distanceMoved = (transform.position - invOrigin).sqrMagnitude;

            if (distanceMoved > 0.01f)
            {
                invOrigin = transform.position;

                Vector3 targetPos = invOrigin + //(Vector3.up * 0.1f) +  
                    (transform.TransformDirection(Vector3.up) * -0.1f);
                Vector3 direction = characterRoot.transform.position - targetPos;
                Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);

                // lerp inventory window to new origin location
                StartCoroutine(LerpToTarget(myStuff, targetPos, targetRot, 0.5f));
            }
        }


        if (Input.GetKeyUp(KeyCode.End))
        {
            ToggleInventory();
        }

    }

    public void ChangeMode()
    {
        currentMode++;
        if (currentMode > modeCanvases.Count - 1) currentMode = 0;
        SetMode(currentMode);
    }

    public void ToggleInventory()
    {
        if (inventoryIsOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
        inventoryIsOpen = !inventoryIsOpen;
    }


    private void OpenInventory()
    {
        myStuff = Instantiate(InventoryContainer, invOrigin + (Vector3.up * 0.2f), Quaternion.identity);
    }

    private void CloseInventory()
    {
        Destroy(myStuff);
    }



    private IEnumerator LerpToTarget(GameObject objToLerp, Vector3 endPosition, Quaternion endRotation, float duration)
    {
        yield return new WaitForEndOfFrame();

        Transform t = objToLerp.transform;
        float time = 0;
        while (time < duration)
        {
            objToLerp.transform.position = Vector3.Lerp(t.position, endPosition, time / duration);
            objToLerp.transform.rotation = Quaternion.Slerp(t.rotation, endRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        objToLerp.transform.position = endPosition;
        objToLerp.transform.rotation = endRotation;
    }
}
