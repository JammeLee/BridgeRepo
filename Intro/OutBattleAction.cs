using System;
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

    public class OutBattleAction : MonoBehaviour
    {
        //控制除技能外的动作（登场、死亡、胜利等）
        
        private int executingIndex;
        private DataLoader loader;
        private OutBattleEffect[] introObe;
        private OutBattleEffect[] outroObe;
        private OutBattleEffect curObe;
        private ControlsScript controlsScript;
        private MoveSetScript moveSetScript;

        void Start()
        {
            loader = new DataLoader();
            controlsScript = GetComponent<ControlsScript>();
            moveSetScript = controlsScript.character.GetComponent<MoveSetScript>();
            Load();
        }

        void Load()
        {
            var heroBase = loader.GetHeroUInfo(controlsScript.uid);
            var assetName = System.IO.Path.GetFileName(heroBase.resource_path) + "_OutBattle";
            var assetPath = string.Format("{0}/{1}", heroBase.resource_path, assetName);
            var info = AssetLoader.Instance.Load(typeof(OutBattleEffectInfo), assetPath) as OutBattleEffectInfo;
            if (info != null)
            {
                introObe = info.Intro;
            }
            else
            {
                LogMgr.W("load fail");
            }
        }

        public void Intro()
        {
            Execute();
        }
        
        private void Execute()
        {
            var obe = GetEffectAndAddIndex();
            //TODO: 可能在这里判定IntroPlayed的状态
            if (obe == null)
            {
                if(moveSetScript.intro == null)
                    controlsScript.introPlay = ActionPlayStatus.Played;
                return;
            }
                
            
            switch (obe.effectType)
            {
                case OutBattleEffectType.Particle:
                    StartCoroutine(PlayParticle(obe));
                    break;
                case OutBattleEffectType.FadeIn:
                    StartCoroutine(FadeIn(obe));
                    break;
                case OutBattleEffectType.Move:
                    StartCoroutine(PlayMove(obe));
                    break;
            }
        }

        private IEnumerator PlayParticle(OutBattleEffect particle)
        {
            yield return particle.delayTime > 0 ? new WaitForSeconds(particle.delayTime) : null;
            
            GameObject pTemp = Instantiate(AssetLoader.Instance.Load(typeof(GameObject), particle.effectName)) as GameObject;
            pTemp.transform.SetParent(transform);
            pTemp.transform.localPosition = Vector3.zero;
            Destroy(pTemp, particle.destroyTime);

            //延时执行下一效果

            Execute();
        }

        private IEnumerator FadeIn(OutBattleEffect fadeIn)
        {
            yield return fadeIn.delayTime > 0 ? new WaitForSeconds(fadeIn.delayTime) : null;
            
//            var intro = moveSetScript.intro;
            var renderers = controlsScript.character.GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer renderer;
            for(int i = 0; i < renderers.Length; i++)
            {
                renderer = renderers[i];
//                var abc = renderer.gameObject.NeedComponent<MaterialPropertyTweener>();
//                abc.SetProperty("_Color", MaterialPropertyTweener.PropertyType.Color);
//                abc.Tween(new Vector4(1, 1, 1, 0), new Vector4(1, 1, 1, 1), fadeIn.duration);
                var abc = renderer.gameObject.NeedComponent<TweenMaterialProperty>();
                abc.delay = fadeIn.delay;
                abc.loops = fadeIn.loops;
                abc.duration = fadeIn.duration;
                abc.SetProperty("_Color", TweenMaterialProperty.PropertyType.Color);
                var tween = abc.tweener;
                tween.StartFrom(new Vector4(1, 1, 1, 0)).EndAt(new Vector4(1, 1, 1, 1));
//                var status = abc.DoTween(true, true);
//                if (status)
//                {
//                    var tween = abc.tweener;
//                    tween.StartFrom(new Vector4(1, 1, 1, 0)).EndAt(new Vector4(1, 1, 1, 1));
//                    var rend = renderers;
//                    var idx = i;
//                    tween.CompleteWith((w) =>
//                    {
//                        if (idx == rend.Length - 1)
//                        {
//                            if (intro != null) { }
//                        }
//                        
//                    });
//                }
            }
            Execute();
        }

        private IEnumerator PlayMove(OutBattleEffect move)
        {
            yield return move.delayTime > 0 ? new WaitForSeconds(move.delayTime) : null;
            if(moveSetScript.intro)
            {
                controlsScript.CastMove(moveSetScript.intro, true, true, false);
            }
            else
            {
                LogMgr.W("Intro Move is null");
            }
            
            Execute();
        }

        private OutBattleEffect GetEffectAndAddIndex()
        {
            if (introObe == null)
                return null;
            
            if (executingIndex >= introObe.Length)
                return null;
            
            curObe = introObe[executingIndex];
            executingIndex++;
            return curObe;
        }

    //    public void Outro()
    //    {
    //        
    //    }
    }
}