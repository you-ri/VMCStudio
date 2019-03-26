//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

namespace VMCStudio
{
    public class VMCSteamVRTrackedObject : MonoBehaviour
    {
        public enum EIndex
        {
            None = -1,
            Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
            Device1,
            Device2,
            Device3,
            Device4,
            Device5,
            Device6,
            Device7,
            Device8,
            Device9,
            Device10,
            Device11,
            Device12,
            Device13,
            Device14,
            Device15
        }

        public EIndex index;

        public Transform origin;

        public bool enableNoiseCancel = true;
        public float interpolationSpeed = 100;
        public float positionThrethold = 0.1f;
        public float rotationThrethold = 10;

        public bool isValid { get; private set; }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (index == EIndex.None)
                return;

            var i = (int)index;

            isValid = false;
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            isValid = true;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);
            float delta = Time.deltaTime * interpolationSpeed;

            if (enableNoiseCancel) {
                if (origin != null) {
                    transform.position = NosieChancele (transform.position, origin.transform.TransformPoint (pose.pos), Time.deltaTime, interpolationSpeed, positionThrethold);
                    transform.rotation = NosieChancele (transform.rotation, origin.rotation * pose.rot, Time.deltaTime, interpolationSpeed, rotationThrethold);
                }
                else {
                    transform.localPosition = NosieChancele (transform.localPosition, pose.pos, Time.deltaTime, interpolationSpeed, positionThrethold);
                    transform.localRotation = NosieChancele (transform.localRotation, pose.rot, Time.deltaTime, interpolationSpeed, rotationThrethold);
                }
            }
            else {
                if (origin != null) {
                    transform.position = origin.transform.TransformPoint (pose.pos);
                    transform.rotation = origin.rotation * pose.rot;
                }
                else {
                    transform.localPosition = pose.pos;
                    transform.localRotation = pose.rot;
                }

            }
        }

        public static Vector3 NosieChancele (Vector3 from, Vector3 to, float deltaTime, float speed, float threthold)
        {
            float lengthDelth = (Vector3.Magnitude (from - to) * deltaTime) / threthold;
            float delta = lengthDelth * speed * deltaTime;
            return Vector3.Lerp (from, to, Mathf.Clamp01 (delta));
        }

        public static Quaternion NosieChancele (Quaternion from, Quaternion to, float deltaTime, float speed, float threthold)
        {
            float lengthDelth = Quaternion.Angle (from, to) * deltaTime / threthold;
            float delta = lengthDelth * speed * deltaTime;
            return Quaternion.Lerp (from, to, Mathf.Clamp01(delta));
        }

        SteamVR_Events.Action newPosesAction;

        private void Awake()
        {
        }

        void OnEnable()
        {
            var render = SteamVR_Render.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            newPosesAction = SteamVR_Events.NewPosesAction (OnNewPoses);
            newPosesAction.enabled = true;
        }

        void OnDisable()
        {
            newPosesAction.enabled = false;
            isValid = false;
        }

        public void SetDeviceIndex(int index)
        {
            if (System.Enum.IsDefined(typeof(EIndex), index))
                this.index = (EIndex)index;
        }
    }
}