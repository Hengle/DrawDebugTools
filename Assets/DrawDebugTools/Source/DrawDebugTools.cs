﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
//*********************************//
// Structures | Enums              //
//*********************************//
[System.Serializable]
public class BatchedLine
{
    public Vector3 Start;
    public Vector3 End;
    public Vector3 PivotPoint;
    public Quaternion Rotation;
    public Color Color;
    public bool PersistentLine;
    public float RemainLifeTime;

    public BatchedLine(Vector3 InStart, Vector3 InEnd, Vector3 InPivotPoint, Quaternion InRotation, Color InColor, bool InPersistentLine, float InRemainLifeTime)
    {
        Start = InStart;
        End = InEnd;
        PivotPoint = InPivotPoint;
        Rotation = InRotation;
        Color = InColor;
        PersistentLine = InPersistentLine;
        RemainLifeTime = InRemainLifeTime;
    }
};

public enum EDrawPlaneAxis
{
    XZ,
    XY,
    YZ
};

public class DrawDebugTools : MonoBehaviour
{
    //*********************************//
    // Variables                       //
    //*********************************//
    public static DrawDebugTools        Instance;

    // Lines
    private List<BatchedLine>           m_BatchedLines;

    // Materials
    private Material                    m_LineMaterial;
    private Material                    m_AlphaMaterial;

    // Text
    private static List<DebugText>      m_DebugTextesList;
    private Font                        m_DebugTextFont;

    //*********************************//
    // Functions                       //
    //*********************************//
    private void Awake()
    {
        Instance = this;
        m_BatchedLines = new List<BatchedLine>();

        // Init debug text list
        m_DebugTextesList = new List<DebugText>();
    }

    private void Start()
    {
        InitializeMaterials();
        m_DebugTextFont = Font.CreateDynamicFontFromOSFont("Arial", 12);
        m_DebugTextFont.RequestCharactersInTexture(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~", 12, FontStyle.Normal);
    }

    private void OnRenderObject()
    {
        DrawListOfLines();
        DrawListOfTextes();
    }
        
    private void InitializeMaterials()
    {
        if (!m_LineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader);

            Shader shader3 = Shader.Find("Hidden/Internal-GUITexture");
            m_AlphaMaterial = new Material(shader3);

            //m_AlphaMaterial.hideFlags = HideFlags.HideAndDontSave;
           //  Turn on alpha blending
           // m_AlphaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
           // m_AlphaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
           //  Turn backface culling off
           //m_AlphaMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
           //  Turn off depth writes
           // m_AlphaMaterial.SetInt("_ZWrite", 0);
        }
    }

    public static void DrawDebugSphere(Vector3 Center, Quaternion Rotation, float Radius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        List<BatchedLine> Lines;
        Lines = new List<BatchedLine>();
        
        for (int i = 0; i < Segments; i++)
        {
            float PolarAngle = AngleInc;
            float AzimuthalAngle = AngleInc * i;

            float Point_1_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle);
            float Point_1_Y = Mathf.Cos(PolarAngle);
            float Point_1_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle);

            float Point_2_X;
            float Point_2_Y;
            float Point_2_Z;

            for (int J = 0; J < Segments; J++)
            {

                Point_2_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle);
                Point_2_Y = Mathf.Cos(PolarAngle);
                Point_2_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle);

                float Point_3_X = Mathf.Sin(PolarAngle) * Mathf.Cos(AzimuthalAngle + AngleInc);
                float Point_3_Y = Mathf.Cos(PolarAngle);
                float Point_3_Z = Mathf.Sin(PolarAngle) * Mathf.Sin(AzimuthalAngle + AngleInc);

                Vector3 Point_1 = new Vector3(Point_1_X, Point_1_Y, Point_1_Z) * Radius + Center;
                Vector3 Point_2 = new Vector3(Point_2_X, Point_2_Y, Point_2_Z) * Radius + Center;
                Vector3 Point_3 = new Vector3(Point_3_X, Point_3_Y, Point_3_Z) * Radius + Center;

                Lines.Add(new BatchedLine(Point_1, Point_2, Center, Rotation, Color, PersistentLines, LifeTime));
                Lines.Add(new BatchedLine(Point_2, Point_3, Center, Rotation, Color, PersistentLines, LifeTime));

                Point_1_X = Point_2_X;
                Point_1_Y = Point_2_Y;
                Point_1_Z = Point_2_Z;

                PolarAngle += AngleInc;
            }
        }

        DrawDebugTools.Instance.m_BatchedLines.AddRange(Lines);
    }

    public static void DrawDebugLine(Vector3 LineStart, Vector3 LineEnd, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        DrawDebugTools.Instance.m_BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime));
    }

    private static void InternalDrawDebugLine(Vector3 LineStart, Vector3 LineEnd, Vector3 Center, Quaternion Rotation, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        DrawDebugTools.Instance.m_BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Center, Rotation, Color, PersistentLines, LifeTime));
    }

    public static void DrawDebugPoint(Vector3 Position, float Size, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        // X
        InternalDrawDebugLine(Position + new Vector3(-Size / 2.0f, 0.0f, 0.0f), Position + new Vector3(Size / 2.0f, 0.0f, 0.0f), Position, Quaternion.identity, Color, PersistentLines, LifeTime);
        // Y
        InternalDrawDebugLine(Position + new Vector3( 0.0f, -Size / 2.0f,0.0f), Position + new Vector3( 0.0f, Size / 2.0f, 0.0f), Position, Quaternion.identity, Color, PersistentLines, LifeTime);
        // Z
        InternalDrawDebugLine(Position + new Vector3(0.0f,  0.0f, -Size / 2.0f), Position + new Vector3(0.0f, 0.0f, Size / 2.0f), Position, Quaternion.identity, Color, PersistentLines, LifeTime);
    }

    public static void DrawDebugDirectionalArrow(Vector3 LineStart, Vector3 LineEnd, float ArrowSize, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        InternalDrawDebugLine(LineStart, LineEnd, LineStart, Quaternion.identity, Color, PersistentLines, LifeTime);

        Vector3 Dir = (LineEnd - LineStart).normalized;
        Vector3 Right = Vector3.Cross(Vector3.up, Dir);

        InternalDrawDebugLine(LineEnd, LineEnd + (Right - Dir.normalized) * ArrowSize, LineStart, Quaternion.identity, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(LineEnd, LineEnd + (-Right - Dir.normalized) * ArrowSize, LineStart, Quaternion.identity, Color, PersistentLines, LifeTime);
    }

    public static void DrawDebugBox(Vector3 Center, Quaternion Rotation, Vector3 Extent, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        InternalDrawDebugLine(Center + new Vector3(Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, Extent.y, Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);

        InternalDrawDebugLine(Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);

        InternalDrawDebugLine(Center + new Vector3(Extent.x, Extent.y, Extent.z), Center + new Vector3(Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(Extent.x, -Extent.y, Extent.z), Center + new Vector3(Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, -Extent.y, Extent.z), Center + new Vector3(-Extent.x, -Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(Center + new Vector3(-Extent.x, Extent.y, Extent.z), Center + new Vector3(-Extent.x, Extent.y, -Extent.z), Center, Rotation, Color, PersistentLines, LifeTime);
    }

    public static void DrawDebugCircle(Vector3 Center, Quaternion Rotation, float Radius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        float Angle = 0.0f;
        for (int i = 0; i < Segments; i++)
        {
            Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
            Angle += AngleInc;
            Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
            InternalDrawDebugLine(Point_1, Point_2, Center, Rotation, Color, PersistentLines, LifeTime);
        }
    }

    public static void DrawDebugCircle(Vector3 Center, float Radius, int Segments, Color Color, EDrawPlaneAxis DrawPlaneAxis = EDrawPlaneAxis.XZ, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        float Angle = 0.0f;
        switch (DrawPlaneAxis)
        {
            case EDrawPlaneAxis.XZ:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
                    InternalDrawDebugLine(Point_1, Point_2, Center, Quaternion.identity, Color, PersistentLines, LifeTime);
                }
                break;
            case EDrawPlaneAxis.XY:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(0.0f, Mathf.Sin(Angle), Mathf.Cos(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(0.0f, Mathf.Sin(Angle), Mathf.Cos(Angle));
                    InternalDrawDebugLine(Point_1, Point_2, Center, Quaternion.identity, Color, PersistentLines, LifeTime);
                }
                break;
            case EDrawPlaneAxis.YZ:
                for (int i = 0; i < Segments; i++)
                {
                    Vector3 Point_1 = Center + Radius * new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle));
                    Angle += AngleInc;
                    Vector3 Point_2 = Center + Radius * new Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle));
                    InternalDrawDebugLine(Point_1, Point_2, Center, Quaternion.identity, Color, PersistentLines, LifeTime);
                }
                break;
            default:
                break;
        }
    }

    public static void DrawDebugCoordinateSystem(Vector3 Position, Quaternion Rotation, float Scale, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        InternalDrawDebugLine(Position, Position + new Vector3(Scale, 0.0f, 0.0f), Position, Rotation, Color.red, PersistentLines, LifeTime);
        InternalDrawDebugLine(Position, Position + new Vector3(0.0f, Scale, 0.0f), Position, Rotation, Color.green, PersistentLines, LifeTime);
        InternalDrawDebugLine(Position, Position + new Vector3(0.0f, 0.0f, Scale), Position, Rotation, Color.blue, PersistentLines, LifeTime);
    }

    public static void DrawDebug2DDonut(  float InnerRadius, float OuterRadius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebugCylinder(Vector3 Start, Vector3 End, Quaternion Rotation, float Radius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);

        Vector3 CylinderUp = (End - Start).normalized;
        Vector3 CylinderRight = Vector3.Cross(Vector3.up, CylinderUp).normalized;
        Vector3 CylinderForward = Vector3.Cross(CylinderRight, CylinderUp).normalized;
        float CylinderHeight = (End - Start).magnitude;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        Vector3 Perpondicular = Vector3.zero;
        Vector3 Dummy = Vector3.zero;

        Vector3 Center = (Start + End) / 2.0f;
        
        // Debug End
        float Angle = 0.0f;
        Vector3 P_1;
        Vector3 P_2;
        Vector3 P_3;
        Vector3 P_4;

        Vector3 RotatedVect;
        for (int i = 0; i < Segments; i++)
        {
            RotatedVect = Quaternion.AngleAxis(Mathf.Rad2Deg * Angle, CylinderUp) * CylinderRight * Radius;

            P_1 = Start + RotatedVect;
            P_2 = P_1 + CylinderUp * CylinderHeight;

            // Draw lines
            InternalDrawDebugLine(P_1, P_2, Start, Rotation, Color, PersistentLines, LifeTime);

            Angle += AngleInc;
            RotatedVect = Quaternion.AngleAxis(Mathf.Rad2Deg * Angle, CylinderUp) * CylinderRight * Radius;

            P_3 = Start+ RotatedVect;
            P_4 = P_3 + CylinderUp * CylinderHeight;

            // Draw lines
            InternalDrawDebugLine(P_1, P_3, Start, Rotation, Color, PersistentLines, LifeTime);
            InternalDrawDebugLine(P_2, P_4, Start, Rotation, Color, PersistentLines, LifeTime);
        }
    }

    public static void DrawDebugCone(Vector3 Position, Vector3 Direction, float Length, float AngleWidth, float AngleHeight, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);

        float SmallNumber = 0.001f;
        float Angle1 = Mathf.Clamp(AngleHeight * Mathf.Deg2Rad, SmallNumber, Mathf.PI - SmallNumber);
        float Angle2 = Mathf.Clamp(AngleWidth * Mathf.Deg2Rad, SmallNumber, Mathf.PI - SmallNumber);

        float SinX2 = Mathf.Sin(0.5f * Angle1);
        float SinY2 = Mathf.Sin(0.5f * Angle2);

        float SqrSinX2 = SinX2 * SinX2;
        float SqrSinY2 = SinY2 * SinY2;

        float TanX2 = Mathf.Tan(0.5f * Angle1);
        float TanY2 = Mathf.Tan(0.5f * Angle2);

        Vector3[] ConeVerts;
        ConeVerts = new Vector3[Segments];

        for (int i = 0; i < Segments; i++)
        {
            float AngleFragment = (float)i / (float)(Segments);
            float ThiAngle = 2.0f * Mathf.PI * AngleFragment;
            float PhiAngle = Mathf.Atan2(Mathf.Sin(ThiAngle) * SinY2, Mathf.Cos(ThiAngle) * SinX2);
            float SinPhiAngle = Mathf.Sin(PhiAngle);
            float CosPhiAngle = Mathf.Cos(PhiAngle);
            float SqrSinPhi = SinPhiAngle * SinPhiAngle;
            float SqrCosPhi = CosPhiAngle * CosPhiAngle;

            float RSq = SqrSinX2 * SqrSinY2 / (SqrSinX2 * SqrSinPhi + SqrSinY2 * SqrCosPhi);
            float R = Mathf.Sqrt(RSq);
            float Sqr = Mathf.Sqrt(1 - RSq);
            float Alpha = R * CosPhiAngle;
            float Beta = R * SinPhiAngle;


            ConeVerts[i].x = (1 - 2 * RSq);
            ConeVerts[i].y = 2 * Sqr * Alpha;
            ConeVerts[i].z = 2 * Sqr * Beta;
        }

        Vector3 ConeDirection = Direction.normalized;

        Vector3 AngleFromDirection = Quaternion.LookRotation(ConeDirection, Vector3.up).eulerAngles - new Vector3(0.0f, 90.0f, 0.0f);
        Quaternion Q = Quaternion.Euler(new Vector3(AngleFromDirection.z, AngleFromDirection.y, -AngleFromDirection.x));
        Matrix4x4 M = Matrix4x4.TRS(Position, Q, Vector3.one * Length);

        Vector3 CurrentPoint = Vector3.zero;
        Vector3 PrevPoint = Vector3.zero;
        Vector3 FirstPoint = Vector3.zero;

        for (int i = 0; i < Segments; i++)
        {
            CurrentPoint = M.MultiplyPoint(ConeVerts[i]);
            DrawDebugLine(Position, CurrentPoint, Color, PersistentLines, LifeTime);

            if (i == 0)
            {
                FirstPoint = CurrentPoint;
            }
            else
            {
                DrawDebugLine(PrevPoint, CurrentPoint, Color, PersistentLines, LifeTime);
            }
            PrevPoint = CurrentPoint;
        }

        DrawDebugLine(CurrentPoint, FirstPoint, Color, PersistentLines, LifeTime);
    }

    public static void DrawDebugString2D(Vector2  TextLocation, string Text, TextAnchor Anchor, Color  TextColor, float LifeTime = 0.0f)
    {
        AddDebugText(Text, Anchor, TextLocation, TextColor, LifeTime, true);
    }

    public static void DrawDebugString3D(Vector3 TextLocation, string Text, TextAnchor Anchor, Color TextColor, float LifeTime = 0.0f)
    {
        
        AddDebugText(Text, Anchor, TextLocation, TextColor, LifeTime, false);
    }

    public static void DrawDebugFrustum(Camera Camera, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Plane[] FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera);
        Vector3[] NearPlaneCorners = new Vector3[4]; 
        Vector3[] FarePlaneCorners = new Vector3[4]; 
        
        Plane TempPlane = FrustumPlanes[1]; FrustumPlanes[1] = FrustumPlanes[2]; FrustumPlanes[2] = TempPlane;

        for (int i = 0; i < 4; i++)
        {
            NearPlaneCorners[i] = GetIntersectionPointOfPlanes(FrustumPlanes[4], FrustumPlanes[i], FrustumPlanes[(i + 1) % 4]); 
            FarePlaneCorners[i] = GetIntersectionPointOfPlanes(FrustumPlanes[5], FrustumPlanes[i], FrustumPlanes[(i + 1) % 4]);
        }
        
        for (int i = 0; i < 4; i++)
        {
            InternalDrawDebugLine(NearPlaneCorners[i], NearPlaneCorners[(i + 1) % 4], Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
            InternalDrawDebugLine(FarePlaneCorners[i], FarePlaneCorners[(i + 1) % 4], Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
            InternalDrawDebugLine(NearPlaneCorners[i], FarePlaneCorners[i], Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
        }
    }
    private static Vector3 GetIntersectionPointOfPlanes(Plane Plane_1, Plane Plane_2, Plane Plane_3)
    { 
        return ((-Plane_1.distance * Vector3.Cross(Plane_2.normal, Plane_3.normal)) +
                (-Plane_2.distance * Vector3.Cross(Plane_3.normal, Plane_1.normal)) +
                (-Plane_3.distance * Vector3.Cross(Plane_1.normal, Plane_2.normal))) /
            (Vector3.Dot(Plane_1.normal, Vector3.Cross(Plane_2.normal, Plane_3.normal)));
    }

    public static void DrawCircle(Vector3 Base, Vector3 X, Vector3 Z, Color Color, float Radius, int Segments, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        float AngleDelta = 2.0f * Mathf.PI / Segments;
        Vector3 LastPoint = Base + X * Radius;

        for (int i = 0; i < Segments; i++)
        {
            Vector3 Point = Base + (X * Mathf.Cos(AngleDelta * (i + 1)) + Z * Mathf.Sin(AngleDelta * (i + 1))) * Radius;
            InternalDrawDebugLine(LastPoint, Point, Base, Quaternion.identity, Color, PersistentLines, LifeTime);
            LastPoint = Point;
        }
    }

    public static void DrawHalfCircle(Vector3 Base, Vector3 X, Vector3 Z, Color Color, float Radius, int Segments, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        float AngleDelta = 2.0f * Mathf.PI / Segments;
        Vector3 LastPoint = Base + X * Radius;

        for (int i = 0; i < (Segments/2); i++)
        {
            Vector3 Point = Base + (X * Mathf.Cos(AngleDelta * (i + 1)) + Z * Mathf.Sin(AngleDelta * (i + 1))) * Radius;
            InternalDrawDebugLine(LastPoint, Point, Base, Quaternion.identity, Color, PersistentLines, LifeTime);
            LastPoint = Point;
        }
    }

    public static void DrawDebugCapsule(Vector3 Center, float HalfHeight, float Radius, Quaternion Rotation, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        int Segments = 16;

        Matrix4x4 M = Matrix4x4.TRS(Vector3.zero, Rotation, Vector3.one);

        Vector3 AxisX = M.MultiplyVector(Vector3.right);
        Vector3 AxisY = M.MultiplyVector(Vector3.up);
        Vector3 AxisZ = M.MultiplyVector(Vector3.forward);

        float HalfMaxed = Mathf.Max(HalfHeight - Radius, 0.1f);
        Vector3 TopPoint = Center + HalfMaxed * AxisY;
        Vector3 BottomPoint = Center - HalfMaxed * AxisY;

        DrawCircle(TopPoint, AxisX, AxisZ, Color, Radius, Segments, false, LifeTime);
        DrawCircle(BottomPoint, AxisX, AxisZ, Color, Radius, Segments, false, LifeTime);

        DrawHalfCircle(TopPoint, AxisX, AxisY, Color, Radius, Segments, PersistentLines, LifeTime);
        DrawHalfCircle(TopPoint, AxisZ, AxisY, Color, Radius, Segments, PersistentLines, LifeTime);

        DrawHalfCircle(BottomPoint, AxisX, -AxisY, Color, Radius, Segments, PersistentLines, LifeTime);
        DrawHalfCircle(BottomPoint, AxisZ, -AxisY, Color, Radius, Segments, PersistentLines, LifeTime);

        InternalDrawDebugLine(TopPoint + Radius * AxisX, BottomPoint + Radius * AxisX, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(TopPoint - Radius * AxisX, BottomPoint - Radius * AxisX, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(TopPoint + Radius * AxisZ, BottomPoint + Radius * AxisZ, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
        InternalDrawDebugLine(TopPoint - Radius * AxisZ, BottomPoint - Radius * AxisZ, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime);
    }

    public static void DrawDebugCamera(Vector3 Location, Vector3 Rotation, float FOVDeg, Color Color, float Scale = 1.0f, bool PersistentLines = false, float LifeTime = -1.0f) { }

    //public static void  DrawDebugSolidPlane(FPlane P, Vector3 Loc, float Size, FColor Color, bool bPersistent = false, float LifeTime = -1) { }

    //public static void  DrawDebugSolidPlane(FPlane P, Vector3 Loc, Vector32D Extents, FColor Color, bool bPersistent = false, float LifeTime = -1) { }

    //public static void  DrawDebugFloatHistory(FDebugFloatHistory const & FloatHistory, FTransform const & DrawTransform, Vector32D const & DrawSize, FColor const & DrawColor, bool const & bPersistent = false, float const & LifeTime = -1.0f, uint8 const & DepthPriority = 0) { }

    //public static void  DrawDebugFloatHistory(FDebugFloatHistory const & FloatHistory, Vector3 const & DrawLocation, Vector32D const & DrawSize, FColor const & DrawColor, bool const & bPersistent = false, float const & LifeTime = -1.0f, uint8 const & DepthPriority = 0) { }
    
    private void DrawListOfLines()
    {
        if (m_BatchedLines.Count == 0)
            return;

        // Check material is set
        if (!m_LineMaterial)
        {
            InitializeMaterials();
        }

        // Draw lines
        GL.PushMatrix();

        m_LineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        Matrix4x4 M = transform.localToWorldMatrix;

        for (int i = 0; i < m_BatchedLines.Count; i++)
        {
            M.SetTRS(Vector3.zero, m_BatchedLines[i].Rotation, Vector3.one);
                        
            Vector3 S = m_BatchedLines[i].Start - m_BatchedLines[i].PivotPoint;
            Vector3 E = m_BatchedLines[i].End - m_BatchedLines[i].PivotPoint;

            Vector3 ST = M.MultiplyPoint(S);
            Vector3 ET = M.MultiplyPoint(E);

            ST += m_BatchedLines[i].PivotPoint;
            ET += m_BatchedLines[i].PivotPoint;

            GL.Color(m_BatchedLines[i].Color);
            GL.Vertex(ST);
            GL.Vertex(ET);
        }

        GL.End();

        GL.PopMatrix();

        // Update lines
        for (int i = m_BatchedLines.Count - 1; i >= 0; i--)
        {
            if (!m_BatchedLines[i].PersistentLine)
            {
                if (m_BatchedLines[i].RemainLifeTime > 0.0f)
                {
                    m_BatchedLines[i].RemainLifeTime -= Time.deltaTime;
                    if (m_BatchedLines[i].RemainLifeTime <= 0.0f)
                    {
                        m_BatchedLines.RemoveAt(i);
                    }
                }
                else
                {
                    m_BatchedLines.RemoveAt(i);
                }
            }
        }
    }

    private void DrawListOfTextes()
    {
        GL.PushMatrix();
        m_DebugTextFont.material.SetPass(0);        
        GL.Begin(GL.QUADS);

        
        for (int i = 0; i < m_DebugTextesList.Count; i++)
        {
            if (m_DebugTextesList[i].m_Is2DText)
            {
                GL.LoadPixelMatrix();
            }
            else
            {
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            }

            GL.Color(m_DebugTextesList[i].m_TextColor);
            Vector3 OriginPosition = m_DebugTextesList[i].GetTextOriginPosition(m_DebugTextFont);

            for (int j = 0; j < m_DebugTextesList[i].m_TextString.Length; j++)
            {
                char Char = m_DebugTextesList[i].m_TextString.ToCharArray()[j];

                CharacterInfo CharInfos;
                m_DebugTextFont.GetCharacterInfo(Char, out CharInfos);                

                GL.TexCoord(CharInfos.uvBottomLeft);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.minX, CharInfos.minY, 0.0f));

                GL.TexCoord(CharInfos.uvTopLeft);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.minX, CharInfos.maxY, 0.0f));
                
                GL.TexCoord(CharInfos.uvTopRight);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.maxX, CharInfos.maxY, 0.0f));

                GL.TexCoord(CharInfos.uvBottomRight);
                GL.Vertex(OriginPosition + new Vector3(CharInfos.maxX, CharInfos.minY, 0.0f));
                OriginPosition += new Vector3(CharInfos.advance, 0.0f, 0.0f);
            }
        }

        GL.End();

        GL.PopMatrix();

        // Update text life time
        for (int i = m_DebugTextesList.Count - 1; i >= 0; i--)
        {
            m_DebugTextesList[i].m_RemainLifeTime -= Time.deltaTime;
            if (m_DebugTextesList[i].m_RemainLifeTime <= 0.0f)
            {
                m_DebugTextesList.RemoveAt(i);
            }
        }
    }

    // Add new debug text to be drawn
    private static void AddDebugText(string Text, TextAnchor Anchor, Vector3 Position, Color Color, float LifeTime, bool Is2DText)
    {
        m_DebugTextesList.Add(new DebugText(Text, Anchor, Position, Color, LifeTime, Is2DText));
        print("Text = " + Text);
    }

    public static void FlushPersistentDebugLines()
    {
        // Delete all persistent lines
        for (int i = DrawDebugTools.Instance.m_BatchedLines.Count - 1; i >= 0; i--)
        {
            if (DrawDebugTools.Instance.m_BatchedLines[i].PersistentLine)
            {
                DrawDebugTools.Instance.m_BatchedLines.RemoveAt(i);
            }
        }
    }
}

[System.Serializable]
public class DebugText
{
    public string                   m_TextString;
    public TextAnchor               m_TextAnchor;
    public Vector3                  m_TextPosition;
    public Color                    m_TextColor;
    public float                    m_RemainLifeTime;
    public bool                     m_Is2DText;

    public DebugText()
    {
    }

    public DebugText(string Text, TextAnchor TextAnchor, Vector3 TextPosition, Color Color, float LifeTime, bool Is2DText)
    {
        m_TextString        = Text;
        m_TextAnchor        = TextAnchor;
        m_TextPosition      = TextPosition;
        m_TextColor         = Color;
        m_RemainLifeTime    = LifeTime;
        m_Is2DText          = Is2DText;
    }

    // Get text anchored position
    public Vector3 GetTextOriginPosition(Font TextFont)
    {
        Vector3 OriginPos = m_TextPosition;

        float TextWidth = 0.0f;
        float TextHeight = 0.0f;

        // Get text size
        char[] CharsArray = m_TextString.ToCharArray();
        for (int i = 0; i < CharsArray.Length; i++)
        {
            CharacterInfo CharInfos;
            TextFont.GetCharacterInfo(CharsArray[i], out CharInfos);
            TextWidth += CharInfos.advance;
            TextHeight = CharInfos.glyphHeight;
        }

        switch (m_TextAnchor)
        {
            case TextAnchor.UpperLeft:
                OriginPos += new Vector3(0.0f, -TextHeight, 0.0f);
                break;
            case TextAnchor.UpperCenter:
                OriginPos += new Vector3(-TextWidth / 2.0f, -TextHeight, 0.0f);
                break;
            case TextAnchor.UpperRight:
                OriginPos += new Vector3(-TextWidth, -TextHeight, 0.0f);
                break;
            case TextAnchor.MiddleLeft:
                OriginPos += new Vector3(0.0f, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.MiddleCenter:
                OriginPos += new Vector3(-TextWidth / 2.0f, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.MiddleRight:
                OriginPos += new Vector3(-TextWidth, -TextHeight / 2.0f, 0.0f);
                break;
            case TextAnchor.LowerLeft:
                // Default position
                break;
            case TextAnchor.LowerCenter:
                OriginPos += new Vector3(-TextWidth / 2.0f, 0.0f, 0.0f);
                break;
            case TextAnchor.LowerRight:
                OriginPos += new Vector3(-TextWidth, 0.0f, 0.0f);
                break;
            default:
                break;
        }
        return OriginPos;
    }
}