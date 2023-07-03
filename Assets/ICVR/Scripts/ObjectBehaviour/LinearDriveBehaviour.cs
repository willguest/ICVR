using UnityEngine;

namespace ICVR
{
    public class LinearDriveBehaviour : MonoBehaviour
    {
        [SerializeField] private Rigidbody DriveArm;
        [SerializeField] private float MaxActuation;
        [SerializeField] private float MinActuation;

        private bool forwardDrive = false;
        private bool reverseDrive = false;


        void Update()
        {
            if (forwardDrive && DriveArm.transform.localPosition.y < MaxActuation)
            {
                DriveArm.MovePosition(DriveArm.transform.position + transform.up * 0.01f);
            }

            if (reverseDrive && DriveArm.transform.localPosition.y > MinActuation)
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
}
