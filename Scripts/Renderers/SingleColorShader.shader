// from: http://panbaked.com/wp/?p=29

Shader "Computils/SingleColor"
{
    Properties
    {
        // material properties here
        MainColor ("Main Color", Color) = (1,0.5,0.0,0.5)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"

            float4 MainColor;

            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> buf_verts;

            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
            };

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                float3 worldPos = buf_verts[id];

                ps_input o;
                o.pos = mul (UNITY_MATRIX_VP, float4(worldPos, 1.0f));
                return o;
            }

            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return MainColor; //float4(1,0.5f,0.0f,0.5f);
            }
 
            ENDCG
 
        }
    }
 
    Fallback Off
}