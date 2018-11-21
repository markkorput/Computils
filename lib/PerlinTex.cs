// from: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html

using UnityEngine;
using System.Collections;

// Create a texture and fill it with Perlin noise.
// Try varying the xOrg, yOrg and scale values in the inspector
// while in Play mode to see the effect they have on the noise.

public class PerlinTex : MonoBehaviour
{
    // Width and height of the texture in pixels.
	public Vector2Int Resolution = new Vector2Int(256,256);
	public Vector2 PerlinScale = new Vector2(10, 10);
	public Vector2 PerlinOffset = new Vector2(1, 1);
    [Tooltip("Every (in units/second by which to increaed PerlinOffset every frame")]
	public Vector2 PerlinScroll = new Vector2(1, 1);
   
    private Texture2D noiseTex;
    private Color[] pix;
    //private Renderer rend;
   
    void Start() 
    {
		Renderer rend = GetComponent<Renderer>();

		// Set up the texture and a Color array to hold pixels during processing.
		noiseTex = new Texture2D(this.Resolution.x, this.Resolution.y);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.material.mainTexture = noiseTex;
    }

    void CalcNoise()
    {
		int idx = 0;
		Vector2 dim = new Vector2(noiseTex.width, noiseTex.height);
		Vector2 multiplier = this.PerlinScale / dim;
      
		for (int y = 0; y < noiseTex.height; y++) {
        	for (int x = 0; x < noiseTex.width; x++) {
				Vector2 coords = this.PerlinOffset + new Vector2(x,y) * multiplier;
                float sample = Mathf.PerlinNoise(coords.x, coords.y);            
                pix[idx] = new Color(sample, sample, sample);
				idx += 1;
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }
   
    void Update()
    {
        CalcNoise();
		this.PerlinOffset += this.PerlinScroll * Time.deltaTime;
    }
}