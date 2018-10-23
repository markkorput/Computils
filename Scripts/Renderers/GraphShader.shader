Shader "Computils/Graph Shader"
{
    Properties
    {
        // material properties here
        MainColor ("Main Color", Color) = (1,0.5,0.0,0.5)
        Count ("Count", int) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 4.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 MainColor;
            int UseAlphaFactors;
            int Count;
            float4x4 localToWorldMat;
            float2 Origin;
            float2 Size;
            float2 ValRange;


            //The buffer containing the points we want to draw.
            StructuredBuffer<float> buf_values;

            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                // float4 color : TEXCOORD0;
            };

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                float normx = ((float)id) / (float)Count;
                float normy = (buf_values[id] - ValRange.x) / (ValRange.y-ValRange.x);

                float4 localPos = float4(Origin.x + normx * Size.x, Origin.y + normy * Size.y, 0, 1);
                float4 worldPos = mul(localToWorldMat, localPos);

                ps_input o;
                o.pos = mul (UNITY_MATRIX_VP, worldPos);

                // float alpha = MainColor.a;
                // if (UseAlphaFactors) alpha *= buf_alphafactors[id];
                // o.color = float4(MainColor.rgb, alpha);
                return o;
            }

            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return MainColor; // i.color; //float4(1,0.5f,0.0f,0.5f);
            }

            ENDCG
        }
    }

    Fallback Off
}
