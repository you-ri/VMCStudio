using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace VMCStudio.Tests
{
    public class VMCLipSyncControllerTest
    {
        class MoveUpDirection_TestScenario : VMCLipSyncController, IMonoBehaviourTest
        {
            public bool IsTestFinished { get; private set; }

            private void Start ()
            {
                StartCoroutine (TestScenario ());
            }

            private IEnumerator TestScenario ()
            {
                var go = new GameObject ();
                go.AddComponent<VMCLipSyncController> ();

                // 1フレーム待機
                yield return new WaitForSeconds (1);
                yield return null;

                // ここまで到達したらテストは成功
                IsTestFinished = true;

                // 他のテストを実行中にもこのMonoBehaviourが動いてしまうので止める
                gameObject.SetActive (false);
            }
        }

        [UnityTest]
        public IEnumerator ObjectMoveUpDirection ()
        {
            yield return new MonoBehaviourTest<MoveUpDirection_TestScenario> ();
        }
    }
}
