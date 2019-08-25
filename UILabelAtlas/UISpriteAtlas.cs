using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZFrame.Asset;

namespace ZFrame.UGUI{
	public class UISpriteAtlas : UIBehaviour {

		public Sprite sprite;

		[SerializeField]
		[TextArea(3, 10)]
		protected string m_Text;

        public UIGroup group;

        public UISprite uiSprite;

        protected SpriteAtlas m_Atlas;
        public SpriteAtlas atlas
        {
            get { return m_Atlas; }
            set
            {
                if (m_Atlas != value)
                {
                    m_Atlas = value;
                    //if (value)
                    //{
                    //    overrideSprite = value.GetSprite(m_SpriteName);
                    //}
                }
            }
        }

        [SerializeField]
        protected string m_AtlasName;
        public string atlasName
        {
            get { return m_Atlas ? m_Atlas.name : m_AtlasName; }
            set
            {
                m_AtlasName = value;
                atlas = LoadAtlas(value, this);
            }
        }

        [SerializeField]
        protected string m_SpriteName;
        public string spriteName
        {
            get { return m_SpriteName; }
            set
            {
                m_SpriteName = value;
                //if (m_Atlas) overrideSprite = m_Atlas.GetSprite(value);
            }
        }

        protected Sprite m_Sprite;
        public Sprite spriteCache
        {
            get
            {
                return m_Sprite;
            }
            set
            {
                //if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value))
                //{
                //    SetAllDirty();
                //    TrackSprite();
                //}
                m_Sprite = value as Sprite;
            }
        }

        [SerializeField]
        protected GridLayoutGroup m_GridLayoutGroup;
        public GridLayoutGroup gridLayoutGroup
        {
            get
            {
                return m_GridLayoutGroup;
            }
            set
            {
                m_GridLayoutGroup = value as GridLayoutGroup;
            }
        }

        //private Sprite activeSprite => (!(m_OverrideSprite != null)) ? ((object)sprite) : ((object)m_OverrideSprite);


        private static readonly Dictionary<string, SpriteAtlas> LoadedAtlas = new Dictionary<string, SpriteAtlas>();

        private static SpriteAtlas GetAtlas(string atlasPath, string atlasName)
        {
            SpriteAtlas ret = null;
            if (LoadedAtlas.TryGetValue(atlasName, out ret))
            {
                if (ret != null) return ret;
                LoadedAtlas.Remove(atlasName);
            }

            AbstractAssetBundleRef abRef;
            if (AssetLoader.Instance.TryGetAssetBundle(atlasPath, out abRef))
            {
                ret = abRef.Load(atlasName, typeof(SpriteAtlas)) as SpriteAtlas;
                if (ret != null) LoadedAtlas.Add(atlasName, ret);
            }

            return ret;
        }

        //[NoToLua]
        public static SpriteAtlas LoadAtlas(string atlasName, Object context)
        {
            SpriteAtlas ret = null;

            var atlasRoot = UGUITools.settings.atlasRoot;
            var atlasRef = UGUITools.settings.atlasRef;
            if (atlasRef)
            {
                atlasName = UGUITools.settings.atlasRef.GetRef(atlasName);
            }

            if (AssetLoader.Instance == null)
            {
#if UNITY_EDITOR
                var atlasPath = string.Format("{0}{1}/{1}", atlasRoot, atlasName);
                ret = AssetLoader.EditorLoadAsset(null, atlasPath) as SpriteAtlas;
#endif
            }
            else
            {
                ret = GetAtlas((atlasRoot + atlasName).ToLower(), atlasName);
            }

            if (context && ret == null)
            {
                var com = context as Component;
                LogMgr.W(context, "{0}: Load Atlas[{1}] fail!",
                    com ? com.GetHierarchy() : string.Empty, atlasName);
            }

            return ret;
        }

        //[NoToLua]
        public static Sprite LoadSprite(string path, Object context)
        {
            var atlasName = SystemTools.GetDirPath(path);
            var spriteName = System.IO.Path.GetFileName(path);
            var atlas = LoadAtlas(atlasName, context);
            return atlas ? atlas.GetSprite(spriteName) : null;
        }


        protected void InitAtlasSprite()
        {
            if (m_Atlas == null && !string.IsNullOrEmpty(m_AtlasName))
            {
                m_Atlas = LoadAtlas(m_AtlasName, null);
                if (m_Atlas == null)
                {
                    LogMgr.D(this, "{0}: Atlas[{1}] NOT loaded.", this.GetHierarchy(), atlasName);
                    //overrideSprite = null;
                }
            }

            //AutoLoadSprite();
            AutoLoadLabel();
        }

        protected void AutoLoadLabel()
        {
            LogMgr.D("jm -------------------- AutoLoadLabel: " + m_Text);
            if (!string.IsNullOrEmpty(m_Text))
            {
                int idx = m_SpriteName.LastIndexOf('_');
                var subStr = m_SpriteName.Substring(0, idx+1);
                LogMgr.D("jm -------------------: {0}", subStr);
                //var sp = m_Atlas.GetSprite(subStr + m_Text[0].ToString());
                for (int i = 0; i < m_Text.Length; i++)
                {
                    var item = group.Get(i);
                    if (item)
                    {
                        UISprite com = item.GetComponent<UISprite>();
                        if (com == null)
                        {
                            com = item.AddComponent<UISprite>();
                            //group.Add(go.gameObject);
                            //go.transform.SetParent(gameObject.transform);
                        }
                        com.atlas = atlas;
                        com.SetSprite(subStr + i.ToString());
                    }
                    else
                    {
                        var go = Instantiate<UISprite>(uiSprite);
                        go.gameObject.SetActive(true);
                        //go.atlas = atlas;
                        go.atlasName = atlasName;
                        go.SetSprite(subStr + i.ToString());
                        go.transform.SetParent(gameObject.transform);
                        
                        group.Add(go.gameObject);

                    }
                }

            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            //if (isActiveAndEnabled && canvas && canvas.enabled)
            //{
                InitAtlasSprite();
            //}

            base.OnCanvasHierarchyChanged();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitAtlasSprite();
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            //UGUITools.AutoUIRoot(this);
#endif
            base.Start();
            //m_Polygon = GetComponent<PolygonCollider2D>();
        }

#if UNITY_EDITOR
        private void Update()
        {
            //AutoLoadSprite();
            AutoLoadLabel();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            //AutoLoadSprite();
            AutoLoadLabel();
        }
#endif
    }
}
