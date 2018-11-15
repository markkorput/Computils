Shader "Computils/Mesh Particles Shader"
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
        // Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 4.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4x4 ParticleModelMatrix;
            float4 MainColor;
            int UseAlphaFactors;
            int ParticleCount;
            int MeshVertCount;

            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> buf_particle_positions;
            StructuredBuffer<float3> buf_mesh;
            StructuredBuffer<float> buf_alphafactors;

            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                uint particleidx = id / MeshVertCount; //(uint)floor((float)id / (float)MeshVertCount);
                uint meshvertidx = id - (particleidx * MeshVertCount);

                float3 particlePos = buf_particle_positions[particleidx];
                float3 particleWorldPos = mul(ParticleModelMatrix, float4(particlePos, 1));
                float3 meshVertPos = buf_mesh[meshvertidx];
                float3 worldPos = particleWorldPos + meshVertPos;

                ps_input o;
                o.pos = mul (UNITY_MATRIX_VP, float4(worldPos, 1.0f));
                float alpha = MainColor.a;
                if (UseAlphaFactors) alpha *= buf_alphafactors[id];
                o.color = float4(MainColor.rgb, alpha);
                return o;
            }

            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return i.color; //float4(1,0.5f,0.0f,0.5f);
            }

            ENDCG
        }
    }

    Fallback Off
}
