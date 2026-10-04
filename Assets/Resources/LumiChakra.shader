Shader "Vanguard/Chakra"
{
    Properties{_Color("Chakra",Color)=(.15,.7,1,1) _Density("Chakra density",Range(.1,2))=1 _Energy("Brightness",Range(.1,3))=1}
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
            float4 _Color;float _Density,_Energy;
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.local=v.vertex.xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float3 p=normalize(i.local);
                float spin=atan2(p.z,p.x)*7+p.y*17-_Time.y*13;
                float strands=pow(saturate(sin(spin)*.5+.5),14);
                float other=pow(saturate(sin(p.x*23+p.z*16+_Time.y*11)*.5+.5),19);
                float third=pow(saturate(sin(atan2(p.y,p.z)*11+p.x*19+_Time.y*17)*.5+.5),8);
                float rim=pow(1-saturate(dot(normalize(i.normal),normalize(i.view))),2);
                float energy=(.34+strands*.75+other*.4+third*.3+rim*.55)*_Energy;
                return float4(lerp(_Color.rgb,float3(.85,.98,1),saturate(strands*.6+third*.25))*energy,.55*_Density);
            }
            ENDCG
        }
    }
}
