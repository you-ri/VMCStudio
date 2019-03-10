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
    [CustomEditor (typeof (VMCStudio.VMCStudio))]
    class VMCStudioInspector : Editor
    {
        ReorderableList _ro;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            serializedObject.ApplyModifiedProperties ();
        }
    }

}
