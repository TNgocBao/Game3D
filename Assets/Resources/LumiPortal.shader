Shader "Vanguard/Portal"
{
    Properties { _Color ("Energy",Color) = (0.12,0.8,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct varying {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            fixed4 _Color;
            varying vert(input v){varying o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
            fixed4 frag(varying i):SV_Target
            {
                float2 p=(i.uv-.5)*2;
                float r=length(p);
                float angle=atan2(p.y,p.x);
                float spiral=sin(angle*5-r*18+_Time.y*3);
                float rings=pow(saturate(spiral*.5+.5),5);
                float rim=exp(-abs(r-.87)*45);
                float center=exp(-r*6);
                float mask=1-smoothstep(.9,1,r);
                return fixed4(_Color.rgb*(.3+rings*1.3+rim*2+center*2),mask*(.6+rings*.35));
            }
            ENDCG
        }
    }
}
