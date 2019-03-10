using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using VRM;
using System.Linq.Expressions;

namespace VMCStudio
{
    /// <summary>
    /// ブレンドシェイプキー属性
    /// </summary>
    [AttributeUsage (
        AttributeTargets.Field,
        AllowMultiple = true,
        Inherited = false)]
    public class BlendShapeKeyAttribute : Attribute
    {
        public BlendShapeKeyAttribute (string name)
        {
            this.name = name;
        }

        public string name { get; }
    }

    /// <summary>
    /// 高速な動的なゲットとセットを行うデリゲート生成
    /// </summary>
    /// ref: http://neue.cc/2011/04/20_317.html
    static public class TypeExtensions
    {
        // (object target) => (object)((T)target).propertyName
        static public Func<object, object> CreateGetDelegate (this Type type, string propertyName)
        {
            var target = Expression.Parameter (typeof (object), "target");

            var lambda = Expression.Lambda<Func<object, object>> (
                Expression.Convert (
                    Expression.PropertyOrField (
                        Expression.Convert (
                            target
                            , type)
                        , propertyName)
                    , typeof (object))
                , target);
            return lambda.Compile ();
        }

        // (object target, object value) => ((T) target).memberName = (U) value
        static public Action<object, object> CreateSetDelegate (this Type type, string memberName)
        {
            var target = Expression.Parameter (typeof (object), "target");
            var value = Expression.Parameter (typeof (object), "value");

            var left =
                Expression.PropertyOrField (
                    Expression.Convert (target, type), memberName);

            var right = Expression.Convert (value, left.Type);

            var lambda = Expression.Lambda<Action<object, object>> (
                Expression.Assign (left, right),
                target, value);

            return lambda.Compile ();
        }
    }

    /// <summary>
    /// 表情操作
    /// VRMBlendShapeProxyの置き換え用。
    /// VRMBlendShapeProxyでは Timeline での表情編集に対応しないため。
    /// </summary>
    [ExecuteInEditMode]
    public class VMCBlendShapeProxy : MonoBehaviour
    {
        /// <summary>
        /// BlendShapeKeyとメンバフィールドとのバインド
        /// </summary>
        struct BlendShapeKeyBinder
        {
            public Func<object, object> getter;
            public Action<object, object> setter;
            public BlendShapeKey key;

            public BlendShapeKeyBinder (string name, Func<object, object> getter, Action<object, object> setter)
            {
                key = new BlendShapeKey (name);
                this.setter = setter;
                this.getter = getter;
            }
        }

        public BlendShapeAvatar blendShapeAvatar;

        BlendShapeKeyBinder[] _binders;

        BlendShapeMerger _merger;

        [Range (0, 1)]
        [BlendShapeKey ("blink")]
        public float blink;

        [Range (0, 1)]
        [BlendShapeKey ("blink_r")]
        public float blink_r;

        [Range (0, 1)]
        [BlendShapeKey ("blink_l")]
        public float blink_l;

        [Range (0, 1)]
        [BlendShapeKey ("a")]
        public float a;

        [Range (0, 1)]
        [BlendShapeKey ("i")]
        public float i;

        [Range (0, 1)]
        [BlendShapeKey ("u")]
        public float u;

        [Range (0, 1)]
        [BlendShapeKey ("e")]
        public float e;

        [Range (0, 1)]
        [BlendShapeKey ("o")]
        public float o;

        [Range (0, 1)]
        [BlendShapeKey ("fun")]
        public float fun;

        [Range (0, 1)]
        [BlendShapeKey("joy")]
        public float joy;

        [Range (0, 1)]
        [BlendShapeKey ("angry")]
        public float angry;

        [Range (0, 1)]
        [BlendShapeKey ("sorrow")]
        public float sorrow;

        [Range (0, 1)]
        [BlendShapeKey ("surprise")]
        public float surprise;

        private void Reset ()
        {
            _Construction ();
        }

        // Use this for initialization
        private void OnEnable ()
        {
            _Construction ();
        }

        private void OnDisable ()
        {
            _Destruction ();
        }

        void _Construction ()
        {
            if (blendShapeAvatar == null) {
                blendShapeAvatar = GetComponent<VRM.VRMBlendShapeProxy> ().BlendShapeAvatar;
            }
            if (blendShapeAvatar != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger = new BlendShapeMerger (validatedBlendShapeAvatarClips, this.transform);
            }

            List<BlendShapeKeyBinder> entities = new List<BlendShapeKeyBinder> ();
            foreach (var info in GetType().GetFields (BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)) {

                Attribute[] attributes = Attribute.GetCustomAttributes (
                    info, typeof (BlendShapeKeyAttribute));

                foreach (Attribute attr in attributes) {
                    var blendShapeEntityAttr = attr as BlendShapeKeyAttribute;
                    if (blendShapeEntityAttr == null) continue;

#if false
                    Func<VMCBlendShapeProxy, string> getDelegate =
                        (Func<VMCBlendShapeProxy, float>)Delegate.CreateDelegate (
                                 typeof (Func<VMCBlendShapeProxy, float>),
                                 info.GetValue (nonPublic: true));
#endif
                    entities.Add (
                        new BlendShapeKeyBinder (
                            blendShapeEntityAttr.name,
                            GetType ().CreateGetDelegate (info.Name),
                            GetType ().CreateSetDelegate (info.Name)
                        )
                    );
                }
            }

            _binders = entities.ToArray ();
        }


        public void _Destruction ()
        {
            if (_merger != null) {
                var validatedBlendShapeAvatarClips = blendShapeAvatar.Clips.Where (t => t != null);  // ここで NULL を排除しなとエラーが発生する。VRM.BlendShapeMerger のバグ？
                _merger.RestoreMaterialInitialValues (validatedBlendShapeAvatarClips);
                _merger = null;
            }
            _binders = null;
        }

        private void Start ()
        {
            if (GetComponent<VRM.VRMBlendShapeProxy> ()) {
                GetComponent<VRM.VRMBlendShapeProxy> ().enabled = false;
            }
        }


        private void LateUpdate ()
        {
            foreach (var entity in _binders) {
                _merger.SetValue (entity.key, (float)entity.getter(this), false);
            }
            _merger.Apply ();
        }

        private void OnDestroy ()
        {
            _Destruction ();
        }

        public void Apply()
        {
            if (_merger != null) {
                _merger.Apply ();
            }
        }

        public void SetValue(string name, float value)
        {
            var binders = _binders.Where (t => t.key.Name == name.ToUpper());
            if (binders.Count() == 0) {
                //Debug.LogWarning ($"undefined blendshape member. name:{name}");
                return;
            }
            binders.First ().setter (this, value);
        }

        public void SetValue (BlendShapePreset preset, float value)
        {
            SetValue (preset.ToString(), value);
        }

        public float GetValue (BlendShapeKey key)
        {
            if (_merger == null) {
                return 0;
            }
            return _merger.GetValue (key);
        }

        public float GetValue (BlendShapePreset key)
        {
            return GetValue (new BlendShapeKey (key));
        }



    }
}