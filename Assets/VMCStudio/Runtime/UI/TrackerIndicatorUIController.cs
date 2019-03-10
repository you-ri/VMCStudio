using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Valve.VR;

namespace VMCStudio
{


    public class TrackerIndicatorUIController : MonoBehaviour
    {
        [System.Serializable]
        struct References
        {
            public TextMeshProUGUI labelText;
            public TextMeshProUGUI headText;
            public Image indicatorImage;
            public TextMeshProUGUI batteryText;
        }

        public string serial;

        TrackerState _tracker;

        public float minVelocityChangeRate = 0.01f;
        public float maxVelocityChangeRate = 0.3f;
        public float indicatorSpeed = 10.0f;

        float _weight;

        [SerializeField]
        References refs;

        Vector3 _prevAngularVelocity;
        Vector3 _prevVelocity;

        private void Start ()
        {
            _prevVelocity = Vector3.zero;
            _prevAngularVelocity = Vector3.zero;
            _weight = 0;
            refs.indicatorImage.color = Mathf.CorrelatedColorTemperatureToRGB (Mathf.Lerp (40000, 1000, _weight));
        }

        public void Setup (TrackerState tracker)
        {
            _tracker = tracker;
            _UpdateDisplay ();
        }

        void _UpdateDisplay()
        {
            Debug.Assert (_tracker != null);

            serial = _tracker.serial;
            float battery = _tracker.GetBatteryPercentage ();

            refs.labelText.text = serial;
            refs.headText.text = _tracker.trackingPoint.ToString ();
            GetComponent<Image> ().color = _tracker.valid ? Color.white : Color.gray;
            if (_tracker.GetBatteryPercentage () <= 20) {
                GetComponent<Image> ().color = Color.red;
            }
            refs.batteryText.rectTransform.parent.gameObject.SetActive (_tracker.valid);
            refs.indicatorImage.rectTransform.parent.gameObject.SetActive (_tracker.valid);
        }

        private void Update ()
        {
            _UpdateDisplay ();

            if (_tracker == null || _tracker.transform == null) {
                return;
            }

            var t = sh_akira.OVRTracking.OpenVRWrapper.Instance.GetTransform (_tracker.deviceIndex);
            if (t == null) return;

            var weight = Smooth (minVelocityChangeRate, maxVelocityChangeRate, (_prevVelocity - t.velocity).magnitude + (_prevAngularVelocity - t.angularVelocity).magnitude*0.2f);
            _weight = Mathf.Lerp (_weight, weight, Time.deltaTime * indicatorSpeed);

            refs.indicatorImage.color = Mathf.CorrelatedColorTemperatureToRGB (Mathf.Lerp (40000, 1000, _weight));
            _prevVelocity = t.velocity;
            _prevAngularVelocity = t.angularVelocity;

            _UpdateBattery ();
        }

        void _UpdateBattery()
        {
            float battery = _tracker.GetBatteryPercentage ();
            if (!float.IsNaN (battery)) {
                refs.batteryText.text = $"{(int)_tracker.GetBatteryPercentage ()}%";
            }
            else {
                refs.batteryText.text = $"NaN";
            }
        }

        public static float Smooth (float min, float max, float value)
        {
            return Mathf.Clamp01 ((value - min) / (max - min));
        }
    }

}