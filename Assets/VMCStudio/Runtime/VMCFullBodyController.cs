using sh_akira.OVRTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VRM;
using RootMotion.FinalIK;
using Valve.VR;

#if UNITY_EDITOR   // エディタ上でしか動きません。
using UnityEditor;
#endif

namespace VMCStudio
{

    /// <summary>
    /// トラック位置
    /// </summary>
    public enum TrackingPoint
    {
        Unassinged,
        Head,
        RightHand,
        LeftHand,
        Hips,
        RightFoot,
        LeftFoot,
        RightElbow,
        LeftElbow,
        RightKnee,
        LeftKnee
    }

    /// <summary>
    /// トラッカー情報
    /// </summary>
    [System.Serializable]
    public class TrackerState
    {
        public string serial;

        public TrackingPoint trackingPoint;

        [System.NonSerialized]
        public uint deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

        [System.NonSerialized]
        public Transform transform;

        public bool valid
        {
            get { return deviceIndex != OpenVR.k_unTrackedDeviceIndexInvalid; }
        }

        public TrackerState (string serial, TrackingPoint trackingPoint)
        {
            this.serial = serial;
            this.transform = null;
            this.trackingPoint = trackingPoint;
            this.deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
        }

        public void Update (VMCFullBodyController controlWPFWindow)
        {
            deviceIndex = OpenVRWrapper.Instance.GetDeviceIndexBySerialNumber (serial);
            if (!OpenVRWrapper.Instance.IsDeviceValid (deviceIndex)) {
                transform = null;
            }
            else {
                transform = controlWPFWindow.GetTrackerTransformBySerialNumber (serial);
            }

        }

        public float GetBatteryPercentage ()
        {
            return OpenVRWrapper.Instance.GetDeviceBatteryPercentage (deviceIndex);
        }

    }

    [DisallowMultipleComponent]
    [RequireComponent (typeof (VMCModelController))]
    public class VMCFullBodyController : MonoBehaviour, IVMCModelController, ICalibratable, IRecenterble
    {
        [System.Serializable]
        class DebugParameters
        {
            public bool disableTrackers = false;
        }

        [Serializable]
        class References
        {
            public TrackerHandler handler = null;
            public Animator dummy;
            public Transform trackersRoot;
            public WristRotationFix wristRotationFix;
            public Transform HandTrackerRoot;
            public Transform HeadTrackerRoot;
            public Transform PelvisTrackerRoot;
            public Transform RealTrackerRoot;
        }


        [Serializable]
        class Data
        {
            public Transform headTracker;
            public Transform leftHandTracker;
            public Transform rightHandTracker;
            public Transform bodyTracker;
            public Transform leftFootTracker;
            public Transform rightFootTracker;
            public Transform leftElbowTracker;
            public Transform rightElbowTracker;
            public Transform leftKneeTracker;
            public Transform rightKneeTracker;
        }

        private VRIK dummyVrik = null;

        private Animator dummyAnimator { get { return _refs.dummy; } }

        [Tooltip("キャリブレーション後をスケールをアクターと合わせる。")]
        public bool enableScaleAdjust = false;

        [Header ("Tracker Adjustment")]
        [Tooltip ("腰トラッカーの重力方向にずらす。どうしても腰が曲がる場合値を大きくすると効果的。単位はm")]
        public float pelvisTrackerAdjustmentToBottom = 0.1f;

        [Tooltip ("左手トラッカーから手首中心までの肩方向の距離。単位はm")]
        public float leftHandTrackerOffsetToBodySide = 0.05f;

        [Tooltip ("左手トラッカーから手首中心までの底面方向の距離。単位はm")]
        public float leftHandTrackerOffsetToBottom = 0.02f;

        [Tooltip ("右手トラッカーから手首中心までの肩方向の距離。単位はm")]
        public float rightHandTrackerOffsetToBodySide = 0.05f;

        [Tooltip ("右手トラッカーから手首中心までの底面方向の距離。単位はm")]
        public float rightHandTrackerOffsetToBottom = 0.02f;


        [Header ("Dummy Model Adjustment")]
        [HideInInspector]
        public float LeftLowerArmAngle = -60f;
        [HideInInspector]
        public float RightLowerArmAngle = -60f;
        [HideInInspector]
        public float LeftUpperArmAngle = -60f;
        [HideInInspector]
        public float RightUpperArmAngle = -60f;

        [Tooltip ("左手首のひねり角度調整。ディフォルトは90")]
        public float leftHandRotation = 90;

        [Tooltip ("右手首のひねり角度調整。ディフォルトは90")]
        public float rightHandRotation = 90;

        [SerializeField]
        [HideInInspector]
        private Data _data;

        /// <summary>
        /// Tポーズの記憶
        /// </summary>
        PoseMemory _teePoseMemory = new PoseMemory ();

        public Animator dummy { get { return _refs.dummy; } }

        [SerializeField]
        [FormerlySerializedAs ("refs")]
        References _refs;

        [Tooltip ("トラッカー定義リスト")]
        [HideInInspector]
        public List<TrackerState> trackers;

        [SerializeField]
        [FormerlySerializedAs ("debug")]
        DebugParameters _debug;

        private HumanPoseHandler _dummyPoseHandler;

        void Awake()
        {
            _SetupDummyVRIK (dummyAnimator);
        }

        // Use this for initialization
        void Start ()
        {
            _dummyPoseHandler = new HumanPoseHandler (dummyAnimator.avatar, dummyAnimator.transform);

            _teePoseMemory.Store (_refs.dummy.gameObject);

            // リアルトラッカーの位置を初期化
            foreach (Transform t in _refs.RealTrackerRoot) {
                t.localPosition = new Vector3 (0, -100f, 0);
            }

        }

        void FixedUpdate()
        {
            if (dummyVrik == null) return;
            dummyVrik.UpdateSolverExternal ();
        }

        private void Update ()
        {
            _UpdateTrackers ();
        }

        public void OnModelChanged (Animator target)
        {
            // リターゲット設定
            if (_refs.dummy.GetComponent<HumanoidRetargetRuntimeController> () != null) {
                _refs.dummy.GetComponent<HumanoidRetargetRuntimeController> ().SetTarget(target);
            }

            if (target != null) {
                SetupTargetModel (target);
            }
        }

        private void _UpdateHandRotation ()
        {
            //return; // return for debug
            if (dummyVrik == null) return;

            Transform leftHandAdjusterTransform = dummyVrik.solver.leftArm.target;
            Transform rightHandAdjusterTransform = dummyVrik.solver.rightArm.target;
            if (leftHandAdjusterTransform == null || rightHandAdjusterTransform == null) return;
            var angles = leftHandAdjusterTransform.localEulerAngles;
            leftHandAdjusterTransform.localEulerAngles = new Vector3 (leftHandRotation, angles.y, angles.z);
            angles = rightHandAdjusterTransform.localEulerAngles;
            rightHandAdjusterTransform.localEulerAngles = new Vector3 (rightHandRotation, angles.y, angles.z);
        }


        private Vector3 _FixKneeBone (Transform UpperLeg, Transform Knee, Transform Ankle)
        {
            var a = UpperLeg.position;
            var b = Ankle.position;
            var z = Mathf.Max (a.z, b.z) + 0.001f;
            var x = Mathf.Lerp (a.x, b.x, 0.5f);
            var offset = Knee.position - new Vector3 (x, Knee.position.y, z);
            Knee.position -= offset;
            Ankle.position += offset;
            return offset;
        }

        private Vector3 _FixPelvisBone (Transform Spine, Transform Pelvis)
        {
            if (Spine.position.z < Pelvis.position.z) {
                return Vector3.zero;
            }

            var offset = new Vector3 (0, 0, Pelvis.position.z - Spine.position.z + 0.1f);
            Pelvis.position -= offset;
            foreach (var child in Pelvis.GetComponentsInChildren<Transform> (true)) {
                //child.position += offset;
            }
            return offset;
        }


        private void _UnfixKneeBone (Vector3 offset, Transform Knee, Transform Ankle)
        {
            //return;
            Knee.position += offset;
            Ankle.position -= offset;
        }

        private void _SetupDummyVRIK (Animator animator)
        {
            //膝のボーンの曲がる方向で膝の向きが決まってしまうため、強制的に膝のボーンを少し前に曲げる
            var leftOffset = Vector3.zero;
            var rightOffset = Vector3.zero;
            if (this.dummyAnimator != null) {
                leftOffset = _FixKneeBone (this.dummyAnimator.GetBoneTransform (HumanBodyBones.LeftUpperLeg), this.dummyAnimator.GetBoneTransform (HumanBodyBones.LeftLowerLeg), this.dummyAnimator.GetBoneTransform (HumanBodyBones.LeftFoot));
                rightOffset = _FixKneeBone (this.dummyAnimator.GetBoneTransform (HumanBodyBones.RightUpperLeg), this.dummyAnimator.GetBoneTransform (HumanBodyBones.RightLowerLeg), this.dummyAnimator.GetBoneTransform (HumanBodyBones.RightFoot));
                _FixPelvisBone (this.dummyAnimator.GetBoneTransform (HumanBodyBones.Spine), this.dummyAnimator.GetBoneTransform (HumanBodyBones.Hips));
            }

            dummyVrik = animator.GetComponent<VRIK> ();
            if (dummyVrik == null) {
                dummyVrik = animator.gameObject.AddComponent<VRIK> ();
            }
            dummyVrik.solver.IKPositionWeight = 0f;
            dummyVrik.solver.leftArm.stretchCurve = new AnimationCurve ();
            dummyVrik.solver.rightArm.stretchCurve = new AnimationCurve ();
            dummyVrik.UpdateSolverExternal ();

            //膝のボーンの曲がる方向で膝の向きが決まってしまうため、強制的に膝のボーンを少し前に曲げる
            //if (animator != null)
            //{
            //    unfixKneeBone(leftOffset, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), animator.GetBoneTransform(HumanBodyBones.LeftFoot));
            //    unfixKneeBone(rightOffset, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), animator.GetBoneTransform(HumanBodyBones.RightFoot));
            //}
            //if (animator != null)
            //{
            //    var leftWrist = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).gameObject;
            //    var rightWrist = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).gameObject;
            //    var leftRelaxer = leftWrist.AddComponent<TwistRelaxer>();
            //    var rightRelaxer = rightWrist.AddComponent<TwistRelaxer>();
            //    leftRelaxer.ik = vrik;
            //    rightRelaxer.ik = vrik;
            //}

            if (dummyAnimator != null) {
                dummyAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
                dummyAnimator.GetBoneTransform (HumanBodyBones.LeftLowerArm).eulerAngles = new Vector3 (LeftLowerArmAngle, 0, 0);
                dummyAnimator.GetBoneTransform (HumanBodyBones.RightLowerArm).eulerAngles = new Vector3 (RightLowerArmAngle, 0, 0);
                dummyAnimator.GetBoneTransform (HumanBodyBones.LeftUpperArm).eulerAngles = new Vector3 (LeftUpperArmAngle, 0, 0);
                dummyAnimator.GetBoneTransform (HumanBodyBones.RightUpperArm).eulerAngles = new Vector3 (RightUpperArmAngle, 0, 0);
                _refs.wristRotationFix.SetVRIK (dummyVrik);
            }

#if false
        // 背骨の位置確認用
        AddPointGizmo (animator.GetBoneTransform (HumanBodyBones.Chest));
        AddPointGizmo (animator.GetBoneTransform (HumanBodyBones.Hips));
        AddPointGizmo (animator.GetBoneTransform (HumanBodyBones.Spine));
        AddPointGizmo (animator.GetBoneTransform (HumanBodyBones.Neck));
        AddPointGizmo (animator.GetBoneTransform (HumanBodyBones.Head));
#endif
        }


        /// <summary>
        /// キャリブレーション準備開始
        /// </summary>
        public void PrepareCalibrate ()
        {
            var _controlWPFWindow = this;
            // トラッカー割当
            if (!_debug.disableTrackers) {
                _data.headTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.Head));
                _data.leftHandTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.LeftHand));
                _data.rightHandTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.RightHand));
                _data.bodyTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.Hips));
                _data.leftFootTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.LeftFoot));
                _data.rightFootTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.RightFoot));
                _data.leftElbowTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.LeftElbow));
                _data.rightElbowTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.RightElbow));
                _data.leftKneeTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.LeftKnee));
                _data.rightKneeTracker = GetTrackerTransformBySerialNumber (GetTrackerName (TrackingPoint.RightKnee));
            }

            if (dummyAnimator != null) {
                //トラッカー位置の表示
                _refs.RealTrackerRoot.gameObject.SetActive (true);
                foreach (Transform t in _refs.RealTrackerRoot) {
                    t.localPosition = new Vector3 (0, -100f, 0);
                }

            }

            // 各トラッカーの調整用オブジェクトを削除する
            List<GameObject> allTrackers = _refs.handler.Trackers.Concat (_refs.handler.Controllers).Append (_refs.handler.HMDObject).ToList ();
            foreach (var tracker in allTrackers) {
                foreach (Transform adjuster in tracker.transform) {
                    if (!adjuster.gameObject.activeSelf) continue;
                    Destroy (adjuster.gameObject);
                }
            }

            // IK 機能を無効化する
            var vrik = dummyAnimator.GetComponent<VRIK> ();
            vrik.solver.IKPositionWeight = 0;

            // VRIKRootController を削除する
            var vrikRootController = dummyAnimator.GetComponent<VRIKRootController> ();
            if (vrikRootController != null) {
                GameObject.Destroy (vrikRootController);
            }
        }

        public IEnumerator Calibrate ()
        {
            var settings = new RootMotion.FinalIK.VRIKCalibrator.Settings ();

            //トラッカー
            //xをプラス方向に動かすとトラッカーの左(LEDを上に見たとき)に進む
            //yをプラス方向に動かすとトラッカーの上(LED方向)に進む
            //zをマイナス方向に動かすとトラッカーの底面に向かって進む

            //角度補正(左手なら右のトラッカーに向けた)後
            //xを＋方向は体の正面に向かって進む
            //yを＋方向は体の上(天井方向)に向かって進む
            //zを＋方向は体中心(左手なら右手の方向)に向かって進む
            var leftHandOffset = new Vector3 (1.0f, leftHandTrackerOffsetToBottom, leftHandTrackerOffsetToBodySide); // Vector3 (IsEnable, ToTrackerBottom, ToBodySide)
                                                                                                                 //}
                                                                                                                 //if (CurrentSettings.RightHand.Item1 == ETrackedDeviceClass.GenericTracker) {
                                                                                                                 //角度補正(左手なら右のトラッカーに向けた)後
                                                                                                                 //xを－方向は体の正面に向かって進む
                                                                                                                 //yを＋方向は体の上(天井方向)に向かって進む
                                                                                                                 //zを＋方向は体中心(左手なら右手の方向)に向かって進む
            var rightHandOffset = new Vector3 (1.0f, rightHandTrackerOffsetToBottom, rightHandTrackerOffsetToBodySide); // Vector3 (IsEnable, ToTrackerBottom, ToBodySide)
                                                                                                                    //}
                                                                                                                    //if (calibrateType == PipeCommands.CalibrateType.Default) {
            yield return Calibrator.CalibrateScaled (_refs.RealTrackerRoot, _refs.HandTrackerRoot, _refs.HeadTrackerRoot, _refs.PelvisTrackerRoot, 
                dummyVrik, settings, leftHandOffset, rightHandOffset, pelvisTrackerAdjustmentToBottom,
                _data.headTracker, _data.bodyTracker, _data.leftHandTracker, _data.rightHandTracker, _data.leftFootTracker, _data.rightFootTracker, _data.leftElbowTracker, _data.rightElbowTracker, _data.leftKneeTracker, _data.rightKneeTracker);

            dummyVrik.solver.IKPositionWeight = 1.0f;
            if (_refs.handler.Trackers.Count == 1) {
                dummyVrik.solver.plantFeet = true;
                dummyVrik.solver.locomotion.weight = 1.0f;
                var rootController = dummyVrik.references.root.GetComponent<RootMotion.FinalIK.VRIKRootController> ();
                if (rootController != null) GameObject.Destroy (rootController);
            }

            dummyVrik.solver.locomotion.footDistance = 0.06f;
            dummyVrik.solver.locomotion.stepThreshold = 0.2f;
            dummyVrik.solver.locomotion.angleThreshold = 45f;
            dummyVrik.solver.locomotion.maxVelocity = 0.04f;
            dummyVrik.solver.locomotion.velocityFactor = 0.04f;
            dummyVrik.solver.locomotion.rootSpeed = 40;
            dummyVrik.solver.locomotion.stepSpeed = 2;

            //トラッカー位置の非表示
            _refs.RealTrackerRoot.gameObject.SetActive (false);

            _UpdateHandRotation ();
        }

        public void AddPointGizmo (Transform parent)
        {
            var shpere = Instantiate (VMCStudioAssets.entity.pointGizmo, parent);
            shpere.gameObject.name = "point";
            shpere.transform.localPosition = Vector3.zero;
            shpere.transform.localRotation = Quaternion.identity;
        }


        public void SetupTargetModel (Animator targetAnimator)
        {
            var dummyAnimator = _refs.dummy;

            // すべてのスキンメッシュの updateWhnOfscreen を true に
            foreach (var renderer in targetAnimator.GetComponentsInChildren<SkinnedMeshRenderer> (true)) {
                renderer.updateWhenOffscreen = true;
            }

            // キャリブレーション後処理
            var vrikRootController = dummyVrik.GetComponent<VRIKRootController> ();
            if (vrikRootController != null) {
                vrikRootController.enabled = true;
            }

            // ダミーとターゲットのスケール合わせる
            if (enableScaleAdjust) {
                var scale = this.dummyAnimator.GetBoneTransform (HumanBodyBones.Head).position.y / targetAnimator.GetBoneTransform (HumanBodyBones.Head).position.y;
                targetAnimator.transform.localScale = Vector3.one * scale;
            }
        }
 
        /// <summary>
        /// リセンター
        /// 現在の位置と姿勢が中心と正面を向くようにすべてのトラッカーに影響がある座標系を調整する。
        /// </summary>
        public void Recenter ()
        {
            Debug.Assert (_refs.trackersRoot != null);
            Debug.Assert (dummyAnimator != null);

            var pose = new UnityEngine.HumanPose ();
            _dummyPoseHandler.GetHumanPose (ref pose);

            var trackersRoot = _refs.trackersRoot;

            var affectPosition = new Vector3 (pose.bodyPosition.x, 0, pose.bodyPosition.z);
            var affectEulerAngles = new Vector3 (0, pose.bodyRotation.eulerAngles.y, 0);
            trackersRoot.RotateAround (affectPosition, Vector3.up, -affectEulerAngles.y);
            trackersRoot.Translate (-affectPosition, Space.World);
        }



        /// <summary>
        /// シリアルに対応するトラッカーのトランスフォームを取得
        /// </summary>
        public Transform GetTrackerTransformBySerialNumber (string serial)
        {
            if (_refs.handler.HMDObject.transform.name == serial) {
                return _refs.handler.HMDObject.transform;
            }

            foreach (var controller in _refs.handler.Controllers) {
                if (controller != null && controller.transform.name == serial) {
                    return controller.transform;
                }
            }

            foreach (var tracker in _refs.handler.Trackers) {
                if (tracker != null && tracker.transform.name == serial) {
                    return tracker.transform;
                }
            }
            return null;
        }

        /// <summary>
        /// 全トラッカー情報を更新
        /// </summary>
        /// <returns></returns>
        void _UpdateTrackers ()
        {
            TrackerHandler realTrackerHandler = _refs.handler;
            Debug.Assert (realTrackerHandler != null);

            // トラッカーの追加
            IEnumerable<GameObject> realTrackers = realTrackerHandler.Trackers.Concat (realTrackerHandler.Controllers).Concat (new GameObject[] { realTrackerHandler.HMDObject });
            foreach (var tracker in realTrackers) {
                if (!trackers.Any (t => t.serial == tracker.name)) {
                    trackers.Add (new TrackerState (tracker.name, TrackingPoint.Unassinged));
                }
            }

            // トラッカーのトランスフォーム情報を更新
            foreach (var tracker in trackers) {
                tracker.Update (this);
            }
        }


        public Transform GetTrackerTransform (TrackingPoint trackingPoint)
        {
            var tracker = trackers.Where (t => t.trackingPoint == trackingPoint).FirstOrDefault ();
            if (tracker == null) return null;
            return tracker.transform;
        }

        public string GetTrackerName (TrackingPoint trackingPoint)
        {
            var tracker = trackers.Where (t => t.trackingPoint == trackingPoint).FirstOrDefault ();
            if (tracker == null) return null;
            return tracker.serial;
        }

#if false
        private void OnDrawGizmos ()
        {
            _sourcePoseHandler.GetHumanPose (ref _currentPose);

            float size = 2;
            Gizmos.matrix = Matrix4x4.TRS (_currentPose.bodyPosition, _currentPose.bodyRotation, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawLine (Vector3.zero, Vector3.right * size);
            Gizmos.color = Color.green;
            Gizmos.DrawLine (Vector3.zero, Vector3.up * size);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine (Vector3.zero, Vector3.forward * size);
        }
#endif


    }

}
