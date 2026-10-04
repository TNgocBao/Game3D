Shader "Lumi/Environment Tree Wind"
{
 Properties
 {
  _Color("Tint",Color)=(1,1,1,1)
  _MainTex("Albedo",2D)="white"{}
  _WindStrength("Bend strength",Range(0,0.06))=0.018
  _Flutter("Tip flutter",Range(0,0.02))=0.003
  _GustPeriod("Seconds between gusts",Range(5,30))=13
  [HideInInspector] _WindTime("Preview time (-1 uses game time)",Float)=-1
 }
 SubShader
 {
  Tags { "RenderType"="Opaque" }
  LOD 200
  CGPROGRAM
  #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex;
  fixed4 _Color;
  float _WindStrength,_Flutter,_GustPeriod,_WindTime;
  struct Input { float2 uv_MainTex; };
  void vert(inout appdata_full v)
  {
   float3 origin=unity_ObjectToWorld._m03_m13_m23;
   float phase=dot(origin.xz,float2(0.17,0.11));
   float t=_WindTime>=0?_WindTime:_Time.y;
   float envelope=smoothstep(0.25,0.9,sin(t*6.283185/max(_GustPeriod,5)+phase*0.12));
   float weight=saturate(v.vertex.y);
   weight*=weight;
   float sway=sin(t*1.5+phase)*_WindStrength*(0.15+envelope);
   float flutter=sin(t*5.2+phase+v.vertex.x*9+v.vertex.z*7)*_Flutter*envelope*weight;
   v.vertex.x+=weight*sway+flutter;
   v.vertex.z+=weight*cos(t*1.1+phase)*_WindStrength*0.45*(0.15+envelope);
  }
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;
   o.Albedo=c.rgb;o.Metallic=0;o.Smoothness=0.08;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
