using ImGuiNET;
using System.IO;
using VfxEditor.EidFormat.BindPoint;
using VfxEditor.Parsing;
using VfxEditor.Parsing.String;

namespace VfxEditor.EidFormat {
    public class EidBindPointOld : EidBindPoint {
        public int BindPointId => Id.Value;

        public readonly ParsedPaddedString Name = new( "Bone Name", "n_root", 12, 0x00 );
        public readonly ParsedInt Id = new( "Id" );
        public readonly ParsedFloat3 Position = new( "Position" );
        public readonly ParsedDegrees3 Rotation = new( "Rotation" );

        public EidBindPointOld() { }

        public EidBindPointOld( BinaryReader reader ) {
            Id.Read( reader );
            Position.Read( reader );
            Rotation.Read( reader );
            Name.Read( reader );
            reader.ReadInt32(); // padding
        }

        public override void Write( BinaryWriter writer ) {
            Id.Write( writer );
            Position.Write( writer );
            Rotation.Write( writer );
            Name.Write( writer );
            writer.Write( 0 );
        }
        public override int GetId() => Id.Value;

        public override void Draw() {
            ImGui.TextDisabled( "Data Version: [OLD]" );

            Name.Draw( CommandManager.Eid );
            Id.Draw( CommandManager.Eid );
            Position.Draw( CommandManager.Eid );
            Rotation.Draw( CommandManager.Eid );
        }
    }
}