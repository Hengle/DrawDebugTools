﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawString2DExample : MonoBehaviour {

	#region ========== Variables ==========
	private float		m_GridSize = 4.0f;
	private Vector3		m_Position;
	#endregion

	#region ========== Functions ==========
	void Update () {
		// Draw grid
		DrawDebugTools.DrawGrid(transform.position, m_GridSize, 1.0f, 0.0f);

		// Draw shape
		m_Position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-m_GridSize / 2.0f, 1.0f, 0.0f));
		DrawDebugTools.DrawString2D(m_Position, "Hello World!", TextAnchor.MiddleCenter, Color.green);

		// Draw 3d label
		m_Position = transform.position + new Vector3(0.0f, 0.0f, -m_GridSize / 2.0f - 0.5f);
		DrawDebugTools.DrawString3D(m_Position, Quaternion.Euler(-90.0f, 180.0f, 0.0f), "DrawString2D", TextAnchor.LowerCenter, Color.white, 1.5f);
	}
	#endregion
}