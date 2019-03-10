using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

namespace VMCStudio
{
    /// <summary>
    /// CinemachineVirtualCameraの操作
    /// </summary>
    [RequireComponent (typeof(CinemachineVirtualCamera))]
    public class VMCVirtualCameraController : MonoBehaviour, IVMCModelController
    {
        /// <summary>
        /// シーンに配置している有効な VMCVirtualCameraController を全て取得する
        /// </summary>
        public static List<VMCVirtualCameraController> controllers
        {
            get {
                return FindObjectsOfType<VMCVirtualCameraController> ().Where (t => t.isActiveAndEnabled).ToList ();
            }
        }

        /// <summary>
        /// シーンに配置している有効な VMCVirtualCameraController を１つ取得する
        /// </summary>
        public static VMCVirtualCameraController controller
        {
            get { return controllers.FirstOrDefault (); }
        }


        public HumanBodyBones lookAtBone = HumanBodyBones.Head;

        ControlMonitor _controlSystemManager;

        // Start is called before the first frame update
        void Start ()
        {
        }

        [ContextMenu("Construct")]
        public void Construct(Animator target)
        {
            if (VMCModelController.currentTarget == null) return;

            var lookAt = VMCModelController.currentTarget.GetBoneTransform (lookAtBone);
            if (lookAt != null) {
                GetComponent<CinemachineVirtualCamera> ().LookAt = lookAt;
            }
        }

        public void OnModelChanged (Animator target)
        {
            Construct (target);
        }
    }

}

