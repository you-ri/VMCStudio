using UnityEngine;
using UnityEditor;

/// <summary>
/// Transform 上に記録している Float 値
/// </summary>
/// Hide フラグによりユーザーは見えない＆操作できないTransformを自動生成する。
/// 実際はこのTransform の localPosition が値を記録する。
/// UnityRecorder等で値の変化を記録できるようにするため。UnityRecorderはTransform 位置を記録する。
[System.Serializable]
public struct FloatOnTransform
{
    public Transform transform;

    public float value
    {
        get {
            return transform != null ? transform.localPosition.x : 0;
        }
        set {
            if (transform != null) {
                transform.localPosition = Vector3.right * value;
            }
        }
    }

    public void CreateTransform (string name, Transform parent)
    {
        if (parent == null) return;
        if (transform == null) {
            var cgo = parent.Find (name);
            if (cgo != null) {
                GameObject.DestroyImmediate (cgo.gameObject);
            }
            var go = new GameObject (name);

            //go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.SetParent (parent);
            transform = go.transform;
        }
    }

    public void DeleteTransform()
    {
        if (transform == null) return;
        GameObject.DestroyImmediate (transform.gameObject);
        transform = null;
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer (typeof (FloatOnTransform))]
    public class FloatRangeDrawer : PropertyDrawer
    {


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope (position, label, property)) {

                FloatOnTransform target = (FloatOnTransform)fieldInfo.GetValue (property.serializedObject.targetObject);

                position.height = EditorGUIUtility.singleLineHeight;

                var sliderPosition = new Rect (position) {
                    width = position.width - EditorGUIUtility.labelWidth,
                    x = position.x + EditorGUIUtility.labelWidth
                };
                var transformPosition = new Rect (sliderPosition) {
                    y = sliderPosition.yMax
                };


                //各プロパティーの Rect を求める
                //var transformProperty = property.FindPropertyRelative ("transform");
                //var transform = (Transform)transformProperty.objectReferenceValue;


                EditorGUI.LabelField (position, label);

                if (target.transform != null) {

                    SerializedObject transformSerializedObject = new UnityEditor.SerializedObject (target.transform);
                    transformSerializedObject.Update ();
                    var sss = transformSerializedObject.FindProperty ("m_LocalPosition");
                    var v = EditorGUI.Slider (sliderPosition, sss.vector3Value.x, 0, 1);
                    sss.vector3Value = Vector3.right * v;
                    transformSerializedObject.ApplyModifiedProperties ();
                }

                EditorGUI.ObjectField (transformPosition, target.transform, typeof (Transform), true);
            }
        }

        //戻り値として返した値が GUI の高さとして使用されるようになる
        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight (property, label);

            return height * 2;
        }
    }
#endif
}