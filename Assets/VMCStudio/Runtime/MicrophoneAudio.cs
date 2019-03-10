using UnityEngine;
using System.Collections;

namespace VMCStudio
{


    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneAudio : MonoBehaviour
    {

        void Start()
        {

            var audio = GetComponent<AudioSource>();
            audio.clip = Microphone.Start(null, true, 999, 44100);  // マイクからのAudio-InをAudioSourceに流す
            audio.loop = true;                                      // ループ再生にしておく
            //audio.mute = true;                                      // マイクからの入力音なので音を流す必要がない
            while (!(Microphone.GetPosition("") > 0)) { }             // マイクが取れるまで待つ。空文字でデフォルトのマイクを探してくれる
            audio.Play();                                           // 再生する
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update()
        {


            //float vol = GetAveragedVolume();
            //Debug.Log(vol);

        }

    }

}
