using UnityEngine;

public class VRFootIK : MonoBehaviour
{
    [SerializeField] private bool enableFootSounds = true;
    [SerializeField] private AudioClip defaultStepSound;

    private AudioSource footAudio;
    private Animator animator;

    private float rightFootPosWeight = 1.0f;
    private float rightFootRotWeight = 1.0f;
    private float leftFootPosWeight = 1.0f;
    private float leftFootRotWeight = 1.0f;

    private float stepDelay = 0.3f;
    private float stepTick;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        stepTick = Time.time;

        if (enableFootSounds)
        {
            ConfigureFootAudioSource();
        }
    }

    private void ConfigureFootAudioSource()
    {
        if (!TryGetComponent(out footAudio))
        {
            footAudio = gameObject.AddComponent<AudioSource>();
        }
        footAudio.clip = defaultStepSound;
        footAudio.playOnAwake = false;
        footAudio.volume = 0.1f;
        footAudio.pitch = 0.8f;
        footAudio.loop = false;
        footAudio.maxDistance = 2f;
        footAudio.minDistance = 0.2f;
        footAudio.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Vector3 rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        RaycastHit hit;

        bool hasHit = Physics.Raycast(rightFootPos + (Vector3.up * 0.5f), Vector3.down, out hit);
        if (hasHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPosWeight);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point);

            Quaternion rightFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);

            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotWeight);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.identity);
        }

        Vector3 leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);

        hasHit = Physics.Raycast(leftFootPos + (Vector3.up * 0.5f), Vector3.down, out hit);

        if (hasHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPosWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point);

            Quaternion leftFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotWeight);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.identity);
        }
    }

    /// <summary>
    /// Used by the animation events to play footstep sounds.
    /// </summary>
    private void PlayFootstepSound()
    {
        if (enableFootSounds && (Time.time - stepTick > stepDelay))
        {
            stepTick = Time.time;
            //footAudio.Play();
        }
    }
}
