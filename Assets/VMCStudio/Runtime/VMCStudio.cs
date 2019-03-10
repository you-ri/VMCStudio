using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using VRM;
using RootMotion.FinalIK;
using Valve.VR;
using sh_akira.OVRTracking;
using UnityEditor;

namespace VMCStudio
{
    /// <summary>
    /// キャリブレーションを実装するコンポーネントはこれを継承する
    /// </summary>
    public interface ICalibratable
    {
        /// <summary>
        /// キャリブレーション準備
        /// </summary>
        void PrepareCalibrate ();

        /// <summary>
        /// キャリブレーション実行
        /// </summary>
        IEnumerator Calibrate ();
    }

    /// <summary>
    /// リセンターを実装するコンポーネントはこれを継承する
    /// </summary>
    public interface IRecenterble
    {
        /// <summary>
        /// リセンター実行
        /// </summary>
        void Recenter ();
    }


    /// <summary>
    /// モデルを操作するコンポーネントはこれを継承する
    /// </summary>
    public interface IVMCModelController
    {
        /// <summary>
        /// ターゲットが変化した時、及びスタート時に呼ばれる
        /// </summary>
        /// <param name="target"></param>
        void OnModelChanged (Animator target);
    }

    /// <summary>
    /// レコード処理を行うコンポーネントはこれを継承する
    /// </summary>
    public interface IAnimationRecorer
    {
        /// <summary>
        /// 記録中か？
        /// </summary>
        bool isRecording { get; }

        /// <summary>
        /// 記録開始
        /// </summary>
        /// <param name="target"></param>
        void StartRecording (GameObject target);

        /// <summary>
        /// 記録停止
        /// </summary>
        void StopRecoreding ();
    }

    /// <summary>
    /// VMCStudioマネージャー
    /// </summary>
    public class VMCStudio : MonoBehaviour
    {
        [System.Serializable]
        public struct Refs
        {
            public GameObject vrCameraRoot;
            public Camera mainCamera;
        }

        [System.Serializable]
        class DebugParameters
        {
        }

        public bool enableVR = false;
        public bool enableNDI = false;

        public Refs refs;

        [SerializeField]
        DebugParameters debug;

        [Obsolete]
        public static VMCFullBodyController calibrationController
        {
            get { return FindObjectOfType<VMCFullBodyController> (); }
        }

        // Start is called before the first frame update
        void Start ()
        {
            // マルチディスプレイ初期化
            if (Display.displays.Length > 1)
                Display.displays[1].Activate ();

            if (enableVR)
            {
                OpenVRWrapper.Instance.Setup(EVRApplicationType.VRApplication_Scene);
                refs.vrCameraRoot.SetActive(true);
            }
            else
            {
                OpenVRWrapper.Instance.Setup(EVRApplicationType.VRApplication_Background);
                refs.vrCameraRoot.SetActive(false);
            }

            refs.mainCamera.GetComponent<Klak.Ndi.NdiSender> ().enabled = enableNDI;
        }

    }

}
