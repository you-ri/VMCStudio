#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.IO;

namespace VMCStudio
{
    public class VMCGenericAnimationRecorder : MonoBehaviour, IAnimationRecorer
    {
        public float frameRate = 60;
        public bool capFrameRate = true;
        public FrameRatePlayback frameRatePlayback = FrameRatePlayback.Constant;
        RecorderController m_RecorderController;

        public bool recordTransform = true;
        public bool recordBlendShapeProxy = true;

        RecorderControllerSettings controllerSettings;

        System.Action _onEndRecorded;

        System.DateTime _startTime;

        public bool isRecording
        {
            get { return m_RecorderController.IsRecording (); }
        }

        public void OnEnable ()
        {
            controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings> ();
            m_RecorderController = new RecorderController (controllerSettings);
        }

        void Start ()
        {
            _startTime = System.DateTime.Now;
            //var path = string.Format ("Assets/Recordings/{0}_{1:yyMMdd_HHmmss}.anim", _animator.name,);

        }


        public void StartRecording (GameObject target)
        {
            if (target == null) return;
            if (!(recordTransform || recordBlendShapeProxy)) {
                Debug.LogWarning ("It can not be recorded because there are no valid elements.");
                return;
            }

            // Animation
            var animationRecorder = ScriptableObject.CreateInstance<AnimationRecorderSettings> ();
            animationRecorder.name = "My Animation Recorder";
            animationRecorder.enabled = true;

            animationRecorder.animationInputSettings = new AnimationInputSettings {
                gameObject = target,
                recursive = true,
            };

            if (recordTransform) {
                animationRecorder.animationInputSettings.AddComponentToRecord (typeof (Transform));
            }
            if (recordBlendShapeProxy) {
                animationRecorder.animationInputSettings.AddComponentToRecord (typeof (VMCBlendShapeProxy));
            }

            SafeCreateDirectory ("Assets/Recordings");

            var recordingAnimationClipPath = $"Assets/Recordings/{target.name}_{_startTime:yyMMddHHmmss}_{RecordedObjectEntity.take}";
            recordingAnimationClipPath = AssetDatabase.GenerateUniqueAssetPath (recordingAnimationClipPath);
            animationRecorder.outputFile = System.IO.Path.GetFullPath (recordingAnimationClipPath);

            // Setup Recording
            controllerSettings.AddRecorderSettings (animationRecorder);
            controllerSettings.SetRecordModeToManual ();
            controllerSettings.frameRate = frameRate;
            controllerSettings.capFrameRate = capFrameRate;
            controllerSettings.frameRatePlayback = frameRatePlayback;

            Options.verboseMode = false;
            m_RecorderController.StartRecording ();

            _onEndRecorded = () => {

                var latestRecordedClip = AssetDatabase.LoadAssetAtPath<AnimationClip> (recordingAnimationClipPath + ".anim");

                //RecordedObjectEntity.latestEntities = new RecordedObjectEntity[0];
                RecordedObjectEntity.latestEntities.Add (new RecordedObjectEntity (target.GetComponent<Animator> (), latestRecordedClip));
            };
        }

        public void StopRecoreding ()
        {
            if (!isRecording) return;

            m_RecorderController.StopRecording ();
            _onEndRecorded ();

            RecordedObjectEntity.take += 1;
        }

        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory (string path)
        {
            return Directory.Exists (path) ? null : Directory.CreateDirectory (path);
        }
    }

    [System.Serializable]
    public class RecordedObjectEntity
    {
        public Animator target;
        public AnimationClip clip;

        public RecordedObjectEntity (Animator target, AnimationClip clip)
        {
            this.target = target;
            this.clip = clip;
        }


        public static int take = 0;
        public static List<RecordedObjectEntity> latestEntities = new List<RecordedObjectEntity> ();
    }

}

#endif
