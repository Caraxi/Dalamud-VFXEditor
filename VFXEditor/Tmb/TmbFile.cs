using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VFXEditor.Tmb {
    public class TmbFile {
        private List<TmbTmac> TMAC = new();

        private short TMDH_Unk1;
        private short TMDH_Unk2;
        private short TMDH_Unk3;

        public TmbFile(BinaryReader reader) {
            reader.ReadInt32(); // TMLB
            reader.ReadInt32(); // 0x0C
            reader.ReadInt32(); // entry count (not including TMBL)

            reader.ReadInt32(); // TMDH
            reader.ReadInt32(); // 0x10
            reader.ReadInt16(); // id
            TMDH_Unk1 = reader.ReadInt16();
            TMDH_Unk2 = reader.ReadInt16(); // ?
            TMDH_Unk3 = reader.ReadInt16(); // 3

            reader.ReadInt32(); // TMAL
            reader.ReadInt32(); // 0x10
            reader.ReadInt32(); // offset from [TMAL] + 8 to timeline
            var numTmac = reader.ReadInt32(); // Number of TMAC

            for (var i = 0; i < numTmac; i++) {
                TMAC.Add( new TmbTmac( reader ) );
            }
            foreach( var tmac in TMAC ) tmac.ReadTmtr( reader );
            foreach( var tmac in TMAC ) tmac.ReadEntries( reader );
        }

        public void Draw(string id) {
            if (ImGui.CollapsingHeader($"TMDH{id}")) {
                ImGui.Indent();

                ShortInput( $"TMDH Unknown #1{id}", ref TMDH_Unk1 );
                ShortInput( $"TMDH Unknown #2{id}", ref TMDH_Unk2 );
                ShortInput( $"TMDH Unknown #3{id}", ref TMDH_Unk3 );

                ImGui.Unindent();
            }

            var i = 0;
            foreach (var tmac in TMAC) {
                if( ImGui.CollapsingHeader( $"TMAC {i}{id}{i}" ) ) {
                    ImGui.Indent();
                    tmac.Draw( $"{id}{i}" );
                    ImGui.Unindent();
                }
                i++;
            }
        }

        public byte[] ToBytes() {
            var headerSize = 0x0C + 0x10 + 0x10 + TMAC.Count * 0x1C;
            var entriesSize = TMAC.Select(x => x.EntrySize).Sum();
            var extraSize = TMAC.Select(x => x.ExtraSize).Sum();
            var stringSize = TMAC.Select(x => x.StringSize).Sum();
            var entryCount = TMAC.Count + TMAC.Select( x => x.EntryCount ).Sum(); // include TMTR + entries
            var timelineSize = 2 * entryCount;

            // Calculate starting positions
            var entriesPos = headerSize;
            var extraPos = entriesPos + entriesSize;
            var timelinePos = extraPos + extraSize;
            var stringPos = timelinePos + timelineSize;
            var totalSize = headerSize + entriesSize + extraSize + timelineSize + stringSize;
            short id = 1;

            // write header
            var headerData = new byte[headerSize];
            using MemoryStream headerMs = new( headerData );
            using BinaryWriter headerWriter = new( headerMs );
            headerWriter.BaseStream.Seek( 0, SeekOrigin.Begin );

            WriteString( headerWriter, "TMLB" );
            headerWriter.Write( totalSize );
            headerWriter.Write( entryCount + 2 ); // + 2 for TMDH and TMAL

            WriteString( headerWriter, "TMDH" );
            headerWriter.Write( 0x10 );
            headerWriter.Write( id++ );
            headerWriter.Write( TMDH_Unk1 );
            headerWriter.Write( TMDH_Unk2 );
            headerWriter.Write( TMDH_Unk3 );

            WriteString( headerWriter, "TMAL" );
            headerWriter.Write( 0x10 );
            headerWriter.Write( timelinePos - (int)headerWriter.BaseStream.Position );
            headerWriter.Write( TMAC.Count );

            // write timeline
            var timelineData = new byte[timelineSize];
            using MemoryStream timelineMs = new( timelineData );
            using BinaryWriter timelineWriter = new( timelineMs );
            timelineWriter.BaseStream.Seek( 0, SeekOrigin.Begin );

            for (int i = 0; i < entryCount; i++ ) {
                timelineWriter.Write( ( short )(2 + i) );
            }

            // entries, extra, string
            var entriesData = new byte[entriesSize];
            using MemoryStream entriesMs = new( entriesData );
            using BinaryWriter entriesWriter = new( entriesMs );
            entriesWriter.BaseStream.Seek( 0, SeekOrigin.Begin );

            var extraData = new byte[extraSize];
            using MemoryStream extraMs = new( extraData );
            using BinaryWriter extraWriter = new( extraMs );
            extraWriter.BaseStream.Seek( 0, SeekOrigin.Begin );

            var stringData = new byte[stringSize];
            using MemoryStream stringMs = new( stringData );
            using BinaryWriter stringWriter = new( stringMs );
            stringWriter.BaseStream.Seek( 0, SeekOrigin.Begin );

            foreach( var tmac in TMAC ) tmac.Write( headerWriter, timelinePos, ref id );
            foreach( var tmac in TMAC ) tmac.WriteTmtr( entriesWriter, entriesPos, timelinePos, ref id );
            foreach( var tmac in TMAC ) tmac.WriteEntries( entriesWriter, entriesPos, extraWriter, extraPos, stringWriter, stringPos, timelinePos, ref id );

            var output = new byte[totalSize];
            Buffer.BlockCopy( headerData, 0, output, 0, headerData.Length );
            Buffer.BlockCopy( entriesData, 0, output, entriesPos, entriesData.Length );
            Buffer.BlockCopy( extraData, 0, output, extraPos, extraData.Length );
            Buffer.BlockCopy( timelineData, 0, output, timelinePos, timelineData.Length );
            Buffer.BlockCopy( stringData, 0, output, stringPos, stringData.Length );

            return output;
        }

        // ====================================

        public static string ReadString( BinaryReader input ) {
            List<byte> strBytes = new List<byte>();
            int b;
            while( ( b = input.ReadByte() ) != 0x00 )
                strBytes.Add( ( byte )b );
            return Encoding.ASCII.GetString( strBytes.ToArray() );
        }

        public static void WriteString( BinaryWriter writer, string str, bool writeNull = false) {
            writer.Write( Encoding.ASCII.GetBytes( str.Trim().Trim( '\0' ) ) );
            if( writeNull ) writer.Write( ( byte )0 );
        }

        public static bool ShortInput(string id, ref short value) {
            var val = ( int )value;
            if (ImGui.InputInt(id, ref val)) {
                value = ( short )val;
                return true;
            }
            return false;
        }
    }
}
