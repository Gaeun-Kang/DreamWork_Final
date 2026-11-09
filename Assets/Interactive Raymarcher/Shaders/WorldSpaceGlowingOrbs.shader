Shader "Interactive Raymarcher/WorldSpaceGlowingOrbs"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.25, 0.44, 0.6, 1.0)
        [HDR] _GlowColor ("Glow Color", Color) = (1.5,0.0,0.0,1)
        _NoiseScale ("Noise Scale", Float) = 106.3
        _SmoothAmount ("Smooth Amount", Range(0, 0.2)) = 0.1563
        _FresnelIntensity ("Fresnel Intensity", Range(0, 10)) = 5.46
        _FresnelRamp ("Fresnel Ramp", Range(0, 10)) = 0.65
        _GyroidScale ("Gyroid Scale", Float) = 15.0
        _GyroidThickness ("Gyroid Thickness", Range(0.0, 0.1)) = 0.05
        _Gloss ("Gloss", Range(1, 100)) = 1.5
        _ScatteringRadius ("Scattering Radius", Float) = 1
        _Intensity("Intensity", Float) = 1
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
            #include "Assets/Interactive Raymarcher/Shaders/Compute/noise.cginc"

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
            float _SmoothAmount;
            float _FresnelRamp, _FresnelIntensity;
            fixed4 _BaseColor, _GlowColor;
            float _NoiseScale, _GyroidScale, _GyroidThickness;
            float _Gloss;
            float _ScatteringRadius;
            float _TestScale = 0.4;
            float _Intensity;

            uniform float4 _Position;
            uniform float _Size;
            uniform float4x4 _Rotation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.ro = _WorldSpaceCameraPos; 
                o.hitPos = mul(unity_ObjectToWorld, v.vertex); 
                return o;
            }

            float3 getDiffuseLight(float3 normal)
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 lightColor = _LightColor0.rgb;
                float lightFalloff = max(0, dot(lightDir, normal));
                float3 directDiffuseLight = lightColor * lightFalloff;
                return directDiffuseLight;
            }

            float3 getAmbientLight(float3 normal)
            {
                return ShadeSH9(float4(normal, 1.0));
            }

            float3 getSpecularLight(float3 normal, float3 worldPos)
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float3 viewReflect = reflect(-viewDir, normal);
                float specularFalloff = max(0, dot(viewReflect, _WorldSpaceLightPos0.xyz));
                return pow(specularFalloff, _Gloss);
            }

            float3 applyLighting(float3 normal, float3 worldPos)
            {
                float3 diffuse = getDiffuseLight(normal);
                float3 ambient = getAmbientLight(normal);
                float3 specular = getSpecularLight(normal, worldPos);
                float3 diffuseLight = ambient + diffuse;
                return diffuseLight * _BaseColor.rgb + (specular * _LightColor0.rgb);
            }

            float smin(float a, float b, float k) 
            {
                float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            float sphere(float3 p, float r, float3 worldPos, float4x4 rotMatrix)
            {
                p -= worldPos;
                p = mul(p, rotMatrix);
                return length(p) - r;
            }

            float gyroid(float3 p, float3 worldPos, float4x4 rotMatrix)
            {
                p -= worldPos;
                p = mul(p, rotMatrix);
                float rescaleFactor = _GyroidScale;
                p *= rescaleFactor;
                p.y += _Time.y; 
                float gyroid = abs(0.7 * dot(sin(p), cos(p.yzx)) / rescaleFactor) - 0.03;
                return gyroid;
            }

            float ballGyroidHollow(float3 p, float r, float3 worldPos, float4x4 rotMatrix)
            {
                float s = sphere(p, r, worldPos, rotMatrix);
                s = abs(s) - _GyroidThickness;
                float g = gyroid(p, worldPos, rotMatrix);
                return smin(s, g, -0.1);
            }

            float ballGyroidSolid(float3 p, float r, float3 worldPos, float4x4 rotMatrix)
            {
                float s = sphere(p, r, worldPos, rotMatrix);
                float g = gyroid(p, worldPos, rotMatrix);
                return smin(s, g, -0.1);
            }

            float orb(float3 p, float r, float3 worldPos, float4x4 rotMatrix)
            {
                float d = sphere(p, r, worldPos, rotMatrix);
                float s = ballGyroidHollow(p, r, worldPos, rotMatrix);
                return smin(s, d, 0.1);
            }

            float getDist(float3 p)
            {
                return orb(p, _Size, _Position, _Rotation);
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

            float subSurfaceScattering(float3 p, float3 rd)
            {
                float cds = dot(dot(p, rd), dot(p, rd)); 
                float sss = smoothstep(0.2, 0.0, cds); 
                sss = min(1.0 - sss, 2.0);
                float b = ballGyroidSolid(p, _Size * 1.5, 0, _Rotation);
                sss *= smoothstep(-0.03, 0.0, b);
                return sss;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 ro = i.ro; 
                float3 rd = normalize(i.hitPos - ro);
                fixed4 col = 0;

                float d = rayMarch(ro, rd);

                if (d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    float3 n = getNormal(p);
                    float3 l = applyLighting(n, p);

                    p -= _Position;
                    float3 pRot = mul(p, _Rotation);
                    
                    float sss = subSurfaceScattering(p, rd);
                    
                    col.rgb = l * _BaseColor;
                    col.rgb += sss * (_GlowColor * _Intensity);

                    float noise = 1.0 - clamp(snoise(pRot * _NoiseScale), 0.6, 0.8);
                    noise = lerp(noise, 0.5, clamp(1.0 - _GlowColor.r * sss, 0.0, 1.0));
                    col.rgb *= noise;
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