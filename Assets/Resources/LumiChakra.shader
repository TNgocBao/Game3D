Shader "Vanguard/Chakra"
{
    Properties{_Color("Chakra",Color)=(.15,.7,1,1)}
    SubShader
    {
        Tags{"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha One ZWrite Off Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;};
            struct v2f{float4 position:SV_POSITION;float3 local:TEXCOORD0;float3 normal:TEXCOORD1;float3 view:TEXCOORD2;};
            float4 _Color;
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.local=v.vertex.xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float3 p=normalize(i.local);
                float spin=atan2(p.z,p.x)*7+p.y*17-_Time.y*13;
                float strands=pow(saturate(sin(spin)*.5+.5),14);
                float other=pow(saturate(sin(p.x*23+p.z*16+_Time.y*11)*.5+.5),19);
                float rim=pow(1-saturate(dot(normalize(i.normal),normalize(i.view))),2);
                float energy=.18+strands*.95+other*.65+rim*.7;
                return float4(lerp(_Color.rgb,float3(.8,.96,1),strands*.75)*energy,.6);
            }
            ENDCG
        }
    }
}
