using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.IO;
using Device = SharpDX.Direct3D11.Device;

namespace VfxEditor.DirectX {
    public class DirectXManager {
        private Device Device;
        private DeviceContext Ctx;

        public readonly ModelPreview ModelPreview;
        public readonly GradientView GradientView;
        public readonly PapPreview PapPreview;
        public readonly BoneNamePreview PhybPreview;
        public readonly BoneNamePreview SklbPreview;
        public readonly BoneNamePreview EidPreview;
        public readonly MaterialPreview MaterialPreview;

        public static Include IncludeHandler { get; private set; }

        public DirectXManager() {
            var shaderPath = Path.Combine( Plugin.RootLocation, "Shaders" );
            IncludeHandler = new HLSLFileIncludeHandler( shaderPath );
            Device = Dalamud.PluginInterface.UiBuilder.Device;
            Ctx = Device.ImmediateContext;
            ModelPreview = new( Device, Ctx, shaderPath );
            GradientView = new( Device, Ctx, shaderPath );
            PapPreview = new( Device, Ctx, shaderPath );
            PhybPreview = new( Device, Ctx, shaderPath );
            SklbPreview = new( Device, Ctx, shaderPath );
            EidPreview = new( Device, Ctx, shaderPath );
            MaterialPreview = new( Device, Ctx, shaderPath );
        }

        public void Dispose() {
            ModelPreview.Dispose();
            GradientView.Dispose();
            PapPreview.Dispose();
            PhybPreview.Dispose();
            SklbPreview.Dispose();
            EidPreview.Dispose();
            MaterialPreview.Dispose();

            Device = null;
            Ctx = null;
        }
    }
}
