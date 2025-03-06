using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLevel : MonoBehaviour
{
	public float maxHeight = 7.5f;
	public float speed = 1.5f;
	private int _direction = 0;

	void Start()
	{
		transform.position = new Vector3(0, -maxHeight, 0);
	}

	// Update is called once per frame
	void Update()
	{
		if (transform.position.y >= maxHeight) _direction = -1;
		else if (transform.position.y <= -maxHeight) _direction = 1;

		transform.position += new Vector3(0, _direction * Time.deltaTime * speed, 0);
	}

	// DEBUGGING
	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		// Gizmos.DrawCube(new Vector3(2.5f, (-maxHeight + transform.position.y) / 2, 55), new Vector3(5, transform.position.y + maxHeight, 5));
	}
}
