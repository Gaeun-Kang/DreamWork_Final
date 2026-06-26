Shader "Custom/DepthOccluder"
{

    SubShader
    {
       Tags 
        { 
          "RenderType"="Transparent"             
          "RenderPipeline"="UniversalPipeline" 
          "Queue"="Transparent-1" 
        }

        Pass
        {
            Name "DepthMask"
            
            // Render Face를 Both로 설정하여 VR에서 카메라가 손/몸 내부로 들어가도 뚫려 보이지 않게 합니다.
            Cull Off 
            
            ColorMask 0 // 컬러 버퍼(RGBA)에 아무것도 기록하지 않음 -> 투명화
            ZWrite On   // 깊이 버퍼(Depth)에는 이 오브젝트의 형태를 기록함
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // 정점 위치를 클립 공간으로 변환 (Depth 버퍼 기록용 필수 연산)
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // ColorMask 0 때문에 이 리턴값은 화면에 반영되지 않지만, 
                // 셰이더 문법 규격상 프래그먼트 함수의 아웃풋은 존재해야 합니다.
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    
    }
}
