using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Tween
{
    
    public class MaterialPropertyTweener : BaseTweener, ITweenable<Vector4>
    {

        private Renderer _renderer;
        private static MaterialPropertyBlock __Block;
        public static MaterialPropertyBlock sharedBlock { get { if (__Block == null) __Block = new MaterialPropertyBlock(); return __Block; } }
        
        public enum PropertyType { Color, Vector, Float, Range, TexEnv }
        
        private PropertyType m_PropertyType;
        
        private string m_PropertyName;
        
        private int _PropertyId = -1;
        public int propertyId {
            get {
                if (_PropertyId < 0 && !string.IsNullOrEmpty(m_PropertyName)) {
                    _PropertyId = Shader.PropertyToID(m_PropertyName);
                }
                return _PropertyId;
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public override ZTweener Tween(object @from, object to, float duration)
        {
            if (to is Vector4) {
                if (from is Vector4) {
                    return Tween((Vector4)from, (Vector4)to, duration);
                } else {
                    return Tween((Vector4)to, duration);
                }
            }

            return null;
        }

        public ZTweener Tween(Vector4 to, float duration)
        {
            var trans = _renderer as Renderer;
            if (trans == null) return null;

            return trans.Tween(GetVector, SetVector, to, duration);
        }

        public ZTweener Tween(Vector4 @from, Vector4 to, float duration)
        {
            var trans = _renderer as Renderer;
            if (trans == null) return null;

            return trans.Tween(GetVector, SetVector, to, duration);
        }

        public void SetProperty(string propName, PropertyType propType)
        {
            m_PropertyName = propName;
            m_PropertyType = propType;
            _PropertyId = -1;
        }

        Vector4 GetVector()
        {
            sharedBlock.Clear();
            _renderer.GetPropertyBlock(sharedBlock);
            return sharedBlock.GetVector(propertyId);
        }

        void SetVector(Vector4 value)
        {
            sharedBlock.Clear();
            _renderer.GetPropertyBlock(sharedBlock);
            sharedBlock.SetVector(propertyId, value);
            _renderer.SetPropertyBlock(sharedBlock);
        }
    }

}
