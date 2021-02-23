using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GPUNoiseGenerator {
	
	static readonly string kernelNameFloatArr = "PerlinNoiseSphere";
	static readonly string kernelNameRenderTex = "PerlinNoiseSphereTex";


	[System.Serializable]
    public class NoiseData {
		public int octaves;
        public float frequency;
        public float amplitude;
        public float lacunarity;
		public float persistence;
		public float maxNoiseHeight = 1f;
	}



	// Init perlin noise shader without setting kernel name and kernel specific parameters
	static ComputeShader InitPerlinNoiseShader(int seed, NoiseData noiseData, float radius, int meshGridSize, int noiseResolution, Vector3 offset, Vector3 rotation, Vector3 textureOffset) {

		ComputeShader perlinNoiseShader = (ComputeShader)Resources.Load("Shaders/PerlinCS");

		//offset *= noiseResolution / 10; // TODO: move this somewhere else
		//textureOffset *= noiseResolution; // TODO: move this somewhere else
		//offset *= 100; // TODO: move this somewhere else
		textureOffset *= noiseResolution; // TODO: move this somewhere else

		// Init shader
		perlinNoiseShader.SetFloat("Octaves", noiseData.octaves);
		perlinNoiseShader.SetFloat("Frequency", noiseData.frequency);
		perlinNoiseShader.SetFloat("Amplitude", noiseData.amplitude);
		perlinNoiseShader.SetFloat("Lacunarity", noiseData.lacunarity);
		perlinNoiseShader.SetFloat("Persistence", noiseData.persistence);

		perlinNoiseShader.SetFloat("MaxNoiseHeight", noiseData.maxNoiseHeight);

		perlinNoiseShader.SetVector("Offset", offset);
		perlinNoiseShader.SetVector("Rotation", rotation);
		perlinNoiseShader.SetVector("TextureOffset", textureOffset);

		perlinNoiseShader.SetFloat("Resolution", noiseResolution);
		perlinNoiseShader.SetFloat("Radius", radius);

		return perlinNoiseShader;
	} 



	/* SPHERICAL HEIGHTMAP */

	// Float Array
	public static float[,] GenerateSphericalHeigtmap(int seed, NoiseData noiseData, float radius, int meshGridSize, int noiseResolution, Vector3 offset, Vector3 rotation, Vector3 textureOffset) {
		if (noiseResolution % 32 != 0) {
			noiseResolution = (noiseResolution - (noiseResolution % 32) + 32); // Ceil resolution to be divisible by 32
		}



		ComputeShader perlinNoiseShader = InitPerlinNoiseShader(seed, noiseData, radius, meshGridSize, noiseResolution, offset, rotation, textureOffset);

		int size = noiseResolution * noiseResolution;
		float[] heightMapArray = new float[size];
		ComputeBuffer buffer = new ComputeBuffer(size, sizeof(float));
		buffer.SetData(heightMapArray);


		// Dispatch Shader
		int kernelIndex = perlinNoiseShader.FindKernel(kernelNameFloatArr);
		perlinNoiseShader.SetBuffer(kernelIndex, "buffer", buffer);
		perlinNoiseShader.Dispatch(kernelIndex, noiseResolution / 32, noiseResolution / 32, 1);

		buffer.GetData(heightMapArray);
		buffer.Dispose();
		buffer.Release();
		buffer = null;


		// Generate heightmap from result
		float[,] heightmap = new float[meshGridSize, meshGridSize];

		float hmPosMultiplier = ((float)noiseResolution / meshGridSize);

		int x, y, i;
		for (i = 0; i < meshGridSize; ++i) {
			x = Mathf.RoundToInt(i * hmPosMultiplier);
			for (int j = 0; j < meshGridSize; ++j) {
				y = Mathf.RoundToInt(j * hmPosMultiplier);
				heightmap[i, j] = heightMapArray[x + y * noiseResolution];
			}
		}

		return heightmap;
	}


	// Render Texture
	public static RenderTexture GenerateSphericalHeigtmapTex(int seed, NoiseData noiseData, float radius, int meshGridSize, int noiseResolution, Vector3 offset, Vector3 rotation, Vector3 textureOffset) {
		if (noiseResolution % 32 != 0) {
			noiseResolution = (noiseResolution - (noiseResolution % 32) + 32); // Ceil resolution to be divisible by 32
		}

		// Init shader
		ComputeShader perlinNoiseShader = InitPerlinNoiseShader(seed, noiseData, radius, meshGridSize, noiseResolution, offset, rotation, textureOffset);

		// Create render texture
		RenderTexture texture = RenderTexture.GetTemporary(noiseResolution, noiseResolution, 0, RenderTextureFormat.RFloat);
		//RenderTexture texture = RenderTexture.GetTemporary(noiseResolution, noiseResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		texture.filterMode = FilterMode.Point;
		texture.enableRandomWrite = true;
		texture.Create();


		// Dispatch Shader
		int kernelIndex = perlinNoiseShader.FindKernel(kernelNameRenderTex);
		perlinNoiseShader.SetTexture(kernelIndex, "tex", texture);
		perlinNoiseShader.Dispatch(kernelIndex, noiseResolution / 32, noiseResolution / 32, 1);
		

		return texture;
	}

}
