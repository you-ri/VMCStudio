using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VMCStudio;
using System.Linq;
using System.IO;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using UnityEditor.Formats.Fbx.Exporter;

namespace VMCStudioEditor
{

    public class FbxAnimationExporterWindow : EditorWindow
    {
        //public static RecordedObjectEntity[] objects = new RecordedObjectEntity[0];

        public string exportFolder = "./";

        [MenuItem ("Window/VMCStudio/FBX Animation Exporter")]
        public static void ShowWindow ()
        {
            EditorWindow.GetWindow<FbxAnimationExporterWindow>("FBX Animation Exporter");
        }


        void OnGUI ()
        {
            GUILayout.Label ("Export Objects", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("+")) {
                RecordedObjectEntity.latestEntities.Add (new RecordedObjectEntity (null, null));
            }
            GUILayout.EndHorizontal ();

            int removeIndex = -1;
            int index = 0;
            foreach (var obj in RecordedObjectEntity.latestEntities) {
                GUILayout.BeginHorizontal ();
                obj.target = (Animator)EditorGUILayout.ObjectField (obj.target, typeof (Animator), true);
                obj.clip = (AnimationClip)EditorGUILayout.ObjectField (obj.clip, typeof (AnimationClip), false);
                // 削除マーク
                if (GUILayout.Button ("-", GUILayout.Width(24))) {
                    removeIndex = index;
                }
                GUILayout.EndHorizontal ();
                index++;
            }
            // マークしたものを削除
            if (removeIndex >= 0) {
                RecordedObjectEntity.latestEntities.RemoveAt (removeIndex);
            }

            GUILayout.Space (16);
            GUILayout.Label ("Export Directory", EditorStyles.boldLabel);
            GUILayout.Label (Directory.GetCurrentDirectory ());
            exportFolder = EditorGUILayout.TextField (this.exportFolder);

            GUILayout.Space (16);
            if (GUILayout.Button ("Export", GUILayout.Height(42))) {
                Export ();
            }
            GUILayout.Space (16);
        }

        protected void Export ()
        {
            var validatedObjects = RecordedObjectEntity.latestEntities.Where (t => t.target != null).ToArray ();
            if (validatedObjects.Length == 0) {
                Debug.LogError ("Please set the export objects.");
                return;
            }

            var animatorControllerBackup = new Dictionary<Animator, RuntimeAnimatorController> ();

            try {
                // 出力用のAnimator.runtimeAnimatorController を作成
                foreach (var obj in validatedObjects) {
                    var target = obj.target;

                    animatorControllerBackup[target] = target.runtimeAnimatorController;


                    var controller = CreateAnimatorControllerWithClip (obj.clip);
                    target.runtimeAnimatorController = controller;
                }

                // 単数の場合はクリップ名なければ、オブジェクト名で複数ある場合はシーン名で出力する。
                string filename = SceneManager.GetActiveScene ().name;
                if (validatedObjects.Length == 1) {
                    if (validatedObjects[0].clip == null) {
                        filename = validatedObjects[0].target.name;
                    }
                    else {
                        filename = validatedObjects[0].clip.name;
                    }
                }

                string filePath = Path.Combine (exportFolder, filename + ".fbx");
                ModelExporter.ExportObjects (filePath, validatedObjects.Select (t => t.target).ToArray ());
            }
            finally {
                // Animator.runtimeAnimatorController をもとに戻す
                foreach (var obj in validatedObjects) {
                    Object.DestroyImmediate (obj.target.runtimeAnimatorController);
                    obj.target.runtimeAnimatorController = animatorControllerBackup[obj.target];
                }
            }



        }

        /// <summary>
        /// １個のAnitionClipを再生するだけの AnimatorController を作成する
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static RuntimeAnimatorController CreateAnimatorControllerWithClip (AnimationClip clip)
        {
            var animatorController = new AnimatorController ();
            animatorController.AddLayer ("main");
            animatorController.AddMotion (clip);
            EditorUtility.SetDirty (animatorController);

            return animatorController;
        }
    }

}
