using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VMCStudio;

namespace VMCStudioEditor
{

    /// <summary>
    /// VMCFaceController インスペクター
    /// </summary>
    [CustomEditor (typeof (VMCFaceController))]
    class FaceControllerEditor : Editor
    {
        ReorderableList _ro;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            var t = (VMCFaceController)target;
            if (_ro == null) {
                _ro = new ReorderableList (serializedObject, serializedObject.FindProperty ("elements"));
                _ro.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    var sp = _ro.serializedProperty.GetArrayElementAtIndex (index);
                    var data = t.elements[index];

                    rect.height -= 4;
                    rect.width = 100f;

                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("blendShapeName"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;

                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("inputName"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 80;
                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("inputType"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 40;
                    EditorGUI.PropertyField(rect, sp.FindPropertyRelative("speed"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 40;
                    EditorGUI.PropertyField (rect, sp.FindPropertyRelative ("_layer"), GUIContent.none);
                    rect.x += 10;
                    rect.x += rect.width;
                };
                _ro.drawHeaderCallback = rect => {
                    //rect.height -= 4;
                    rect.x += 10;
                    rect.width = 100f;

                    EditorGUI.LabelField(rect, "Name");
                    rect.x += 10;
                    rect.x += rect.width;

                    EditorGUI.LabelField(rect, "InputName");
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 80;
                    EditorGUI.LabelField(rect, "InputType");
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 40;
                    EditorGUI.LabelField(rect, "Speed");
                    rect.x += 10;
                    rect.x += rect.width;

                    rect.width = 40;
                    EditorGUI.LabelField (rect, "Layer");
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
