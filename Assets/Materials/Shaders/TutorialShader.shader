Shader "Custom/SpriteOutline"{

    Properties{
        _MainTex("Texture", 2D) = "white" {}
        //Outline color
        _Color("Color", Color) = (1,1,1,1);
    }

    SubShader{
        //Render every pixel
        Cull Off
        Blend One OneMinusSRCAlpha


        Pass{

            CGPROGRAM

            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            //Vertex to fragment
            struct v2f{
                //Var name pos, grabbign data from sv_pos
                float4 pos : SV_POSITION;

                //A half is a less precise float
                //(great for saving data in shaders)
                half2 uv : TEXCOORD0;
            };

            //appdata_base is specific for 2D
            v2f vertexFunc(appdata_base v){
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.texcoord;


                return o;
            }

            fixed4 _Color;
            //Pixel size (look up later)
            float4 _MainTex_TexelSize;

            fixed4 fragmentFunc(v2f i) : COLOR{
                half4 c = tex2D(_MainTex, i.uv);

                return c;
            }

            ENDCG
        }
    }
}