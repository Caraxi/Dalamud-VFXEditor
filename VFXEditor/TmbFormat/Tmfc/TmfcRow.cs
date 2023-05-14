using System.IO;
using VfxEditor.Parsing;

namespace VfxEditor.TmbFormat {
    public class TmfcRow {
        public readonly ParsedUInt Unk1 = new( "Unknown 1" );
        public readonly ParsedFloat Time = new( "Time" );
        public readonly ParsedFloat Unk2 = new( "Unknown 2" );
        public readonly ParsedFloat Unk3 = new( "Unknown 3" );
        public readonly ParsedFloat Unk4 = new( "Unknown 4" );
        public readonly ParsedFloat Unk5 = new( "Unknown 5" );

        public TmfcRow( BinaryReader reader ) {
            Unk1.Read( reader );
            Time.Read( reader );
            Unk2.Read( reader );
            Unk3.Read( reader );
            Unk4.Read( reader );
            Unk5.Read( reader );
        }

        public void Write( BinaryWriter writer ) {
            Unk1.Write( writer );
            Time.Write( writer );
            Unk2.Write( writer );
            Unk3.Write( writer );
            Unk4.Write( writer );
            Unk5.Write( writer );
        }

        public void Draw( CommandManager command ) {
            Unk1.Draw( command );
            Time.Draw( command );
            Unk2.Draw( command );
            Unk3.Draw( command );
            Unk4.Draw( command );
            Unk5.Draw( command );
        }
    }
}
