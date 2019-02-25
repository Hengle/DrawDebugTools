using System.Collections;
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
    private static List<XElement>       m_XmlLettersList;
    public float                m_FontSizeModifier = 0.5f;

    //*********************************//
    // Functions                       //
    //*********************************//
    private void Awake()
    {
        Instance = this;
        m_BatchedLines = new List<BatchedLine>();
        
        // Initialize font xml
        XDocument doc = XDocument.Parse(TextDatas.DebugTextFontXml, LoadOptions.PreserveWhitespace);
        m_XmlLettersList = doc.Element("BitmapFont").Elements("Letter").ToList<XElement>();

        // Init debug text list
        m_DebugTextesList = new List<DebugText>();
    }

    private void Start()
    {
        InitializeMaterials();
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
            m_AlphaMaterial.SetTexture("_MainTex", TextDatas.GetFontTexture());

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

    public static void DrawDebugString(Vector3  TextLocation, string Text, TextAnchor Anchor, Color  TextColor, float LifeTime = 0.0f)
    {
        AddDebugText(Text, Anchor, TextLocation, LifeTime);
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
    //private void DrawListOfQuads()
    //{
    //    // Draw lines
    //    GL.PushMatrix();

    //    AlphaMaterial.SetPass(0);

    //    GL.Begin(GL.QUADS);
    //    Matrix4x4 M = transform.localToWorldMatrix;

    //    GL.LoadPixelMatrix();

    //    GL.Color(new Color(1.0f, 0.0f, 0.0f, 0.7f));

    //    //@
    //    float image_w = 512.0f;
    //    float image_h = 512.0f;

    //    float uv_x = x / image_w;
    //    float uv_y = (image_h - (y + height)) / image_h;
    //    float uv_width = width / image_w;// 256.0f;
    //    float uv_height = height / image_h;//256.0f;

    //    //print("x = " + x + " | y = " + y + " | w = " + width + " | h = " + height);
    //    GL.TexCoord2(uv_x, uv_y);
    //    GL.Vertex3(0, 0f, 0);

    //    GL.TexCoord2(uv_x, uv_y + uv_height);
    //    GL.Vertex3(0f, height * 0.7f, 0);

    //    GL.TexCoord2(uv_x + uv_width, uv_y + uv_height);
    //    GL.Vertex3(width*0.7f, height * 0.7f, 0);

    //    GL.TexCoord2(uv_x + uv_width, uv_y);
    //    GL.Vertex3(width * 0.7f, 0, 0);

    //    GL.End();

    //    GL.PopMatrix();
    //}
    private void DrawListOfTextes()
    {
        GL.PushMatrix();

        m_AlphaMaterial.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.LoadPixelMatrix();

        //GL.Color(new Color(1.0f, 0.0f, 0.0f, 0.7f));

        
        Vector3 OriginPosition = Vector3.zero;
        for (int i = 0; i < m_DebugTextesList.Count; i++)
        {
            float image_w = 256.0f;
            float image_h = 256.0f;
            float LastCharWidth = 0.0f;

            OriginPosition = m_DebugTextesList[i].GetTextOriginPosition();

            for (int j = 0; j < m_DebugTextesList[i].m_TextCharsList.Count; j++)
            {
                //print("ghghgh i | j = " + i + " | " + j);
                DebugChar CurrenDebugChar = m_DebugTextesList[i].m_TextCharsList[j];

                float uv_x = CurrenDebugChar.X / image_w;
                float uv_y = (image_h - (CurrenDebugChar.Y + CurrenDebugChar.H)) / image_h;
                float uv_width = CurrenDebugChar.W / image_w;
                float uv_height = CurrenDebugChar.H / image_h;

                // Set vertices position
                //float SizeModifier = 0.5f;
                Vector3 VertexPos_1 = new Vector3(OriginPosition.x + LastCharWidth, OriginPosition.y, 0.0f);
                Vector3 VertexPos_2 = new Vector3(OriginPosition.x + LastCharWidth, OriginPosition.y + CurrenDebugChar.H * m_FontSizeModifier, 0.0f);
                Vector3 VertexPos_3 = new Vector3(OriginPosition.x + LastCharWidth + CurrenDebugChar.W * m_FontSizeModifier, OriginPosition.y + CurrenDebugChar.H * m_FontSizeModifier, 0.0f);
                Vector3 VertexPos_4 = new Vector3(OriginPosition.x + LastCharWidth + CurrenDebugChar.W * m_FontSizeModifier, OriginPosition.y, 0.0f);

                LastCharWidth += CurrenDebugChar.W * m_FontSizeModifier;

                GL.TexCoord2(uv_x, uv_y);
                GL.Vertex(VertexPos_1);

                GL.TexCoord2(uv_x, uv_y + uv_height);
                GL.Vertex(VertexPos_2);

                GL.TexCoord2(uv_x + uv_width, uv_y + uv_height);
                GL.Vertex(VertexPos_3);

                GL.TexCoord2(uv_x + uv_width, uv_y);
                GL.Vertex(VertexPos_4);
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
    private static void AddDebugText(string Text, TextAnchor Anchor, Vector3 Position, float LifeTime)
    {
        char[] LettersChars = Text.ToCharArray();
        List<DebugChar> DebugCharList=new List<DebugChar>();
        for (int i = 0; i < LettersChars.Length; i++)
        {
            DebugChar DC = GetLetterInfosFromXml(LettersChars[i].ToString());
            DebugCharList.Add(DC);
        }
        float Size = Mathf.Floor(DrawDebugTools.Instance.m_FontSizeModifier * 256.0f) / 256.0f;
        print("size = " + (DrawDebugTools.Instance.m_FontSizeModifier * 256.0f));
        DrawDebugTools.Instance.m_FontSizeModifier = Size;
        m_DebugTextesList.Add(new DebugText(DebugCharList, Anchor, Position, Size, LifeTime));
    }

    // Get letter infos from xml
    private static DebugChar GetLetterInfosFromXml(string Letter)
    {
        DebugChar LetterInfos = new DebugChar();
        for (int i = 0; i < m_XmlLettersList.Count; i++)
        {
            if (m_XmlLettersList[i].Attribute("Char").Value == Letter)
            {
                LetterInfos.Char = m_XmlLettersList[i].Attribute("Char").Value;
                LetterInfos.X = float.Parse(m_XmlLettersList[i].Attribute("X").Value);
                LetterInfos.Y = float.Parse(m_XmlLettersList[i].Attribute("Y").Value);
                LetterInfos.W = float.Parse(m_XmlLettersList[i].Attribute("Width").Value);
                LetterInfos.H = float.Parse(m_XmlLettersList[i].Attribute("Height").Value);
            }
        }
        return LetterInfos;
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

public class DebugChar
{
    public string   Char;
    public float    X;
    public float    Y;
    public float    W;
    public float    H;

    public DebugChar()
    {
        X = 0.0f;
        Y = 0.0f;
        W = 0.0f;
        H = 0.0f;
    }

    public DebugChar(string InChar, float InX, float InY, float InW, float InH)
    {
        Char = InChar;
        X = InX;
        Y = InY;
        W = InW;
        H = InH;
    }

    public Vector2 GetLetterSize()
    {
        return new Vector2(W, H);
    }
    
}

[System.Serializable]
public class DebugText
{
    public List<DebugChar>          m_TextCharsList;
    public TextAnchor               m_TextAnchor;
    public Vector3                  m_TextPosition;
    public float                    m_RemainLifeTime;
    public float                    m_FontSize;

    public DebugText()
    {
        m_TextCharsList = new List<DebugChar>();
    }

    public DebugText(List<DebugChar> TextCharsList, TextAnchor TextAnchor, Vector3 TextPosition, float FontSize, float LifeTime)
    {
        // Initialize text letters list
        m_TextCharsList     = new List<DebugChar>();
        m_TextCharsList     = TextCharsList;
        m_TextAnchor        = TextAnchor;
        m_TextPosition      = TextPosition;
        m_FontSize          = FontSize;
        m_RemainLifeTime    = LifeTime;
    }

    public float GetTextWidth()
    {
        float SumWidth = 0.0f;
        for (int i = 0; i < m_TextCharsList.Count; i++)
        {
            SumWidth += m_TextCharsList[i].W;
        }
        return SumWidth;
    }
    public float GetTextHeight()
    {
        return m_TextCharsList[0].H;
    }

    // Get text origin position to start draw from
    public Vector3 GetTextOriginPosition()
    {
        Vector3 OriginPos = m_TextPosition;

        float TextWidth = GetTextWidth() * m_FontSize;
        float TextHeight = GetTextHeight() * m_FontSize;

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



public static class TextDatas
{
    public static Texture2D GetFontTexture()
    {
        byte[] ImageBytes = System.Convert.FromBase64String(DebugTextFontBitmap);

        Texture2D FontTexture = new Texture2D(256, 256);
        FontTexture.LoadImage(ImageBytes);
        FontTexture.anisoLevel = 0;
        FontTexture.filterMode = FilterMode.Trilinear;
        return FontTexture;
    }

    public static string UnlitTransparentShader = @"
Shader ""Unlit/Transparent"" {
    Properties {
	    _MainTex (""Base (RGB) Trans (A)"", 2D) = ""white"" {}
    }

    SubShader {
	    Tags {""Queue""=""Transparent"" ""IgnoreProjector""=""True"" ""RenderType""=""Transparent""}
	    LOD 100
	
	    ZWrite Off
	    Blend SrcAlpha OneMinusSrcAlpha 
	
	    Pass {  
		    CGPROGRAM
			    #pragma vertex vert
			    #pragma fragment frag
			    #pragma target 2.0
			    #pragma multi_compile_fog
			
			    #include ""UnityCG.cginc""

			    struct appdata_t {
				    float4 vertex : POSITION;
				    float2 texcoord : TEXCOORD0;
				    UNITY_VERTEX_INPUT_INSTANCE_ID
			    };

			    struct v2f {
				    float4 vertex : SV_POSITION;
				    float2 texcoord : TEXCOORD0;
				    UNITY_FOG_COORDS(1)
				    UNITY_VERTEX_OUTPUT_STEREO
			    };

			    sampler2D _MainTex;
			    float4 _MainTex_ST;
			
			    v2f vert (appdata_t v)
			    {
				    v2f o;
				    UNITY_SETUP_INSTANCE_ID(v);
				    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				    o.vertex = UnityObjectToClipPos(v.vertex);
				    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				    UNITY_TRANSFER_FOG(o,o.vertex);
				    return o;
			    }
			
			    fixed4 frag (v2f i) : SV_Target
			    {
				    fixed4 col = tex2D(_MainTex, i.texcoord);
				    UNITY_APPLY_FOG(i.fogCoord, col);
				    return col;
			    }
		    ENDCG
	    }
    }
}
";

    public static string DebugTextFontBitmap = @"
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
";

    public static string DebugTextFontXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BitmapFont SpriteSheet=""FontImage.png"" FontSize=""17"">
<Letter Char="" "" X=""0"" Y=""0"" Width=""7.555554"" Height=""28.51041"" />
<Letter Char=""!"" X=""7.555554"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""&quot;"" X=""29.12283"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""#"" X=""50.69009"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""$"" X=""72.25736"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""%"" X=""93.82463"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""&amp;"" X=""115.3919"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""'"" X=""136.9592"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""("" X=""158.5264"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char="")"" X=""180.0937"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""*"" X=""201.661"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""+"" X=""223.2283"" Y=""0"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char="","" X=""0"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""-"" X=""21.56727"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""."" X=""43.13454"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""/"" X=""64.70181"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""0"" X=""86.26908"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""1"" X=""107.8363"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""2"" X=""129.4036"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""3"" X=""150.9709"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""4"" X=""172.5382"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""5"" X=""194.1055"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""6"" X=""215.6727"" Y=""28.51041"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""7"" X=""0"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""8"" X=""21.56727"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""9"" X=""43.13454"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char="":"" X=""64.70181"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char="";"" X=""86.26908"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""&lt;"" X=""107.8363"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""="" X=""129.4036"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""&gt;"" X=""150.9709"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""?"" X=""172.5382"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""@"" X=""194.1055"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""A"" X=""215.6727"" Y=""57.02082"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""B"" X=""0"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""C"" X=""21.56727"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""D"" X=""43.13454"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""E"" X=""64.70181"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""F"" X=""86.26908"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""G"" X=""107.8363"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""H"" X=""129.4036"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""I"" X=""150.9709"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""J"" X=""172.5382"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""K"" X=""194.1055"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""L"" X=""215.6727"" Y=""85.53123"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""M"" X=""0"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""N"" X=""21.56727"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""O"" X=""43.13454"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""P"" X=""64.70181"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""Q"" X=""86.26908"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""R"" X=""107.8363"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""S"" X=""129.4036"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""T"" X=""150.9709"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""U"" X=""172.5382"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""V"" X=""194.1055"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""W"" X=""215.6727"" Y=""114.0416"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""X"" X=""0"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""Y"" X=""21.56727"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""Z"" X=""43.13454"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""["" X=""64.70181"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""\"" X=""86.26908"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""]"" X=""107.8363"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""^"" X=""129.4036"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""_"" X=""150.9709"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""`"" X=""172.5382"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""a"" X=""194.1055"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""b"" X=""215.6727"" Y=""142.5521"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""c"" X=""0"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""d"" X=""21.56727"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""e"" X=""43.13454"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""f"" X=""64.70181"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""g"" X=""86.26908"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""h"" X=""107.8363"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""i"" X=""129.4036"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""j"" X=""150.9709"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""k"" X=""172.5382"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""l"" X=""194.1055"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""m"" X=""215.6727"" Y=""171.0625"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""n"" X=""0"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""o"" X=""21.56727"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""p"" X=""43.13454"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""q"" X=""64.70181"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""r"" X=""86.26908"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""s"" X=""107.8363"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""t"" X=""129.4036"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""u"" X=""150.9709"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""v"" X=""172.5382"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""w"" X=""194.1055"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""x"" X=""215.6727"" Y=""199.5729"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""y"" X=""0"" Y=""228.0833"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""z"" X=""21.56727"" Y=""228.0833"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""{"" X=""43.13454"" Y=""228.0833"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""|"" X=""64.70181"" Y=""228.0833"" Width=""21.56727"" Height=""28.51041"" />
<Letter Char=""}"" X=""86.26908"" Y=""228.0833"" Width=""21.56727"" Height=""28.51041"" />
</BitmapFont>
";
        
}