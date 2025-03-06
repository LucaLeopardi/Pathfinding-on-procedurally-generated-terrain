public class Edge
{
	public Node from;
	public Node to;
	public float length;
	public float cost;

	public Edge(Node from, Node to, float length)
	{
		this.from = from;
		this.to = to;
		this.length = length;
		this.cost = length;
	}
}