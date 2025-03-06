using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
	public static List<Node> unvisitedNodes;

	public static Stack<Node> FindPath(Graph graph, Node start, Node end, float agentSpeed, float safetyDistance, float maxWaterLevel, float waterSpeed, float maxExecutionTime)
	{
		// Setup
		var timer = System.Diagnostics.Stopwatch.StartNew();

		unvisitedNodes = new List<Node>(graph.GetNodes());
		start.cost = 0f;
		start.eta = 0f;
		start.heuristic = Vector3.Distance(start.position, end.position);
		start.predictedDistanceToWater = 17.5f; // Just for debugging

		// Other nodes have distance and heuristic set to float.MaxValue in their constructor

		// Algorithm loop
		while (end.cost == float.MaxValue)  // First suitable path is chosen
		{
			if (timer.Elapsed.TotalMilliseconds > maxExecutionTime) // Time limit exceeded
			{
				Debug.Log("A*:: Pathfinding exceeded " + maxExecutionTime + " msec. Aborting.");
				return null;
			}

			Node currentNode = GetNextNode();
			if (currentNode.cost == float.MaxValue) break;          // No path found

			foreach (Edge edge in graph.GetEdges(currentNode))
			{
				Node toNode = edge.to;
				if (currentNode.position.y > maxWaterLevel && toNode.position.y > maxWaterLevel) edge.cost = 0.001f;   // Safe nodes are almost "free" to traverse

				// Check if toNode will be under water
				float t = currentNode.eta + edge.length / agentSpeed;
				float distanceToWater = toNode.position.y - PredictedWaterLevel(t, maxWaterLevel, waterSpeed);
				if (distanceToWater < safetyDistance) continue; // Skip this edge: toNode will be under water

				if (currentNode.cost + edge.cost < toNode.cost)
				{

					toNode.eta = t;
					toNode.predictedDistanceToWater = distanceToWater;     // Only used for debugging

					toNode.cost = currentNode.cost + edge.cost;
					toNode.previousNode = currentNode;
					toNode.heuristic = toNode.cost + Vector3.Distance(toNode.position, end.position);

					// Make toNode visitable again, with updated distance and heuristic
					if (!unvisitedNodes.Contains(toNode)) unvisitedNodes.Add(toNode);
				}
			}
			unvisitedNodes.Remove(currentNode);
		}

		// If no path found
		if (end.cost == float.MaxValue)
		{
			Debug.Log("A*:: No path found");
			return null;
		}
		// Else walk back and return path
		Stack<Node> path = new Stack<Node>();

		Node pathNode = end;
		path.Push(pathNode);
		while (pathNode != start)
		{
			path.Push(pathNode.previousNode);
			pathNode = pathNode.previousNode;
		}
		timer.Stop();
		Debug.Log("Pathfinding took " + timer.Elapsed.TotalMilliseconds + " msec.");
		return path;
	}

	// Choose the unvisited node with best heuristic
	private static Node GetNextNode()
	{
		Node candidateNode = null;
		float candidateHeuristic = float.MaxValue;
		foreach (Node node in unvisitedNodes)
		{
			if (candidateHeuristic > node.heuristic || candidateNode == null)
			{
				candidateNode = node;
				candidateHeuristic = node.heuristic;
			}
		}
		return candidateNode;
	}

	private static float PredictedWaterLevel(float eta, float maxWaterLevel, float waterSpeed)
	{
		return -maxWaterLevel + Mathf.PingPong(eta * waterSpeed, maxWaterLevel * 2);
	}
}