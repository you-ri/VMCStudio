using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class VMCStudioAssets : ScriptableObject
{
    public const string PATH = "VMCStudio Assets";

    public Transform pointGizmo;


    //MyScriptableObjectの実体
    private static VMCStudioAssets _entity;
    public static VMCStudioAssets entity
    {
        get {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<VMCStudioAssets>(PATH);

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(PATH + " not found");
                }
            }

            return _entity;
        }
    }
}
