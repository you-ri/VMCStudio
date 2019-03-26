using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VMCStudio;
using VRM;

namespace VMCStudioEditor
{

    /// <summary>
    /// VTAvatarInputController インスペクター
    /// </summary>
    [CustomEditor (typeof (VMCStudio.VMCLipSyncController))]
    class VMCLipSyncControllerEditor : Editor
    {
        SerializedProperty deviceIndex;

        public void OnEnable ()
        {

            deviceIndex = serializedObject.FindProperty ("deviceIndex");
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            var t = (VMCLipSyncController)this.target;

            if (t.useMicrophone) {
                deviceIndex.intValue = EditorGUILayout.Popup ("Device", deviceIndex.intValue, Microphone.devices);
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }

}
