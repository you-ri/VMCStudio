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
        private const int kMicFrequency = 44100;
        private const int kLengthSeconds = 1;

        static readonly BlendShapePreset[] kMouseBlendShapePresets = new BlendShapePreset[] {
            BlendShapePreset.A,
            BlendShapePreset.I,
            BlendShapePreset.U,
            BlendShapePreset.E,
            BlendShapePreset.O,
        };
        // Manually assign the skinned mesh renderer to this script
        public VMCBlendShapeProxy blendShapeProxy = null;

        public int smoothAmount = 100;

        [Tooltip ("ループバックを有効にする")]
        public bool enalbeLoopback = false;

        [Tooltip ("最も高い口形素以外をゼロにする")]
        public bool maxVisemesEmphasis = true;

        [Tooltip ("口形素の倍率")]
        public float visemesMultiply = 1.0f;

        [Tooltip ("最小口形素のしきい値")]
        public float visemesThreashold = 0;

        [Tooltip ("「A」の口形のみにする")]
        public bool onlyVisemesA = true;

        public int deviceIndex { get; set; } = 0;

        public string selectedDevice {
            get {
                return Microphone.devices.Skip (deviceIndex).FirstOrDefault ();
            }
        }

        private bool micSelected { get { return !string.IsNullOrEmpty (selectedDevice); } }

        private int head = 0;
        private float[] processBuffer = new float[1024];
        private float[] microphoneBuffer = new float[kLengthSeconds * kMicFrequency];

        /// <summary>
        /// Start this instance.
        /// </summary>
        void Start ()
        {
            // Send smoothing amount to context
            this.Smoothing = smoothAmount;

            GetMicCaps ();
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
                audioSource = GetComponent<AudioSource> ();
                if (audioSource == null) {
                    _instancedAudioSource = head.gameObject.AddComponent<AudioSource> ();
                    audioSource = _instancedAudioSource;
                }
                audioSource.playOnAwake = true;
                audioSource.loop = true;
                //audioSource.mute = true;
            }
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update ()
        {
            if (blendShapeProxy != null) {

                if (audioSource != null) {
                    ProcessMicrophoneAudioReadFast ();
                }

                // trap inputs and send signals to phoneme engine for testing purposes
                // get the current viseme frame
                if (Frame != null) {
                    SetVisemeToMorphTarget (Frame);
                }
            }

        }

        float[] _visemes;

        /// <summary>
        /// Sets the viseme to morph target.
        /// </summary>
        void SetVisemeToMorphTarget (OVRLipSync.Frame frame)
        {

            _visemes = new float[] {
                frame.Visemes[(int)OVRLipSync.Viseme.aa],
                frame.Visemes[(int)OVRLipSync.Viseme.ih],
                frame.Visemes[(int)OVRLipSync.Viseme.ou],
                frame.Visemes[(int)OVRLipSync.Viseme.E],
                frame.Visemes[(int)OVRLipSync.Viseme.oh],
            };

            int maxIndex = 0;
            float maxVisemes = 0;
            for (int i = 0; i < _visemes.Length; i++) {
                if (_visemes[i] < visemesThreashold) _visemes[i] = 0;
                if (maxVisemes < _visemes[i]) {
                    maxIndex = i;
                    maxVisemes = _visemes[i];
                }
            }

            if (maxVisemesEmphasis) {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    if (i != maxIndex) _visemes[i] = 0.0f;
                }
            }


            // すべてを母音を「A」にまとめる。
            if (onlyVisemesA) {
                blendShapeProxy.SetValue (kMouseBlendShapePresets[0], maxVisemes * visemesMultiply);
            }
            /// A, I, U, E, O のそれぞれに反映する。
            else {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    blendShapeProxy.SetValue (kMouseBlendShapePresets[i], _visemes[i] * visemesMultiply);
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
            if (!enalbeLoopback) {
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
            if (!micSelected) return;

            audioSource.volume = 1;
            if (!Microphone.IsRecording (selectedDevice)) {
                StartMicrophone ();
            }

            var position = Microphone.GetPosition (selectedDevice);
            if (position < 0 || head == position) {
                return;
            }

            float maxVolume = 0;

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

                //maxVolume = Math.Max (processBuffer.Max (), maxVolume);
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
            audioSource.clip = Microphone.Start (selectedDevice, true, 1, kMicFrequency);

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
