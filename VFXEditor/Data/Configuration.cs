using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using ImGuiFileDialog;
using ImGuiNET;
using Dalamud.Logging;
using Dalamud.Configuration;

using VFXSelect;
using VFXEditor.Dialogs;

namespace VFXEditor {
    [Serializable]
    public class Configuration : GenericDialog, IPluginConfiguration {
        public int Version { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;

        public bool LogAllFiles = false;
        public bool LogDebug = false;
        public bool ShowVfxAssigned = false;

        public bool HideWithUI = true;
        public int SaveRecentLimit = 10;
        public bool OverlayLimit = true;
        public string WriteLocation = Path.Combine( new[] {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XIVLauncher",
            "pluginConfigs",
            "VFXEditor",
        } );
        public List<SelectResult> RecentSelects = new();
        public bool FilepickerImagePreview = true;

        public Configuration() : base("Settings") {
            Size = new Vector2( 300, 150 );
        }

        public void Setup() {
            Plugin.PluginInterface.UiBuilder.DisableUserUiHide = !HideWithUI;
            FileDialogManager.ImagePreview = FilepickerImagePreview;
            Directory.CreateDirectory( WriteLocation );
            PluginLog.Log( "Write location: " + WriteLocation );
        }

        public void AddRecent( SelectResult result ) {
            if( RecentSelects.Contains( result ) ) {
                RecentSelects.Remove( result ); // want to move it to the top
            }
            RecentSelects.Add( result );
            if( RecentSelects.Count > SaveRecentLimit ) {
                RecentSelects.RemoveRange( 0, RecentSelects.Count - SaveRecentLimit );
            }
            Save();
        }

        public void Save() {
            Plugin.PluginInterface.SavePluginConfig( this );
            Plugin.PluginInterface.UiBuilder.DisableUserUiHide = !HideWithUI;
        }

        public override void DrawBody() {
            ImGui.Text( "Changes to the temp file location may require a restart to take effect" );
            if( ImGui.InputText( "Temp file location", ref WriteLocation, 255 ) ) {
                Save();
            }

            if( ImGui.Checkbox( "Log all files##Settings", ref LogAllFiles ) ) {
                Save();
            }

            if( ImGui.Checkbox( "Log debug information##Settings", ref LogDebug ) ) {
                Save();
            }

            if( ImGui.Checkbox( "Show assigned parameter status##Settings", ref ShowVfxAssigned ) ) {
                Save();
            }

            if( ImGui.Checkbox( "Hide with UI##Settings", ref HideWithUI ) ) {
                Save();
            }

            if( ImGui.Checkbox( "File picker image preview##Settings", ref FilepickerImagePreview ) ) {
                FileDialogManager.ImagePreview = FilepickerImagePreview;
                Save();
            }

            ImGui.SetNextItemWidth( 135 );
            if( ImGui.InputInt( "Recent VFX Limit##Settings", ref SaveRecentLimit ) ) {
                SaveRecentLimit = Math.Max( SaveRecentLimit, 0 );
                Save();
            }

            if( ImGui.Checkbox( "Live Overlay Limit by Distance##Settings", ref OverlayLimit ) ) {
                Save();
            }
        }
    }
}