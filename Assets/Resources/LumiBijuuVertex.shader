Shader "Lumi/Bijuu Vertex Color" {
 Properties { _Glow("Chakra glow",Range(0,1))=.1 _RimColor("Rim",Color)=(0,.3,.5,1) }
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 struct Input {float4 vertexColor;float3 viewDir;};
 float _Glow;fixed4 _RimColor;
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.vertexColor=v.color;}
 void surf(Input IN,inout SurfaceOutputStandard o){fixed3 c=GammaToLinearSpace(IN.vertexColor.rgb);o.Albedo=c;o.Metallic=0;o.Smoothness=.26;o.Emission=c*_Glow+_RimColor.rgb*pow(1-saturate(dot(normalize(IN.viewDir),o.Normal)),3)*.035;o.Alpha=1;}
 ENDCG
 } Fallback "Diffuse"
}
