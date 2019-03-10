using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using System.Linq;
using Valve.VR;
using UnityEngine.Serialization;


namespace VMCStudio
{

    /// <summary>
    /// コントローラー入力で表情や目線等を操作する
    /// </summary>v
    [DisallowMultipleComponent]
    [RequireComponent (typeof (VMCModelController))]
    [RequireComponent (typeof(VMCBlinker))]
    public class VMCFaceController : MonoBehaviour, IVMCModelController
    {
        [System.Serializable]
        public enum InputType
        {
            Button,
            AxisPlus,
            AxisMinus,
        }

        [System.Serializable]
        public class BlendShapeEntitiy : ILayerdValue
        {
            public string blendShapeName;
            public string inputName;

            [FormerlySerializedAs ("type")]
            public InputType inputType;
            public float speed = 5f;

            public float value { get; set; }

            public int layer { get { return _layer; } }

            [FormerlySerializedAs ("layer")]
            [SerializeField]
            int _layer;

            BlendShapeKey _blendShapeKey;

            public float GetInputValue ()
            {
                if (string.IsNullOrEmpty (inputName)) return 0;

                var value = Input.GetAxis (inputName);
                if (inputType == InputType.AxisPlus) {
                    return value;
                }
                else if (inputType == InputType.AxisMinus) {
                    return -value;
                }
                else {
                    return Input.GetButton (inputName) ? 1 : 0;
                }
            }
        }


        [System.Serializable]
        public class HandsEntitiy
        {
            [System.Serializable]
            public enum InputType
            {
                Boolean,
                Single,
            }

            public string presetName;
            public string inputName;

            public InputType inputType;
            public float speed = 5f;
            public int layer;

            [System.NonSerialized]
            public float weight;

            public float GetValue (SteamVR_Input_Sources source)
            {
                return SteamVR_Input.GetSingleAction ("default", inputName).GetAxis (source);
            }
        }

        public VMCBlendShapeProxy blendShapeProxy { get; set; }

        public VMCBlinker blinker { get { return GetComponent<VMCBlinker> (); } }

        public float maxEyeYaw = 60;
        public float maxEyePitch = 60;
        VRMLookAtHead _lookAtHead;

        [HideInInspector]
        public BlendShapeEntitiy[] elements;

        void Awake ()
        {
        }

        public void OnModelChanged (Animator target)
        {
            if (target != null) {
                blendShapeProxy = target.GetComponent<VMCBlendShapeProxy> ();
                if (blendShapeProxy != null) {
                    _lookAtHead = blendShapeProxy.GetComponent<VRMLookAtHead> ();
                    Debug.Assert (_lookAtHead != null);

                    blinker.blendShapes = blendShapeProxy;
                }
            }
        }

        // Update is called once per frame
        void Update ()
        {
            if (blendShapeProxy == null) return;

            bool disableBlink = false;

            // 目線の操作
            _lookAtHead.RaiseYawPitchChanged (-Input.GetAxis ("Horizontal") * maxEyeYaw, Input.GetAxis ("Vertical") * maxEyePitch);

            // 瞬き中は操作不能に
            if (blendShapeProxy.GetValue(BlendShapePreset.Blink) != 0) {
                return;
            }

            foreach (var element in elements) {
                var targetWeight = element.GetInputValue ();
                var weight = element.value;

                if (targetWeight > weight) {
                    weight = Mathf.Clamp (weight + 1 * Time.deltaTime * element.speed, 0, 1);
                }
                else if (targetWeight < weight) {
                    weight = Mathf.Clamp (weight +  -1 * Time.deltaTime * element.speed, 0, 1);
                }

                if (weight != 0) {
                    disableBlink = true;
                }
                element.value = weight;
            }

            LayerdBlendUtility.Blend (ref elements);

            foreach (var element in elements) {
                blendShapeProxy.SetValue (element.blendShapeName, element.value);
            }

            // 瞬き抑制
            if (disableBlink) {
                blinker.enabled = false;
            }
            else {
                blinker.enabled = true;
            }

        }

        private void LateUpdate ()
        {
            if (blendShapeProxy == null) return;
            blendShapeProxy.Apply ();
        }


    }

}