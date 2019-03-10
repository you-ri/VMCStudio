using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;

using VMCStudio;
using VMCStudio.Experimental;
using VRM;

namespace VMCStudioEditor.Experimental
{

    [CustomEditor (typeof (VMCLayerdBlendShapeProxy))]
    public class VMCLayerdBlendShapeControllerEditor : Editor
    {
        private VMCLayerdBlendShapeProxy _target;
        private BlendShapeAvatar _asset;

        private string[] _clipNames;
        private GUIContent _empty;

        private SerializedProperty _propBlendShapeAvatar = null;


        private void ResetEditor ()
        {
            _target = target as VMCLayerdBlendShapeProxy;
            _propBlendShapeAvatar = serializedObject.FindProperty ("blendShapeAvatar");
            _empty = new GUIContent ();
        }

        private bool InitializeSetNames ()
        {
            var asset = _propBlendShapeAvatar.objectReferenceValue as BlendShapeAvatar;

            if (_asset != asset) {
                _clipNames = null != asset ? asset.Clips.Select (c => c.name).ToArray () : null;
                _asset = asset;
            }

            return _clipNames != null;
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();

            if (_target != target) {
                ResetEditor ();
            }

            EditorGUILayout.ObjectField (_propBlendShapeAvatar);

            if (_target.bindings != null) {
                int index = 0;
                foreach (var binding in _target.bindings) {
                    using (new EditorGUILayout.HorizontalScope ()) {
                        EditorGUILayout.LabelField (binding.key.Name);
                        EditorGUILayout.LabelField (binding.key.Preset.ToString(), GUILayout.Width(100));

                        _target.bindings[index].value = EditorGUILayout.Slider (binding.value, 0, 1);
                        using (new EditorGUI.DisabledScope (VMCLayerdBlendShapeProxy.HasPresetLayer (binding.key.Preset))) {
                            _target.bindings[index].layer = EditorGUILayout.IntField (binding.layer, GUILayout.Width (24));
                        }
                        _target.CalcurateWeights (index);
                    }
                    index++;
                }
            }

            serializedObject.ApplyModifiedProperties ();

            //base.OnInspectorGUI ();
        }
    }
}