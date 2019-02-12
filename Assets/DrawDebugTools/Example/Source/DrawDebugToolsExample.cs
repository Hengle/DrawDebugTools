using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugToolsExample : MonoBehaviour {

	private void Start () {
        DrawDebugTools.DrawDebugSphere(new Vector3(0.0f, 0.0f, 0.0f), 10.0f, 12, Color.red, false, 3.0f);
        DrawDebugTools.DrawDebugSphere(new Vector3(60.0f, 0.0f, 0.0f), 50.0f, 8, Color.blue, true, 0.0f);
    }
	
	private void Update () {
        float sin = Mathf.Sin(Time.timeSinceLevelLoad) * 70.0f;
        Vector3 Pos = new Vector3(0.0f, 0.0f, sin);
        DrawDebugTools.DrawDebugSphere(Pos, 50.0f, 8, Color.gray);

        if (Input.GetKeyDown(KeyCode.F))
        {
            DrawDebugTools.FlushPersistentDebugLines();
        }
    }
}
