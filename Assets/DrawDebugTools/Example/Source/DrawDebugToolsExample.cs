using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugToolsExample : MonoBehaviour {

	private void Start () {
    }

    public Vector3 Rot = Vector3.zero;
	private void Update () {
        // Draw moving sphere
        float SinValue = Mathf.Sin(Time.timeSinceLevelLoad) * 40.0f;
        Vector3 Pos = new Vector3(0.0f, 0.0f, SinValue);
        DrawDebugTools.DrawDebugSphere(Pos, Quaternion.Euler(Rot), 20.0f, 8, Color.gray);

        // Draw line
        Vector3 LineStart = new Vector3(30.0f, 0.0f, 0.0f);
        Vector3 LineEnd = new Vector3(30.0f, 0.0f, 100.0f);
        DrawDebugTools.DrawDebugLine(LineStart, LineEnd, Color.green, false);

        // Draw point
        Vector3 PointPosition = new Vector3(40.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawDebugPoint(PointPosition, 8.0f, Color.red, false);

        // Draw Box
        Vector3 BoxPosition = new Vector3(80.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawDebugBox(BoxPosition, Quaternion.Euler(Rot), new Vector3(20.0f, 10.0f, 50.0f), Color.red, false);

        // Draw circle
        Vector3 CirclePosition = new Vector3(150.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawDebugCircle(CirclePosition, Quaternion.Euler(Rot), 20.0f, 24, Color.yellow, false);

        // Draw coordinates
        Vector3 CoorsPosition = new Vector3(200.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawDebugCoordinateSystem(CoorsPosition, Quaternion.Euler(Rot), 20.0f, false);

        // Draw arrow
        Vector3 ArrowStartPosition = new Vector3(250.0f, 0.0f, 0.0f);
        Vector3 ArrowEndPosition = new Vector3(250.0f, 3.0f, 10.0f);
        DrawDebugTools.DrawDebugDirectionalArrow(ArrowStartPosition, ArrowEndPosition, 2.0f, Color.cyan, false);

        // Draw cylinder
        //Vector3 CylinderStart = new Vector3(300.0f, 0.0f, 0.0f);
        //Vector3 CylinderEnd = new Vector3(300.0f, 10.0f, 30.0f);
        DrawDebugTools.DrawDebugCylinder(CylinderStart, CylinderEnd, Quaternion.Euler(Rot), 5.0f, 12, Color.red, false);


        // Remove persistent lines
        if (Input.GetKeyDown(KeyCode.F))
        {
            DrawDebugTools.FlushPersistentDebugLines();
        }
    }
    public Vector3 CylinderStart = new Vector3(300.0f, 0.0f, 0.0f);
    public Vector3 CylinderEnd = new Vector3(300.0f, 10.0f, 30.0f);
}
