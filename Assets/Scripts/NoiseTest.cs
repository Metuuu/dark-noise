using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NoiseTest : MonoBehaviour {


	// - Init -
	float[,] heightmap;
	RenderTexture renderTexture;
	Vector3[] originalVertices;
	float originalScale = 0;

	[SerializeField] Image UIImage;
	Renderer rend;
	MeshFilter mf;

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


	void Start() {
		var UIImageObj = GameObject.FindWithTag("UINoiseImage");
		if (UIImage == null && UIImageObj != null) {
			UIImage = GameObject.FindWithTag("UINoiseImage").GetComponent<Image>();
		}

		mf = gameObject.GetComponent<MeshFilter>();
		rend = gameObject.GetComponent<Renderer>();
		originalScale = transform.localScale.x;
		if (mf != null) {
			SetMeshGrid(gridSize);
			originalVertices = (Vector3[])mf.mesh.vertices.Clone();
		}


	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space) || (autoUpdate && (noisePerformanceTestLoopTimes != lastNoisePerformanceTestLoopTimes || seed != lastSeed || octaves != lastOctaves || frequency != lastFrequency || amplitude != lastAmplitude || persistance != lastPersistance || lacunarity != lastLacunarity || maxNoiseHeight != lastMaxNoiseHeight || offset != lastOffset || noiseResolution != lastNoiseResolution || gridSize != lastGridSize || enableHeight != lastEnableHeight || radius != lastRadius || rotation != lastRotation || lastTextureOffset != textureOffset || lastUseRenderTextureForDrawingTexture != useRenderTextureForDrawingTexture))) {

			// Heightmap
			if (mf && (!useRenderTextureForDrawingTexture || enableHeight)) {
				heightmap = GenerateHeightmap();
			}

			// RenderTexture
			if (renderTexture != null) renderTexture.Release();
			if (useRenderTextureForDrawingTexture) {
				renderTexture = GenerateHeightmapTexture();
			}

			// Drawing noise
			if (useRenderTextureForDrawingTexture)
				DrawNoiseToPlane(renderTexture);
			else
				DrawNoiseToPlane(heightmap);

			#region [ USING MESH FOR NOISE ]
			if (mf != null) {
				// Grid
				if (lastGridSize != gridSize) {
					SetMeshGrid(gridSize);
					originalVertices = (Vector3[])mf.mesh.vertices.Clone();
				}

				// Height
				if (enableHeight)
					AddHeightToPlane(heightmap);
				else
					ClearPlaneHeight();
			}
			#endregion

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

	void OnDestroy() {
		if (renderTexture != null)
			renderTexture.Release();
	}

	private void OnApplicationQuit() {
		if (UIImage && UIImage.canvasRenderer.GetMaterial(0)) {
			UIImage.canvasRenderer.GetMaterial(0).SetTexture("_MainTex", null);
			UIImage.canvasRenderer.GetMaterial(0).SetTexture("_ColorNoiseTex", null);
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

	RenderTexture GenerateHeightmapTexture() {
		GPUNoiseGenerator.NoiseData noiseData = new GPUNoiseGenerator.NoiseData {
			frequency = frequency,
			octaves = octaves,
			persistence = persistance,
			lacunarity = lacunarity,
			maxNoiseHeight = maxNoiseHeight,
			amplitude = amplitude,
		};

		RenderTexture renderTexture = null;

		System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
		st.Start();
		for (int i = 0; i < noisePerformanceTestLoopTimes; ++i) {
			renderTexture = GPUNoiseGenerator.GenerateSphericalHeigtmapTex(seed, noiseData, radius, gridSize, noiseResolution, offset, rotation, textureOffset); // Generate RenderTexture
		}
		st.Stop();
		if (noisePerformanceTestLoopTimes > 1) {
			Debug.Log(string.Format("Generated noise using renderTexture {0} times and it took {1} ms to complete.", noisePerformanceTestLoopTimes, st.ElapsedMilliseconds));
		}

		return renderTexture;
	}



	/* DRAW NOISE TO PLANE */

	void DrawNoiseToPlane(float[,] heightmap) {
		Texture2D noiseTexture = TextureGenerator.TextureFromHeightMap(heightmap);
		if (UIImage) {
			UIImage.canvasRenderer.SetTexture(noiseTexture);
		}
		if (rend) {
			rend.material.SetTexture("_MainTex", noiseTexture);
			rend.material.SetTexture("_ColorNoiseTex", noiseTexture);
		}
	}

	void DrawNoiseToPlane(RenderTexture renderTexture) {
		if (UIImage && UIImage.canvasRenderer.GetMaterial(0)) {
			//UIImage.canvasRenderer.SetTexture(renderTexture);
			UIImage.canvasRenderer.GetMaterial(0).SetTexture("_MainTex", renderTexture);
			UIImage.canvasRenderer.GetMaterial(0).SetTexture("_ColorNoiseTex", renderTexture);
		}
		if (rend) {
			rend.material.SetTexture("_MainTex", renderTexture);
			rend.material.SetTexture("_ColorNoiseTex", renderTexture);
		}
	}



	/* GRID */
	private void SetMeshGrid(int gridSize) {
		Mesh mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
		//Vector3[] normals = new Vector3[(gridSize + 1) * (gridSize + 1)];
		Vector2[] uv = new Vector2[vertices.Length];
		for (int i = 0, y = 0; y <= gridSize; y++) {
			for (int x = 0; x <= gridSize; x++, i++) {
				vertices[i] = new Vector3(x - gridSize / 2, y - gridSize / 2);
				//normals[i] = vertices[i].normalized;
				uv[i] = new Vector2((float)x / gridSize, (float)y / gridSize);
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uv;

		int[] triangles = new int[gridSize * gridSize * 6];
		for (int ti = 0, vi = 0, y = 0; y < gridSize; y++, vi++) {
			for (int x = 0; x < gridSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + gridSize + 1;
				triangles[ti + 5] = vi + gridSize + 2;
			}
		}
		mesh.triangles = triangles;
		//mesh.normals = normals;
		mesh.RecalculateNormals();
		mf.mesh = mesh;
		transform.localScale = new Vector3(originalScale / gridSize, originalScale / gridSize, 1);
	}



	/* HEIGHT */
	void AddHeightToPlane(float[,] heightmap) {
		ClearPlaneHeight();
		Vector3[] vertices = mf.mesh.vertices;
		Vector3[] normals = mf.mesh.normals;

		int verticesCount = vertices.Length;
		int verticesWidth = (int)Mathf.Sqrt(verticesCount);

		Vector3[] newVertices = new Vector3[verticesCount];

		int resolution = heightmap.GetLength(0);
		float hmPosMultiplier = (float)(resolution - 1) / (float)(verticesWidth - 1);

		for (int x = 0; x < verticesWidth; ++x) {
			for (int y = 0; y < verticesWidth; ++y) {
				newVertices[x + y * verticesWidth] = vertices[x + y * verticesWidth]
					+ (new Vector3(0, 0, -1)
					* (heightmap[Mathf.RoundToInt(x * hmPosMultiplier), Mathf.RoundToInt(y * hmPosMultiplier)] / 1f)
				);
			}
		}
		mf.mesh.vertices = newVertices;
	}

	void ClearPlaneHeight() {
		mf.mesh.vertices = (Vector3[])originalVertices.Clone();
	}

}
