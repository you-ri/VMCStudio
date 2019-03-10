using System.Collections;
using UnityEngine;
using VRM;


namespace VMCStudio
{

    [DisallowMultipleComponent]
    [RequireComponent (typeof (VMCModelController))]
    public class VMCBlinker : MonoBehaviour, IVMCModelController
    {
        [SerializeField]
        public VMCBlendShapeProxy blendShapes;


        [SerializeField]
        float m_interVal = 5.0f;

        [SerializeField]
        float _closingTime = 0.06f;

        [SerializeField]
        float _openingSeconds = 0.03f;

        [SerializeField]
        float _closeSeconds = 0.1f;

        protected Coroutine _coroutine;

        float _nextRequest;
        bool _request;
        public bool Request
        {
            get { return _request; }
            set
            {
                if (Time.time < _nextRequest)
                {
                    return;
                }
                _request = value;
                _nextRequest = Time.time + 1.0f;
            }
        }

        private void Awake ()
        {
            if (blendShapes == null) blendShapes = GetComponent<VMCBlendShapeProxy> ();
        }

        private void OnEnable ()
        {
            if (blendShapes == null) return;
            _coroutine = StartCoroutine (BlinkRoutine ());
        }

        private void OnDisable ()
        {
            if (_coroutine != null) {
                StopCoroutine (_coroutine);
                _coroutine = null;
            }
        }


        protected IEnumerator BlinkRoutine()
        {
            while (blendShapes != null)
            {
                var waitTime = Time.time + Random.value * m_interVal;
                while (waitTime > Time.time)
                {
                    if (Request)
                    {
                        _request = false;
                        break;
                    }
                    yield return null;
                }

                // close
                var value = 0.0f;
                var closeSpeed = 1.0f / _closeSeconds;
                while (true)
                {
                    value += Time.deltaTime * closeSpeed;
                    if (value >= 1.0f)
                    {
                        break;
                    }

                    blendShapes.SetValue (BlendShapePreset.Blink, Mathf.Clamp01(value));
                    yield return null;
                }
                blendShapes.SetValue (BlendShapePreset.Blink, 1.0f);

                // wait...
                yield return new WaitForSeconds(_closingTime);

                // open
                value = 1.0f;
                var openSpeed = 1.0f / _openingSeconds;
                while (true)
                {
                    value -= Time.deltaTime * openSpeed;
                    if (value < 0)
                    {
                        break;
                    }

                    blendShapes.SetValue (BlendShapePreset.Blink, Mathf.Clamp01 (value));
                    yield return null;
                }
                blendShapes.SetValue (BlendShapePreset.Blink, 0);
            }
        }

        public void OnModelChanged (Animator target)
        {
            StopAllCoroutines ();
            blendShapes = null;

            if (target != null) {
                blendShapes = target.GetComponent<VMCBlendShapeProxy> ();
                _coroutine = StartCoroutine (BlinkRoutine ());
            }
        }
    }
}
