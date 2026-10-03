using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LumiAdventure
{
    public static class LumiArt
    {
        private const string Root = "Assets/ThirdParty/Kenney/";
#if !UNITY_EDITOR
        private static LumiAssetCatalog catalog;
#endif
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> uiSprites =
            new System.Collections.Generic.Dictionary<string, Sprite>();

        public static Sprite UiSprite(string relativePath)
        {
            if (uiSprites.TryGetValue(relativePath, out Sprite cached)) return cached;
            Texture2D texture = Load<Texture2D>(Root + "SciFiUI/PNG/" + relativePath);
            if (texture == null) return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, 100f);
            uiSprites[relativePath] = sprite;
            return sprite;
        }

        public static GameObject CreatePlayer(Transform parent)
        {
            return LumiNarutoArt.Create(parent);
        }

        public static GameObject CreateBlaster(Transform parent, bool large = false)
        {
            return LumiChibiWeapon.Pistol(parent,large);
        }

        public static GameObject CreateEnemyModel(LumiEnemyType type, Transform parent)
        {
            Color color = type == LumiEnemyType.Sprout ? new Color(.48f,.28f,.2f) : type == LumiEnemyType.Ranger ? new Color(.38f,.25f,.38f) : new Color(.29f,.32f,.27f);
            return LumiCharacterArt.Soldier(parent,color,true,type == LumiEnemyType.Golem ? 1.45f : .95f);
        }

        public static GameObject CreateProp(string fileName, Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject prop = InstantiateAsset(Root + "Platformer/Models/FBX format/" + fileName, parent);
            if (prop == null) return null;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            return prop;
        }

        private static AnimationClip LoadClip(string path)
        {
            GameObject source = Load<GameObject>(path);
            if (source == null) return null;
            AnimationClip[] clips = GetSubAssets<AnimationClip>(path);
            foreach (AnimationClip clip in clips)
                if (!clip.name.StartsWith("__preview__")) return clip;
            return null;
        }

        private static GameObject InstantiateAsset(string path, Transform parent)
        {
            GameObject prefab = Load<GameObject>(path);
            return prefab == null ? null : Object.Instantiate(prefab, parent);
        }

        private static T Load<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            if (catalog == null) catalog = Resources.Load<LumiAssetCatalog>("LumiAssetCatalog");
            return catalog != null ? catalog.Get<T>(path) : null;
#endif
        }

        private static T[] GetSubAssets<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            System.Collections.Generic.List<T> result = new System.Collections.Generic.List<T>();
            foreach (Object item in all) if (item is T typed) result.Add(typed);
            return result.ToArray();
#else
            return new T[0];
#endif
        }
    }

    public class LumiModelAnimator : MonoBehaviour
    {
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private Animator animator;
        private int current = -1;

        public void Initialize(Animator target, AnimationClip idle, AnimationClip run, AnimationClip jump)
        {
            animator = target;
            if (animator == null || idle == null || run == null || jump == null) return;
            graph = PlayableGraph.Create("Nova Character Animation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            mixer = AnimationMixerPlayable.Create(graph, 3);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Nova Output", animator);
            output.SetSourcePlayable(mixer);
            AnimationClipPlayable idlePlayable = AnimationClipPlayable.Create(graph, idle);
            AnimationClipPlayable runPlayable = AnimationClipPlayable.Create(graph, run);
            AnimationClipPlayable jumpPlayable = AnimationClipPlayable.Create(graph, jump);
            idlePlayable.SetApplyFootIK(true);
            runPlayable.SetApplyFootIK(true);
            graph.Connect(idlePlayable, 0, mixer, 0);
            graph.Connect(runPlayable, 0, mixer, 1);
            graph.Connect(jumpPlayable, 0, mixer, 2);
            graph.Play();
            SetState(0);
        }

        public void SetMovement(float amount, bool grounded)
        {
            if (!graph.IsValid()) return;
            SetState(!grounded ? 2 : (amount > 0.08f ? 1 : 0));
        }

        private void SetState(int state)
        {
            if (state == current || !graph.IsValid()) return;
            current = state;
            for (int i = 0; i < 3; i++) mixer.SetInputWeight(i, i == state ? 1f : 0f);
        }

        private void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}

