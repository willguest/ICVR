using UnityEngine;

public class LinearDriveContoller : MonoBehaviour
{
    [SerializeField] private Rigidbody DriveArm;

    private bool forwardDrive = false;
    private bool reverseDrive = false;


    void Update()
    {
        if (forwardDrive && DriveArm.transform.localPosition.y < 0.9f)
        {
            DriveArm.MovePosition(DriveArm.transform.position + transform.up * 0.01f);
        }
        
        if (reverseDrive && DriveArm.transform.localPosition.y > 0.065f)
        {
            DriveArm.MovePosition(DriveArm.transform.position + transform.up * -0.01f);
        }
    }

    public void ActuatePositive()
    {
        forwardDrive = true;
    }

    public void ActuateNegative()
    {
        reverseDrive = true;
    }

    public void Halt()
    {
        forwardDrive = false;
        reverseDrive = false;
    }
}
