Shader "VRBat/SlowViewTint" {
 Properties { _Amount("Amount",Range(0,1))=0 _Strength("Strength",Float)=1 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Overlay-10" "RenderType"="Transparent"}
  Pass { Tags {"LightMode"="SRPDefaultUnlit"} Blend One SrcAlpha ZWrite Off ZTest Always Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Amount;float _Strength;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
   Varyings Vert(Attributes v){Varyings o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=float4(v.positionOS.xy,UNITY_NEAR_CLIP_VALUE,1);return o;}
   half4 Frag(Varyings i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    // A small linear-light lift keeps dark scenes readable without a milky white veil.
    float amount=saturate(_Amount)*_Strength;
    return half4(float3(.003,.006,.012)*amount,1-.045*amount);
   }
   ENDHLSL
  }
 }
}
