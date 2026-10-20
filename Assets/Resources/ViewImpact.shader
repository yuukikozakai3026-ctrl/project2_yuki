Shader "VRBat/ViewImpact" {
 Properties { _Pulse("Impact",Range(0,1))=0 _Pattern("Pattern",Float)=0 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Overlay" "RenderType"="Transparent"}
  Pass { Tags {"LightMode"="SRPDefaultUnlit"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest Always Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Pulse;float _Pattern;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float2 screen:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
   Varyings Vert(Attributes v){Varyings o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=float4(v.positionOS.xy,UNITY_NEAR_CLIP_VALUE,1);o.screen=v.positionOS.xy;return o;}
   float Hash(float n){return frac(sin(n*91.37+_Pattern*13.8)*438.21);}
   half4 Frag(Varyings i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 p=i.screen;float radius=length(p);float angle=(atan2(p.y,p.x)+PI)/(2*PI)*40;
    float sector=floor(angle),offset=Hash(sector)*.22+.39;float spoke=abs(frac(angle)-offset);
    float start=.68+Hash(sector+41)*.23;float lengthFade=saturate((radius-start)/(.50));
    float width=(.035+Hash(sector+12)*.055)*lengthFade;float edge=max(fwidth(angle)*.8,.003);
    float alpha=(1-smoothstep(width,width+edge,spoke))*smoothstep(start,start+.08,radius)*_Pulse;
    float3 tint=lerp(float3(1,.87,.57),float3(1,.98,.90),step(.22,Hash(sector+7)));
    return half4(tint,alpha);
   }
   ENDHLSL
  }
 }
}
