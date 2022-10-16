Shader "Unlit/WheelWipeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WipeAmount ("Wipe Amount", Range(0, 1)) = 0
        _WipeSoftness ("Wipe Softness", Range(0,1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WipeAmount;
            float _WipeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

            float2 uv = i.uv * 2 - 1;
            float mask = atan2(uv.x, uv.y) / (2 * 3.14159265) + 0.5;
            mask = smoothstep(_WipeAmount * (1 + _WipeSoftness), _WipeAmount*(1+_WipeSoftness) - _WipeSoftness, mask);
            col.a *= mask;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
