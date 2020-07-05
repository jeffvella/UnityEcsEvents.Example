using UnityEngine;
using UnityEngine.Timeline;

public class TimelordStateBehavior : StateMachineBehaviour
{
    private float _duration = 1f;
    private float _endTimeSeconds;
    private bool _faceCompleted;
    private RaycastHit _hit;
    private float _startTimeSeconds;

    public bool FaceWallOnEnter;
    public TimelineAsset TimelineAsset;
    public TimelordMixer Timelines { get; set; }
    public bool IsActive { get; set; }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Timelines.Play(TimelineAsset);

        animator.SetBool(Parameters.Keys.IsTimelinePlaying, true);

        IsActive = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IsActive = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (IsActive && !Timelines.IsTimelinePlaying)
            animator.SetBool(Parameters.Keys.IsTimelinePlaying, false);
    }
}