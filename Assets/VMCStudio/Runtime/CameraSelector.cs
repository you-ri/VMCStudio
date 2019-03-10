using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;
using UnityEngineInternal.Input;

/// <summary>
/// カメラを選択する
/// </summary>
public class CameraSelector : MonoBehaviour
{
    Camera _camera;
    CinemachineBrain _brain;

    public int baseCameraLayer = 23;

    private void Awake ()
    {
        _camera = GetComponent<Camera> ();
        _brain = GetComponent<CinemachineBrain> ();
    }

    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    void Update ()
    { 

    }

    /// <summary>
    /// カメラ切り替え
    /// </summary>
    /// <param name="no"></param>
    void SelectCamera (int layer)
    {
        _camera.cullingMask = (int)(_camera.cullingMask & 0x007FFFFF) | (1 << layer);
    }
}
