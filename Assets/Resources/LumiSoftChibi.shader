Shader "Lumi/SoftChibi" {
 Properties { _MainTex("Painted palette",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;
 struct Input {float2 uv_MainTex;float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;float3 local=mul(unity_WorldToObject,float4(IN.worldPos,1)).xyz;
 float skin=1-step(.06,distance(c.rgb,float3(.94,.74,.60)));
 float blush=exp(-dot(float2(abs(local.x)-.29,local.y-1.435),float2(abs(local.x)-.29,local.y-1.435))*190)*step(.2,local.z)*skin;
 o.Albedo=lerp(c.rgb,float3(.96,.58,.48),blush*.19);
 float steel=1-step(.05,distance(c.rgb,float3(.76,.79,.8)));
 o.Metallic=steel*.38;o.Smoothness=lerp(.23,.48,steel);o.Alpha=1;}
 ENDCG
 }
 Fallback "Diffuse"
}
