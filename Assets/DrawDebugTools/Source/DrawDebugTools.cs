using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*********************************//
// Structures                      //
//*********************************//
[System.Serializable]
public class BatchedLine
{
    public Vector3 Start;
    public Vector3 End;
    public Color Color;
    public bool PersistentLine;
    public float RemainLifeTime;

    public BatchedLine(Vector3 InStart, Vector3 InEnd, Color InColor, bool InPersistentLine, float InRemainLifeTime)
    {
        Start = InStart;
        End = InEnd;
        Color = InColor;
        PersistentLine = InPersistentLine;
        RemainLifeTime = InRemainLifeTime;
    }
};

public class DrawDebugTools : MonoBehaviour
{
    //*********************************//
    // Variables                       //
    //*********************************//
    public static DrawDebugTools Instance;
    [HideInInspector]
    public List<BatchedLine> BatchedLines;
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

    public static void DrawDebugSphere(Vector3 Center, float Radius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)
    {
        Segments = Mathf.Max(Segments, 4);
        Segments = (int)Mathf.Round((float)Segments / 4.0f) * 4;

        float AngleInc = 2.0f * Mathf.PI / (float)Segments;

        List<BatchedLine> Lines;
        Lines = new List<BatchedLine>();
        
        for (int i = 0; i <= Segments; i++)
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

                Lines.Add(new BatchedLine(Point_1, Point_2, Color, PersistentLines, LifeTime));
                Lines.Add(new BatchedLine(Point_2, Point_3, Color, PersistentLines, LifeTime));

                Point_1_X = Point_2_X;
                Point_1_Y = Point_2_Y;
                Point_1_Z = Point_2_Z;

                PolarAngle += AngleInc;
            }
        }

        DrawDebugTools.Instance.BatchedLines.AddRange(Lines);
    }

    public static void DrawDebugLine(Vector3 LineStart, Vector3 LineEnd, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)    { }

    public static void DrawDebugPoint(Vector3 Position, float Size, Color PointColor, bool PersistentLines = false, float LifeTime = -1.0f)    { }

    public static void DrawDebugDirectionalArrow(Vector3 LineStart, Vector3 LineEnd, float ArrowSize, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)    { }

    public static void DrawDebugBox(Vector3 Center, Vector3 Extent, Color Color, bool PersistentLines = false, float LifeTime = -1.0f)    { }

    public static void DrawDebugCircle(float Radius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f, bool bDrawAxis = true) {}

    public static void DrawDebugCircle(Vector3 Center, float Radius, int Segments, Color Color, Vector3 YAxis, Vector3 ZAxis, bool PersistentLines = false, float LifeTime = -1.0f, bool bDrawAxis = true) {}

    public static void DrawDebugCoordinateSystem(Vector3 AxisLoc, Vector3 AxisRot, float Scale, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebugCrosshairs( Vector3  AxisLoc, Vector3  AxisRot, float Scale, Color Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebug2DDonut(  float InnerRadius, float OuterRadius, int Segments, Color Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebugCylinder( Vector3  Start, Vector3  End, float Radius, int Segments, Color  Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

    public static void DrawDebugCone( Vector3  Origin, Vector3  Direction, float Length, float AngleWidth, float AngleHeight, int NumSides, Color  Color, bool PersistentLines = false, float LifeTime = -1.0f) { }

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
        GL.MultMatrix(transform.localToWorldMatrix);
        LineMaterial.SetPass(0);

        GL.Begin(GL.LINES);


        for (int i = 0; i < BatchedLines.Count; i++)
        {
            GL.Color(BatchedLines[i].Color);
            GL.Vertex(BatchedLines[i].Start);
            GL.Vertex(BatchedLines[i].End);
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
