Shader "Custom/FieldContainer" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Team0Color ("Team 0 Color", Color) = (1,1,1,1)
		_Team1Color ("Team 1 Color", Color) = (1,1,1,1)
		_HivePosition ("Hive Position", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
		LOD 200

		//Cull Front
		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma Target 3.0	
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			struct Input {
				float3 positionOS:POSITION;
			};
			struct V2F 
			{
				float3 positionWS:S_POSITION;
			};
			float _HivePosition;
			half4 _Color;
			half4 _Team0Color;
			half4 _Team1Color;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			//UNITY_INSTANCING_BUFFER_START(Props)
			//	// put more per-instance properties here
			//UNITY_INSTANCING_BUFFER_END(Props)

			V2F vert(Input i) {
				V2F v;
				v.positionWS = TransformObjectToWorld(i.positionOS);
				return v;
			}

			half4 frag(V2F IN) :SV_Target {
				half4 c = _Color;
				c = lerp(c,_Team0Color,smoothstep(-_HivePosition,-_HivePosition - .5f,IN.positionWS.x));
				c = lerp(c,_Team1Color,smoothstep(_HivePosition,_HivePosition + .5f,IN.positionWS.x));
				return half4(0.5,0.5,0.5,1);
			}
			ENDHLSL
		}
	}
}
