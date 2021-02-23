using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PerlinArea : MonoBehaviour {


	// - Init -
	float[,] heightmap;

	[SerializeField] int noisePerformanceTestLoopTimes = 1;
	[SerializeField] int seed = 1;
	[SerializeField] int octaves = 4;
	[SerializeField] float frequency = 1;
	[SerializeField] float amplitude = 1;
	[SerializeField] float persistance = 0.5f;
	[SerializeField] float lacunarity = 1.87f;
	[SerializeField] float maxNoiseHeight = 1f;
	[SerializeField] float radius = 100;
	[SerializeField] public Vector3 offset = new Vector3(0, 0, 0);
	[SerializeField] public Vector3 rotation = new Vector3(0, 0, 0);
	[SerializeField] public Vector3 textureOffset = new Vector3(0, 0, 0);

	[SerializeField] int noiseResolution = 256;

	[Range(1, 255)]
	[SerializeField] int gridSize = 24;
	[SerializeField] bool useRenderTextureForDrawingTexture = true;

	[SerializeField] bool enableHeight = false;
	[SerializeField] bool autoUpdate = true;


	int lastNoisePerformanceTestLoopTimes = 0;
	int lastSeed = 0;
	int lastOctaves = 0;
	float lastFrequency = 0;
	float lastAmplitude = 0;
	float lastPersistance = 0;
	float lastLacunarity = 0;
	float lastMaxNoiseHeight = 0;
	float lastRadius = 0;
	Vector3 lastOffset;
	Vector3 lastRotation;
	Vector3 lastTextureOffset;
	int lastNoiseResolution;
	int lastGridSize = 0;
	bool lastEnableHeight = false;
	bool lastUseRenderTextureForDrawingTexture;


	void Update() {
		if (Input.GetKeyDown(KeyCode.Space) || (autoUpdate && (noisePerformanceTestLoopTimes != lastNoisePerformanceTestLoopTimes || seed != lastSeed || octaves != lastOctaves || frequency != lastFrequency || amplitude != lastAmplitude || persistance != lastPersistance || lacunarity != lastLacunarity || maxNoiseHeight != lastMaxNoiseHeight || offset != lastOffset || noiseResolution != lastNoiseResolution || gridSize != lastGridSize || enableHeight != lastEnableHeight || radius != lastRadius || rotation != lastRotation || lastTextureOffset != textureOffset || lastUseRenderTextureForDrawingTexture != useRenderTextureForDrawingTexture))) {

			// Heightmap
			heightmap = GenerateHeightmap();

			// Update last values
			lastNoisePerformanceTestLoopTimes = noisePerformanceTestLoopTimes;
			lastSeed = seed;
			lastOctaves = octaves;
			lastFrequency = frequency;
			lastAmplitude = amplitude;
			lastPersistance = persistance;
			lastLacunarity = lacunarity;
			lastMaxNoiseHeight = maxNoiseHeight;
			lastOffset = offset;
			lastNoiseResolution = noiseResolution;
			lastGridSize = gridSize;
			lastEnableHeight = enableHeight;
			lastRadius = radius;
			lastTextureOffset = textureOffset;
			lastRotation = rotation;
			lastUseRenderTextureForDrawingTexture = useRenderTextureForDrawingTexture;
		}
	}


	/* GENERATE HEIGHT MAP */
	float[,] GenerateHeightmap() {
		GPUNoiseGenerator.NoiseData noiseData = new GPUNoiseGenerator.NoiseData {
			frequency = frequency,
			octaves = octaves,
			persistence = persistance,
			lacunarity = lacunarity,
			maxNoiseHeight = maxNoiseHeight,
			amplitude = amplitude,
		};

		float[,] heightmap = null;

		System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
		st.Start();
		for (int i = 0; i < noisePerformanceTestLoopTimes; ++i) {
			heightmap = GPUNoiseGenerator.GenerateSphericalHeigtmap(seed, noiseData, radius, gridSize, noiseResolution, offset, rotation, textureOffset); // Generate Heightmap
		}
		st.Stop();
		if (noisePerformanceTestLoopTimes > 1) {
			Debug.Log(string.Format("Generated noise using float array {0} times and it took {1} ms to complete.", noisePerformanceTestLoopTimes, st.ElapsedMilliseconds));
		}

		return heightmap;
	}

}
