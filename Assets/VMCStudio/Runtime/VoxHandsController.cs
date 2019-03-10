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
    [RequireComponent (typeof(VMCModelController))]
    public class VoxHandsController : MonoBehaviour
    {
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

        public string baseHandsName = "Relaxed";
        public List<HandsEntitiy> selectionHands;

        Animator _target;
        Vox.Hands.HandController[] _handsControllers;

        private void Start ()
        {
            _target = GetComponent<VMCModelController> ().target;
            if (_target == null) {
                enabled = false;
                return;
            }

            _handsControllers = _target.GetComponentsInChildren<Vox.Hands.HandController> ();
            foreach (var voxHands in _handsControllers) {
                voxHands.SetBasePoseFromCurrentPreset (baseHandsName);
            }
        }

        // Update is called once per frame
        void Update ()
        {
            foreach (var voxHands in _handsControllers) {
                if (voxHands.Hand == Vox.Hands.HandType.LeftHand) {
                    _UpdateHands (voxHands, SteamVR_Input_Sources.LeftHand);
                }
                else {
                    _UpdateHands (voxHands, SteamVR_Input_Sources.RightHand);
                }

            }
        }

        private void LateUpdate ()
        {
        }

        public void _UpdateHands (Vox.Hands.HandController voxHands, SteamVR_Input_Sources source)
        {
            if (selectionHands.Count == 0) return;

            var position = SteamVR_Input.GetVector2 ("default", "Selection", source);
            var elementsCount = selectionHands.Count;
            int index = 0;
            if (position != Vector2.zero) {
                var angle = (Vector2.SignedAngle (Vector2.down, position) + 180) / 360;
                index = Mathf.FloorToInt (((angle * elementsCount) + 0.5f) % elementsCount);
            }

            //var blendShapeKey = new BlendShapeKey (element.blendShapeName);
            var targetWeight = SteamVR_Input.GetSingle ("default", "GrabGrip", source);
            //var targetWeight = element.GetValue (HandType);

            //element.weight += (targetWeight - element.weight) * Time.deltaTime * element.speed;
            //element.weight = Mathf.Clamp01 (element.weight);

            voxHands.SetHandPose (selectionHands[index].presetName, targetWeight);
        }
    }


}