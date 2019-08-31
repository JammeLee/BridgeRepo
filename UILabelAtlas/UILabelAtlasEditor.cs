using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using System.IO;
using System.Linq;
using UnityEngine.UI;

namespace ZFrame.Editors
{
    using UGUI;

    [CustomEditor(typeof(UILabelAtlas))]
    [CanEditMultipleObjects]
    public class UILabelAtlasEditor : Editor
    {

        private SerializedProperty _mAtlasName, _mSpriteName, _mPrefab, _mGroup, _mValue, _mPrefixName, _mArrSpriteName, _mSpriteCache;

        private static System.Type _spriteEditorWindowType;

        public static void OpenSpriteEditor(SpriteAtlas atlas, Sprite sprite)
        {
            if (_spriteEditorWindowType == null)
            {
                _spriteEditorWindowType = typeof(EditorWindow).Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "SpriteEditorWindow");
            }

            var spriteName = sprite.name;
            if (spriteName.EndsWith("(Clone)"))
            {
                spriteName = spriteName.Substring(0, spriteName.Length - 7);
            }
            var path = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(atlas))
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == spriteName);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture>(path);
            EditorWindow.GetWindow(_spriteEditorWindowType);
        }

        protected void OnEnable()
        {
            //base.OnEnable();

            _mAtlasName = serializedObject.FindProperty("mAtlasName");
            _mSpriteName = serializedObject.FindProperty("mSpriteName");
            _mPrefab = serializedObject.FindProperty("mTemplate");
            _mGroup = serializedObject.FindProperty("mGroup");
            _mValue = serializedObject.FindProperty("mValue");
            _mPrefixName = serializedObject.FindProperty("mPrefixName");
            _mArrSpriteName = serializedObject.FindProperty("mArrSpriteName");
            _mSpriteCache = serializedObject.FindProperty("mSpriteCache");
            //m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            //m_Type = serializedObject.FindProperty("m_Type");
        }

        public static SpriteAtlas FindAtlas(string bundleName, string atlasName)
        {
            var atlasPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, atlasName);
            if (atlasPaths != null && atlasPaths.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath(atlasPaths[0], typeof(SpriteAtlas)) as SpriteAtlas;
            }
            return null;
        }

        private void UpdateSprite(Sprite newSprite)
        {
            var self = (UILabelAtlas)target;
            self.SpriteCache = newSprite;
            //if (newSprite)
            //{
            //    Image.Type oldType = (Image.Type)m_Type.enumValueIndex;
            //    if (newSprite.border.SqrMagnitude() > 0)
            //    {
            //        m_Type.enumValueIndex = (int)Image.Type.Sliced;
            //    }
            //    else if (oldType == Image.Type.Sliced)
            //    {
            //        m_Type.enumValueIndex = (int)Image.Type.Simple;
            //    }
            //}
        }

        private void OnSpriteChanged(string spriteName)
        {
//            _mSpriteName.stringValue = spriteName;

            var self = (UILabelAtlas)target;
            self.SpriteCache = self.Atlas.GetSprite(spriteName);
            self.SpriteName = spriteName;
//            UpdateSprite(self.Atlas.GetSprite(spriteName));
            var endIdx = spriteName.LastIndexOf('_');
            if (endIdx != -1)
            {
                string prefix = spriteName.Substring(0, endIdx + 1);
//                _mPrefixName.stringValue = prefix;
                self.PrefixName = prefix;
            }
//            serializedObject.ApplyModifiedProperties();
        }

        private bool OverrideSpriteGUI()
        {
            var self = (UILabelAtlas)target;
            EditorGUILayout.BeginHorizontal();

            bool changed = false;
            EditorGUI.BeginChangeCheck();
            var newSprite = EditorGUILayout.ObjectField(self.SpriteCache, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSprite(newSprite);
                if (newSprite)
                {
                    changed = true;
                }
                else
                {
                    self.SpriteCache = null;
                    _mSpriteName.stringValue = null;
//                    _mPrefixName.stringValue = null;
                    self.PrefixName = null;
                }
            }

            EditorGUI.BeginDisabledGroup(self.Atlas == null);
            if (GUILayout.Button("Pick", EditorStyles.miniButtonLeft, GUILayout.Width(40)))
            {
                SpriteSelector.Show(self.Atlas, _mSpriteName.stringValue, OnSpriteChanged);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(self.SpriteCache == null);
            if (GUILayout.Button("Edit", EditorStyles.miniButtonMid, GUILayout.Width(40)))
            {
                OpenSpriteEditor(self.Atlas, self.SpriteCache);
            }

            if (GUILayout.Button("Del", EditorStyles.miniButtonRight, GUILayout.Width(40)))
            {
                self.SpriteCache = null;
                _mSpriteName.stringValue = null;
//                _mPrefixName.stringValue = null;
                self.PrefixName = null;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            return changed;
        }

        private void BaseInspectorGUI()
        {
            var self = (UILabelAtlas)target;

            EditorGUILayout.PropertyField(_mPrefab);
            EditorGUILayout.PropertyField(_mGroup);
            EditorGUILayout.PropertyField(_mValue);
            EditorGUILayout.PropertyField(_mArrSpriteName);

            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();


            //SpriteGUI();
            //AppearanceControlsGUI();
            //RaycastControlsGUI();
            //TypeGUI();
            //++EditorGUI.indentLevel;
            //EditorGUILayout.PropertyField(m_PreserveAspect);
            //--EditorGUI.indentLevel;

            //var imgType = (Image.Type)m_Type.enumValueIndex;
            //var showNative = self.overrideSprite && (imgType == Image.Type.Simple || imgType == Image.Type.Filled);
            //SetShowNativeSize(true, true);
            //EditorGUI.BeginDisabledGroup(!showNative);
            //NativeSizeButtonGUI();
            //EditorGUI.EndDisabledGroup();
        }

        private void AtlasGUI()
        {
            var atlasRoot = UGUITools.settings.atlasRoot;
            var self = (UILabelAtlas)target;

            var atlasName = _mAtlasName.stringValue;
            if (!string.IsNullOrEmpty(atlasName) && self.Atlas == null)
            {
                foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
                {
                    if (bundleName.OrdinalStartsWith(atlasRoot))
                    {
                        if (bundleName.OrdinalIgnoreCaseEndsWith(atlasName))
                        {
                            self.Atlas = FindAtlas(bundleName, atlasName);
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            var atlas = EditorGUILayout.ObjectField(self.Atlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (GUILayout.Button("SpriteAtlas", EditorStyles.miniButton, GUILayout.Width(120)))
            {
                EditorGUIUtility.ShowObjectPicker<SpriteAtlas>(self.Atlas, false, string.Empty, 101);
            }

            EditorGUILayout.EndHorizontal();

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                var ctlId = EditorGUIUtility.GetObjectPickerControlID();
                if (ctlId == 101)
                {
                    atlas = EditorGUIUtility.GetObjectPickerObject() as SpriteAtlas;
                }
            }

            if (atlas != self.Atlas)
            {
                self.Atlas = atlas;
                self.SpriteCache = null;
                if (atlas)
                {
                    _mAtlasName.stringValue = atlas ? atlas.name : null;
                }
                else
                {
                    _mAtlasName.stringValue = null;
                    _mSpriteName.stringValue = null;
//                    _mPrefixName.stringValue = null;
                    self.PrefixName = null;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var self = (UILabelAtlas)target;

            /*
            if (self.overrideSprite && string.IsNullOrEmpty(_mSpriteName.stringValue)) {
                self.overrideSprite = null;
            }
            //*/

//            self.sprite = self.spriteCache;
            //base.OnInspectorGUI();

            BaseInspectorGUI();
//            self.sprite = null;

            //if (Application.isPlaying)
            //{
                //var grayscale = UGUITools.IsGrayscale(self.material);
                //m_Grayscale = EditorGUILayout.Toggle("Grayscale", grayscale);
                //if (grayscale != m_Grayscale)
                //{
                //    m_Material.objectReferenceValue = UGUITools.ToggleGrayscale(self.material, m_Grayscale);
                //}
            //}

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Sprite Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;

            AtlasGUI();

            if (OverrideSpriteGUI())
            {
                var overrideSp = self.SpriteCache;
//                self.sprite = overrideSp;
                if (overrideSp)
                {
                    string packingTag, spriteName, prefix;
                    int endIdx;
                    var atlasPath = GetSpriteAssetRef(overrideSp, out packingTag, out spriteName);
                    _mSpriteName.stringValue = spriteName;
                    _mAtlasName.stringValue = packingTag;
                    self.Atlas = FindAtlas(atlasPath, packingTag);
                    endIdx = spriteName.LastIndexOf('_');
                    if(endIdx != -1)
                    {
                        prefix = spriteName.Substring(0, endIdx + 1);
//                        _mPrefixName.stringValue = prefix;
                        self.PrefixName = prefix;
                    }
                    
                }
                else
                {
                    _mSpriteName.stringValue = null;
                    _mAtlasName.stringValue = null;
                    self.Atlas = null;
//                    _mPrefixName.stringValue = null;
                    self.PrefixName = null;
                }
            }

            /*
            if (!Application.isPlaying) {
                if (self.atlas && !string.IsNullOrEmpty(_mSpriteName.stringValue) && self.overrideSprite == null) {
                    self.overrideSprite = self.atlas.GetSprite(_mSpriteName.stringValue);
                    if (self.overrideSprite == null) {
                        //_mSpriteName.stringValue = null;
                    }
                }
            }
            //*/

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_mAtlasName);
            EditorGUILayout.PropertyField(_mSpriteName);
            EditorGUILayout.PropertyField(_mPrefixName);
            //EditorGUILayout.PropertyField(m_Text);
            EditorGUI.EndDisabledGroup();
            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();
        }

        public override string GetInfoString()
        {
            UILabelAtlas image = (UILabelAtlas)target;
            Sprite sprite = image.SpriteCache;

            int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
            int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

            return string.Format("Image Size: {0}x{1}", x, y);
        }

        public static string GetSpriteAssetRef(Sprite sprite, out string atlasName, out string spriteName)
        {
            string atlasPath = null, packingTag = null;
            spriteName = sprite != null ? sprite.name : null;

            var ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            if (ti == null) goto END_FINDING_ATLAS;

            var atlasRoot = UGUITools.settings.atlasRoot;
            var listAtlas = new List<string>();
            foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
            {
                if (bundleName.OrdinalStartsWith(atlasRoot))
                {
                    listAtlas.Add(bundleName);
                }
            }
            foreach (var atlasBundle in listAtlas)
            {
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(atlasBundle);
                foreach (var asset in assets)
                {
                    if (asset.OrdinalEndsWith(".spriteatlas"))
                    {
                        var sprites = AssetDatabase.GetDependencies(asset);
                        foreach (var sp in sprites)
                        {
                            if (sp == ti.assetPath)
                            {
                                atlasPath = atlasBundle;
                                packingTag = System.IO.Path.GetFileNameWithoutExtension(asset);
                                goto END_FINDING_ATLAS;
                            }
                        }
                    }
                }
            }

            END_FINDING_ATLAS:
            atlasName = packingTag;
            return atlasPath;
        }


    }
}
