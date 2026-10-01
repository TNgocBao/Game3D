using UnityEngine;

namespace LumiAdventure
{
    public static class LumiTerrainFinish
    {
        private static readonly Texture2D[] textures=new Texture2D[5];
        public static Texture2D Texture(int level)
        {
            if(textures[level-1]!=null)return textures[level-1];
            const int size=128;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Painted terrain "+level,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float broad=Mathf.PerlinNoise(x*.04f+level*11,y*.04f)*.16f;
                float grain=Mathf.PerlinNoise(x*.6f,y*.6f)*.07f;
                float shade=.8f+broad+grain;
                colors[y*size+x]=new Color(shade,shade,shade);
            }
            texture.SetPixels(colors);texture.Apply(true,true);textures[level-1]=texture;return texture;
        }
    }
}
