using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static DrawDebugTools Instance;
    
    private List<BatchedLine> BatchedLines;
    private Material LineMaterial;

    //*********************************//
    // Functions                       //
    //*********************************//
    private void Awake()
    {
        Instance = this;
        BatchedLines = new List<BatchedLine>();
    }

    private void Start()
    {
        CreateLineMaterial();
        
    }

    private void OnRenderObject()
    {
        DrawListOfLines();
    }
        
    private void CreateLineMaterial()
    {
        if (!LineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            LineMaterial = new Material(shader);
            //lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            //lineMaterial.SetInt("_ZWrite", 0);
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

        DrawDebugTools.Instance.BatchedLines.AddRange(Lines);
    }

    public static void DrawDebugLine(Vector3 LineStart, Vector3 LineEnd, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        DrawDebugTools.Instance.BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Vector3.zero, Quaternion.identity, Color, PersistentLines, LifeTime));
    }

    private static void InternalDrawDebugLine(Vector3 LineStart, Vector3 LineEnd, Vector3 Center, Quaternion Rotation, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        DrawDebugTools.Instance.BatchedLines.Add(new BatchedLine(LineStart, LineEnd, Center, Rotation, Color, PersistentLines, LifeTime));
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
    public Vector3 Angle = Vector3.zero;

    public static void DrawDebugAltCone( Vector3  Origin, Vector3  Rotation, float Length, float AngleWidth, float AngleHeight, Color  DrawColor, bool PersistentLines = false, float LifeTime = -1.0f, float Thickness = 0.0f) { }

    public static void DrawDebugString( Vector3  TextLocation, string Text, Color  TextColor, float Duration = -1.000000f, bool  DrawShadow = false) { }

    public static void DrawDebugFrustum(Color Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawCircle(Vector3 Base, Vector3 X, Vector3 Y, Color Color, float Radius, int NumSides, bool PersistentLines = false, float LifeTime = -1.0f, float Thickness = 0) { }

    public static void DrawDebugCapsule(Vector3 Center, float HalfHeight, float Radius, Vector3 Rotation, Color Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebugCamera(Vector3 Location, Vector3 Rotation, float FOVDeg, Color Color, float Scale = 1.0f, bool PersistentLines = false, float LifeTime = -1.0f) { }

    //public static void  DrawDebugSolidPlane(FPlane P, Vector3 Loc, float Size, FColor Color, bool bPersistent = false, float LifeTime = -1) { }

    //public static void  DrawDebugSolidPlane(FPlane P, Vector3 Loc, Vector32D Extents, FColor Color, bool bPersistent = false, float LifeTime = -1) { }

    //public static void  DrawDebugFloatHistory(FDebugFloatHistory const & FloatHistory, FTransform const & DrawTransform, Vector32D const & DrawSize, FColor const & DrawColor, bool const & bPersistent = false, float const & LifeTime = -1.0f, uint8 const & DepthPriority = 0) { }

    //public static void  DrawDebugFloatHistory(FDebugFloatHistory const & FloatHistory, Vector3 const & DrawLocation, Vector32D const & DrawSize, FColor const & DrawColor, bool const & bPersistent = false, float const & LifeTime = -1.0f, uint8 const & DepthPriority = 0) { }


    private void DrawListOfLines()
    {
        if (BatchedLines.Count == 0)
            return;

        // Check material is set
        if (!LineMaterial)
        {
            CreateLineMaterial();
        }

        // Draw lines
        GL.PushMatrix();

        LineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        Matrix4x4 M = transform.localToWorldMatrix;

        for (int i = 0; i < BatchedLines.Count; i++)
        {
            M.SetTRS(Vector3.zero, BatchedLines[i].Rotation, Vector3.one);
                        
            Vector3 S = BatchedLines[i].Start - BatchedLines[i].PivotPoint;
            Vector3 E = BatchedLines[i].End - BatchedLines[i].PivotPoint;

            Vector3 ST = M.MultiplyPoint(S);
            Vector3 ET = M.MultiplyPoint(E);

            ST += BatchedLines[i].PivotPoint;
            ET += BatchedLines[i].PivotPoint;

            GL.Color(BatchedLines[i].Color);
            GL.Vertex(ST);
            GL.Vertex(ET);
        }

        GL.End();

        GL.PopMatrix();

        // Update lines
        for (int i = BatchedLines.Count - 1; i >= 0; i--)
        {
            if (!BatchedLines[i].PersistentLine)
            {
                if (BatchedLines[i].RemainLifeTime > 0.0f)
                {
                    BatchedLines[i].RemainLifeTime -= Time.deltaTime;
                    if (BatchedLines[i].RemainLifeTime <= 0.0f)
                    {
                        BatchedLines.RemoveAt(i);
                    }
                }
                else
                {
                    BatchedLines.RemoveAt(i);
                }
            }
        }
    }

    public static void FlushPersistentDebugLines()
    {
        // Delete all persistent lines
        for (int i = DrawDebugTools.Instance.BatchedLines.Count - 1; i >= 0; i--)
        {
            if (DrawDebugTools.Instance.BatchedLines[i].PersistentLine)
            {
                DrawDebugTools.Instance.BatchedLines.RemoveAt(i);
            }
        }
    }
}
