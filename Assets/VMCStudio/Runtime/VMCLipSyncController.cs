using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using VRM;

namespace VMCStudio
{
    [DisallowMultipleComponent]
    [RequireComponent (typeof (VMCModelController))]
    public class VMCLipSyncController : LipSyncContextBase, IVMCModelController
    {
        static readonly BlendShapePreset[] kMouseBlendShapePresets = new BlendShapePreset[] {
            BlendShapePreset.A,
            BlendShapePreset.I,
            BlendShapePreset.U,
            BlendShapePreset.E,
            BlendShapePreset.O,
        };
        // Manually assign the skinned mesh renderer to this script
        public VMCBlendShapeProxy blendShapeProxy = null;

        public float weightMagnitude = 1;

        // smoothing amount
        public int SmoothAmount = 100;

        public float gain = 1.0f;

        public bool enalbeLoopback = false;

        public bool maxWeightEmphasis = false;
        public bool maxWeightEnable = false;
        public float WeightThreashold = 0;
        public float MaxLevel = 1.0f;

        [Tooltip ("「A」の口のみにする")]
        public bool onlyMouseA = true;

        public string selectedDevice;

        private float sourceVolume = 100;
        private bool micSelected = false;
        private int head = 0;
        private const int micFrequency = 44100;
        private const int lengthSeconds = 1;
        private float[] processBuffer = new float[1024];
        private float[] microphoneBuffer = new float[lengthSeconds * micFrequency];

        /// <summary>
        /// Start this instance.
        /// </summary>
        void Start ()
        {
            // Send smoothing amount to context
            this.Smoothing = SmoothAmount;

            if (Microphone.devices.Length != 0 && string.IsNullOrWhiteSpace (selectedDevice)) {
                selectedDevice = Microphone.devices.Last ().ToString ();
                micSelected = true;
                GetMicCaps ();
            }

        }

        AudioSource _instancedAudioSource;

        public void OnModelChanged (Animator target)
        {
            if (_instancedAudioSource) { 
                Destroy (_instancedAudioSource);
            }
            blendShapeProxy = null;
            audioSource = null;

            if (target != null && target.GetBoneTransform (HumanBodyBones.Head) != null && target.GetComponent<VMCBlendShapeProxy> ()) {
                blendShapeProxy = target.GetComponent<VMCBlendShapeProxy> ();

                var head = target.GetBoneTransform (HumanBodyBones.Head);
                audioSource = head.GetComponent<AudioSource> ();
                if (audioSource == null) {
                    _instancedAudioSource = head.gameObject.AddComponent<AudioSource> ();
                    audioSource = _instancedAudioSource;
                }
                audioSource.playOnAwake = true;
                audioSource.loop = true;
                audioSource.mute = true;
            }
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update ()
        {
            if (blendShapeProxy != null) {

                // trap inputs and send signals to phoneme engine for testing purposes
                // get the current viseme frame
                OVRLipSync.Frame frame = GetCurrentPhonemeFrame ();
                if (frame != null) {
                    SetVisemeToMorphTarget (frame);
                }
            }

            if (audioSource != null) {
                ProcessMicrophoneAudioReadFast ();
            }
        }

        float[] _visemes;

        /// <summary>
        /// Sets the viseme to morph target.
        /// </summary>
        void SetVisemeToMorphTarget (OVRLipSync.Frame frame)
        {
            if (_visemes == null) {
                _visemes = new float[] {
                    frame.Visemes[(int)OVRLipSync.Viseme.aa],
                    frame.Visemes[(int)OVRLipSync.Viseme.ih],
                    frame.Visemes[(int)OVRLipSync.Viseme.ou],
                    frame.Visemes[(int)OVRLipSync.Viseme.E],
                    frame.Visemes[(int)OVRLipSync.Viseme.oh],
                };
            }

            int maxIndex = 0;
            float maxVisemes = 0;
            for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                if (_visemes[i] < WeightThreashold) _visemes[i] = 0;
                if (maxVisemes < _visemes[i]) {
                    maxIndex = i;
                    maxVisemes = _visemes[i];
                }
            }

            if (maxWeightEmphasis) {
                _visemes[maxIndex] = Mathf.Clamp (_visemes[maxIndex] * 3, 0.0f, 1.0f);
            }

            if (maxWeightEnable) {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    if (i != maxIndex) _visemes[i] = 0.0f;
                }
            }


            // すべてを母音を「A」にまとめる。
            if (onlyMouseA) {
                blendShapeProxy.SetValue (kMouseBlendShapePresets[0], maxVisemes * MaxLevel);
            }
            /// A, I, U, E, O のそれぞれに反映する。
            else {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    _visemes[i] *= MaxLevel;
                    blendShapeProxy.SetValue (kMouseBlendShapePresets[i], _visemes[i]);
                }
            }
        }

        /// <summary>
        /// Raises the audio filter read event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="channels">Channels.</param>
        void OnAudioFilterRead (float[] data, int channels)
        {
            if (enalbeLoopback) {
                // 音がループバックしないように消去
                for (int i = 0; i < data.Length; ++i)
                    data[i] = data[i] * 0.0f;
            }
        }

        /// <summary>
        /// マイク入力専用音データ取得
        /// OnAudioFilterRead を使うより低遅延
        /// </summary>
        void ProcessMicrophoneAudioReadFast ()
        {
            if (string.IsNullOrEmpty (selectedDevice) == false) {
                audioSource.volume = (sourceVolume / 100);
                if (!Microphone.IsRecording (selectedDevice)) {
                    StartMicrophone ();
                }

                var position = Microphone.GetPosition (selectedDevice);
                if (position < 0 || head == position) {
                    return;
                }

                audioSource.clip.GetData (microphoneBuffer, 0);
                while (GetDataLength (microphoneBuffer.Length, head, position) > processBuffer.Length) {
                    var remain = microphoneBuffer.Length - head;
                    if (remain < processBuffer.Length) {
                        Array.Copy (microphoneBuffer, head, processBuffer, 0, remain);
                        Array.Copy (microphoneBuffer, 0, processBuffer, remain, processBuffer.Length - remain);
                    }
                    else {
                        Array.Copy (microphoneBuffer, head, processBuffer, 0, processBuffer.Length);
                    }

                    OVRLipSync.ProcessFrame (Context, processBuffer, Frame);

                    head += processBuffer.Length;
                    if (head > microphoneBuffer.Length) {
                        head -= microphoneBuffer.Length;
                    }
                    break;
                }
            }
        }


        static int GetDataLength (int bufferLength, int head, int tail)
        {
            if (head < tail) {
                return tail - head;
            }
            else {
                return bufferLength - head + tail;
            }
        }


        private int minFreq, maxFreq;
        public void GetMicCaps ()
        {
            if (micSelected == false) return;

            //Gets the frequency of the device
            Microphone.GetDeviceCaps (selectedDevice, out minFreq, out maxFreq);

            if (minFreq == 0 && maxFreq == 0) {
                Debug.LogWarning ("GetMicCaps warning:: min and max frequencies are 0");
                minFreq = 44100;
                maxFreq = 44100;
            }

        }

        public void StartMicrophone ()
        {
            if (micSelected == false) return;

            // Starts recording
            audioSource.clip = Microphone.Start (selectedDevice, true, 1, micFrequency);

            // Wait until the recording has started
            // 入力デバイスによっては無限ループする可能性があるのでコメントアウト
            //while (!(Microphone.GetPosition (selectedDevice) > 0)) { }

            // Play the audio source
            audioSource.Play ();
        }

        public void StopMicrophone ()
        {
            if (micSelected == false) return;

            // Overriden with a clip to play? Don't stop the audio source
            if ((audioSource != null) && (audioSource.clip != null) && (audioSource.clip.name == "Microphone")) {
                audioSource.Stop ();
            }

            Microphone.End (selectedDevice);
        }


        void OnDisable ()
        {
            StopMicrophone ();
        }

    }

}
