using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using System.Linq;

namespace VMCStudio
{

    public class PreviewCameraController : MonoBehaviour
    {
        public Camera previewCamera;

        public CinemachineVirtualCamera virtualCamera
        {
            get { return _virtualCamera; }
        }
        CinemachineVirtualCamera _virtualCamera;

        [System.Serializable]
        public struct Refs
        {
            public Button activateButton;
            public TextMeshProUGUI noText;
            public TextMeshProUGUI headerText;
        }

        public Refs refs;

        // Start is called before the first frame update
        void Start ()
        {
            if (previewCamera != null) {
                _virtualCamera = FindObjectsOfType<CinemachineVirtualCamera> ().Where (t => t.gameObject.layer == previewCamera.gameObject.layer).FirstOrDefault ();
            }
            if (_virtualCamera != null) {
                refs.headerText.text = _virtualCamera.name;
            }
            else {
                refs.headerText.text = "NONE";

            }
        }

    }


}