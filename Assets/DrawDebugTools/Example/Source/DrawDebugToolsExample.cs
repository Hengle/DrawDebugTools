using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugToolsExample : MonoBehaviour {
	private void Start () {
        //DrawDebugTools.DrawDebugString3D(TextPos, "Hello world", anchof, Color.green, 0.0f);

        //DrawDebugTools.Log("Text in Start function" , Color.green, 10.0f);
    }

    public Vector3 Rot = Vector3.zero;
    private void Update()
    {
        float Xpos = 0.0f;
        float Ypos = 0.5f;
        float XposIncrement = 5.0f;

        // Draw moving sphere
        float SinValue = Mathf.Sin(Time.timeSinceLevelLoad * 4.0f) * FloatMultiplier;
        Vector3 MovingSpherePos = new Vector3(Xpos, 0.0f, SinValue);
        DrawDebugTools.DrawSphere(MovingSpherePos, Quaternion.Euler(Rot), 2.0f, 8, Color.green);
        if (Camera.main)
        {
            DrawDebugTools.DrawString2D(Camera.main.WorldToScreenPoint(MovingSpherePos), MovingSpherePos.ToString(), TextAnchor.UpperLeft, Color.cyan, 0.0f);
        }

        // Draw line
        Xpos += XposIncrement;
        Vector3 LineStart = new Vector3(Xpos, Ypos, 0.0f);
        Vector3 LineEnd = new Vector3(Xpos, 10.0f, 10.0f);
        DrawDebugTools.DrawLine(LineStart, LineEnd, Color.green);

        // Draw point
        Xpos += XposIncrement;
        Vector3 PointPosition = new Vector3(Xpos, Ypos, 0.0f);
        DrawDebugTools.DrawPoint(PointPosition, 1.0f, Color.red);

        // Draw Box
        Xpos += XposIncrement;
        Vector3 BoxPosition = new Vector3(Xpos, 0.0f, 0.0f);
        DrawDebugTools.DrawBox(BoxPosition, Quaternion.Euler(Rot), new Vector3(2.0f, 1.0f, 5.0f), Color.red);

        // Draw circle
        Xpos += XposIncrement;
        Vector3 CirclePosition = new Vector3(Xpos, 0.0f, 0.0f);
        DrawDebugTools.DrawCircle(CirclePosition, Quaternion.Euler(Rot), 2.0f, 24, Color.yellow);

        // Draw coordinates
        Xpos += XposIncrement;
        Vector3 CoorsPosition = new Vector3(Xpos, Ypos, 0.0f);
        DrawDebugTools.DrawCoordinateSystem(CoorsPosition, Quaternion.Euler(Rot), 2.0f);

        // Draw arrow
        Xpos += XposIncrement;
        Vector3 ArrowStartPosition = new Vector3(Xpos, Ypos, 0.0f);
        Vector3 ArrowEndPosition = new Vector3(Xpos, 3.0f, 10.0f);
        DrawDebugTools.DrawDirectionalArrow(ArrowStartPosition, ArrowEndPosition, 1.0f, Color.cyan);

        // Draw cylinder
        Xpos += XposIncrement;
        Vector3 CylinderStart = new Vector3(Xpos, 0.0f, 0.0f);
        Vector3 CylinderEnd = new Vector3(Xpos, 10.0f, 3.0f);
        DrawDebugTools.DrawCylinder(CylinderStart, CylinderEnd, 1.0f, 12, Color.red);

        // Draw cone
        Xpos += XposIncrement;
        Vector3 ConePosition = new Vector3(Xpos, 0.0f, 0.0f);
        Vector3 ConeDirection = Vector3.forward;
        float ConeLength = 2.0f;
        float ConeAngleWidth = 30.0f;
        float ConeAngleHeight = 60.0f;
        int ConeSegments = 12;
        DrawDebugTools.DrawCone(ConePosition, ConeDirection, ConeLength, ConeAngleWidth, ConeAngleHeight, ConeSegments, Color.green);

        // Draw frustum
        DrawDebugTools.DrawFrustum(Camera.main, Color.yellow);

        // Draw capsule
        Xpos += XposIncrement;
        Vector3 CapsulePosition = new Vector3(Xpos, 0.0f, 0.0f);
        Vector3 CapsuleRotation = new Vector3(0.0f, 0.0f, 0.0f);
        float CapsuleHalfHeight = 2.0f;
        float CapsuleRadius = 0.5f;
        DrawDebugTools.DrawCapsule(CapsulePosition, CapsuleHalfHeight, CapsuleRadius, Quaternion.Euler(CapsuleRotation), Color.gray);

        // Draw text
        Xpos += XposIncrement;
        DrawDebugTools.DrawString2D(TextPos, "2D text example", TextAnchor.MiddleRight, Color.green, 0.0f);
        Quaternion RotationText = Quaternion.identity;
        if (Camera.main)
        {
            RotationText = Quaternion.LookRotation((new Vector3(0.0f, 30.0f, 0.0f) - Camera.main.transform.position).normalized);

        }
        DrawDebugTools.DrawString3D(new Vector3(Xpos, Ypos, 0.0f), Quaternion.identity, "HELLO WORLD TEXT", TextAnchor.MiddleCenter, Color.green, 1.0f, 0.0f);
        
        // Draw distance
        DrawDebugTools.DrawDistance(new Vector3(-10.0f, 0.0f, 0.0f), MovingSpherePos, Color.green, 0.0f);

        // Draw Grid
        float GridSize = 100.0f;
        float CellSize = 5.0f;
        DrawDebugTools.DrawGrid(new Vector3(0.0f, 0.0f, 0.0f), GridSize, CellSize, 0.0f);     
                
        // Draw camera
        DrawDebugTools.DrawActiveCamera(Color.cyan);
        
        // Draw float debug
        if (Input.GetKey(KeyCode.W)) m += 500.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) m -= 500.0f * Time.deltaTime;

        DrawDebugTools.DrawFloatGraph("Sin Value * 2", SinValue * 2.0f, 6.0f, true, FloatSamplesCount);
        DrawDebugTools.Log("Sin Value * 2 = " + (SinValue * 2.0f), Color.white, 0.0f);
        DrawDebugTools.DrawFloatGraph("Sin Value", SinValue, 6.0f, false, FloatSamplesCount);
               
        // Log
        if (Input.GetMouseButtonDown(0))
        {
            DrawDebugTools.Log("Hello Click" + Time.timeSinceLevelLoad, Color.red, 4.0f);
        }
        

    }


    float m = 0.0f;
    public Vector3 TextPos=new Vector3(10.0f, 100.0f, 0.0f);
    public float FloatMultiplier = 1.0f;
    public int FloatSamplesCount = 10;

}
