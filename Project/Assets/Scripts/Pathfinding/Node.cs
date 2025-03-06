using System;
using UnityEngine;

public class Node
{
	public Vector3 position;

	public Node previousNode;
	public float cost;      // A* cost
	public float heuristic; // A* heuristic
	public float eta;       // Estimated Time of Arrival (in seconds since path start)
	public float predictedDistanceToWater;

	public Node(Vector3 position)
	{
		this.position = position;
		this.cost = float.MaxValue;
		this.heuristic = float.MaxValue;
		this.eta = float.MaxValue;
		this.predictedDistanceToWater = float.MaxValue;
	}

	public Node(Node toNode)
	{
		this.position = toNode.position;
		this.previousNode = toNode.previousNode;
		this.cost = toNode.cost;
		this.heuristic = toNode.heuristic;
		this.eta = toNode.eta;
		this.predictedDistanceToWater = toNode.predictedDistanceToWater;
	}
}