using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
	public float baseSpeed;
	public float sprintMultiplier;

	void Start()
	{
		Cursor.visible = false;
	}

	private void FixedUpdate()
	{
		transform.position += ((new Vector3((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
			(Input.GetKey(KeyCode.Q) ? 1 : 0) - (Input.GetKey(KeyCode.R) ? 1 : 0),
			(Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0))) * baseSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1));
	}
}
