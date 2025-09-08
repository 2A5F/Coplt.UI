using System.Runtime.CompilerServices;
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
        swap_chain = new(context, new WindowInteropHelper(this).EnsureHandle(), new((uint)Width, (uint)Height))
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
            BackgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.5f),
            Border = new(0, 30, 30, 30),
            BorderColor = new(
                new Color(0.95f, 0.5f, 0.5f, 0.5f),
                new Color(0.5f, 0.95f, 0.5f, 0.5f),
                new Color(0.5f, 0.5f, 0.95f, 0.5f),
                new Color(0.95f, 0.95f, 0.5f, 0.5f)
            ),
            BorderRadius = 100,
            BorderRadiusMode = BorderRadiusMode.Cosine,
        };
        document.Root.Add(child);

        // var child2 = new UIElement<GpuRd, object>
        // {
        //     Name = "Child2",
        //     Size = new(500, 300),
        //     Border = new(10, 20, 30, 40),
        //     BorderColor = new(
        //         new Color(0.95f, 0.5f, 0.5f, 1f),
        //         new Color(0.5f, 0.95f, 0.5f, 0.5f),
        //         new Color(0.5f, 0.5f, 0.95f, 0.5f),
        //         new Color(0.95f, 0.95f, 0.5f, 0.5f)
        //     ),
        //     // BackgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.5f),
        //     BorderRadius = 100,
        //     BorderRadiusMode = BorderRadiusMode.Circle,
        // };
        // document.Root.Add(child2);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        running = false;
        m_render_thread?.Join();
        Dispose();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var size = Target.RenderSize;
        swap_chain.OnResize(new((uint)size.Width, (uint)size.Height));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        m_render_thread = new Thread(() =>
        {
            var first = true;
            try
            {
                while (running)
                {
                    if (first) first = false;
                    else
                    {
                        swap_chain.WaitFrameReady();
                        context.ReadyNextFrameNoWait();
                    }

                    var size = swap_chain.Size;

                    var layout_changed = document.ComputeLayout(new(size.x, size.y));
                    if (layout_changed) Console.WriteLine(document);

                    renderer.BeginFrame();
                    renderer.Update();

                    renderer.Render(size.x, size.y);

                    renderer.EndFrame();
                    context.SubmitNotEnd();
                    swap_chain.PresentNoWait();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Application.Current.Shutdown();
            }
        });
        m_render_thread.Start();
    }
}
