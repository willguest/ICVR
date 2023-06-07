using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ExitBehaviour : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
    public string exitUrl = "https://www.oxfordigitalab.com";


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnDoubleClick()
    {
        Debug.Log("Exiting scene");
        openWindow(exitUrl);
    }
}
