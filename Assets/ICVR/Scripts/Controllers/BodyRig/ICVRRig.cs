using ICVR;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class VRMap
{
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map(Transform vrTarget)
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class ICVRRig : MonoBehaviour
{
    [Tooltip("Connection to ICVR \nHint: ICVRCameraSet ➥ CharacterRoot ➥ Body")]
    [SerializeField] private BodyController bodyController;

    public VRMap Body;
    public VRMap Head;
    public VRMap LeftHand;
    public VRMap RightHand;

    private Transform headRef;
    private Transform bodyRef;
    private Transform leftHandRef;
    private Transform rightHandRef;

    private void Start()
    {
        bodyController.avatar = GetComponent<ICVRAvatarController>();

        headRef = bodyController.GetBodyReference("head");
        bodyRef = bodyController.GetBodyReference("body");
        leftHandRef = bodyController.GetBodyReference("leftHand");
        rightHandRef = bodyController.GetBodyReference("rightHand");
    }

    void Update()
    {
        Head.Map(headRef);
        Body.Map(bodyRef);
        RightHand.Map(rightHandRef);
        LeftHand.Map(leftHandRef);
    }
}
