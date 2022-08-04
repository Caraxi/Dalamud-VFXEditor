using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.IO;
using System.Numerics;

using VFXEditor.AVFX.VFX;
using VFXEditor.FileManager;
using VFXEditor.Helper;

namespace VFXEditor.AVFX {
    public partial class AVFXDocument : FileManagerDocument<AVFXFile, WorkspaceMetaAvfx> {
        private DateTime LastUpdate = DateTime.Now;

        public AVFXDocument( string writeLocation ) : base( writeLocation, "Vfx", "VFX" ) {
        }
        public AVFXDocument( string writeLocation, string localPath, SelectResult source, SelectResult replace ) : base( writeLocation, localPath, source, replace, "Vfx", "VFX" ) {
        }
        public AVFXDocument( string writeLocation, string localPath, WorkspaceMetaAvfx data ) : this( writeLocation, localPath, data.Source, data.Replace ) {
            CurrentFile.ReadRenamingMap( data.Renaming );
        }

        protected override void LoadLocal( string localPath ) {
            if( File.Exists( localPath ) ) {
                try {
                    using var br = new BinaryReader( File.Open( localPath, FileMode.Open ) );
                    CurrentFile = new AVFXFile( br );
                    UIHelper.OkNotification( "VFX file loaded" );
                }
                catch( Exception e ) {
                    PluginLog.Error(e, "Error Reading File", e );
                    UIHelper.ErrorNotification( "Error reading file" );
                }
            }
        }

        protected override void LoadGame( string gamePath ) {
            if( Plugin.DataManager.FileExists( gamePath ) ) {
                try {
                    var file = Plugin.DataManager.GetFile( gamePath );
                    using var ms = new MemoryStream( file.Data );
                    using var br = new BinaryReader( ms );
                    CurrentFile = new AVFXFile( br );
                    UIHelper.OkNotification( "VFX file loaded" );
                }
                catch( Exception e ) {
                    PluginLog.Error( e, "Error Reading File" );
                    UIHelper.ErrorNotification( "Error reading file" );
                }
            }
        }

        protected override void UpdateFile() {
            if( CurrentFile == null ) return;
            if( Plugin.Configuration?.LogDebug == true ) PluginLog.Log( "Wrote VFX file to {0}", WriteLocation );
            File.WriteAllBytes( WriteLocation, CurrentFile.ToBytes() );
        }

        protected override void Update() {
            if( ( DateTime.Now - LastUpdate ).TotalSeconds > 0.5 ) { // only allow updates every 1/2 second
                UpdateFile();
                Reload();
                Plugin.ResourceLoader.ReRender();
                LastUpdate = DateTime.Now;
            }
        }

        protected override void ExportRaw() {
            UIHelper.WriteBytesDialog( ".avfx", CurrentFile.ToBytes(), "avfx" );
        }

        protected override bool GetVerified() => CurrentFile.Verified;

        protected override void DrawBody() { }

        public override void Draw() {
            ImGui.Columns( 3, "MainInterfaceFileColumns", false );

            // ======== INPUT TEXT =========
            ImGui.SetColumnWidth( 0, 150 );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 2 );
            UIHelper.HelpMarker( "The source of the new VFX. For example, if you wanted to replace the Fire animation with that of Blizzard, Blizzard would be the loaded VFX" ); ImGui.SameLine();
            ImGui.Text( "Loaded VFX" );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            UIHelper.HelpMarker( "The VFX which is being replaced. For example, if you wanted to replace the Fire animation with that of Blizzard, Fire would be the replaced VFX" ); ImGui.SameLine();
            ImGui.Text( "VFX Being Replaced" );
            ImGui.NextColumn();

            // ======= SEARCH BARS =========
            ImGui.SetColumnWidth( 1, ImGui.GetWindowWidth() - 265 );
            ImGui.PushItemWidth( ImGui.GetColumnWidth() - 100 );

            DisplaySearchBars();

            ImGui.PopItemWidth();

            // ======= TEMPLATES =========
            ImGui.NextColumn();
            ImGui.SetColumnWidth( 3, 150 );

            if( ImGui.Button( $"Library", new Vector2( 80, 23 ) ) ) {
                AVFXManager.NodeLibrary.Show();
            }

            // ======= SPAWN + EYE =========
            var previewSpawn = Replace.Path;
            var spawnDisabled = string.IsNullOrEmpty( previewSpawn );
            if( !Plugin.SpawnExists() ) {
                if( spawnDisabled ) {
                    ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                }
                if( ImGui.Button( "Spawn", new Vector2( 50, 23 ) ) && !spawnDisabled ) {
                    ImGui.OpenPopup( "Spawn_Popup" );
                }
                if( spawnDisabled ) {
                    ImGui.PopStyleVar();
                    UIHelper.Tooltip( "Select both a loaded VFX and a VFX to replace in order to use the spawn function" );
                }
            }
            else {
                if( ImGui.Button( "Remove" ) ) Plugin.RemoveSpawnVfx();
            }
            if( ImGui.BeginPopup( "Spawn_Popup" ) ) {
                if( ImGui.Selectable( "On Ground" ) ) Plugin.SpawnOnGround( previewSpawn );
                if( ImGui.Selectable( "On Self" ) ) Plugin.SpawnOnSelf( previewSpawn );
                if( ImGui.Selectable( "On Taget" ) ) Plugin.SpawnOnTarget( previewSpawn );
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 6 );
            ImGui.PushFont( UiBuilder.IconFont );
            if( ImGui.Button( $"{( !Plugin.VfxTracker.Enabled ? ( char )FontAwesomeIcon.Eye : ( char )FontAwesomeIcon.Times )}###MainInterfaceFiles-MarkVfx", new Vector2( 28, 23 ) ) ) {
                Plugin.VfxTracker.Toggle();
                if( !Plugin.VfxTracker.Enabled ) {
                    Plugin.VfxTracker.Reset();
                    Plugin.PluginInterface.UiBuilder.DisableCutsceneUiHide = false;
                }
                else {
                    Plugin.PluginInterface.UiBuilder.DisableCutsceneUiHide = true;
                }
            }
            ImGui.PopFont();
            UIHelper.Tooltip( "VFX path overlay" );

            ImGui.Columns( 1 );
            ImGui.Separator();

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( CurrentFile == null ) DisplayBeginHelpText();
            else {
                DisplayFileControls();

                ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
                CurrentFile.Draw();
            }
        }

        public void Import( string path ) {
            if( CurrentFile != null && File.Exists( path ) ) CurrentFile.Import( path );
        }

        public void ShowExportDialog( UINode node ) => CurrentFile.ShowExportDialog( node );

        protected override void SourceShow() => AVFXManager.SourceSelect.Show();

        protected override void ReplaceShow() => AVFXManager.ReplaceSelect.Show();

        public void OpenTemplate( string path ) {
            var newResult = new SelectResult {
                DisplayString = "[NEW]",
                Type = SelectResultType.Local,
                Path = Path.Combine( Plugin.RootLocation, "Files", path )
            };
            SetSource( newResult );
        }
    }
}
