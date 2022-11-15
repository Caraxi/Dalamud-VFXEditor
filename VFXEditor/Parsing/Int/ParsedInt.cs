using ImGuiNET;
using System.IO;

namespace VfxEditor.Parsing {
    public class ParsedInt : ParsedBase {
        public readonly string Name;
        private int Size;
        public int Value = 0;

        public ParsedInt( string name, int size = 4 ) {
            Name = name;
            Size = size;
        }

        public override void Read( BinaryReader reader ) => Read( reader, Size );

        public override void Read( BinaryReader reader, int size ) {
            Size = size;
            Value = Size switch {
                4 => reader.ReadInt32(),
                2 => reader.ReadInt16(),
                1 => reader.ReadByte(),
                _ => reader.ReadByte()
            };
        }

        public override void Write( BinaryWriter writer ) {
            if( Size == 4 ) writer.Write( Value );
            else if( Size == 2 ) writer.Write( ( short )Value );
            else writer.Write( ( byte )Value );
        }

        public override void Draw( string id, CommandManager manager ) {
            // Copy/Paste
            var copy = manager.Copy;
            if( copy.IsCopying ) copy.Ints[Name] = Value;
            if( copy.IsPasting && copy.Ints.TryGetValue( Name, out var val ) ) {
                copy.PasteCommand.Add( new ParsedIntCommand( this, val ) );
            }

            var value = Value;
            if( ImGui.InputInt( Name + id, ref value ) ) {
                manager.Add( new ParsedIntCommand( this, value ) );
            }
        }
    }
}
