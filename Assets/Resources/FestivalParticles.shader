Shader "VRBat/FestivalParticles" {
 Properties { _MainTex("Sprite",2D)="white"{} [HDR]_TintColor("Light",Color)=(2,2,2,1) _EdgeSoftness("Soft beam edges",Float)=0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass { Blend SrcAlpha One ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _TintColor; float _EdgeSoftness;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
   Varyings Vert(Attributes v){Varyings o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color*_TintColor;return o;}
   half4 Frag(Varyings i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;if(_EdgeSoftness>0)c.a*=pow(saturate(1-abs(i.uv.y*2-1)),_EdgeSoftness);return c;}
   ENDHLSL
  }
 }
}
