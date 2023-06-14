using System.Collections;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using ICVR.SharedAssets;
using UnityEngine;
using WebXR;


public class DataController : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void SendData(string msg);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
