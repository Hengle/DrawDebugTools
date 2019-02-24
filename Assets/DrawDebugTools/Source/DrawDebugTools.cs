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
    private static float                m_FontSizeModifier = 0.5f;

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

            Shader shader3 = Shader.Find("Unlit/Transparent");
            m_AlphaMaterial = new Material(shader3);
            m_AlphaMaterial.SetTexture("_MainTex", TextDatas.GetFontTexture());

            m_AlphaMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            m_AlphaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_AlphaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            m_AlphaMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            m_AlphaMaterial.SetInt("_ZWrite", 0);
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

        GL.Color(new Color(1.0f, 0.0f, 0.0f, 0.7f));

        
        Vector3 OriginPosition = Vector3.zero;
        for (int i = 0; i < m_DebugTextesList.Count; i++)
        {
            float image_w = 512.0f;
            float image_h = 512.0f;
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
        m_DebugTextesList.Add(new DebugText(DebugCharList, Anchor, Position, m_FontSizeModifier, LifeTime));
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

        Texture2D FontTexture = new Texture2D(512, 512);
        FontTexture.LoadImage(ImageBytes);
        return FontTexture;
    }

    public static string DebugTextFontBitmap = "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AAAgEwAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAADAAAAGAAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAADAAAAGAAAAAAAwAAABgAAADAAAAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAGAAAAAAAwAAABgAAADAAAAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAADAAAAGAAAABgAGQCAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAAMgEAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAADAAAAGAAAAAAAwAAABgAAADAAAAAAAYAAAAGAAAAMAAAAIABAAAADAAAAGAAAAAAAwAAABgAAADAAAAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAABkAmAADAAAAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAIBMAAGAAAAAAAwAAABgAAADAAAAAAAYAAAAwAAAAgAEAAAAMAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAYAAAAAADAAAAGAAAAMAAAAAABgAAADAAAACAAQAAAAwAAABgAAAAAAMAAAAYAAAAwAAAAAAGAAAAMAAAAIABAAAADAAAAGAAAABgAAAAwOfwvwEAf2kOXsBVXt8AAAAASUVORK5CYII=";
    public static string DebugTextFontXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BitmapFont SpriteSheet=""SourceCodeFont.png"" FontSize=""24"">
<Letter Char="" "" X=""0"" Y=""0"" Width=""10.66667"" Height=""44.22399"" />
<Letter Char=""!"" X=""10.66667"" Y=""0"" Width=""20.20267"" Height=""44.22399"" />
<Letter Char=""&quot;"" X=""30.86933"" Y=""0"" Width=""24.71466"" Height=""44.22399"" />
<Letter Char=""#"" X=""55.584"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""$"" X=""82.63466"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""%"" X=""109.6853"" Y=""0"" Width=""37.83466"" Height=""44.22399"" />
<Letter Char=""&amp;"" X=""147.52"" Y=""0"" Width=""30.73066"" Height=""44.22399"" />
<Letter Char=""'"" X=""178.2507"" Y=""0"" Width=""18.85867"" Height=""44.22399"" />
<Letter Char=""("" X=""197.1093"" Y=""0"" Width=""20.65067"" Height=""44.22399"" />
<Letter Char="")"" X=""217.76"" Y=""0"" Width=""20.65067"" Height=""44.22399"" />
<Letter Char=""*"" X=""238.4106"" Y=""0"" Width=""24.45866"" Height=""44.22399"" />
<Letter Char=""+"" X=""262.8693"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char="","" X=""289.92"" Y=""0"" Width=""18.85867"" Height=""44.22399"" />
<Letter Char=""-"" X=""308.7787"" Y=""0"" Width=""20.90666"" Height=""44.22399"" />
<Letter Char=""."" X=""329.6853"" Y=""0"" Width=""18.85867"" Height=""44.22399"" />
<Letter Char=""/"" X=""348.544"" Y=""0"" Width=""22.18666"" Height=""44.22399"" />
<Letter Char=""0"" X=""370.7307"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""1"" X=""397.7813"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""2"" X=""424.832"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""3"" X=""451.8827"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""4"" X=""478.9333"" Y=""0"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""5"" X=""0"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""6"" X=""27.05066"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""7"" X=""54.10133"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""8"" X=""81.15199"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""9"" X=""108.2027"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char="":"" X=""135.2533"" Y=""44.22399"" Width=""18.85867"" Height=""44.22399"" />
<Letter Char="";"" X=""154.112"" Y=""44.22399"" Width=""18.85867"" Height=""44.22399"" />
<Letter Char=""&lt;"" X=""172.9707"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""="" X=""200.0213"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""&gt;"" X=""227.072"" Y=""44.22399"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""?"" X=""254.1227"" Y=""44.22399"" Width=""24.68266"" Height=""44.22399"" />
<Letter Char=""@"" X=""278.8053"" Y=""44.22399"" Width=""38.57066"" Height=""44.22399"" />
<Letter Char=""A"" X=""317.376"" Y=""44.22399"" Width=""28.39466"" Height=""44.22399"" />
<Letter Char=""B"" X=""345.7706"" Y=""44.22399"" Width=""29.86666"" Height=""44.22399"" />
<Letter Char=""C"" X=""375.6373"" Y=""44.22399"" Width=""29.32266"" Height=""44.22399"" />
<Letter Char=""D"" X=""404.96"" Y=""44.22399"" Width=""30.82666"" Height=""44.22399"" />
<Letter Char=""E"" X=""435.7866"" Y=""44.22399"" Width=""27.88266"" Height=""44.22399"" />
<Letter Char=""F"" X=""463.6693"" Y=""44.22399"" Width=""26.79466"" Height=""44.22399"" />
<Letter Char=""G"" X=""0"" Y=""88.44798"" Width=""30.82666"" Height=""44.22399"" />
<Letter Char=""H"" X=""30.82666"" Y=""88.44798"" Width=""31.97866"" Height=""44.22399"" />
<Letter Char=""I"" X=""62.80533"" Y=""88.44798"" Width=""19.17867"" Height=""44.22399"" />
<Letter Char=""J"" X=""81.98399"" Y=""88.44798"" Width=""26.28266"" Height=""44.22399"" />
<Letter Char=""K"" X=""108.2667"" Y=""88.44798"" Width=""29.57866"" Height=""44.22399"" />
<Letter Char=""L"" X=""137.8453"" Y=""88.44798"" Width=""26.50666"" Height=""44.22399"" />
<Letter Char=""M"" X=""164.352"" Y=""88.44798"" Width=""34.47466"" Height=""44.22399"" />
<Letter Char=""N"" X=""198.8267"" Y=""88.44798"" Width=""31.81866"" Height=""44.22399"" />
<Letter Char=""O"" X=""230.6453"" Y=""88.44798"" Width=""32.39466"" Height=""44.22399"" />
<Letter Char=""P"" X=""263.04"" Y=""88.44798"" Width=""29.48266"" Height=""44.22399"" />
<Letter Char=""Q"" X=""292.5226"" Y=""88.44798"" Width=""32.39466"" Height=""44.22399"" />
<Letter Char=""R"" X=""324.9173"" Y=""88.44798"" Width=""29.64266"" Height=""44.22399"" />
<Letter Char=""S"" X=""354.56"" Y=""88.44798"" Width=""28.10666"" Height=""44.22399"" />
<Letter Char=""T"" X=""382.6666"" Y=""88.44798"" Width=""28.17066"" Height=""44.22399"" />
<Letter Char=""U"" X=""410.8373"" Y=""88.44798"" Width=""31.75466"" Height=""44.22399"" />
<Letter Char=""V"" X=""442.5919"" Y=""88.44798"" Width=""27.46666"" Height=""44.22399"" />
<Letter Char=""W"" X=""470.0586"" Y=""88.44798"" Width=""36.39466"" Height=""44.22399"" />
<Letter Char=""X"" X=""0"" Y=""132.672"" Width=""27.40266"" Height=""44.22399"" />
<Letter Char=""Y"" X=""27.40266"" Y=""132.672"" Width=""26.18666"" Height=""44.22399"" />
<Letter Char=""Z"" X=""53.58933"" Y=""132.672"" Width=""28.26666"" Height=""44.22399"" />
<Letter Char=""["" X=""81.856"" Y=""132.672"" Width=""20.65067"" Height=""44.22399"" />
<Letter Char=""\"" X=""102.5067"" Y=""132.672"" Width=""22.18666"" Height=""44.22399"" />
<Letter Char=""]"" X=""124.6933"" Y=""132.672"" Width=""20.65067"" Height=""44.22399"" />
<Letter Char=""^"" X=""145.344"" Y=""132.672"" Width=""27.05066"" Height=""44.22399"" />
<Letter Char=""_"" X=""172.3947"" Y=""132.672"" Width=""27.14666"" Height=""44.22399"" />
<Letter Char=""`"" X=""199.5413"" Y=""132.672"" Width=""28.52266"" Height=""44.22399"" />
<Letter Char=""a"" X=""228.064"" Y=""132.672"" Width=""27.53066"" Height=""44.22399"" />
<Letter Char=""b"" X=""255.5947"" Y=""132.672"" Width=""28.97066"" Height=""44.22399"" />
<Letter Char=""c"" X=""284.5653"" Y=""132.672"" Width=""25.70667"" Height=""44.22399"" />
<Letter Char=""d"" X=""310.272"" Y=""132.672"" Width=""28.97066"" Height=""44.22399"" />
<Letter Char=""e"" X=""339.2426"" Y=""132.672"" Width=""27.01866"" Height=""44.22399"" />
<Letter Char=""f"" X=""366.2613"" Y=""132.672"" Width=""20.29866"" Height=""44.22399"" />
<Letter Char=""g"" X=""386.56"" Y=""132.672"" Width=""27.27466"" Height=""44.22399"" />
<Letter Char=""h"" X=""413.8347"" Y=""132.672"" Width=""28.58666"" Height=""44.22399"" />
<Letter Char=""i"" X=""442.4213"" Y=""132.672"" Width=""18.76266"" Height=""44.22399"" />
<Letter Char=""j"" X=""461.184"" Y=""132.672"" Width=""18.79466"" Height=""44.22399"" />
<Letter Char=""k"" X=""479.9787"" Y=""132.672"" Width=""26.98666"" Height=""44.22399"" />
<Letter Char=""l"" X=""0"" Y=""176.896"" Width=""19.08266"" Height=""44.22399"" />
<Letter Char=""m"" X=""19.08266"" Y=""176.896"" Width=""37.99466"" Height=""44.22399"" />
<Letter Char=""n"" X=""57.07733"" Y=""176.896"" Width=""28.68266"" Height=""44.22399"" />
<Letter Char=""o"" X=""85.75999"" Y=""176.896"" Width=""28.52266"" Height=""44.22399"" />
<Letter Char=""p"" X=""114.2827"" Y=""176.896"" Width=""28.97066"" Height=""44.22399"" />
<Letter Char=""q"" X=""143.2533"" Y=""176.896"" Width=""28.77866"" Height=""44.22399"" />
<Letter Char=""r"" X=""172.032"" Y=""176.896"" Width=""22.09066"" Height=""44.22399"" />
<Letter Char=""s"" X=""194.1227"" Y=""176.896"" Width=""24.49066"" Height=""44.22399"" />
<Letter Char=""t"" X=""218.6133"" Y=""176.896"" Width=""21.80266"" Height=""44.22399"" />
<Letter Char=""u"" X=""240.416"" Y=""176.896"" Width=""28.58666"" Height=""44.22399"" />
<Letter Char=""v"" X=""269.0026"" Y=""176.896"" Width=""26.05866"" Height=""44.22399"" />
<Letter Char=""w"" X=""295.0613"" Y=""176.896"" Width=""34.34666"" Height=""44.22399"" />
<Letter Char=""x"" X=""329.408"" Y=""176.896"" Width=""25.35466"" Height=""44.22399"" />
<Letter Char=""y"" X=""354.7626"" Y=""176.896"" Width=""26.05866"" Height=""44.22399"" />
<Letter Char=""z"" X=""380.8213"" Y=""176.896"" Width=""24.68266"" Height=""44.22399"" />
<Letter Char=""{"" X=""405.504"" Y=""176.896"" Width=""20.65067"" Height=""44.22399"" />
<Letter Char=""|"" X=""426.1546"" Y=""176.896"" Width=""18.60266"" Height=""44.22399"" />
<Letter Char=""}"" X=""444.7573"" Y=""176.896"" Width=""20.65067"" Height=""44.22399"" />
</BitmapFont>
";
        
}