using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VMCStudio;

namespace VMCStudioEditor
{

    /// <summary>
    /// VMCFullBodyControllerEditor インスペクター
    /// </summary>
    [CustomEditor (typeof (VMCFullBodyController))]
    class VMCFullBodyControllerEditor : Editor
    {
        ReorderableList _ro;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            EditorGUILayout.LabelField ("Trackers");

            var t = (VMCStudio.VMCFullBodyController)target;
            if (_ro == null) {
                _ro = new ReorderableList (serializedObject, serializedObject.FindProperty ("trackers"));
                _ro.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    var sp = _ro.serializedProperty.GetArrayElementAtIndex (index);
                    var data = t.trackers[index];

                    rect.height -= 4;
                    rect.width = 100f;

                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("serial"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;

                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("trackingPoint"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;
                };
                _ro.drawHeaderCallback = rect => {
                    //rect.height -= 4;
                    rect.x += 10;
                    rect.width = 100f;

                    EditorGUI.LabelField(rect, "Serial");
                    rect.x += 10;
                    rect.x += rect.width;

                    EditorGUI.LabelField(rect, "TrackingPoint");
                    rect.x += 10;
                    rect.x += rect.width;
                };
            }

            // DoLayoutListを呼ばないと最終的に表示しない
            _ro.DoLayoutList ();

            serializedObject.ApplyModifiedProperties ();
        }
    }

}
