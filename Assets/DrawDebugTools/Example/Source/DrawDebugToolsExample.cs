using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugToolsExample : MonoBehaviour {
	private void Start () {
        //DrawDebugTools.DrawDebugString3D(TextPos, "Hello world", anchof, Color.green, 0.0f);

        DrawDebugTools.Log("Text in Start function" , Color.green, 5.0f);
    }

    public Vector3 Rot = Vector3.zero;
	private void Update () {
        // Draw moving sphere
        float SinValue = Mathf.Sin(Time.timeSinceLevelLoad * 4.0f) * FloatMultiplier;
        Vector3 MovingSpherePos = new Vector3(0.0f, 0.0f, SinValue);
        DrawDebugTools.DrawSphere(MovingSpherePos, Quaternion.Euler(Rot), 2.0f, 8, Color.gray);
        if (Camera.main)
        {
            DrawDebugTools.DrawString2D(Camera.main.WorldToScreenPoint(MovingSpherePos), MovingSpherePos.ToString(), TextAnchor.UpperLeft, Color.cyan, 0.0f);
        }

        // Draw line
        Vector3 LineStart = new Vector3(3.0f, 0.0f, 0.0f);
        Vector3 LineEnd = new Vector3(3.0f, 0.0f, 10.0f);
        DrawDebugTools.DrawLine(LineStart, LineEnd, Color.green);

        // Draw point
        Vector3 PointPosition = new Vector3(4.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawPoint(PointPosition, 1.0f, Color.red);

        // Draw Box
        Vector3 BoxPosition = new Vector3(8.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawBox(BoxPosition, Quaternion.Euler(Rot), new Vector3(2.0f, 1.0f, 5.0f), Color.red);

        // Draw circle
        Vector3 CirclePosition = new Vector3(15.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawCircle(CirclePosition, Quaternion.Euler(Rot), 2.0f, 24, Color.yellow);

        // Draw coordinates
        Vector3 CoorsPosition = new Vector3(20.0f, 0.0f, 0.0f);
        DrawDebugTools.DrawCoordinateSystem(CoorsPosition, Quaternion.Euler(Rot), 2.0f);

        // Draw arrow
        Vector3 ArrowStartPosition = new Vector3(25.0f, 0.0f, 0.0f);
        Vector3 ArrowEndPosition = new Vector3(25.0f, 3.0f, 10.0f);
        DrawDebugTools.DrawDirectionalArrow(ArrowStartPosition, ArrowEndPosition, 1.0f, Color.cyan);

        // Draw cylinder
        Vector3 CylinderStart = new Vector3(30.0f, 0.0f, 0.0f);
        Vector3 CylinderEnd = new Vector3(30.0f, 10.0f, 3.0f);
        DrawDebugTools.DrawCylinder(CylinderStart, CylinderEnd, Quaternion.Euler(Rot), 1.0f, 12, Color.red);

        // Draw cone
        Vector3 ConePosition = new Vector3(35.0f, 0.0f, 0.0f);
        Vector3 ConeDirection = Vector3.forward;
        float ConeLength = 2.0f;
        float ConeAngleWidth = 30.0f;
        float ConeAngleHeight = 60.0f;
        int ConeSegments = 12;
        DrawDebugTools.DrawCone(ConePosition, ConeDirection, ConeLength, ConeAngleWidth, ConeAngleHeight, ConeSegments, Color.green);

        // Draw frustum
        DrawDebugTools.DrawFrustum(Camera.main, Color.yellow);

        // Draw capsule
        Vector3 CapsulePosition = new Vector3(40.0f, 0.0f, 0.0f);
        Vector3 CapsuleRotation = new Vector3(0.0f, 0.0f, 0.0f);
        float CapsuleHalfHeight = 2.0f;
        float CapsuleRadius = 0.5f;
        DrawDebugTools.DrawCapsule(CapsulePosition, CapsuleHalfHeight, CapsuleRadius, Quaternion.Euler(CapsuleRotation), Color.gray);

        // Draw text
        //DrawDebugTools.DrawString2D(TextPos, "The quick brown fox jumps over the lazy dog", anchof, Color.green, 0.0f);
        Quaternion RotationText = Quaternion.identity;
        if (Camera.main)
        {
            RotationText = Quaternion.LookRotation((new Vector3(0.0f, 30.0f, 0.0f) - Camera.main.transform.position).normalized);

        }
        DrawDebugTools.DrawString3D(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(Rot), "HELLO TEXT WORLD", anchof, Color.green, 0.01f, 0.0f);

        // Draw float debug
        if (Input.GetKey(KeyCode.W)) m += 500.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) m -= 500.0f * Time.deltaTime;

        DrawDebugTools.DrawFloatGraph("Sin Value * 2", SinValue * 2.0f, 6.0f, true, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("Sin Value", SinValue, 6.0f, false, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("m Value", m, 1000.0f, false, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("Sin Value 1", SinValue, 6.0f, false, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("Sin Value 2", SinValue, 6.0f, false, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("Sin Value 3", SinValue, 6.0f, false, FloatSamplesCount);
        DrawDebugTools.DrawFloatGraph("Sin Value 4", SinValue, 6.0f, false, FloatSamplesCount);

        // Draw distance
        DrawDebugTools.DrawDistance(new Vector3(-10.0f, 0.0f, 0.0f), MovingSpherePos, Color.green, 0.0f);

        // Log
        DrawDebugTools.Log("Hello world - " + Time.deltaTime, Color.green, 1.0f);
        

        if (Input.GetMouseButtonDown(0))
        {
            DrawDebugTools.Log("Hello Click", Color.red, 4.0f);
        }
    }

    float m = 0.0f;
    public Vector3 TextPos=new Vector3(10.0f, 100.0f, 0.0f);
    public TextAnchor anchof;
    public float FloatMultiplier = 1.0f;
    public int FloatSamplesCount = 10;

}
