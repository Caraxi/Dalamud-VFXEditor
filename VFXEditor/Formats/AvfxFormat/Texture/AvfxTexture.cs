using ImGuiNET;
using OtterGui.Raii;
using System.IO;
using System.Numerics;
using VfxEditor.AvfxFormat.Nodes;
using VfxEditor.Formats.TextureFormat.Textures;
using VfxEditor.Library.Texture;
using VfxEditor.Parsing;

namespace VfxEditor.AvfxFormat {
    public class AvfxTexture : AvfxNode {
        public const string NAME = "Tex";

        public readonly AvfxString Path = new( "Path", "Path", false );
        public readonly UiNodeGraphView NodeView;

        public AvfxTexture() : base( NAME, AvfxNodeGroupSet.TextureColor ) {
            NodeView = new( this );
        }

        public override void ReadContents( BinaryReader reader, int size ) => Path.ReadContents( reader, size );

        protected override void RecurseChildrenAssigned( bool assigned ) { }

        public override void WriteContents( BinaryWriter writer ) => Path.WriteContents( writer );

        public override void Draw() {
            using var _ = ImRaii.PushId( "Texture" );
            NodeView.Draw();
            DrawRename();

            var preCombo = ImGui.GetCursorPosX();

            Plugin.LibraryManager.DrawTextureCombo( Path.Value, ( TextureLeaf texture ) => {
                if( texture.DrawSelectable() ) {
                    var newValue = texture.GetPath().Trim().Trim( '\0' );
                    CommandManager.Avfx.Add( new ParsedSimpleCommand<string>( Path.Parsed, newValue ) );
                }
            } );

            var imguiStyle = ImGui.GetStyle();
            using( var style = ImRaii.PushStyle( ImGuiStyleVar.ItemSpacing, new Vector2( imguiStyle.ItemInnerSpacing.X, imguiStyle.ItemSpacing.Y ) ) ) {
                ImGui.SameLine();
                Path.Draw( ImGui.GetCursorPosX() - preCombo );
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 2 );
            GetTexture()?.Draw();
        }

        public override void ShowTooltip() {
            ImGui.BeginTooltip();
            GetTexture()?.DrawImage();
            ImGui.EndTooltip();
        }

        public TextureDrawable GetTexture() => Plugin.TextureManager.GetTexture( Path.Value );

        public override string GetDefaultText() => $"Texture {GetIdx()}";

        public override string GetWorkspaceId() => $"Tex{GetIdx()}";
    }
}