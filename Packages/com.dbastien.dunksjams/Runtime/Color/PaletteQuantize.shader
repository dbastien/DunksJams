Shader "Hidden/DunksJams/PaletteQuantize"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LutTex ("LUT", 2D) = "white" {}
        _LutSize ("LUT Size", Float) = 16
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _LutTex;
            float _LutSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 SampleLut(float3 rgb)
            {
                // flattened LUT: width = N*N, height = N
                float N = _LutSize;
                float slice = floor(rgb.b * (N - 1.0));
                float zOffset = rgb.b * (N - 1.0) - slice;

                float x = rgb.r * (N - 1.0);
                float y = rgb.g * (N - 1.0);

                // compute uv for first sample
                float u1 = (x + y * N + 0.5) / (N * N);
                float v1 = (slice + 0.5) / N;

                // sample first slice
                fixed4 c1 = tex2D(_LutTex, float2(u1, v1));

                // if interpolation between slices is needed, sample next slice
                float nextSlice = min(slice + 1.0, N - 1.0);
                float u2 = (x + y * N + 0.5) / (N * N);
                float v2 = (nextSlice + 0.5) / N;
                fixed4 c2 = tex2D(_LutTex, float2(u2, v2));

                return lerp(c1, c2, zOffset);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 outc = SampleLut(col.rgb);
                return fixed4(outc.rgb, col.a);
            }
            ENDCG
        }
    }
    FallBack Off
}