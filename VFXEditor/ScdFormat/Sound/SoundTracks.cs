using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using VfxEditor.FileManager;
using VfxEditor.Parsing;
using VfxEditor.Utils;

namespace VfxEditor.ScdFormat {
    public class SoundTracks {
        public List<SoundTrackInfo> Tracks = new();

        public void Read( BinaryReader reader, byte trackCount ) {
            for( var i = 0; i < trackCount; i++ ) {
                var newTrack = new SoundTrackInfo();
                newTrack.Read( reader );
                Tracks.Add( newTrack );
            }
        }

        public void Write( BinaryWriter writer ) {
            Tracks.ForEach( x => x.Write( writer ) );
        }

        public void Draw( string id ) {
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 3 );

            for( var idx = 0; idx < Tracks.Count; idx++ ) {
                if( ImGui.CollapsingHeader( $"Track #{idx}{id}", ImGuiTreeNodeFlags.DefaultOpen ) ) {
                    ImGui.Indent();

                    if( UiUtils.RemoveButton( $"Delete{id}{idx}", true ) ) { // REMOVE
                        CommandManager.Scd.Add( new GenericRemoveCommand<SoundTrackInfo>( Tracks, Tracks[idx] ) );
                        ImGui.Unindent(); break;
                    }

                    Tracks[idx].Draw( $"{id}{idx}" );
                    ImGui.Unindent();
                    ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 3 );
                }
            }

            if( ImGui.Button( $"+ New{id}" ) ) { // NEW
                CommandManager.Scd.Add( new GenericAddCommand<SoundTrackInfo>( Tracks, new SoundTrackInfo() ) );
            }
        }
    }

    public class SoundTrackInfo {
        public readonly ParsedShort TrackIdx = new( "Track Index" );
        public readonly ParsedShort AudioIdx = new( "Audio Index" );

        public void Read( BinaryReader reader ) {
            TrackIdx.Read( reader );
            AudioIdx.Read( reader );
        }

        public void Write( BinaryWriter writer ) {
            TrackIdx.Write( writer );
            AudioIdx.Write( writer );
        }

        public void Draw( string parentId ) {
            TrackIdx.Draw( parentId, CommandManager.Scd );
            AudioIdx.Draw( parentId, CommandManager.Scd );
        }
    }
}
