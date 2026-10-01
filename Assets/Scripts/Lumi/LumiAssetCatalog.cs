using UnityEngine;

namespace LumiAdventure
{
    public sealed class LumiAssetCatalog : ScriptableObject
    {
        [System.Serializable] public class Entry { public string path; public Object asset; }
        public Entry[] entries;
        public T Get<T>(string path) where T:Object
        {
            if(entries==null)return null;
            foreach(Entry entry in entries)if(entry.path==path)return entry.asset as T;
            return null;
        }
    }
}
