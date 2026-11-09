Shader "Interactive Raymarcher/NoiseSphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SphereRadius ("Sphere Radius", Range(0.1, 0.5)) = 0.25
        _NoiseStrength("Noise Strength", Range(0.0, 0.5)) = 0.2
        _NoiseScale("Noise Scale", Range(0.0, 10.0)) = 8.0
        _NoiseSpeed("Noise Speed", Range(0.0, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 1e-3

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SphereRadius;
            float _NoiseStrength, _NoiseScale, _NoiseSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1));
                o.hitPos = v.vertex;
                return o;
            }

            float noise(float3 p)
            {
                p.y += _Time.y * _NoiseSpeed;
                
                float3 ip=floor(p);
                p-=ip; 
                float3 s=float3(7,157,113);
                float4 h=float4(0.,s.yz,s.y+s.z)+dot(ip,s);
                p=p*p*(3.-2.*p); 
                h=lerp(frac(sin(h)*43758.5),frac(sin(h+s.x)*43758.5),p.x);
                h.xy=lerp(h.xz,h.yw,p.y);
                return lerp(h.x,h.y,p.z); 
            }

            float sphere(float3 p, float r)
            {
                float d = length(p) - r;
                return d;
            }

            float mapNoise(float3 p)
            {
                return sphere(p, _SphereRadius -_NoiseStrength * noise(_NoiseScale*p+3.));
            }

            float getDist(float3 p)
            {
                float m = mapNoise(p);
                return m;
            }

            float rayMarch(float3 ro, float3 rd) 
            {
                float dO = 0;
                float dS;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 p = ro + rd * dO;
                    dS = getDist(p);
                    dO += dS;
                    if (dS<SURF_DIST || dO>MAX_DIST) break;
                }
                return dO;
            }

            float3 getNormal(float3 p) 
            {
                float2 e = float2(1e-2, 0);
                float3 n = getDist(p) - float3(
                    getDist(p-e.xyy),
                    getDist(p-e.yxy),
                    getDist(p-e.yyx)
                );
                return normalize(n);
            }

            float3 getLighting(float3 normalVec)
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
    float3 lightCol = _LightColor0.rgb;
    float3 ambient = float3(.2, .2, .2); // 약간 상향

    // 하프 램버트 변환 (0~1의 범위를 0.5~1로 보정하여 암부를 밝힘)
    float halfLambert = dot(normalVec, lightDir) * 0.5 + 0.5;
    
    float3 lighting = (halfLambert * lightCol) + ambient;
    return lighting;
            }

            fixed4 frag (v2f i) : SV_Target
            {float2 uv = i.uv - .5; // 이 uv는 레이마칭 연산용 변수로 아래에서 분리됩니다.
    float3 ro = i.ro; 
    float3 rd = normalize(i.hitPos - ro); 
    fixed4 col = 0;

    float d = rayMarch(ro, rd);

    if (d < MAX_DIST){
        float3 p = ro + rd * d;
        float3 n = getNormal(p);
        float3 l = getLighting(n);
        
        // 1. 메쉬의 원래 UV(_MainTex_ST 반영됨)를 사용해 텍스처 색상 추출
        fixed4 texCol = tex2D(_MainTex, i.uv);
        
        // 2. 텍스처 색상과 라이팅(음영)을 결합
        col.rgb = texCol.rgb * l;
        col.a = texCol.a;
    }
    else
    {
        discard;
    }
    
    return col;
            }
            ENDCG
        }
    }
}