using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
	[SerializeField] private bool _showPath = true;
	[SerializeField]
	private GameObject _failPanel = null;
	[SerializeField]
	private Terrain _terrain = null;
	[SerializeField]
	private TerrainGrid _grid = null;
	[SerializeField]
	private WaterLevel _water = null;

	[Header("Movement and Pathfinding")]
	[SerializeField]
	[Tooltip("Maximum execution time for A* in seconds. If exceeded, Pathfinding fails.")]
	[Range(1, 10)]
	private int _maxPathfindingTime = 5;
	[SerializeField]
	private float _targetReachedRadius = 0.2f;
	[SerializeField]
	private float _speed = 2f;
	[SerializeField]
	[Tooltip("If true, Agent's Y coordinate will be constrained to the terrain surface instead of being approximated. May cause fluctuations in effective speed on irregular surfaces.")]
	private bool _constraintToTerrain = false;

	private Stack<Node> _path = null;
	private Node _target;
	void Start()
	{
		float safetyDistance = _grid.minDistanceToWater;
		_path = AStar.FindPath(_grid.graph, _grid.startNode, _grid.endNode, _speed, safetyDistance, _water.maxHeight, _water.speed, _maxPathfindingTime * 1000f);
		_water.gameObject.SetActive(true);              // Only activate water after pathfinding has completed, otherwise its level will have risen

		// If pathfinding failed
		if (_path == null)
		{
			_failPanel.SetActive(true);
			this.gameObject.SetActive(false);
			return;
		}

		// DEBUGGING
		// Sanity test: check that Path Nodes are not predicted to be under water
		Node[] nodes = _path.Reverse().ToArray();
		for (int i = 0; i < nodes.Length; i++)
		{
			Debug.Assert(nodes[i].predictedDistanceToWater >= safetyDistance,
			"AGENT:: Path contains nodes predicted to be under water! Node: " + i + ", by " + (nodes[i].predictedDistanceToWater - safetyDistance));
		}

		transform.position = _path.Pop().position;  // Start at the first Node
		_target = _path.Pop();                      // !!! Assumes path has at least 2 Nodes
	}

	void Update()
	{
		// Pop new path Node if previous was reached
		if (Vector3.Distance(_target.position, transform.position) <= _targetReachedRadius)
		{
			if (_path.Count == 0)
			{
				Debug.Log("AGENT:: Reached destination!");
				this.gameObject.GetComponent<Agent>().enabled = false;
				return;
			}
			_target = _path.Pop();
		}

		// Move the agent towards the target
		transform.position += _speed * Time.deltaTime * Vector3.Normalize(_target.position - transform.position);

		// Compenetration avoidance constraint: place the agent on the terrain surface. 
		// This may cause the agent's actual speed to be higher than _speed on highly irregular surfaces, but empirically the difference per frame is < 0.5%
		if (_constraintToTerrain)
		{
			Vector3 coordinatesInTerrain = (transform.position - _terrain.transform.position) / _terrain.terrainData.size.x;
			float terrainHeight = _terrain.transform.position.y + _terrain.terrainData.GetInterpolatedHeight(coordinatesInTerrain.x, coordinatesInTerrain.z);
			transform.position = new Vector3(transform.position.x, terrainHeight, transform.position.z);
		}

		// TEST: Check that the agent is not under water
		Debug.Assert(
			transform.position.y > _water.transform.position.y,
			"AGENT:: Agent touched water!"
			);
	}

	// DEBUGGING
	void OnDrawGizmos()
	{
		if (!_showPath || _path == null) return;  // Only works in Play mode

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(_target.position, _targetReachedRadius);
		foreach (Node node in _path) Gizmos.DrawCube(node.position, Vector3.one * 0.8f);

		// Predicted water level visualization
		Gizmos.color = Color.red;
		//Gizmos.DrawCube(new Vector3(-2.5f, (-_water.maxHeight + _predictedWaterLevel) / 2, 55), new Vector3(5, _water.maxHeight + _predictedWaterLevel, 5));
	}
}
