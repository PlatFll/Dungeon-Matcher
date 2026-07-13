Shader "Dungeon Matcher/UI/White Flash"
{
    Properties
    {
        [PerRendererData]
        _MainTex("Sprite Texture", 2D) = "white" {}

        _Color("Tint", Color) = (1, 1, 1, 1)

        _FlashColor(
            "Flash Color",
            Color
        ) = (1, 1, 1, 1)

        _FlashAmount(
            "Flash Amount",
            Range(0, 1)
        ) = 0

        _StencilComp(
            "Stencil Comparison",
            Float
        ) = 8

        _Stencil(
            "Stencil ID",
            Float
        ) = 0

        _StencilOp(
            "Stencil Operation",
            Float
        ) = 0

        _StencilWriteMask(
            "Stencil Write Mask",
            Float
        ) = 255

        _StencilReadMask(
            "Stencil Read Mask",
            Float
        ) = 255

        _ColorMask(
            "Color Mask",
            Float
        ) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip(
            "Use Alpha Clip",
            Float
        ) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM

            #pragma target 2.0
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local \
                _ UNITY_UI_CLIP_RECT

            #pragma multi_compile_local \
                _ UNITY_UI_ALPHACLIP

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texCoord : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texCoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _FlashColor;
            fixed4 _TextureSampleAdd;

            float _FlashAmount;
            float4 _ClipRect;
            float4 _MainTex_ST;

            VertexOutput VertexProgram(
                VertexInput input)
            {
                VertexOutput output;

                UNITY_SETUP_INSTANCE_ID(input);

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(
                    output
                );

                output.worldPosition =
                    input.vertex;

                output.vertex =
                    UnityObjectToClipPos(
                        input.vertex
                    );

                output.texCoord =
                    TRANSFORM_TEX(
                        input.texCoord,
                        _MainTex
                    );

                output.color =
                    input.color *
                    _Color;

                return output;
            }

            fixed4 FragmentProgram(
                VertexOutput input
            ) : SV_Target
            {
                fixed4 spriteColor =
                    (
                        tex2D(
                            _MainTex,
                            input.texCoord
                        ) +
                        _TextureSampleAdd
                    ) *
                    input.color;

                spriteColor.rgb =
                    lerp(
                        spriteColor.rgb,
                        _FlashColor.rgb,
                        saturate(_FlashAmount)
                    );

                #ifdef UNITY_UI_CLIP_RECT

                spriteColor.a *=
                    UnityGet2DClipping(
                        input.worldPosition.xy,
                        _ClipRect
                    );

                #endif

                #ifdef UNITY_UI_ALPHACLIP

                clip(
                    spriteColor.a -
                    0.001
                );

                #endif

                return spriteColor;
            }

            ENDCG
        }
    }
}