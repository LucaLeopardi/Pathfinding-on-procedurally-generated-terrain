using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinTerrain))]
public class PerlinTerrainEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		PerlinTerrain perlinTerrain = (PerlinTerrain)target;

		if (GUILayout.Button("Reset Terrain")) perlinTerrain.ResetTerrain();
		if (GUILayout.Button("Generate Terrain")) perlinTerrain.GenerateTerrain();


	}
}
