using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph
{
	private Dictionary<Node, List<Edge>> graph;

	public Graph()
	{
		graph = new Dictionary<Node, List<Edge>>();
	}

	public void AddNode(Node node)
	{
		if (!graph.ContainsKey(node)) graph.Add(node, new List<Edge>());
	}

	public void AddEdge(Edge edge)
	{
		AddNode(edge.from);
		AddNode(edge.to);
		if (!graph[edge.from].Contains(edge)) graph[edge.from].Add(edge);

		Edge reverseEdge = new Edge(edge.to, edge.from, edge.length);
		if (!graph[edge.to].Contains(reverseEdge)) graph[edge.to].Add(reverseEdge);
	}

	public Node[] GetNodes()
	{
		return graph.Keys.ToArray();
	}

	public Edge[] GetEdges(Node node)
	{
		if (graph.ContainsKey(node)) return graph[node].ToArray();
		else return new Edge[0];
	}
}
