using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZFrame.Asset;

namespace ZFrame.UGUI{
	public class UILabelAtlas : UIBehaviour {

//		public Sprite sprite;
        [SerializeField]
        protected string[] mArrSpriteName = new string[10];

        [SerializeField]
        protected int mValue;
        public int Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (value != mValue)
                {
                    mValue = value;
                    AutoLoadLabel();
                }
            }
        }

//		[SerializeField]
//        [TextArea(3, 10)]
//        protected string m_Text;
//        public string text
//        {
//            get
//            {
//                return m_Text;
//            }
//            set
//            {
//                if (!value.Equals(m_Text))
//                {
//                    m_Text = value;
//                    AutoLoadLabel();
//                }
//            }
//        }

        [SerializeField]
        protected UIGroup mGroup;
        public UIGroup Group
        {
            get
            {
                return mGroup;
            }
            set
            {
                mGroup = value;
            }
        }

        [SerializeField]
        protected UISprite mTemplate;//uiSprite
        public UISprite Template
        {
            get
            {
                return mTemplate;
            }
            set
            {
                mTemplate = value;
            }
        }

        protected SpriteAtlas mAtlas;
        public SpriteAtlas Atlas
        {
            get { return mAtlas; }
            set
            {
                if (mAtlas != value)
                {
                    mAtlas = value;
                    //if (value)
                    //{
                    //    overrideSprite = value.GetSprite(m_SpriteName);
                    //}
                }
            }
        }

        [SerializeField]
        protected string mAtlasName;
        public string AtlasName
        {
            get { return mAtlas ? mAtlas.name : mAtlasName; }
            set
            {
                mAtlasName = value;
                Atlas = LoadAtlas(value, this);
            }
        }

        [SerializeField]
        protected string mSpriteName;
        public string SpriteName
        {
            get { return mSpriteName; }
            set
            {
                if(!value.Equals(mSpriteName))
                    mSpriteName = value;
            }
        }

        [SerializeField]
        protected string mPrefixName;
        public string PrefixName
        {
            get { return mPrefixName; }
            set
            {
                if(!value.Equals(mPrefixName))
                {
                    int i = 0;
                    while (i < mArrSpriteName.Length)
                    {
                        LogMgr.D("iiiiiiiiiiiiiiiii: " + mPrefixName + i);
                        mArrSpriteName[i] = mPrefixName + i.ToString();
                        i++;
                    }
                    mPrefixName = value;
                }
                
            }
        }
    
        [SerializeField]
        protected Sprite mSpriteCache;
        public Sprite SpriteCache
        {
            get
            {
                return mSpriteCache;
            }
            set
            {
                if (value && mGridLayoutGroup) {
                    mGridLayoutGroup.cellSize = new Vector2(value.rect.width, value.rect.height);
                }
                mSpriteCache = value as Sprite;
                AutoLoadLabel();
            }
        }

        [SerializeField]
        protected GridLayoutGroup mGridLayoutGroup;
        public GridLayoutGroup gridLayoutGroup
        {
            get
            {
                return mGridLayoutGroup;
            }
            set
            {
                if (value && mSpriteCache)
                {
                    value.cellSize = new Vector2(mSpriteCache.rect.width, mSpriteCache.rect.height);
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


        protected void InitAtlasSprite()
        {
            if (mAtlas == null && !string.IsNullOrEmpty(mAtlasName))
            {
                mAtlas = LoadAtlas(mAtlasName, null);
                if (mAtlas == null)
                {
                    LogMgr.D(this, "{0}: Atlas[{1}] NOT loaded.", this.GetHierarchy(), AtlasName);
                    SpriteCache = null;
                }
            }

            //AutoLoadSprite();
            AutoLoadLabel();
        }
        
        public void SetSprite(string path, bool warnIfMissing)
        {
            if (!string.IsNullOrEmpty(path)) {
                mAtlasName = SystemTools.GetDirPath(path);
                mSpriteName = System.IO.Path.GetFileName(path);
            } else {
                mAtlasName = null;
                mSpriteName = null;
            }

            if (string.IsNullOrEmpty(mAtlasName)) {
                mAtlas = null;
                SpriteCache = null;
            } else if (string.IsNullOrEmpty(mSpriteName)) {
                SpriteCache = null;
            } else {
                mAtlas = LoadAtlas(mAtlasName, warnIfMissing ? this : null);
                if (mAtlas) {
                    SpriteCache = mAtlas.GetSprite(mSpriteName);
                    if (SpriteCache == null && warnIfMissing) {
                        LogMgr.W("Load <Sprite:{0}> Fail!", path);
                    }
                }
            }
        }

        protected void AutoLoadLabel()
        {
            if ((SpriteCache == null || SpriteCache.texture == null) && mAtlas && !string.IsNullOrEmpty(mSpriteName)) {
                SpriteCache = mAtlas.GetSprite(mSpriteName);
                if (SpriteCache == null) {
                    LogMgr.W(this, "Load <Sprite:{0}/{1}> Fail! @ {2}", mAtlasName, mSpriteName, this.GetHierarchy());
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                        mSpriteName = null;
                }
            }
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
                    LogMgr.D("jm ====================== set: {0}", AtlasName + "/" + mArrSpriteName[quotient % 10]);
                    com.SetSprite(AtlasName + "/" + mArrSpriteName[quotient % 10]);
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
                    LogMgr.D("jm ====================== set2: {0}", AtlasName + "/" + mArrSpriteName[quotient % 10]);
                    ui.SetSprite(AtlasName + "/" + mArrSpriteName[quotient % 10]);
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
            //AutoLoadSprite();
            AutoLoadLabel();
        }
#endif
    }
}
