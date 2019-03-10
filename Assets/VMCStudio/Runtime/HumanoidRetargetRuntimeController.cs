using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using System;
using System.Reflection;
using Entum;

[DefaultExecutionOrder (90)]
public class HumanoidRetargetRuntimeController : MonoBehaviour
{
    public Animator source;

    [SerializeField]
    private Animator _target;

    private HumanPose _currentPose;
    private HumanPoseHandler _sourcePoseHandler;
    private HumanPoseHandler _targetPoseHandler;

    private void Start ()
    {
        Debug.Assert (source != null);

        SetTarget (_target);

        _currentPose = new UnityEngine.HumanPose ();
    }

    private void FixedUpdate ()
    {
        if (_target != null && _targetPoseHandler == null) {
            _targetPoseHandler = new HumanPoseHandler (_target.avatar, _target.transform);
            source.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
            source.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
            source.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 1);
            source.SetIKRotationWeight (AvatarIKGoal.RightFoot, 1);
        }
        if (_sourcePoseHandler != null && _targetPoseHandler != null ) {
            _sourcePoseHandler.GetHumanPose (ref _currentPose);
            _targetPoseHandler.SetHumanPose (ref _currentPose);
        }
    }

    public void SetTarget (Animator target)
    {
        this._target = target;
        if (source != null) {
            _sourcePoseHandler = new HumanPoseHandler (source.avatar, source.transform);
        }
        if (target != null) {
            _targetPoseHandler = new HumanPoseHandler (target.avatar, target.transform);
        }
    }


}