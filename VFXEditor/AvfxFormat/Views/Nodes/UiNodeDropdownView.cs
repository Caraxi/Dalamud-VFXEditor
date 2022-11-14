using ImGuiNET;
using System.IO;
using VfxEditor.AVFXLib;
using VfxEditor.Utils;

namespace VfxEditor.AvfxFormat.Vfx {
    public abstract class UiNodeDropdownView<T> : IUiBase, IUiNodeView<T> where T : UiNode {
        public readonly AVFXMain Avfx;
        public readonly AvfxFile VfxFile;
        public readonly UiNodeGroup<T> Group;

        private readonly string Id;
        private readonly string DefaultText;
        private readonly string DefaultPath;
        private readonly bool AllowNew;
        private readonly bool AllowDelete;

        private T Selected = null;

        public UiNodeDropdownView( AvfxFile vfxFile, AVFXMain avfx, UiNodeGroup<T> group, string name, bool allowNew, bool allowDelete, string defaultPath ) {
            VfxFile = vfxFile;
            Avfx = avfx;
            Group = group;
            AllowNew = allowNew;
            AllowDelete = allowDelete;

            Id = $"##{name}";
            DefaultText = $"Select {UiUtils.GetArticle(name)} {name}";
            DefaultPath = Path.Combine( Plugin.RootLocation, "Files", defaultPath );
        }

        public abstract void RemoveFromAvfx( T item );
        public abstract void AddToAvfx( T item, int idx );
        public abstract T AddToAvfx( BinaryReader reader, int size, bool has_dependencies = false );

        public abstract void OnExport( BinaryWriter writer, T item );
        public abstract void OnSelect( T item );
        public void ImportDefault() => VfxFile.Import( DefaultPath );

        public void AddToGroup( T item ) => Group.AddAndUpdate( item );

        public void DrawInline( string parentId = "" ) {
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ViewSelect();

            if( AllowNew ) ImGui.SameLine();
            IUiNodeView<T>.DrawControls( this, VfxFile, Selected, Group, AllowNew, AllowDelete, Id );

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            ImGui.Separator();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 10 );

            if( Selected != null ) Selected.DrawInline( Id );
        }

        public void ViewSelect() {
            var selectedString = ( Selected != null ) ? Selected.GetText() : DefaultText;
            if( ImGui.BeginCombo( Id + "-Select", selectedString ) ) {
                for(var idx = 0; idx < Group.Items.Count; idx++) {
                    var item = Group.Items[idx];
                    if( ImGui.Selectable( $"{item.GetText()}{Id}{idx}", Selected == item ) ) {
                        Selected = item;
                        OnSelect( item );
                    }
                }
                ImGui.EndCombo();
            }
        }

        public void ResetSelected() { Selected = null; }
    }
}