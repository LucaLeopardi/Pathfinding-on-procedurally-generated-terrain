using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Terrain))]
public class PerlinTerrain : MonoBehaviour
{
	[SerializeField]
	private TerrainGrid _grid = null;
	[SerializeField]
	private Texture2D additionalHeightmapTexture = null;
	[SerializeField]
	[Range(32, 256)] // Hard-coded to harmonics minimum subsampling and TerrainData resolution
	private int _subsampling;
	[SerializeField]
	[Range(1.0f, 2.0f)]
	private float _amplitude = 1.4f;
	[SerializeField]
	private bool _addHarmonics = false;
	[SerializeField]
	private int _randomSeed = 0;

	private Terrain _terrain;
	private TerrainData _terrainData;
	private int _heightmapResolution;

	private void InitializeScript()
	{
		_terrain = GetComponent<Terrain>();
		_terrainData = _terrain.terrainData;
		_heightmapResolution = _terrainData.heightmapResolution;
	}

	public void ResetTerrain()
	{
		InitializeScript();

		_terrainData.SetHeights(0, 0, new float[_heightmapResolution, _heightmapResolution]);   // Terrain uses flipped coordinates
		_terrain.transform.position = new Vector3(-_terrainData.size.x / 2f, 0, -_terrainData.size.z / 2f);

		if (_grid) _grid.Build();
	}

	public void GenerateTerrain()
	{
		ResetTerrain();

		if (_randomSeed == 0) _randomSeed = (int)System.DateTime.Now.Ticks;
		Random.InitState(_randomSeed);

		float[,] heights = PerlinNoise(_heightmapResolution, _subsampling, _amplitude);

		if (_addHarmonics)
		{
			float[,] noise2 = PerlinNoise(_heightmapResolution, _subsampling / 2, _amplitude / 2);
			float[,] noise4 = PerlinNoise(_heightmapResolution, _subsampling / 4, _amplitude / 4);

			float h_min = float.MaxValue;
			float h_max = float.MinValue;

			for (int x = 0; x < _heightmapResolution; x++)
			{
				for (int y = 0; y < _heightmapResolution; y++)
				{
					heights[y, x] += noise2[y, x] + noise4[y, x];
					if (heights[y, x] < h_min) h_min = heights[y, x];
					if (heights[y, x] > h_max) h_max = heights[y, x];
				}
			}
			// Normalize again after adding harmonics
			heights = NormalizeMat(heights, h_min, h_max, _amplitude);
		}

		// Heighmap texture is added AFTER noise generation, to guarantee base height in corners
		// !!! Texture size is assumed to be equal to heightmap resolution !!!
		if (additionalHeightmapTexture)
		{
			for (int x = 0; x < _heightmapResolution; x++)
			{
				for (int y = 0; y < _heightmapResolution; y++)
				{
					heights[y, x] += additionalHeightmapTexture.GetPixel(x, y).grayscale * (1f - heights[y, x]);  // Added height is somewhat scaled to how much height is short of maximum, to avoid large flat corners
				}
			}
		}

		_terrainData.SetHeights(0, 0, heights);
		_terrain.transform.position = -(_terrainData.size / 2f);   // Center terrain vertically

		if (_grid) _grid.Build();
	}

	private static float[,] PerlinNoise(int outputResolution, int subsampling, float amplitude)
	{
		/* Differences with the code from the lectures:
		In the example code, the lattice is big as the output resolution, but is then only sampled at indices multiple of subsampling.
		Here, the lattice is only as big as the actual number of lattice points. This yields a smaller matrix, but it's necessary to multiply the indices by subsampling when calculating distance vectors withic a lattice patch.
		*/
		int latticeResolution = 2 + Mathf.CeilToInt(outputResolution / subsampling);
		float[,] noise = new float[outputResolution, outputResolution];
		Vector2[,] latticeValues = new Vector2[latticeResolution, latticeResolution];

		float h_min = float.MaxValue;
		float h_max = float.MinValue;

		// Lattice points random values
		for (int x = 0; x < latticeResolution; x++)
		{
			for (int y = 0; y < latticeResolution; y++)
			{
				latticeValues[y, x] = Random.insideUnitCircle;
			}
		}

		// Lattice interpolation
		for (int x = 0; x < outputResolution; x++)
		{
			for (int y = 0; y < outputResolution; y++)
			{
				// 4 lattice points neighbourhood
				int x0 = Mathf.FloorToInt((float)x / subsampling);
				int x1 = x0 + 1;
				int y0 = Mathf.FloorToInt((float)y / subsampling);
				int y1 = y0 + 1;

				// Height contributions
				float h_00 = Vector2.Dot(latticeValues[y0, x0], new Vector2(x, y) - new Vector2(x0, y0) * subsampling);
				float h_01 = Vector2.Dot(latticeValues[y1, x0], new Vector2(x, y) - new Vector2(x0, y1) * subsampling);
				float h_10 = Vector2.Dot(latticeValues[y0, x1], new Vector2(x, y) - new Vector2(x1, y0) * subsampling);
				float h_11 = Vector2.Dot(latticeValues[y1, x1], new Vector2(x, y) - new Vector2(x1, y1) * subsampling);

				// Coordinates in the lattice square
				float u = (float)(x - x0 * subsampling) / subsampling;
				float v = (float)(y - y0 * subsampling) / subsampling;

				// Interpolation
				float h_u = Mathf.Lerp(h_00, h_10, Smoothstep(u));
				float h_v = Mathf.Lerp(h_01, h_11, Smoothstep(u));
				float h = Mathf.Lerp(h_u, h_v, Smoothstep(v));

				if (h < h_min) h_min = h;
				if (h > h_max) h_max = h;

				noise[y, x] = h;  // Terrain uses flipped coordinates
			}
		}
		return NormalizeMat(noise, h_min, h_max, amplitude);
	}

	private static float[,] NormalizeMat(float[,] mat, float min, float max, float amplitude)
	{
		for (int x = 0; x < mat.GetLength(0); x++)
		{
			for (int y = 0; y < mat.GetLength(1); y++)
			{
				mat[y, x] = amplitude * (mat[y, x] - min) / (max - min);
			}
		}
		return mat;
	}

	private static float Smoothstep(float x)
	{
		return 6f * Mathf.Pow(x, 5) - 15f * Mathf.Pow(x, 4) + 10f * Mathf.Pow(x, 3);
	}
}
