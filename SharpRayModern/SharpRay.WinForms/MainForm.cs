using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpRay.Core;

namespace SharpRay.WinForms;

public sealed class MainForm : Form
{
    private SceneDocument document = SceneDocument.CreateDemo();
    private readonly SceneViewport preview = new();
    private readonly ListBox objects = new() { Dock = DockStyle.Fill };
    private readonly ListBox lights = new() { Dock = DockStyle.Fill };
    private readonly PropertyGrid inspector = new() { Dock = DockStyle.Fill, HelpVisible = true };
    private readonly TabControl editorTabs = new() { Dock = DockStyle.Fill };
    private readonly Button renderButton = new() { Text = "Render", Width = 85 };
    private readonly Button cancelButton = new() { Text = "Cancel", Width = 85, Enabled = false };
    private readonly Button exportButton = new() { Text = "Save PNG", Width = 95, Enabled = false };
    private readonly NumericUpDown widthInput = new() { Minimum = 64, Maximum = 4000, Increment = 64, Value = 800, Width = 85 };
    private readonly NumericUpDown heightInput = new() { Minimum = 64, Maximum = 4000, Increment = 64, Value = 500, Width = 85 };
    private readonly NumericUpDown samplesInput = new() { Minimum = 1, Maximum = 64, Value = 4, Width = 55 };
    private readonly ToolStripStatusLabel status = new("Ready");
    private CancellationTokenSource? cancellation;

    public MainForm()
    {
        Text = "SharpRay Modern — Scene Editor + BVH Renderer";
        Width = 1360;
        Height = 825;
        MinimumSize = new Size(860, 550);
        StartPosition = FormStartPosition.CenterScreen;

        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add("New demo scene", null, (_, _) => NewScene());
        fileMenu.DropDownItems.Add("Open scene JSON...", null, (_, _) => OpenScene());
        fileMenu.DropDownItems.Add("Save scene JSON...", null, (_, _) => SaveScene());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Export PNG...", null, (_, _) => SavePng());
        menu.Items.Add(fileMenu);
        MainMenuStrip = menu;

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, WrapContents = false,
            Padding = new Padding(7, 8, 0, 0)
        };
        toolbar.Controls.AddRange(
        [
            new Label { Text = "Width", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, widthInput,
            new Label { Text = "Height", AutoSize = true, Padding = new Padding(4, 6, 0, 0) }, heightInput,
            new Label { Text = "Samples", AutoSize = true, Padding = new Padding(4, 6, 0, 0) }, samplesInput,
            renderButton, cancelButton, exportButton
        ]);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Vertical,
            Size = new Size(1200, 680),
            SplitterWidth = 6, SplitterDistance = 380,
            Panel1MinSize = 290, Panel2MinSize = 300
        };
        var editorSplit = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Horizontal,
            Size = new Size(380, 680),
            SplitterDistance = 360, Panel1MinSize = 180, Panel2MinSize = 160
        };

        editorTabs.TabPages.Add(MakeObjectTab());
        editorTabs.TabPages.Add(MakeLightTab());
        var cameraTab = new TabPage("Camera");
        cameraTab.Controls.Add(new Label
        {
            Dock = DockStyle.Fill, Text = "Select this tab to edit the camera below.\r\n\r\n" +
                "Change properties, then press Render.\r\nScene JSON stores all editor settings.",
            Padding = new Padding(15)
        });
        editorTabs.TabPages.Add(cameraTab);
        editorTabs.SelectedIndexChanged += (_, _) => SetInspectorSelection();
        objects.SelectedIndexChanged += (_, _) => SetInspectorSelection();
        lights.SelectedIndexChanged += (_, _) => SetInspectorSelection();
        inspector.PropertyValueChanged += (_, _) =>
        {
            objects.Refresh();
            lights.Refresh();
            preview.Invalidate();
            status.Text = "Scene modified — press Render to update";
        };
        preview.Picked = shape =>
        {
            editorTabs.SelectedIndex = 0;
            objects.SelectedItem = shape;
        };
        preview.PositionChanged = () =>
        {
            inspector.Refresh();
            objects.Refresh();
            status.Text = "Dragging scene object…";
        };
        preview.EditFinished = () =>
        {
            if (cancellation is null) _ = RenderAsync();
            else status.Text = "Position updated — render after current render completes";
        };

        editorSplit.Panel1.Controls.Add(editorTabs);
        editorSplit.Panel2.Controls.Add(inspector);
        split.Panel1.Controls.Add(editorSplit);
        split.Panel2.Controls.Add(preview);

        var strip = new StatusStrip();
        strip.Items.Add(status);
        Controls.Add(split);
        Controls.Add(strip);
        Controls.Add(toolbar);
        Controls.Add(menu);

        renderButton.Click += async (_, _) => await RenderAsync();
        cancelButton.Click += (_, _) => cancellation?.Cancel();
        exportButton.Click += (_, _) => SavePng();
        Shown += async (_, _) => await RenderAsync();
        FormClosed += (_, _) =>
        {
            cancellation?.Cancel();
            preview.Image?.Dispose();
        };
        RefreshEditor();
    }

    private TabPage MakeObjectTab()
    {
        var page = new TabPage("Objects");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        foreach (ShapeKind kind in Enum.GetValues<ShapeKind>())
        {
            ShapeKind chosen = kind;
            var button = new Button { Text = $"+ {kind}", AutoSize = true };
            button.Click += (_, _) =>
            {
                document.Objects.Add(new EditableShape { Name = $"{chosen} {document.Objects.Count + 1}", Shape = chosen });
                RefreshEditor();
                objects.SelectedIndex = objects.Items.Count - 1;
            };
            buttons.Controls.Add(button);
        }
        var remove = new Button { Text = "Remove", AutoSize = true };
        remove.Click += (_, _) =>
        {
            if (objects.SelectedIndex < 0) return;
            document.Objects.RemoveAt(objects.SelectedIndex);
            RefreshEditor();
        };
        var bitmap = new Button { Text = "Choose BMP...", AutoSize = true };
        bitmap.Click += (_, _) =>
        {
            if (objects.SelectedItem is not EditableShape shape) return;
            using OpenFileDialog dialog = new() { Filter = "Windows Bitmap|*.bmp" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            shape.BitmapPath = dialog.FileName;
            shape.Texture = TextureKind.Bitmap;
            inspector.Refresh();
        };
        buttons.Controls.Add(remove);
        buttons.Controls.Add(bitmap);
        layout.Controls.Add(objects, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage MakeLightTab()
    {
        var page = new TabPage("Lights");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var add = new Button { Text = "+ Point light", AutoSize = true };
        add.Click += (_, _) =>
        {
            document.Lights.Add(new EditableLight { Name = $"Light {document.Lights.Count + 1}" });
            RefreshEditor();
            lights.SelectedIndex = lights.Items.Count - 1;
        };
        var remove = new Button { Text = "Remove", AutoSize = true };
        remove.Click += (_, _) =>
        {
            if (lights.SelectedIndex < 0) return;
            document.Lights.RemoveAt(lights.SelectedIndex);
            RefreshEditor();
        };
        buttons.Controls.Add(add);
        buttons.Controls.Add(remove);
        layout.Controls.Add(lights, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private void RefreshEditor()
    {
        objects.BeginUpdate();
        objects.Items.Clear();
        foreach (var obj in document.Objects) objects.Items.Add(obj);
        objects.EndUpdate();
        lights.BeginUpdate();
        lights.Items.Clear();
        foreach (var light in document.Lights) lights.Items.Add(light);
        lights.EndUpdate();
        if (objects.Items.Count > 0) objects.SelectedIndex = 0;
        if (lights.Items.Count > 0) lights.SelectedIndex = 0;
        preview.Document = document;
        SetInspectorSelection();
    }

    private void SetInspectorSelection()
    {
        preview.Selection = editorTabs.SelectedIndex == 0 ? objects.SelectedItem as EditableShape : null;
        preview.Invalidate();
        inspector.SelectedObject = editorTabs.SelectedIndex switch
        {
            0 => objects.SelectedItem,
            1 => lights.SelectedItem,
            2 => document.Camera,
            _ => null
        };
    }

    private void NewScene()
    {
        if (cancellation is not null) return;
        document = SceneDocument.CreateDemo();
        RefreshEditor();
        status.Text = "Demo scene loaded";
    }

    private void OpenScene()
    {
        if (cancellation is not null) return;
        using OpenFileDialog dialog = new() { Filter = "SharpRay scenes|*.json|All files|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            document = SceneDocument.Load(dialog.FileName);
            RefreshEditor();
            status.Text = $"Opened {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception error) { ShowError(error); }
    }

    private void SaveScene()
    {
        using SaveFileDialog dialog = new() { Filter = "SharpRay scenes|*.json", FileName = "Scene.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            document.Save(dialog.FileName);
            status.Text = $"Saved {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception error) { ShowError(error); }
    }

    private void SavePng()
    {
        if (preview.Image is null) return;
        using SaveFileDialog dialog = new() { Filter = "PNG images|*.png", FileName = "SharpRay.png" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { preview.Image.Save(dialog.FileName, ImageFormat.Png); }
        catch (Exception error) { ShowError(error); }
    }

    private async Task RenderAsync()
    {
        if (cancellation is not null) return;
        cancellation = new CancellationTokenSource();
        renderButton.Enabled = false;
        cancelButton.Enabled = true;
        exportButton.Enabled = false;
        status.Text = "Building BVH and rendering...";
        try
        {
            (Scene scene, Camera camera) = document.Build();
            var options = new RenderOptions((int)widthInput.Value,
                (int)heightInput.Value, (int)samplesInput.Value);
            var progress = new Progress<int>(p => status.Text = $"Rendering {p}%");
            int[] pixels = await new Renderer().RenderAsync(scene, camera, options, progress, cancellation.Token);
            using Bitmap bitmap = new(options.Width, options.Height, PixelFormat.Format32bppArgb);
            Rectangle bounds = new(0, 0, options.Width, options.Height);
            BitmapData data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int row = 0; row < options.Height; row++)
                    Marshal.Copy(pixels, row * options.Width,
                        IntPtr.Add(data.Scan0, row * data.Stride), options.Width);
            }
            finally { bitmap.UnlockBits(data); }
            Image? old = preview.Image;
            preview.Image = (Bitmap)bitmap.Clone();
            old?.Dispose();
            exportButton.Enabled = true;
            status.Text = $"Complete — {options.Width} × {options.Height}, {options.Samples} spp";
        }
        catch (OperationCanceledException) { status.Text = "Render cancelled"; }
        catch (Exception error) { status.Text = "Render failed"; ShowError(error); }
        finally
        {
            cancellation?.Dispose();
            cancellation = null;
            renderButton.Enabled = true;
            cancelButton.Enabled = false;
        }
    }

    private void ShowError(Exception error) =>
        MessageBox.Show(this, error.Message, "SharpRay Modern",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
