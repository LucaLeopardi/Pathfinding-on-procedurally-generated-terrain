using System;
using UnityEngine;

public class TerrainGrid : MonoBehaviour
{
	[Header("Gizmos")]
	[SerializeField] private bool _showPathfindingNodes = true;
	[SerializeField] private bool _showPathfindingEdges = false;

	[Header("Pathfinding")]
	[SerializeField]
	private Terrain _terrain = null;
	[SerializeField]
	private WaterLevel _water = null;
	public float minDistanceToWater = 1f;
	[SerializeField]
	[Range(32, 512)]
	private int _resolution = 128;
	private Node[,] _grid;
	public Graph graph = null;
	public Node startNode = null;
	public Node endNode = null;
	[Range(0f, 17.5f)]
	[SerializeField]
	private Vector2Int _startNodeIndices;
	[SerializeField]
	private Vector2Int _endNodeIndices = new(0, 0);


	void OnValidate()
	{
		_startNodeIndices.x = Mathf.Clamp(_startNodeIndices.x, 0, _resolution - 1);
		_startNodeIndices.y = Mathf.Clamp(_startNodeIndices.y, 0, _resolution - 1);
		_endNodeIndices.x = Mathf.Clamp(_endNodeIndices.x, 0, _resolution - 1);
		_endNodeIndices.y = Mathf.Clamp(_endNodeIndices.y, 0, _resolution - 1);
		Build();
	}

	public void Build()
	{
		if (_terrain == null) return;

		_grid = new Node[_resolution, _resolution];
		graph = new Graph();

		float terrainSize = _terrain.terrainData.size.x;

		for (int x = 0; x < _resolution; x++)
		{
			for (int z = 0; z < _resolution; z++)
			{
				Vector3 terrainCoordinates = new Vector3(x, 0, z) / _resolution;

				Vector3 position = new Vector3(x * terrainSize / _resolution,
												_terrain.terrainData.GetInterpolatedHeight(terrainCoordinates.x, terrainCoordinates.z),
												z * terrainSize / _resolution);
				position += _terrain.transform.position;
				_grid[x, z] = new Node(position);

				// Populate pathfinding graph
				AddToGraph(x, z);
			}
		}

		startNode = _grid[_startNodeIndices.x, _startNodeIndices.y];
		endNode = _grid[_endNodeIndices.x, _endNodeIndices.y];
	}

	private void AddToGraph(int x, int z)
	{
		graph.AddNode(_grid[x, z]);
		if (x > 0 && z > 0) graph.AddEdge(CreateEdge(_grid[x, z], _grid[x - 1, z - 1]));
		if (x > 0) graph.AddEdge(CreateEdge(_grid[x, z], _grid[x - 1, z]));
		if (x > 0 && z < _resolution - 1) graph.AddEdge(CreateEdge(_grid[x, z], _grid[x - 1, z + 1]));
		if (z > 0) graph.AddEdge(CreateEdge(_grid[x, z], _grid[x, z - 1]));

		Edge CreateEdge(Node from, Node to)
		{
			float distance = Vector3.Distance(from.position, to.position);
			return new Edge(from, to, distance);
		}
	}

	void OnDrawGizmos()
	{
		if (graph == null) return;

		// Start and end Nodes
		Gizmos.color = Color.red;
		Gizmos.DrawCube(startNode.position, new Vector3(2f, 0.3f, 2f));
		Gizmos.DrawCube(endNode.position, new Vector3(2f, 0.3f, 2f));

		if (_showPathfindingNodes || _showPathfindingEdges) foreach (Node node in graph.GetNodes())
			{
				if (_water == null) Gizmos.color = Color.red;                                       // Water disabled
				else if (node.position.y > _water.maxHeight) Gizmos.color = Color.yellow;           // Safe Nodes
				else if (node.position.y > _water.transform.position.y) Gizmos.color = Color.blue;  // Nodes above water
				else Gizmos.color = Color.red;                                                      // Nodes under water

				// Grid Nodes
				if (node == startNode || node == endNode) continue;
				else if (_showPathfindingNodes) Gizmos.DrawCube(node.position, Vector3.one * 0.3f);

				// Grid Edges
				if (_showPathfindingEdges) foreach (Edge edge in graph.GetEdges(node))
					{
						if (edge.length == 0f) Gizmos.color = Color.yellow;
						else Gizmos.color = Color.red;
						Gizmos.DrawLine(edge.from.position, edge.to.position);
					}
			}
	}
}
