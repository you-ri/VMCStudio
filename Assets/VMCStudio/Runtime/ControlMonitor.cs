using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Cinemachine;

namespace VMCStudio
{
    /// <summary>
    /// コントロールモニタの作用と操作
    /// </summary>
    public class ControlMonitor : MonoBehaviour
    {

        [System.Serializable]
        public struct Refs
        {
            public PreviewCameraController[] previewCameras;

            public Button recodingButton;
            public Button recenterButton;
            public Button calibrationButton;
            public TextMeshProUGUI calibrationCountdownText;
            public TextMeshProUGUI mainCalibrationCountdownText;
            public CinemachineBrain mainCameraBrain;
            public Toggle roomButton;
            public RectTransform backgroundPanel;
            public Toggle ngToggleButton;

            public TrackersUIController trackerUIController;
        }

        // 背景用オブジェクト
        public GameObject background;

        public int calibrationDelayTime = 5;

        public Refs refs;

        /// <summary>
        /// NG状態か？
        /// </summary>
        bool _ng = false;

        /// <summary>
        /// 背景色
        /// </summary>
        Color[] _backgroundColors = { new Color (0.95f, 0.95f, 0.95f, 1), Color.green, Color.blue };

        public void OnValidate ()
        {
            refs.previewCameras = GetComponentsInChildren<PreviewCameraController> ().ToArray ();

        }

        public void Reset ()
        {
        }

        public void Start ()
        {
            refs.calibrationCountdownText.gameObject.SetActive (false);
            refs.mainCalibrationCountdownText.gameObject.SetActive (false);

            refs.calibrationButton.onClick.AddListener (() => {
                StopCoroutine (StartCalibration ());
                StartCoroutine (StartCalibration ());
            } );
            refs.recodingButton.onClick.AddListener (() => ToggleRecording ());
            refs.recenterButton.onClick.AddListener (() => Recenter ());

            foreach (var previewCamera in refs.previewCameras) {
                previewCamera.refs.activateButton.onClick.AddListener (() => ChangeVirtualCamera (previewCamera.virtualCamera));
            }

            Dictionary<Toggle, int> buttonMap = new Dictionary<Toggle, int> ();
            int index = 0;
            foreach (var toggle in refs.backgroundPanel.GetComponentsInChildren<Toggle> ()) {
                buttonMap[toggle] = index;
                toggle.onValueChanged.AddListener ((v) => { if (v) { ChangeBackground (buttonMap[toggle]); } });
                index++;
            }
            refs.ngToggleButton.onValueChanged.AddListener ((v) => SetActiveNgMode (v));

            SetActiveNgMode (false);

            refs.roomButton.GetComponentInChildren<TextMeshProUGUI> ().text = background != null ? background.name : "None";

            StartCoroutine (_TrackerUpdateLoop ());
        }

        IEnumerator _TrackerUpdateLoop ()
        {
            while (true) {
                yield return new WaitForSeconds (1);
                _UpdateTrackersList ();
            }
        }

        private void Update ()
        {
            // キーボード1,2,3... キーによるカメラ切り替え
            var keyCode = KeyCode.Alpha1;
            for (int i = 0; i < refs.previewCameras.Count (); i++) {
                if (Input.GetKey (keyCode) && refs.previewCameras.ElementAtOrDefault (i) != null) {
                    ChangeVirtualCamera (refs.previewCameras[i].virtualCamera);
                }
                keyCode += 1;
            }
        }

        /// <summary>
        /// キャリブレーション実施
        /// </summary>
        public IEnumerator StartCalibration ()
        {
            var target = VMCModelController.currentTarget;
            var calibratar = VMCModelController.controller.GetComponent<VMCFullBodyController> ();
            refs.calibrationButton.interactable = false;

            yield return StartCalibration (
                target.gameObject,
                calibrationDelayTime,
                (count) => {
                    refs.calibrationCountdownText.gameObject.SetActive (true);
                    refs.calibrationCountdownText.text = $"{count}";
                    refs.mainCalibrationCountdownText.gameObject.SetActive (true);
                    refs.mainCalibrationCountdownText.text = $"Callibration\n{count}";
                    ChangeVirtualCamera (refs.previewCameras[0].virtualCamera);
                    _UpdateTrackersList ();
                },
                () => {
                    refs.calibrationButton.interactable = true;
                    refs.calibrationCountdownText.gameObject.SetActive (false);
                    refs.mainCalibrationCountdownText.gameObject.SetActive (false);
                    _UpdateTrackersList ();
                }
            );

            _UpdateTrackersList ();
        }

        /// <summary>
        /// キャリブレーション
        /// </summary>
        /// <param name="target"></param>
        /// <param name="calibrationDelayTime"></param>
        /// <param name="countdown"></param>
        /// <param name="finish"></param>           
        /// <returns></returns>
        public IEnumerator StartCalibration (GameObject target, int calibrationDelayTime, Action<int> countdown, Action finish)
        {
            var avatarController = VMCModelController.controller;
            var calibratables = avatarController.GetComponents<ICalibratable> ();

            // キャリブレーション準備
            foreach (var calibratable in calibratables) {
                calibratable.PrepareCalibrate ();
            }

            // カウントダウン
            for (var i = 0; i < calibrationDelayTime; ++i) {
                countdown (calibrationDelayTime - i);
                yield return new WaitForSeconds (1);
            }
            countdown (0);

            // VMC のキャリブレーション機能を呼びだす
            foreach (var calibratable in calibratables) {
                yield return calibratable.Calibrate ();
            }

            // キャリブレーション後にリセンター
            Recenter ();

            finish ();
        }

        public void ToggleRecording ()
        {
            var target = VMCModelController.currentTarget;

            var recorders = GetComponents<MonoBehaviour> ().Where (t => t.isActiveAndEnabled && t is IAnimationRecorer).Select (t => t as IAnimationRecorer);

            bool isRecordingPrev = recorders.Any (t => t.isRecording);
            foreach (var recorder in recorders) {
                if (!isRecordingPrev) {
                    recorder.StartRecording (target.gameObject);
                }
                else {
                    recorder.StopRecoreding ();
                }
            }

            bool isRecording = recorders.Any (t => t.isRecording);
            if (isRecording) {
                refs.recodingButton.image.color = Color.red;
                refs.recodingButton.GetComponentInChildren<TextMeshProUGUI> ().text = "STOP\nRECORDING";
            }
            else {
                refs.recodingButton.image.color = Color.white;
                refs.recodingButton.GetComponentInChildren<TextMeshProUGUI> ().text = "START\nRECORDING";
            }
        }

        /// <summary>
        /// カメラ切り替え
        /// </summary>
        /// <param name="previewCamera"></param>
        public void ChangeVirtualCamera (CinemachineVirtualCamera previewCamera)
        {
            if (previewCamera == null) return;

            var camera = refs.mainCameraBrain.OutputCamera;
            Debug.Assert (camera != null);

            camera.gameObject.SetActive (true);
            camera.cullingMask = (int)(camera.cullingMask & 0x007FFFFF) | (1 << previewCamera.gameObject.layer);
        }

        int _currentBackgroundMode = 0;

        /// <summary>
        /// 背景切り替え
        /// </summary>
        public void ChangeBackground (int index)
        {
            if (!_ng) {
                if (index != 0) {
                    Camera.main.backgroundColor = _backgroundColors[index - 1];
                    Camera.main.clearFlags = CameraClearFlags.Color;
                    SetActiveBackgroundModel (false);
                }
                else {
                    Camera.main.clearFlags = CameraClearFlags.Skybox;
                    SetActiveBackgroundModel (true);
                }
            }
            _currentBackgroundMode = index;
        }

        /// <summary>
        /// 背景モデル表示切り替え
        /// </summary>
        public void SetActiveBackgroundModel (bool value)
        {
            // 背景表示
            if (background != null) {
                background.SetActive (value);
            }
        }

        /// <summary>
        /// リセンター
        /// </summary>
        public void Recenter ()
        {
            var recentables = VMCModelController.controller.GetComponents<IRecenterble> ();
            foreach (var recenterble in recentables) {
                recenterble.Recenter ();
            }
        }


        /// <summary>
        /// NGモード
        /// </summary>
        /// <param name="value"></param>
        public void SetActiveNgMode (bool value)
        {
            SetActiveBackgroundModel (false);
            Camera.main.backgroundColor = Color.red;
            Camera.main.clearFlags = CameraClearFlags.Color;

            _ng = value;

            // 背景をもとに戻す
            if (!value) {
                ChangeBackground (_currentBackgroundMode);
            }
        }

         void _UpdateTrackersList()
        {
            refs.trackerUIController.UpdateTrackersList ();
        }
    }

}
