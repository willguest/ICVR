using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public VRMap Body;
    public VRMap Head;
    public VRMap LeftHand;
    public VRMap RightHand;

    void Update()
    {
        Head.Map();
        Body.Map();
        RightHand.Map();
        LeftHand.Map();
    }
}
