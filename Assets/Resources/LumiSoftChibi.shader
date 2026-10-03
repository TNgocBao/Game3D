Shader "Lumi/SoftChibi" {
 Properties { _MainTex("Painted palette",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;
 struct Input {float2 uv_MainTex;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;o.Albedo=c.rgb;o.Metallic=0;o.Smoothness=.28;o.Alpha=1;}
 ENDCG
 }
 Fallback "Diffuse"
}
