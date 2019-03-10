using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace VMCStudio
{

    public class TrackersUIController : MonoBehaviour
    {
        [System.Serializable]
        struct References
        {
            public Tracker‎IndicatorUIController labelTemplate;
            public RectTransform listView;
        }

        [SerializeField]
        References refs;

        // Start is called before the first frame update
        void Start ()
        {
            refs.labelTemplate.gameObject.SetActive (false);
        }

        /// <summary>
        /// トラッカーリストの更新
        /// </summary>
        /// <param name="hta"></param>
        public void UpdateTrackersList ()
        {
            var trackers = VMCStudio.calibrationController.trackers;
            var indicators = refs.listView.GetComponentsInChildren<TrackerIndicatorUIController> ();

            Transform sibling = null;
            foreach (var tracker in trackers) {
                var indicator = indicators.Where (t => t.serial == tracker.serial).FirstOrDefault ();
                if (indicator != null) {
                    indicator.Setup (tracker);
                }
                else {
                    indicator = _AddItem (tracker);
                    indicator.Setup (tracker);
                }
                indicator.transform.SetSiblingIndex (sibling != null ? sibling.GetSiblingIndex () + 1 : 0);     // 順番を維持する
                sibling = indicator.transform;
            }
            foreach (var indicator in indicators) {
                if (trackers.Any (t => t.serial == indicator.serial)) continue;
                Destroy (indicator.gameObject);
            }
        }

        TrackerIndicatorUIController _AddItem (TrackerState tracker)
        {
            var indicator = Instantiate (refs.labelTemplate, this.refs.listView);
            indicator.gameObject.SetActive (true);
            return indicator;
        }
    }

}