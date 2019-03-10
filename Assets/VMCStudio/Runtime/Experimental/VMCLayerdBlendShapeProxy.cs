using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using VRM;

namespace VMCStudio.Experimental
{

    /// <summary>
    /// 表情操作
    /// VRMBlendShapeProxyはエディターでの表情変化に対応しないため。
    /// </summary>
    [ExecuteInEditMode]
    public class VMCLayerdBlendShapeProxy : MonoBehaviour
    {
        [System.Serializable]
        public struct BlendShapePresetBinding : ILayerdValue
        {
            public float value { get; set; }

            public int layer { get { return _layer; } set { _layer = value; } }

            public BlendShapeKey key;

            [SerializeField]
            private int _layer;

        }

        public static readonly Dictionary<BlendShapePreset, int> kPresetLayer = new Dictionary<BlendShapePreset, int>() {
            { BlendShapePreset.A, 0 },
            { BlendShapePreset.I, 0 },
            { BlendShapePreset.U, 0 },
            { BlendShapePreset.E, 0 },
            { BlendShapePreset.O, 0 },
            { BlendShapePreset.Blink, 1 },
            { BlendShapePreset.Blink_L, 1 },
            { BlendShapePreset.Blink_R, 1 },
        };

        public static bool HasPresetLayer(BlendShapePreset preset)
        {
            return kPresetLayer.ContainsKey (preset);
        }


        public BlendShapeAvatar blendShapeAvatar;

        public BlendShapePresetBinding[] bindings;

        BlendShapeMerger _merger;

        private void Reset ()
        {
            blendShapeAvatar = GetComponent<VRM.VRMBlendShapeProxy> ().BlendShapeAvatar;
            _Construction ();
        }

        private void OnValidate ()
        {
            _Construction ();
        }

        private void OnEnable ()
        {
            _Construction ();
        }

        private void OnDisable ()
        {
            _Destruction ();
        }

        void _Construction ()
        {
            if (blendShapeAvatar != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger = new BlendShapeMerger (validatedBlendShapeAvatarClips, this.transform);

                Array.Resize (ref bindings, Mathf.Max (bindings.Count (), validatedBlendShapeAvatarClips.Count ()));
                int index = 0;

                foreach (var clip in validatedBlendShapeAvatarClips) {
                   var key = clip.Preset != BlendShapePreset.Unknown ? new BlendShapeKey (clip.BlendShapeName, clip.Preset) : new BlendShapeKey (clip.BlendShapeName);
                    bindings[index].key = key;
                    if (kPresetLayer.ContainsKey(clip.Preset)) {
                        bindings[index].layer = kPresetLayer[clip.Preset];
                    }
                    index++;
                }
            }
        }

        public void _Destruction ()
        {
            if (_merger != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger.RestoreMaterialInitialValues (validatedBlendShapeAvatarClips);
                _merger = null;
            }
        }

        private void Start ()
        {
            if (GetComponent<VRM.VRMBlendShapeProxy> ()) {
                GetComponent<VRM.VRMBlendShapeProxy> ().enabled = false;
            }
        }

        public void CalcurateWeights(int highPriorityIndex = -1)
        {
            if (bindings == null) return;
            LayerdBlendUtility.Blend (ref bindings, highPriorityIndex);

        }

        private void LateUpdate ()
        {
            CalcurateWeights ();
            if (_merger != null) {
                foreach (var binding in bindings) {
                    _merger.SetValue (binding.key, binding.value, false);
                }
                _merger.Apply ();
            }
        }

        private void OnDestroy ()
        {
            _Destruction ();
        }

        public void Apply()
        {
            if (_merger != null) {
                _merger.Apply ();
            }
        }

        public void SetValue(string name, float value)
        {
            var property = this.GetType ().GetField (name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (property == null) {
                //Debug.LogWarning ($"undefined blendshape member. name:{name}");
                return;
            }

            property.SetValue (this, value);
        }

        public void ImmediatelySetValue (BlendShapeKey key, float value)
        {
            SetValue (key.Name, value);
        }

        public void AccumulateValue (BlendShapePreset preset, float value)
        {
            ImmediatelySetValue (new BlendShapeKey (preset), value);
        }

        public void AccumulateValue (BlendShapeKey key, float value)
        {
            ImmediatelySetValue (key, value);
        }

        public float GetValue (BlendShapeKey key)
        {
            if (_merger == null) {
                return 0;
            }
            return _merger.GetValue (key);
        }

        public float GetValue (BlendShapePreset key)
        {
            return GetValue (new BlendShapeKey (key));
        }
    }
}