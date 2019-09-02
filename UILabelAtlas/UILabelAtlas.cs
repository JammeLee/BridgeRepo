using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZFrame.Asset;

namespace ZFrame.UGUI{
	public class UILabelAtlas : UIBehaviour, IEventSystemHandler {

//		public Sprite sprite;
        [SerializeField]
        protected string[] m_ArrSpriteName = new string[10];

        [SerializeField]
        protected int m_Value;
        public int Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (value != m_Value)
                {
                    m_Value = value;
                    AutoLoadLabel();
                }
            }
        }

        [SerializeField]
        protected UISprite m_Template;//uiSprite
        public UISprite Template
        {
            get
            {
                return m_Template;
            }
            set
            {
                m_Template = value;
            }
        }

        protected SpriteAtlas m_Atlas;
        public SpriteAtlas Atlas
        {
            get { return m_Atlas; }
            set
            {
                if (m_Atlas != value)
                {
                    m_Atlas = value;
                }
            }
        }
        
        
        [SerializeField, AssetRef(name: "Atlas",type: typeof(SpriteAtlas))]
        protected string m_AtlasName;
        public string AtlasName
        {
            get { return m_Atlas ? m_Atlas.name : m_AtlasName; }
            set
            {
                m_AtlasName = value;
                Atlas = LoadAtlas(value, this);
            }
        }

//        [SerializeField]
//        [SerializeField, AssetRef(type: typeof(Sprite))]
        protected string m_SpriteName;
        public string SpriteName
        {
            get { return !string.IsNullOrEmpty(m_SpriteName) ? m_SpriteName : m_ArrSpriteName[0]; }
            set
            {
                if (!value.Equals(m_SpriteName))
                {
                    m_SpriteName = System.IO.Path.GetFileName(value);
                    InitSpriteNameArray();
                }
                    
            }
        }

        [SerializeField]
        protected string m_PrefixName;
        public string PrefixName
        {
            get { return m_PrefixName; }
            set
            {
                LogMgr.D("jm 0000000000000000000000000000");
                if(!value.Equals(m_PrefixName))
                {
                    m_PrefixName = value;
                    InitSpriteNameArray();
                }
                
            }
        }
    
//        [SerializeField]
        protected Sprite m_SpriteCache;
        public Sprite SpriteCache
        {
            get
            {
                return m_SpriteCache;
            }
            set
            {
                if (value && mGridLayoutGroup) {
                    mGridLayoutGroup.cellSize = new Vector2(value.rect.width, value.rect.height);
                }
                m_SpriteCache = value as Sprite;
                AutoLoadLabel();
            }
        }

//        [SerializeField]
        protected GridLayoutGroup mGridLayoutGroup;
        public GridLayoutGroup gridLayoutGroup
        {
            get
            {
                return mGridLayoutGroup;
            }
            set
            {
                if (value && m_SpriteCache)
                {
                    value.cellSize = new Vector2(m_SpriteCache.rect.width, m_SpriteCache.rect.height);
                }
                mGridLayoutGroup = value as GridLayoutGroup;
            }
        }
        


        private static readonly Dictionary<string, SpriteAtlas> LoadedAtlas = new Dictionary<string, SpriteAtlas>();
        private static Dictionary<int, GameObject> numDic = new Dictionary<int, GameObject>();

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

        protected void InitSpriteNameArray()
        {
            if (!string.IsNullOrEmpty(SpriteName))
            {
                var sprName = System.IO.Path.GetFileName(SpriteName);
                var endIdx = sprName.LastIndexOf('_');
                if(endIdx != -1)
                {
                    var prefix = sprName.Substring(0, endIdx + 1);
                    PrefixName = prefix;
                    for (int i = 0, len = m_ArrSpriteName.Length; i < len; i++)
                    {
                        m_ArrSpriteName[i] = prefix + i.ToString();
                    }
                }
            }
        }


        protected void InitAtlasSprite()
        {
            if (m_Atlas == null && !string.IsNullOrEmpty(m_AtlasName))
            {
                m_Atlas = LoadAtlas(m_AtlasName, null);
                if (m_Atlas == null)
                {
                    LogMgr.D(this, "{0}: Atlas[{1}] NOT loaded.", this.GetHierarchy(), AtlasName);
                    SpriteCache = null;
                }
            }

            InitSpriteNameArray();
            AutoLoadSprite();
            AutoLoadLabel();
        }
        
        public void SetSprite(string path)
        {   
#if UNITY_EDITOR
            SetSprite(path, false);
#else
            SetSprite(path, true);
#endif
        }
        
        public void SetSprite(string path, bool warnIfMissing)
        {
            if (!string.IsNullOrEmpty(path)) {
                AtlasName = SystemTools.GetDirPath(path);
                SpriteName = System.IO.Path.GetFileName(path);
            } else {
                AtlasName = null;
                SpriteName = null;
            }

            if (string.IsNullOrEmpty(m_AtlasName)) {
                Atlas = null;
                SpriteCache = null;
            } else if (string.IsNullOrEmpty(m_SpriteName)) {
                SpriteCache = null;
            } else {
                Atlas = LoadAtlas(m_AtlasName, warnIfMissing ? this : null);
                if (m_Atlas) {
                    SpriteCache = m_Atlas.GetSprite(m_SpriteName);
                    if (SpriteCache == null && warnIfMissing) {
                        LogMgr.W("Load <Sprite:{0}> Fail!", path);
                    }
                }
            }
        }

        protected void AutoLoadSprite()
        {
            if ((SpriteCache == null || SpriteCache.texture == null) && m_Atlas && !string.IsNullOrEmpty(m_SpriteName)) {
                SpriteCache = m_Atlas.GetSprite(m_SpriteName);
                if (SpriteCache == null) {
                    LogMgr.W(this, "Load <Sprite:{0}/{1}> Fail! @ {2}", m_AtlasName, m_SpriteName, this.GetHierarchy());
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                        m_SpriteName = null;
                }
            }
        }

        protected void AutoLoadLabel()
        {
            
            if (!Application.isPlaying)
                return;
            var quotient = Value;
            var sub = Value;
            int idx = 1;
            var tsf = transform;
            Transform childTsf;
            int childCount;
            while (true)
            {
                LogMgr.D("jm --------------------------- cout1: {0}, {1}", idx, transform.childCount);
//                LogMgr.D("jm --------------------------- cout2: {0}, {1}", idx, transform.GetChild(idx) == null);

//                mod = value % 10;
                
                childCount = tsf.childCount;
                if (idx < childCount)//numDic.TryGetValue(startIdx, out it))
                {
                    childTsf = tsf.GetChild(idx);
                    var childGo = childTsf.gameObject;
                    UISprite com = childGo.NeedComponent<UISprite>();
                    LayoutElement le = childGo.NeedComponent<LayoutElement>();

                    com.atlas = Atlas;
                    com.canvasRenderer.cull = false;
                    le.ignoreLayout = false;
                    LogMgr.D("jm ====================== set: {0}", AtlasName + "/" + m_ArrSpriteName[quotient % 10]);
                    com.SetSprite(AtlasName + "/" + m_ArrSpriteName[quotient % 10]);
                }
                else
                {
                    var go = ObjectPoolManager.AddChild(gameObject, Template.gameObject);//Instantiate<UISprite>(uiSprite);
                    var ui = go.NeedComponent<UISprite>();
//                    var goTsf = go.transform;
//                    var uiTsf = ui.transform;
                    ui.transform.SetParent(tsf);
                    ui.gameObject.SetActive(true);
                    ui.transform.SetSiblingIndex(idx);
//                    ui.transform.localPosition = new Vector3(goTsf.position.x, goTsf.position.y, 0);
//                    ui.transform.localScale = Vector3.one;
                    //go.atlas = atlas;
                    ui.atlasName = AtlasName;
                    LogMgr.D("jm ====================== set2: {0}", AtlasName + "/" + m_ArrSpriteName[quotient % 10]);
                    ui.SetSprite(AtlasName + "/" + m_ArrSpriteName[quotient % 10]);
                    ui.name = "spNum_" + idx.ToString();

                    numDic.Add(idx, ui.gameObject);
                    //group.Add(go.gameObject);

                }
                quotient /= 10;
                idx++;
                if (quotient == 0)
                {
                    break;
                }
                
            }
            
            ResetCull(idx);

//            LogMgr.D("jm -------------------- AutoLoadLabel: " + m_Text);
//            if (!string.IsNullOrEmpty(m_Text))
//            {
//                //int _Idx = m_SpriteName.LastIndexOf('_');
//                //var prefix = m_SpriteName.Substring(0, _Idx + 1);
//                //LogMgr.D("jm -------------------: {0}", prefix);
//                //var sp = m_Atlas.GetSprite(subStr + m_Text[0].ToString());
//                int startIdx = 0;
//                for (; startIdx < m_Text.Length; startIdx++)
//                {
//                    var item = group.Get(startIdx);
//                    GameObject it = null;
//                    LogMgr.D("jm ---------------------- group item: {0}, {1}", startIdx, item == null);
//                    var tran = gameObject.transform.GetChild(startIdx).gameObject;
//                    if (tran)//numDic.TryGetValue(startIdx, out it))
//                    {
//                        UISprite com = tran.NeedComponent<UISprite>();
//                        LayoutElement le = tran.NeedComponent<LayoutElement>();
//
//                        com.atlas = atlas;
//                        com.canvasRenderer.cull = false;
//                        le.ignoreLayout = false;
//                        com.SetSprite(atlasName + "/" + m_PrefixName + m_Text[startIdx]);
//                    }
//                    else
//                    {
//                        var go = ObjectPoolManager.AddChild(gameObject, uiSprite.gameObject);//Instantiate<UISprite>(uiSprite);
//                        var ui = go.NeedComponent<UISprite>();
//                        ui.transform.SetParent(gameObject.transform);
//                        ui.gameObject.SetActive(true);
//                        ui.transform.localPosition = new Vector3(go.transform.position.x, go.transform.position.y, 0);
//                        ui.transform.localScale = Vector3.one;
//                        //go.atlas = atlas;
//                        ui.atlasName = atlasName;
//                        ui.SetSprite(atlasName + "/" + m_PrefixName + m_Text[startIdx]);
//                        ui.name = "spNum_" + startIdx.ToString();
//
//                        numDic.Add(startIdx, ui.gameObject);
//                        //group.Add(go.gameObject);
//
//                    }
//                }
//                ResetCull(startIdx);
//                //LogMgr.D("jm ==================0 : {0}, {1}", idx, group == null);
//                //GameObject goItem = group.Get(idx);
//                //LogMgr.D("jm ==================1 : {0}, {1}", idx, goItem == null);
//                //while (numDic.TryGetValue(idx, out goItem))
//                //{
//                //    var spri = goItem.GetComponent<UISprite>();
//                //    LogMgr.D("jm ==================2 : {0}", spri);
//                //    if (spri)
//                //    {
//                //        spri.canvasRenderer.cull = true;
//                //        goItem.GetComponent<LayoutElement>().ignoreLayout = true;
//                //    }
//                //    else
//                //        spri.gameObject.SetActive(false);
//                //    //goItem = group.Get(++idx);
//                //    ++idx;
//                //    LogMgr.D("jm ==================3 : {0}, {1}", idx, goItem == null);
//                //}
//
//            }
//            else
//            {
//                ResetCull(0);
//            }
        }

        protected void ResetCull(int startIdx)
        {
            GameObject childGo = null;
            while (transform.childCount > startIdx)
            {
                childGo = transform.GetChild(startIdx).gameObject;
                var com = childGo.GetComponent<UISprite>();
                LogMgr.D("jm ==================2 : {0}", com);
                if (com)
                {
                    com.canvasRenderer.cull = true;
                    childGo.GetComponent<LayoutElement>().ignoreLayout = true;
                }
                else
                {
                    com.gameObject.SetActive(false);
                }
                
                ObjectPoolManager.DestroyPooled(childGo);

                startIdx++;
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            LogMgr.D("jm =-=-=-=-=-=-=-=-=-=-=: OnCanvasHierarchyChanged");
            //if (isActiveAndEnabled && canvas && canvas.enabled)
            //{
            InitAtlasSprite();
            //}

            base.OnCanvasHierarchyChanged();
        }

        protected override void OnEnable()
        {
            LogMgr.D("jm =-=-=-=-=-=-=-=-=-=-=: OnEnable");
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
            //AutoLoadLabel();
        }

        protected override void OnValidate()
        {
            LogMgr.D("jm =-=-=-=-=-=-=-=-=-=-=: OnValidate");
            base.OnValidate();
            InitSpriteNameArray();
            AutoLoadSprite();
            AutoLoadLabel();
        }
#endif
    }
}
