// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [MaterialToggle] _OutlineEnabled("Enable Outline", Float) = 0
        _Outline("Outline", Float) = 0
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineSize("Outline Size", int) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma shader_feature ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float _Outline;
            fixed4 _OutlineColor;
            int _OutlineSize;
            float _OutlineEnabled;
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
             OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_TexelSize;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                #if ETC1_EXTERNAL_ALPHA
             // get the color from an external texture (usecase: Alpha support for ETC1 on android)
             color.a = tex2D(_AlphaTex, uv).r;
                #endif //ETC1_EXTERNAL_ALPHA

                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                // If outline is enabled and this is a transparent pixel, check if it should be part of the outline
                if (_OutlineEnabled > 0.5 && _Outline > 0)
                {
                    if (c.a == 0)
                    {
                        // This is a transparent pixel - check if it should be part of the outline
                        float outlineAlpha = 0;
                        
                        // Limit outline size to avoid compiler errors
                        int size = min(_OutlineSize, 4); // Limiting to maximum of 4 pixels outline
                        
                        // Hard-code the loops to avoid unrolling issues with Metal shader compiler
                        // Sample for outline size 1
                        if (size >= 1)
                        {
                            // Cardinal directions
                            fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, _MainTex_TexelSize.y));
                            fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, _MainTex_TexelSize.y));
                            fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x, 0));
                            fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(_MainTex_TexelSize.x, 0));
                            
                            // Diagonal directions
                            fixed4 pixelUpRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));
                            fixed4 pixelUpLeft = tex2D(_MainTex, IN.texcoord + fixed2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y));
                            fixed4 pixelDownRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
                            fixed4 pixelDownLeft = tex2D(_MainTex, IN.texcoord + fixed2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
                            
                            if (pixelUp.a > 0 || pixelDown.a > 0 || pixelRight.a > 0 || pixelLeft.a > 0 || 
                                pixelUpRight.a > 0 || pixelUpLeft.a > 0 || pixelDownRight.a > 0 || pixelDownLeft.a > 0)
                            {
                                outlineAlpha = 1;
                            }
                        }
                        
                        // Sample for outline size 2
                        if (size >= 2 && outlineAlpha == 0)
                        {
                            // Cardinal directions
                            fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, 2 * _MainTex_TexelSize.y));
                            fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, 2 * _MainTex_TexelSize.y));
                            fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(2 * _MainTex_TexelSize.x, 0));
                            fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(2 * _MainTex_TexelSize.x, 0));
                            
                            // Diagonal directions
                            fixed4 pixelUpRight = tex2D(_MainTex, IN.texcoord + fixed2(2 * _MainTex_TexelSize.x, 2 * _MainTex_TexelSize.y));
                            fixed4 pixelUpLeft = tex2D(_MainTex, IN.texcoord + fixed2(-2 * _MainTex_TexelSize.x, 2 * _MainTex_TexelSize.y));
                            fixed4 pixelDownRight = tex2D(_MainTex, IN.texcoord + fixed2(2 * _MainTex_TexelSize.x, -2 * _MainTex_TexelSize.y));
                            fixed4 pixelDownLeft = tex2D(_MainTex, IN.texcoord + fixed2(-2 * _MainTex_TexelSize.x, -2 * _MainTex_TexelSize.y));
                            
                            if (pixelUp.a > 0 || pixelDown.a > 0 || pixelRight.a > 0 || pixelLeft.a > 0 || 
                                pixelUpRight.a > 0 || pixelUpLeft.a > 0 || pixelDownRight.a > 0 || pixelDownLeft.a > 0)
                            {
                                outlineAlpha = 1;
                            }
                        }
                        
                        // Sample for outline size 3
                        if (size >= 3 && outlineAlpha == 0)
                        {
                            // Cardinal directions
                            fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, 3 * _MainTex_TexelSize.y));
                            fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, 3 * _MainTex_TexelSize.y));
                            fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(3 * _MainTex_TexelSize.x, 0));
                            fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(3 * _MainTex_TexelSize.x, 0));
                            
                            // Diagonal directions
                            fixed4 pixelUpRight = tex2D(_MainTex, IN.texcoord + fixed2(3 * _MainTex_TexelSize.x, 3 * _MainTex_TexelSize.y));
                            fixed4 pixelUpLeft = tex2D(_MainTex, IN.texcoord + fixed2(-3 * _MainTex_TexelSize.x, 3 * _MainTex_TexelSize.y));
                            fixed4 pixelDownRight = tex2D(_MainTex, IN.texcoord + fixed2(3 * _MainTex_TexelSize.x, -3 * _MainTex_TexelSize.y));
                            fixed4 pixelDownLeft = tex2D(_MainTex, IN.texcoord + fixed2(-3 * _MainTex_TexelSize.x, -3 * _MainTex_TexelSize.y));
                            
                            if (pixelUp.a > 0 || pixelDown.a > 0 || pixelRight.a > 0 || pixelLeft.a > 0 || 
                                pixelUpRight.a > 0 || pixelUpLeft.a > 0 || pixelDownRight.a > 0 || pixelDownLeft.a > 0)
                            {
                                outlineAlpha = 1;
                            }
                        }
                        
                        // Sample for outline size 4
                        if (size >= 4 && outlineAlpha == 0)
                        {
                            // Cardinal directions
                            fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, 4 * _MainTex_TexelSize.y));
                            fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, 4 * _MainTex_TexelSize.y));
                            fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(4 * _MainTex_TexelSize.x, 0));
                            fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(4 * _MainTex_TexelSize.x, 0));
                            
                            // Diagonal directions
                            fixed4 pixelUpRight = tex2D(_MainTex, IN.texcoord + fixed2(4 * _MainTex_TexelSize.x, 4 * _MainTex_TexelSize.y));
                            fixed4 pixelUpLeft = tex2D(_MainTex, IN.texcoord + fixed2(-4 * _MainTex_TexelSize.x, 4 * _MainTex_TexelSize.y));
                            fixed4 pixelDownRight = tex2D(_MainTex, IN.texcoord + fixed2(4 * _MainTex_TexelSize.x, -4 * _MainTex_TexelSize.y));
                            fixed4 pixelDownLeft = tex2D(_MainTex, IN.texcoord + fixed2(-4 * _MainTex_TexelSize.x, -4 * _MainTex_TexelSize.y));
                            
                            if (pixelUp.a > 0 || pixelDown.a > 0 || pixelRight.a > 0 || pixelLeft.a > 0 || 
                                pixelUpRight.a > 0 || pixelUpLeft.a > 0 || pixelDownRight.a > 0 || pixelDownLeft.a > 0)
                            {
                                outlineAlpha = 1;
                            }
                        }
                        
                        if (outlineAlpha > 0)
                        {
                            c = _OutlineColor;
                            c.a = outlineAlpha;
                        }
                    }
                }

                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}