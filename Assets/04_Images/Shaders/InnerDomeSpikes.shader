Shader "BORACLES/Inner Dome Procedural Spikes"
{
    Properties
    {
        [Header(Base)]
        _BaseColor ("Base Color", Color) = (0.75, 0.72, 0.65, 1)

        _SpecularStrength ("Specular Strength", Range(0, 1)) = 0.25
        _Smoothness ("Smoothness", Range(0.01, 1)) = 0.45
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 0.7

        [Header(Dome)]
        _CenterOS ("Spike Target Center Object Space", Vector) = (0, 0, 1, 0)
        _PlacementCenterOS ("Spike Placement Center Object Space", Vector) = (0, 0, 0, 0)

        [Header(Spike Shape)]
        _SpikeCount ("Spike Count", Range(1, 21)) = 21
        _SpikeDepthRatio ("Spike Depth Ratio", Range(0, 1)) = 0.55
        _SpikeRadius ("Spike Radius", Range(0.01, 0.8)) = 0.18
        _SpikeSharpness ("Spike Sharpness", Range(0.5, 12.0)) = 2.2
        _TipPull ("Tip Pull", Range(0, 1)) = 1.0

        _HeightJitter ("Height Jitter", Range(0, 0.5)) = 0.18
        _AngleJitter ("Angle Jitter", Range(0, 1)) = 0.35

        [Header(Bottom Protection)]
        _DeformMinZ ("Deform Min Z", Range(-0.5, 1)) = 0.08
        _DeformFade ("Deform Fade", Range(0.001, 0.5)) = 0.12

        [Header(Animation)]
        _SpeedMin ("Speed Min", Float) = 0.2
        _SpeedMax ("Speed Max", Float) = 1.2
        _PulsePower ("Pulse Power", Range(0.5, 5.0)) = 2.0
        _MinimumPulse ("Minimum Pulse", Range(0, 1)) = 0.0
        _SyncedTime ("Synced Time", Float) = 0

        [Header(Debug)]
        _DebugShowAll ("Debug Show All", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;

                float _SpecularStrength;
                float _Smoothness;
                float _AmbientStrength;

                float4 _CenterOS;
                float4 _PlacementCenterOS;

                float _SpikeCount;
                float _SpikeDepthRatio;
                float _SpikeRadius;
                float _SpikeSharpness;
                float _TipPull;
                float _HeightJitter;
                float _AngleJitter;

                float _SpeedMin;
                float _SpeedMax;
                float _PulsePower;
                float _MinimumPulse;
                float _SyncedTime;

                float _DeformMinZ;
                float _DeformFade;

                float _DebugShowAll;
            CBUFFER_END

            float hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            float pulse01(float time, float speed, float phase)
            {
                float s = sin(time * speed + phase) * 0.5 + 0.5;
                s = smoothstep(0.0, 1.0, s);
                s = pow(s, _PulsePower);
                return lerp(s, 1.0, _DebugShowAll);
            }

            float3 GetSpikeCenterDir(int index)
            {
                if (index == 0)
                {
                    return normalize(float3(0, 0, 1));
                }

                if (index < 9)
                {
                    int ringIndex = index - 1;
                    float count = 8.0;

                    float baseTheta = ((float)ringIndex / count) * 6.28318530718;
                    float angleOffset = (hash(index * 17.13) - 0.5) * _AngleJitter;
                    float theta = baseTheta + angleOffset;

                    // 위쪽 링도 높이를 조금씩 다르게
                    float baseZ = 0.58;
                    float zOffset = (hash(index * 41.91) - 0.5) * _HeightJitter;
                    float z = saturate(baseZ + zOffset);

                    float r = sqrt(saturate(1.0 - z * z));

                    return normalize(float3(
                        cos(theta) * r,
                        sin(theta) * r,
                        z
                    ));
                }
                else
                {
                    int ringIndex = index - 9;
                    float count = 12.0;

                    float baseTheta = (((float)ringIndex / count) * 6.28318530718) + 0.261799;
                    float angleOffset = (hash(index * 23.77) - 0.5) * _AngleJitter;
                    float theta = baseTheta + angleOffset;

                    // 아래쪽 링을 일렬로 두지 않고, 어떤 건 아래/어떤 건 위로
                    float baseZ = 0.18;
                    float zOffset = (hash(index * 59.37) - 0.5) * _HeightJitter * 1.4;
                    float z = clamp(baseZ + zOffset, 0.05, 0.38);

                    float r = sqrt(saturate(1.0 - z * z));

                    return normalize(float3(
                        cos(theta) * r,
                        sin(theta) * r,
                        z
                    ));
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 originalPosOS = IN.positionOS.xyz;
                float3 normalOS = normalize(IN.normalOS);

                // 스파이크가 "어디에 생길지"는 Placement Center 기준으로 계산
                float3 placementPos = originalPosOS - _PlacementCenterOS.xyz;
                float domeRadius = length(placementPos);
                float3 vertexDir = normalize(placementPos);

                float3 finalPos = originalPosOS;

                float t = _SyncedTime;

                [loop]
                for (int i = 0; i < 21; i++)
                {
                    if (i >= _SpikeCount)
                        break;

                    float3 centerDir = GetSpikeCenterDir(i);

                    // 현재 vertex가 이 spike 중심과 얼마나 가까운지
                    float d = distance(vertexDir, centerDir);

                    float influence = saturate(1.0 - d / _SpikeRadius);
                    influence = pow(influence, _SpikeSharpness);

                    float fi = (float)i + 1.0;
                    float speed = lerp(_SpeedMin, _SpeedMax, hash(fi * 31.719));
                    float phase = hash(fi * 91.137) * 6.28318530718;

                    float p = pulse01(t, speed, phase);
                    p = max(p, _MinimumPulse);

                    float bottomMask = smoothstep(_DeformMinZ, _DeformMinZ + _DeformFade, vertexDir.z);
                    float weight = influence * p * _TipPull * bottomMask;

                    // 핵심:
                    // spike가 표면에서 시작하는 위치
                    float3 spikeSurfacePoint = _PlacementCenterOS.xyz + centerDir * domeRadius;

                    // spike가 향하는 위치는 Target Center 기준
                    float3 spikeTipPoint = lerp(
                        spikeSurfacePoint,
                        _CenterOS.xyz,
                        _SpikeDepthRatio * p
                    );

                    // 주변 vertex를 tip point로 끌어당김
                    finalPos = lerp(finalPos, spikeTipPoint, weight);
                }

                float3 positionWS = TransformObjectToWorld(finalPos);

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.positionWS = positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                Light mainLight = GetMainLight();

                float3 lightDirWS = normalize(-mainLight.direction);
                float3 lightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                // Diffuse
                float ndotl = saturate(dot(normalWS, lightDirWS));
                float3 diffuse = _BaseColor.rgb * lightColor * ndotl;

                // Ambient / indirect light
                float3 ambient = SampleSH(normalWS) * _BaseColor.rgb * _AmbientStrength;

                // Specular
                float3 halfDir = normalize(lightDirWS + viewDirWS);
                float ndoth = saturate(dot(normalWS, halfDir));

                float specPower = lerp(8.0, 128.0, _Smoothness);
                float specular = pow(ndoth, specPower) * _SpecularStrength;

                float3 specularColor = lightColor * specular;

                float3 finalColor = ambient + diffuse + specularColor;

                return half4(finalColor, _BaseColor.a);
            }

            ENDHLSL
        }
    }

    FallBack Off
}