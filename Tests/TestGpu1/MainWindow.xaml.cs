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
    private readonly UIDocument<GpuRd, object> document = new();
    [Drop(Order = 2)]
    private readonly D3d12GpuContext context;
    [Drop(Order = 1)]
    private readonly HwndSwapChain swap_chain;
    [Drop(Order = 0)]
    private readonly GpuRenderer<object> renderer;

    private bool runing = true;

    static MainWindow()
    {
        D3d12GpuContext.LoggerFunc = (Category, Severity, id, pDescription, pContext) =>
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
        renderer = new(new GpuRendererBackendD3d12(context), document);

        // todo style access
        Unsafe.AsRef(in document.Root.CommonStyle).Size = new(1.Pc(), 1.Pc());
        Unsafe.AsRef(in document.Root.CommonStyle).JustifyContent = JustifyContent.Center;
        Unsafe.AsRef(in document.Root.CommonStyle).AlignItems = AlignItems.Center;

        var child = new UIElement<GpuRd, object> { Name = "Child1" };
        // todo style access
        Unsafe.AsRef(in child.CommonStyle).Size = new(100, 100);
        Unsafe.AsRef(in Unsafe.AsRef(in child.RData).GpuStyle).BackgroundColor = Color.Lime;
        document.Root.Add(child);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        runing = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var size = Target.RenderSize;
        swap_chain.OnResize(new((uint)size.Width, (uint)size.Height));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        new Thread(() =>
        {
            var first = true;
            try
            {
                while (runing)
                {
                    if (first) first = false;
                    else
                    {
                        swap_chain.WaitFrameReady();
                        context.ReadyNextFrameNoWait();
                    }

                    document.ComputeLayout(new(swap_chain.Size.x, swap_chain.Size.y));
                    renderer.Update();

                    renderer.Upload();

                    swap_chain.BarrierToRenderTarget();
                    context.ClearRenderTargetView(swap_chain.CurrentRtv, new float4(1, 1, 1, 1));
                    renderer.Render();
                    swap_chain.BarrierToPresent();

                    context.SubmitNotEnd();
                    swap_chain.PresentNoWait();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Application.Current.Shutdown();
            }
        }).Start();
    }
}
