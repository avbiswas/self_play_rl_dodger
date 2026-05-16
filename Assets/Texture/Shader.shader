Shader "Custom/GameObjectColorShader" {
    Properties {
        _PlayerTag ("PlayerTag", Color) = (1, 0, 0, 1)
        _OpponentTag ("OpponentTag", Color) = (1, 0, 0, 0.5)
        _PlayerBulletTag ("_PlayerBulletTag", Color) = (0, 0, 0.5, 1)
        _OpponentBulletTag ("_OpponentBulletTag", Color) = (0, 1, 0, 1)
        _GroundTag ("_GroundTag", Color) = (0, 0, 1, 1)
        
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _PlayerTag;
            float4 _OpponentTag;
            float4 _PlayerBulletTag;
            float4 _OpponentBulletTag;
            float4 _GroundTag;



            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 tag : TEXCOORD1;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
                float4 vertex : SV_POSITION;
                float4 tagColor : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                if (v.tag.x == 1) { // Player
                    o.tagColor = _PlayerTag;
                }
                else if (v.tag.x == 2) { // Opponent
                    o.tagColor = _OpponentTag;
                }
                else if (v.tag.x == 3) { // PlayerBullet
                    o.tagColor = _PlayerBulletTag;
                }
                else if (v.tag.x == 4) { // OpponentBullet
                    o.tagColor = _OpponentBulletTag;
                }
                else { // Ground or other objects
                    o.tagColor = _GroundTag;
                }

                UNITY_SETUP_INSTANCE_ID(v);
                o.tagColor *= v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 color = i.tagColor;
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 outputColor = texColor * color;
                return outputColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}