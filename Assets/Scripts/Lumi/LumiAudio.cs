using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    public class LumiAudio : MonoBehaviour
    {
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource music;
        private AudioSource effects;

        private float masterVolume=1f, musicVolume=.55f, effectsVolume=.65f;
        private bool muted;
        public float MasterVolume { get => masterVolume; set {masterVolume=Mathf.Clamp01(value);ApplyVolumes();} }
        public float MusicVolume { get => musicVolume; set {musicVolume=Mathf.Clamp01(value);ApplyVolumes();} }
        public float EffectsVolume { get => effectsVolume; set {effectsVolume=Mathf.Clamp01(value);ApplyVolumes();} }
        public bool Muted {get=>muted;set{muted=value;ApplyVolumes();}}
        private void ApplyVolumes()
        {
            if(music==null || effects==null)return;
            music.volume=masterVolume*musicVolume;effects.volume=masterVolume*effectsVolume;
            music.mute=effects.mute=muted;
        }
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("Lumi.Master",masterVolume);PlayerPrefs.SetFloat("Lumi.Music",musicVolume);
            PlayerPrefs.SetFloat("Lumi.Sfx",effectsVolume);PlayerPrefs.SetInt("Lumi.Muted",muted?1:0);PlayerPrefs.Save();
        }

        public void Initialize()
        {
            music = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake=effects.playOnAwake=false;
            music.spatialBlend=effects.spatialBlend=0;
            masterVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("Lumi.Master",1f));
            musicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("Lumi.Music",.55f));
            effectsVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("Lumi.Sfx",.65f));
            muted=PlayerPrefs.GetInt("Lumi.Muted",0)==1;
            ApplyVolumes();

            clips["shoot"] = Tone("shoot", 720f, 0.08f, 0.35f, true);
            clips["chakra"]=Chime("chakra",new[]{280f,420f,620f,880f},.45f);
            clips["wind"]=Tone("wind",160f,.6f,.2f,true);
            clips["chakraExplosion"]=ChakraExplosion();
            clips["clone"]=Tone("clone",95f,.22f,.4f,true);
            clips["hit"] = Tone("hit", 180f, 0.12f, 0.5f, false);
            clips["hurt"] = Tone("hurt", 105f, 0.2f, 0.6f, false);
            clips["pickup"] = Tone("pickup", 880f, 0.18f, 0.45f, true);
            clips["star"] = Chime("star", new[] { 660f, 880f, 1100f }, 0.32f);
            clips["win"] = Chime("win", new[] { 523f, 659f, 784f, 1046f }, 0.8f);
            clips["lose"] = Chime("lose", new[] { 330f, 260f, 196f }, 0.7f);
            clips["ui"] = Tone("ui", 520f, 0.07f, 0.22f, true);
            clips["hover"] = Tone("hover", 740f, 0.035f, 0.12f, true);
            clips["jump"] = Tone("jump", 460f, 0.16f, 0.3f, true);
            clips["land"] = Tone("land", 135f, 0.09f, 0.22f, true);
            clips["step"] = Tone("step", 110f, 0.045f, 0.12f, true);
            clips["dialogue"] = Chime("dialogue", new[] { 620f, 690f }, 0.14f);
        }

        public void Play(string key, float volume = 1f)
        {
            if (clips.TryGetValue(key, out var clip)) effects.PlayOneShot(clip, volume);
        }

        public void PlayLevelMusic(int level)
        {
            PlayNinjaVillageMusic();
        }

        public void PlayMenuMusic()
        {
            PlayNinjaVillageMusic();
        }

        private void PlayNinjaVillageMusic()
        {
            AudioClip clip=Resources.Load<AudioClip>("LumiMusic/NinjaVillage");
            if(clip==null&&!clips.TryGetValue("ninja_village_fallback",out clip))
            {
                clip=Ambient("ninja_village_fallback",220f,2);
                clips["ninja_village_fallback"]=clip;
            }
            music.clip=clip;
            music.Play();
        }

        public void StopMusic() => music.Stop();

        private void OnDestroy()
        {
            foreach(AudioClip clip in clips.Values)if(clip!=null)Destroy(clip);
        }

        private static AudioClip Tone(string name, float frequency, float duration, float amplitude, bool falloff)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(duration * sampleRate);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = falloff ? 1f - i / (float)count : Mathf.Sin(Mathf.PI * i / count);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
        private static AudioClip ChakraExplosion()
        {
            const int sampleRate=22050;var data=new float[sampleRate];uint noise=7319;float filtered=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)sampleRate;noise=1664525*noise+1013904223;
                float white=((noise>>8)/(float)0xFFFFFF)*2-1;filtered=Mathf.Lerp(filtered,white,.14f);
                float envelope=Mathf.Min(1,t*70)*Mathf.Exp(-t*4);
                float rumble=Mathf.Sin(2*Mathf.PI*(62*t-18*t*t))*.26f;
                float vortex=white*.13f*(.65f+.35f*Mathf.Sin(t*95));
                data[i]=(filtered*.55f+rumble+vortex)*envelope;
            }
            AudioClip clip=AudioClip.Create("Chakra vortex explosion",data.Length,1,sampleRate,false);clip.SetData(data,0);return clip;
        }

        private static AudioClip Chime(string name, float[] frequencies, float duration)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(duration * sampleRate);
            var data = new float[count];
            int segment = Mathf.Max(1, count / frequencies.Length);
            for (int i = 0; i < count; i++)
            {
                int note = Mathf.Min(frequencies.Length - 1, i / segment);
                float t = i / (float)sampleRate;
                float local = (i % segment) / (float)segment;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencies[note] * t) * 0.3f * (1f - local);
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip Ambient(string name, float root, int style)
        {
            const int sampleRate = 22050;
            const float duration = 6f;
            int count = Mathf.CeilToInt(duration * sampleRate);
            var data = new float[count];
            float third = style == 4 ? 1.1892f : 1.2599f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float pulse = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * (0.12f + style * 0.015f) * t);
                float chord = Mathf.Sin(2f * Mathf.PI * root * t)
                    + 0.65f * Mathf.Sin(2f * Mathf.PI * root * third * t)
                    + 0.45f * Mathf.Sin(2f * Mathf.PI * root * 1.4983f * t);
                data[i] = chord * 0.025f * pulse;
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
