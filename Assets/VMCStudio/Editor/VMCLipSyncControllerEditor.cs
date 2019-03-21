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

        SerializedObject _so;

        public void OnEnable ()
        {
            _so = new SerializedObject (target);

            deviceIndex = _so.FindProperty ("deviceIndex");
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            var t = (VMCLipSyncController)this.target;

            deviceIndex.intValue = EditorGUILayout.Popup ("Device", deviceIndex.intValue, Microphone.devices);
            _so.ApplyModifiedProperties ();
        }
    }

}
