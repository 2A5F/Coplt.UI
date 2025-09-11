using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Elements;
using Coplt.UI.Rendering;
using Coplt.UI.Rendering.Gpu;
using Coplt.UI.Rendering.Gpu.D3d12;
using Coplt.UI.Styles;

namespace TestGpu1;

[Dropping]
public partial class MainWindow
{
    [Drop(Order = -1)]
    private readonly UIDocument<GpuRd, object> document = new();
    private readonly D3d12GpuContext context;
    private readonly HwndSwapChain swap_chain;
    [Drop(Order = 0)]
    private readonly GpuRenderer<object> renderer;

    private bool running = true;

    private IntPtr Hwnd;
    private Thread? m_render_thread;

    static MainWindow()
    {
        D3d12GpuContext.LoggerFunc = (_, _, _, pDescription, _) =>
        {
            unsafe
            {
                var msg = new string((sbyte*)pDescription);
                Console.WriteLine(msg);
            }
        };
    }

    public MainWindow()
    {
        InitializeComponent();

        var is_debug = Environment.GetCommandLineArgs().Contains("-D");

        context = new(is_debug);
        swap_chain = new(context, Hwnd = new WindowInteropHelper(this).EnsureHandle(), new((uint)Width, (uint)Height))
        {
            VSync = true,
        };
        renderer = new(new GpuRendererBackendD3d12(context, swap_chain), document)
        {
            ClearBackgroundColor = Color.White,
        };

        document.Root.Size = new(1.Pc, 1.Pc);
        document.Root.JustifyContent = JustifyContent.Center;
        document.Root.AlignItems = AlignItems.Center;
        document.Root.FlexDirection = FlexDirection.Row;
        document.Root.FlexWrap = FlexWrap.Wrap;

        var child = new UIElement<GpuRd, object>
        {
            Name = "Child1",
            JustifyContent = JustifyContent.Center,
            Size = new(220, 200),
            // Size = new(1.Pc, 1.Pc),
            // BackgroundColor = Color.Gray,
            BackgroundColor = new Color(0.95f, 0.5f, 0.5f, 1f),
            // BackgroundColor = new Color(0.75f, 0.75f, 0.75f, 1f),
            // Border = new(30, 30, 30, 30),
            // BorderColor = new(
            //     new Color(0.95f, 0.5f, 0.5f, 1f),
            //     new Color(0.5f, 0.95f, 0.5f, 1f),
            //     new Color(0.5f, 0.5f, 0.95f, 1f),
            //     new Color(0.95f, 0.95f, 0.5f, 1f)
            // ),
            BorderRadius = 100,
            BorderRadiusMode = BorderRadiusMode.Cosine,
        };
        document.Root.Add(child);

        var child2 = new UIElement<GpuRd, object>
        {
            Name = "Child2",
            Size = new(100, 100),
            // Border = new(5, 8, 15, 20),
            // BorderColor = new(
            //     new Color(0.95f, 0.5f, 0.5f, 0.5f),
            //     new Color(0.5f, 0.95f, 0.5f, 0.5f),
            //     new Color(0.5f, 0.5f, 0.95f, 0.5f),
            //     new Color(0.95f, 0.95f, 0.5f, 0.5f)
            // ),
            BackgroundColor = new Color(1f, 1f, 1f, 0.5f),
            BorderRadius = 30,
            BorderRadiusMode = BorderRadiusMode.Cosine,
        };
        child.Add(child2);

        var child3 = new UIElement<GpuRd, object>
        {
            Name = "Child3",
            Size = new(20, 20),
            BackgroundColor = new Color(0.5f, 0.5f, 0.95f, 1f),
        };
        document.Root.Add(child3);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        running = false;
        m_render_thread?.Join();
        Dispose();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var size = new uint2((uint)Target.RenderSize.Width, (uint)Target.RenderSize.Height);
        swap_chain.OnResize(size);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        m_render_thread = new Thread(() =>
        {
            var first = true;
            try
            {
                var last_frame = 0;
                var max_frame = HwndSwapChain.FrameCount;
                while (running)
                {
                    var is_first = first;
                    if (first) first = false;
                    else swap_chain.WaitFrameReady();

                    var size = swap_chain.Size;

                    var layout_changed = document.ComputeLayout(new(size.x, size.y));
                    if (layout_changed) Console.WriteLine(document);

                    var need_re_render = renderer.Update(size.x, size.y, layout_changed);

                    if (need_re_render) last_frame = 0;
                    if (last_frame < max_frame)
                    {
                        last_frame++;
                        if (!is_first) context.ReadyNextFrameNoWait();
                        renderer.BeginFrame();
                        renderer.Render();
                        renderer.EndFrame();
                        context.SubmitNotEnd();
                    }

                    swap_chain.PresentNoWait();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Application.Current.Dispatcher.InvokeShutdown();
            }
        });
        m_render_thread.Start();
    }
}
