using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace LumiAdventure
{
    // Local body and newly attached equipment never occlude the first-person camera.
    public sealed class LumiPlayerViewVisibility:MonoBehaviour
    {
        private readonly List<Renderer> owned=new List<Renderer>();
        private readonly Dictionary<Renderer,ShadowCastingMode> originalShadows=new Dictionary<Renderer,ShadowCastingMode>();
        public bool FirstPerson {get;private set;}
        public void Initialize(){Collect();}
        private void Collect()
        {
            owned.Clear();GetComponentsInChildren<Renderer>(true,owned);
            foreach(var renderer in owned)if(!originalShadows.ContainsKey(renderer))originalShadows.Add(renderer,renderer.shadowCastingMode);
        }
        public void SetFirstPerson(bool value)
        {
            FirstPerson=value;Collect();
            foreach(var renderer in owned)renderer.shadowCastingMode=value?ShadowCastingMode.ShadowsOnly:originalShadows[renderer];
        }
        private void LateUpdate(){if(FirstPerson)SetFirstPerson(true);}
        private void OnDestroy()
        {
            foreach(var pair in originalShadows)if(pair.Key!=null)pair.Key.shadowCastingMode=pair.Value;
        }
    }
}
