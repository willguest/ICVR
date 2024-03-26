using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace ICVR
{
    public class ICVRAvatarController : MonoBehaviour
    {
        public float speedThreshold = 0.1f;
        
        [SerializeField] private Rig ArmRig;
        [SerializeField] private Transform rightWrist;
        [SerializeField] private Transform leftWrist;

        private Animator bodyAnimator;
        private Vector3 headsetSpeed;
        private Vector3 headsetLocalSpeed;
        private Vector3 prevPos;

        public delegate void DiveEvent(int newRecord);
        public event DiveEvent OnNewSpeedRecord;

        void Start()
        {
            bodyAnimator = GetComponent<Animator>();
            prevPos = transform.position;
            RelaxArmRig(); 
        }

        public void PrepareArmRig()
        {
            ArmRig.weight = 1.0f;
            rightWrist.localScale = new Vector3(0f, 0f, 1f);
            leftWrist.localScale = new Vector3(0f, 0f, 1f);

            // give focus to "Legs" layer - IK at controllers
            bodyAnimator.SetLayerWeight(0, 0f);
            bodyAnimator.SetLayerWeight(1, 1f);
        }

        public void RelaxArmRig()
        {
            ArmRig.weight = 0.0f;
            rightWrist.localScale = new Vector3(1f, 1f, 1f);
            leftWrist.localScale = new Vector3(1f, 1f, 1f);

            // give focus to "WholeBody" layer - no hand IK
            bodyAnimator.SetLayerWeight(0, 1f);
            bodyAnimator.SetLayerWeight(1, 0f);
        }

        void FixedUpdate()
        {
            headsetSpeed = (transform.position - prevPos) / Time.deltaTime;
            headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);

            bool isMoving = (headsetLocalSpeed.magnitude > speedThreshold)
                && (headsetSpeed.y > -8.0f) && (headsetSpeed.y < 2.0f);

            bodyAnimator.SetBool("isMoving", isMoving);

            if (isMoving)
            {
                bodyAnimator.SetFloat("directionX", Mathf.Clamp(headsetLocalSpeed.x, -1, 1));
                bodyAnimator.SetFloat("directionY", Mathf.Clamp(headsetLocalSpeed.z, -1, 1));
                bodyAnimator.SetFloat("animation_speed", Mathf.Clamp(headsetLocalSpeed.magnitude, 1, 2));
            }

            prevPos = transform.position;
        }
    }

}