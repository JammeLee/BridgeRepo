using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;
using ZFrame.Asset;
using ZFrame.Tween;

namespace Battle
{

    public enum OutBattleEffectType
    {
        Particle,
        FadeIn,
        Move
    }
    
    public class OutBattleEffect
    {
        public int effectID;
        public OutBattleEffectType effectType;
        public string effectName;
        public float delayTime;
        public float nextEffectTime;
    }

    public class ActionManager : MonoBehavior
    {
        //控制除技能外的动作（登场、死亡、胜利等）
        
        private int executingIndex;
        private DataLoader loader;
        private OutBattleEffect[] introObe;
        private OutBattleEffect[] outroObe;
        private OutBattleEffect curObe;
        private ControlsScript controlsScript;
        
        void Start()
        {
            loader = new DataLoader();
            controlsScript = GetComponent<ControlsScript>();
        }

        public void Intro()
        {
            if (controlsScript == null) return;//intro == null || 
            //查表调用淡入接口


            Execute();


        }
        
        private void Execute()
        {
            var obe = GetEffectAndAddIndex();
            if (obe == null)
                return;
            
            switch (obe.effectType)
            {
                case OutBattleEffectType.Particle:
                    PlayParticle();
                    break;
                case OutBattleEffectType.FadeIn:
                    
                    break;
                case OutBattleEffectType.Move:
                    
                    break;
            }
        }

        private void PlayParticle()
        {
            GameObject pTemp = Instantiate(AssetLoader.Instance.Load(typeof(GlobalInfo), curObe.effectName)) as GameObject;
            pTemp.transform.SetParent(transform);
            pTemp.transform.position = Vector3.zero;
//            Destroy(pTemp, particleEffect.particleEffect.duration);

            //延时执行下一效果

            Execute();
        }

        private void FadeIn(MoveInfo intro)
        {
            var renderers = controlsScript.character.GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer renderer;
            for(int i = 0; i < renderers.Length; i++)
            {
                renderer = renderers[i];
                var abc = renderer.gameObject.NeedComponent<TweenMaterialProperty>();
                abc.delay = 0;
                abc.loops = 0;
                abc.duration = 10;
                abc.SetProperty("_Color", TweenMaterialProperty.PropertyType.Color);
                var tw = abc.DoTween(true, true);
                LogMgr.D("jm ------------------------- tw: {0}", tw);
                if (tw)
                {
                    var tww = abc.tweener;
                    var control = controlsScript;
                    var rend = renderers;
                    var idx = i;
                    tww.StartFrom(new Vector4(1, 1, 1, 0)).EndAt(new Vector4(1, 1, 1, 1));
                    tww.UpdateWith((t) =>
                    {
                        LogMgr.D("jm ------------------------ tw update: {0}", t.lifetime);
                    });
                    tww.CompleteWith((w) =>
                    {
                        LogMgr.D("jm ------------------ complete: {0}, {1}", idx, rend.Length - 1);
                        if (idx == rend.Length - 1)
                        {
                            control.introPlay = ActionPlayStatus.Played;
                            if (intro != null)
                            {
                                //淡入的同时播放开场动作
//                                control.CastMove(intro, true, true, false);
                                
                            }
                        }
                        
                    });
                }
            }
        }

        private void PlayMove(MoveInfo intro)
        {
            controlsScript.CastMove(intro, true, true, false);
        }

        private OutBattleEffect GetEffectAndAddIndex()
        {
            if (executingIndex >= obe.Length)
                return null;
            
            curObe = obe[executingIndex];
            executingIndex++;
            return curObe;
        }

    //    public void Outro()
    //    {
    //        
    //    }
    }
}