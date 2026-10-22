using System.Drawing.Drawing2D;
using System.Numerics;
using SharpRay.Core;

namespace SharpRay.WinForms;

/// <summary>
/// Interactive move gizmo rendered in the same projected viewport as the CPU render.
/// Red = world X, green = world Y, blue = world Z. Dragging projects mouse movement
/// onto the visible axis, while clicking geometry performs an exact ray/object pick.
/// </summary>
public sealed class SceneViewport : PictureBox
{
    public SceneDocument? Document { get; set; }
    public EditableShape? Selection { get; set; }
    public Action<EditableShape>? Picked { get; set; }
    public Action? PositionChanged { get; set; }
    public Action? EditFinished { get; set; }

    private int dragAxis = -1;
    private Point mouseAnchor;
    private Vector3 objectAnchor;
    private PointF axisBegin, axisEnd;
    private readonly Color[] axisColours = [Color.FromArgb(242, 88, 91),
        Color.FromArgb(94, 226, 141), Color.FromArgb(91, 164, 255)];

    public SceneViewport()
    {
        Dock = DockStyle.Fill;
        SizeMode = PictureBoxSizeMode.Zoom;
        BackColor = Color.FromArgb(19, 23, 32);
        Cursor = Cursors.Cross;
        DoubleBuffered = true;
    }

    private RectangleF ImageBounds()
    {
        if (Image is null) return new RectangleF(0, 0, Width, Height);
        float scale = MathF.Min((float)Width / Image.Width, (float)Height / Image.Height);
        float w = Image.Width * scale, h = Image.Height * scale;
        return new RectangleF((Width - w) * 0.5f, (Height - h) * 0.5f, w, h);
    }

    private bool Project(Vector3 world, out PointF screen)
    {
        screen = default;
        if (Document is null) return false;
        var area = ImageBounds();
        if (area.Width <= 0 || area.Height <= 0) return false;
        Camera camera = Document.Camera.ToCamera();
        if (!camera.TryProject(world, area.Width / area.Height, out Vector2 uv)) return false;
        screen = new PointF(area.Left + area.Width * uv.X, area.Top + area.Height * uv.Y);
        return true;
    }

    private bool AxisEndpoints(int axis, out PointF start, out PointF end)
    {
        start = end = default;
        if (Selection is null) return false;
        Vector3 centre = new(Selection.X, Selection.Y, Selection.Z);
        Vector3 unit = axis switch
        {
            0 => Vector3.UnitX, 1 => Vector3.UnitY, _ => Vector3.UnitZ
        };
        return Project(centre, out start) && Project(centre + unit, out end);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Selection is null || Document is null || Image is null) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        for (int axis = 0; axis < 3; axis++)
        {
            if (!AxisEndpoints(axis, out PointF begin, out PointF end)) continue;
            using Pen pen = new(axisColours[axis], axis == dragAxis ? 5f : 3f);
            pen.EndCap = LineCap.ArrowAnchor;
            e.Graphics.DrawLine(pen, begin, end);
            using Brush brush = new SolidBrush(axisColours[axis]);
            e.Graphics.FillEllipse(brush, end.X - 7, end.Y - 7, 14, 14);
            e.Graphics.DrawString("XYZ"[axis].ToString(),
                SystemFonts.DefaultFont, brush, end.X + 9, end.Y - 10);
        }
        if (Project(new Vector3(Selection.X, Selection.Y, Selection.Z), out PointF origin))
        {
            using Pen pen = new(Color.White, 2);
            e.Graphics.DrawEllipse(pen, origin.X - 5, origin.Y - 5, 10, 10);
        }
    }

    private static float DistanceToSegment(Point p, PointF a, PointF b)
    {
        Vector2 ab = new(b.X - a.X, b.Y - a.Y);
        Vector2 ap = new(p.X - a.X, p.Y - a.Y);
        float t = Math.Clamp(Vector2.Dot(ap, ab) / Math.Max(ab.LengthSquared(), 0.0001f), 0f, 1f);
        return (ap - ab * t).Length();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || Document is null || Image is null) return;
        int nearest = -1;
        float smallest = 14f;
        if (Selection is not null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!AxisEndpoints(i, out PointF a, out PointF b)) continue;
                float length = (new Vector2(b.X - a.X, b.Y - a.Y)).Length();
                if (length < 12f) continue; // axis approximately parallel to the camera
                float distance = DistanceToSegment(e.Location, a, b);
                if (distance < smallest) { smallest = distance; nearest = i; }
            }
        }
        if (nearest >= 0 && Selection is not null)
        {
            dragAxis = nearest;
            mouseAnchor = e.Location;
            objectAnchor = new Vector3(Selection.X, Selection.Y, Selection.Z);
            AxisEndpoints(dragAxis, out axisBegin, out axisEnd);
            Capture = true;
            Cursor = Cursors.SizeAll;
            Invalidate();
            return;
        }
        PickAt(e.Location);
    }

    private void PickAt(Point mouse)
    {
        if (Document is null) return;
        RectangleF rect = ImageBounds();
        if (!rect.Contains(mouse)) return;
        float u = (mouse.X - rect.Left) / rect.Width;
        float v = (mouse.Y - rect.Top) / rect.Height;
        try
        {
            Ray ray = Document.Camera.ToCamera().GetRay(u, v, rect.Width / rect.Height);
            EditableShape? selected = null;
            float closest = float.MaxValue;
            foreach (EditableShape editable in Document.Objects)
            {
                if (editable.ToGeometry().TryHit(ray, 0.001f, closest, out Hit hit))
                {
                    closest = hit.Distance;
                    selected = editable;
                }
            }
            if (selected is null) return;
            Selection = selected;
            Picked?.Invoke(selected);
            Invalidate();
        }
        catch { /* An incomplete shape/bitmap is editable without preventing other UI interaction. */ }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (dragAxis < 0 || Selection is null) return;
        Vector2 axisScreen = new(axisEnd.X - axisBegin.X, axisEnd.Y - axisBegin.Y);
        Vector2 delta = new(e.X - mouseAnchor.X, e.Y - mouseAnchor.Y);
        float amount = Vector2.Dot(delta, axisScreen) /
                       Math.Max(axisScreen.LengthSquared(), 0.001f);
        Selection.X = objectAnchor.X + (dragAxis == 0 ? amount : 0);
        Selection.Y = objectAnchor.Y + (dragAxis == 1 ? amount : 0);
        Selection.Z = objectAnchor.Z + (dragAxis == 2 ? amount : 0);
        PositionChanged?.Invoke();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (dragAxis < 0) return;
        dragAxis = -1;
        Capture = false;
        Cursor = Cursors.Cross;
        Invalidate();
        EditFinished?.Invoke();
    }
}
