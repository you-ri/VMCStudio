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
        ReorderableList _ro;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            var t = (VMCLipSyncController)this.target;

            t.deviceIndex = EditorGUILayout.Popup ("Device", t.deviceIndex, Microphone.devices);
        }
    }

}
