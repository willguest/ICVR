using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ICVR.SharedAssets;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Transform DefaultRespawnPose;

    [SerializeField] private GameObject characterCollider;
    [SerializeField] private GameObject characterRoot;



    private List<string> specialCases = new List<string>();

    void Start()
    {
        specialCases.Add(characterCollider.name);
    }



    void OnTriggerEnter(Collider col)
    {
        GameObject respawnObject = col.gameObject;
        ManageRespawn(respawnObject);
    }

    private void ManageRespawn(GameObject respawnObject)
    {
        if (specialCases.Contains(respawnObject.name))
        {
            ReplaceSpecial(respawnObject);
        }
        else
        {
            ReplaceObject(respawnObject);
        }
    }

    private void ReplaceObject(GameObject obj)
    {
        string name = obj.name;
        Vector3 scale = obj.transform.localScale;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        SharedAsset sa = obj.GetComponent<SharedAsset>();
        
        // put shared assets back where they started
        if (sa)
        {
            obj.transform.position = sa.DefaultLocation;
            obj.transform.rotation = sa.DefaultRotation;
            obj.transform.localScale = sa.DefaultScale;
        }
        else
        {
            obj.transform.position = DefaultRespawnPose.position;
            obj.transform.rotation = DefaultRespawnPose.rotation;
            obj.transform.localScale = scale;
        }

        // remove momentum
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void ReplaceSpecial(GameObject specialObject)
    {
        // character special case
        if (specialObject.name == characterCollider.name)
        {
            characterRoot.transform.position = Vector3.zero + Vector3.up * 1.0f;
            characterRoot.transform.rotation = Quaternion.identity;
            characterRoot.GetComponent<Rigidbody>().velocity = Vector3.zero;
            characterRoot.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        
    }
}
