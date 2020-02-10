using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMultipleShapesExample : MonoBehaviour {

	#region ========== Variables ==========
	private float		m_GridSize = 6.0f;
	private Vector3		m_Position;
	private float		m_Time = 0.0f;
	private float		m_DrawSphereGhostTime = 0.3f;
	private float		m_DrawSphereTrailTimeCounter = 0.0f;
	private Vector3		m_DrawSphereTrailLastPos;
	#endregion

	#region ========== Functions ==========
	void Update () {


		// Draw grid
		DrawDebugTools.DrawGrid(transform.position, m_GridSize, 1.0f, 0.0f);

		// Draw shape
		m_Time += Time.deltaTime;

		m_Position = transform.position + new Vector3(Mathf.Sin(m_Time) * 2.0f, 2.0f + Mathf.Sin(m_Time), Mathf.Cos(m_Time) * 2.0f);
		DrawDebugTools.DrawSphere(m_Position, 0.4f, 4, Color.green);
		m_DrawSphereTrailTimeCounter += Time.deltaTime;

		// Draw sphere trail
		if (m_DrawSphereTrailTimeCounter > m_DrawSphereGhostTime)
		{
			DrawDebugTools.DrawSphere(m_Position, 0.1f, 4, Color.yellow, 1.0f);
			DrawDebugTools.DrawLine(m_Position, m_DrawSphereTrailLastPos, Color.red, 1.0f);
			m_DrawSphereTrailTimeCounter = 0.0f;
			m_DrawSphereTrailLastPos = m_Position;
		}
		DrawDebugTools.DrawDirectionalArrow(transform.position, m_Position, 0.1f, Color.magenta);
		DrawDebugTools.DrawDirectionalArrow(transform.position, Vector3.ProjectOnPlane(m_Position, Vector3.up), 0.1f, Color.magenta);

		DrawDebugTools.DrawDistance(m_Position, Vector3.ProjectOnPlane(m_Position, Vector3.up), Color.blue);

		DrawDebugTools.DrawFloatGraph("Ball Sin Value", Mathf.Sin(m_Time) * 2.0f, 2.0f, true, 10);
		DrawDebugTools.Log("Ball Sin Value = " + (Mathf.Sin(m_Time) * 2.0f), Color.white, 0.0f);
		// Draw 3d label
		m_Position = transform.position + new Vector3(0.0f, 0.0f, -m_GridSize / 2.0f - 0.5f);
		DrawDebugTools.DrawString3D(m_Position, Quaternion.Euler(-90.0f, 180.0f, 0.0f), "DrawSphere", TextAnchor.LowerCenter, Color.white, 1.5f);
	}
	#endregion
}
