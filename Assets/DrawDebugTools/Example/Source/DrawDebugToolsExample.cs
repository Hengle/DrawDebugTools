using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugToolsExample : MonoBehaviour {
	private void Start () {
        //DrawDebugTools.DrawDebugString3D(TextPos, "Hello world", anchof, Color.green, 0.0f);
        
    }

    public Vector3 Rot = Vector3.zero;
	private void Update () {
        // Draw moving sphere
        float SinValue = Mathf.Sin(Time.timeSinceLevelLoad) * 40.0f;
        Vector3 Pos = new Vector3(0.0f, 0.0f, SinValue);
        DrawDebugTools.DrawSphere(Pos, Quaternion.Euler(Rot), 20.0f, 8, Color.gray);
        if (Camera.main)
            DrawDebugTools.DrawString2D(Camera.main.WorldToScreenPoint(Pos), Pos.ToString(), TextAnchor.UpperLeft, Color.cyan, 0.0f);
        
        // Draw line
        Vector3 LineStart = new Vector3(30.0f, 0.0f, 0.0f);
        Vector3 LineEnd = new Vector3(30.0f, 0.0f, 100.0f);
        DrawDebugTools.DrawLine(LineStart, LineEnd, Color.green);

        // Draw point
        Vector3 PointPosition = new Vector3(40.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawPoint(PointPosition, 8.0f, Color.red);

        // Draw Box
        Vector3 BoxPosition = new Vector3(80.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawBox(BoxPosition, Quaternion.Euler(Rot), new Vector3(20.0f, 10.0f, 50.0f), Color.red);

        // Draw circle
        Vector3 CirclePosition = new Vector3(150.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawCircle(CirclePosition, Quaternion.Euler(Rot), 20.0f, 24, Color.yellow);

        // Draw coordinates
        Vector3 CoorsPosition = new Vector3(200.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawCoordinateSystem(CoorsPosition, Quaternion.Euler(Rot), 20.0f);

        // Draw arrow
        Vector3 ArrowStartPosition = new Vector3(250.0f, 0.0f, 0.0f);
        Vector3 ArrowEndPosition = new Vector3(250.0f, 3.0f, 10.0f);
        DrawDebugTools.DrawDirectionalArrow(ArrowStartPosition, ArrowEndPosition, 2.0f, Color.cyan);

        // Draw cylinder
        Vector3 CylinderStart = new Vector3(300.0f, 0.0f, 0.0f);
        Vector3 CylinderEnd = new Vector3(300.0f, 10.0f, 30.0f);
        DrawDebugTools.DrawCylinder(CylinderStart, CylinderEnd, Quaternion.Euler(Rot), 5.0f, 12, Color.red);

        // Draw cone
        Vector3 ConePosition = new Vector3(350.0f, 0.0f, 0.0f);
        Vector3 ConeDirection = Vector3.forward;
        float ConeLength = 10.0f;
        float ConeAngleWidth = 30.0f;
        float ConeAngleHeight = 60.0f;
        int ConeSegments = 12;

        DrawDebugTools.DrawCone(ConePosition, ConeDirection, ConeLength, ConeAngleWidth, ConeAngleHeight, ConeSegments, Color.green);

        // Draw frustum
        DrawDebugTools.DrawFrustum(Camera.main, Color.yellow);

        // Draw capsule
        Vector3 CapsulePosition = new Vector3(400.0f, 0.0f, 0.0f);
        Vector3 CapsuleRotation = new Vector3(0.0f, 0.0f, 0.0f);
        float CapsuleHalfHeight = 10.0f;
        float CapsuleRadius = 2.0f;
        DrawDebugTools.DrawCapsule(CapsulePosition, CapsuleHalfHeight, CapsuleRadius, Quaternion.Euler(CapsuleRotation), Color.gray);

        // Draw text
        DrawDebugTools.DrawString2D(TextPos, "The quick brown fox jumps over the lazy dog", anchof, Color.green, 0.0f);
        Quaternion RotationText=Quaternion.identity;
        if (Camera.main)
        {
            RotationText = Quaternion.LookRotation((new Vector3(0.0f, 30.0f, 0.0f) - Camera.main.transform.position).normalized);
            
        }
        DrawDebugTools.DrawDirectionalArrow(new Vector3(0.0f, 30.0f, 0.0f), new Vector3(0.0f, 30.0f, 0.0f) + new Vector3(1.0f, 1.0f, 0.0f) * 6.0f, 5.0f, Color.red);
        DrawDebugTools.DrawString3D(new Vector3(0.0f, 30.0f, 0.0f), Quaternion.LookRotation(new Vector3(1.0f, 1.0f, 0.0f)), "HELLO TEXT WORLD", anchof, Color.green, 0.0f);
        // Remove persistent lines
        if (Input.GetKeyDown(KeyCode.F))
        {
            DrawDebugTools.FlushDebugLines();
        }
    }
    public Vector3 TextPos=new Vector3(10.0f, 100.0f, 0.0f);
    public TextAnchor anchof;

}
