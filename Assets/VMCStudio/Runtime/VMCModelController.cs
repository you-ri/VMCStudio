using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VMCStudio
{

    /// <summary>
    /// シェルを操作する
    /// </summary>
    public class VMCModelController : MonoBehaviour
    {
        /// <summary>
        /// シーンに配置している有効な VMCModelController を全て取得する
        /// </summary>
        public static List<VMCModelController> controllers
        {
            get {
                return FindObjectsOfType<VMCModelController> ().Where (t => t.isActiveAndEnabled).ToList ();
            }
        }

        /// <summary>
        /// シーンに配置している有効な VMCModelController を１つ取得する
        /// </summary>
        public static VMCModelController controller
        {
            get { return controllers.FirstOrDefault (); }
        }

        public static Animator currentTarget
        {
            get { return controller != null ? controller.target : null; }
        }

        [FormerlySerializedAs ("avatar")]
        public Animator target;

        public Animator[] models = new Animator[0];

        private int _currentModelIndex = 0;

        public KeyCode modelSwitchKey = KeyCode.Tab;

        private void OnValidate ()
        {
        }

        public void Awake ()
        {
            if (target == null) {
                target = models.Where (t => t != null).FirstOrDefault ();
            }

            for (int i = 0; i < models.Length; i++) {
                if (models[i] == null) continue;
                models[i].gameObject.SetActive (i == _currentModelIndex);
            }
        }

        private void Start ()
        {
            if (target == null) {
                Debug.LogWarning ($"Please set the Shell target at {this}", this);
            }

            _ChangeModel (target);
        }

        void _ChangeModel (Animator target)
        {
            foreach (var controller in GetComponents<IVMCModelController> ()) {
                controller.OnModelChanged (target);
            }

            foreach (var cameraController in VMCVirtualCameraController.controllers) {
                cameraController.OnModelChanged (target);
            }
        }

#if UNITY_EDITOR
        [ContextMenu ("Setup Animator For Generic Animation")]
        public void SetupAvatarForStudio ()
        {
            if (target == null) {
                Debug.LogWarning ("Target is not set. Cancel processing.");
                return;
            }

            target.GetComponent<Animator> ().avatar = null;

            print ($"Setup the Animator setting to GenericAnimation at {target}.");
        }

        [ContextMenu ("Revert Animator")]
        public void SetupAvatarForCapture ()
        {
            if (target == null) {
                Debug.LogWarning ("Target is not set. Cancel processing.");
                return;
            }
            SerializedObject serializedObject = new UnityEditor.SerializedObject (target.GetComponent<Animator> ());
            SerializedProperty serializedPropertyAvatar = serializedObject.FindProperty ("m_Avatar");
            PrefabUtility.RevertPropertyOverride (serializedPropertyAvatar, InteractionMode.UserAction);

            print ($"Revert the Animator settings at {target}.");
        }
#endif

        private void Update ()
        {
            if (Input.GetKeyDown(modelSwitchKey)) {
                if (models.Any()) {
                    _currentModelIndex++;
                    ChangeModel (models[_currentModelIndex % models.Length]);
                }
            }
        }

        public void ChangeModel (Animator target)
        {
            if (this.target != null) {
                this.target.gameObject.SetActive (false);
            }
            this.target = target;

            target.gameObject.SetActive (true);
            _ChangeModel (target);

        }
    }
}