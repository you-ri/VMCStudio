using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VMCStudio
{

    /// <summary>
    /// 姿勢の記録と復元
    /// </summary>
    public class PoseMemory
    {
        public struct Element
        {
            public Transform transform;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;

            public Element (Transform transform)
            {
                this.transform = transform;
                this.localPosition = transform.localPosition;
                this.localRotation = transform.localRotation;
                this.localScale = transform.localScale;
            }

            public void Restore()
            {
                if (transform == null) return;
                transform.localPosition = this.localPosition;
                transform.localRotation = this.localRotation;
                transform.localScale = this.localScale;
            }

        }

        Element[] _temporaryBuffer;

        /// <summary>
        /// 現在の姿勢を記録
        /// </summary>
        /// <param name="target"></param>
        public void Store (GameObject target)
        {
            Debug.Assert (target != null);

            _temporaryBuffer = target.GetComponentsInChildren<Transform> (true).Select (t => new Element (t)).ToArray ();
        }

        /// <summary>
        /// 記録した姿勢を復元
        /// </summary>
        public void Resotre ()
        {
            foreach(var element in _temporaryBuffer) {
                element.Restore ();
            }
        }

        public void Clear()
        {
            _temporaryBuffer = new Element [0];
        }
    }


}