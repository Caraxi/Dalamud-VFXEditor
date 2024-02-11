using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using VfxEditor.Formats.MdlFormat.Mesh;
using VfxEditor.Formats.MdlFormat.Mesh.TerrainShadow;
using VfxEditor.Formats.MdlFormat.Utils;
using VfxEditor.Parsing;
using VfxEditor.Ui.Components;
using VfxEditor.Ui.Interfaces;

namespace VfxEditor.Formats.MdlFormat.Lod {
    public class MdlLod : IUiItem {
        public readonly MdlFile File;

        private readonly ParsedFloat ModelRange = new( "Model Range" );
        private readonly ParsedFloat TextureRange = new( "Texture Range" );

        private readonly ushort _MeshIndex;
        private readonly ushort _MeshCount;
        private readonly ushort _TerrainShadowMeshIndex;
        private readonly ushort _TerrainShadowMeshCount;

        // Just regular meshes
        private readonly ushort _WaterMeshIndex;
        private readonly ushort _WaterMeshCount;
        private readonly ushort _ShadowMeshIndex;
        private readonly ushort _ShadowMeshCount;
        private readonly ushort _VerticalFogMeshIndex;
        private readonly ushort _VerticalFogMeshCount;

        private readonly List<MdlMesh> Meshes = [];
        private readonly UiDropdown<MdlMesh> MeshView;

        private readonly List<MdlTerrainShadowMesh> TerrainShadows = [];
        private readonly UiDropdown<MdlTerrainShadowMesh> TerrainShadowView;

        private readonly List<MdlMesh> WaterMeshes = [];
        private readonly UiDropdown<MdlMesh> WaterMeshView;

        private readonly List<MdlMesh> ShadowMeshes = [];
        private readonly UiDropdown<MdlMesh> ShadowMeshView;

        private readonly List<MdlMesh> VerticalFogMeshes = [];
        private readonly UiDropdown<MdlMesh> VerticalFogMeshView;

        public MdlLod( MdlFile file, BinaryReader reader ) {
            File = file;

            _MeshIndex = reader.ReadUInt16();
            _MeshCount = reader.ReadUInt16();
            ModelRange.Read( reader );
            TextureRange.Read( reader );
            _WaterMeshIndex = reader.ReadUInt16();
            _WaterMeshCount = reader.ReadUInt16();
            _ShadowMeshIndex = reader.ReadUInt16();
            _ShadowMeshCount = reader.ReadUInt16();
            _TerrainShadowMeshIndex = reader.ReadUInt16();
            _TerrainShadowMeshCount = reader.ReadUInt16();
            _VerticalFogMeshIndex = reader.ReadUInt16();
            _VerticalFogMeshCount = reader.ReadUInt16();

            var edgeGeometrySize = reader.ReadUInt32();
            var edgeGeometryOffset = reader.ReadUInt32(); // equal to `vertexBufferOffset + vertexBufferSize` if `edgeGeometrySize = 0`
            var polygonCount = reader.ReadUInt32();
            var unknown = reader.ReadUInt32();
            reader.ReadUInt32(); // vertex buffer size, same as MdlFile
            reader.ReadUInt32(); // index buffer size
            reader.ReadUInt32(); // vertex data offset
            reader.ReadUInt32(); // index data offset

            // https://github.com/xivdev/Xande/blob/8fc75ce5192edcdabc4d55ac93ca0199eee18bc9/Xande.GltfImporter/MdlFileBuilder.cs#L558
            if( edgeGeometrySize != 0 || polygonCount != 0 || unknown != 0 ) {
                Dalamud.Error( $"LoD: {edgeGeometrySize}/{edgeGeometryOffset} {polygonCount} {unknown}" );
            }

            // ========= VIEWS ==============

            MeshView = new( "Mesh", Meshes );
            TerrainShadowView = new( "Terrain Shadow", TerrainShadows );
            WaterMeshView = new( "Water Mesh", WaterMeshes );
            ShadowMeshView = new( "Shadow Mesh", ShadowMeshes );
            VerticalFogMeshView = new( "Vertical Fog Mesh", VerticalFogMeshes );
        }

        public void Draw() {
            using var tabBar = ImRaii.TabBar( "Tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton );
            if( !tabBar ) return;

            using( var tab = ImRaii.TabItem( "Meshes" ) ) {
                if( tab ) MeshView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Terrain Shadows" ) ) {
                if( tab ) TerrainShadowView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Water" ) ) {
                if( tab ) WaterMeshView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Shadows" ) ) {
                if( tab ) ShadowMeshView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Vertical Fog" ) ) {
                if( tab ) VerticalFogMeshView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Parameters" ) ) {
                if( tab ) {
                    ModelRange.Draw();
                    TextureRange.Draw();
                }
            }
        }

        public void Populate( MdlReaderData data, BinaryReader reader, int lod ) {
            Meshes.AddRange( data.Meshes.GetRange( _MeshIndex, _MeshCount ) );
            foreach( var mesh in Meshes ) mesh.Populate( data, reader, lod );

            TerrainShadows.AddRange( data.TerrainShadowMeshes.GetRange( _TerrainShadowMeshIndex, _TerrainShadowMeshCount ) );
            foreach( var mesh in TerrainShadows ) mesh.Populate( data, reader, lod );

            WaterMeshes.AddRange( data.Meshes.GetRange( _WaterMeshIndex, _WaterMeshCount ) );
            foreach( var mesh in WaterMeshes ) mesh.Populate( data, reader, lod );

            ShadowMeshes.AddRange( data.Meshes.GetRange( _ShadowMeshIndex, _ShadowMeshCount ) );
            foreach( var mesh in ShadowMeshes ) mesh.Populate( data, reader, lod );

            VerticalFogMeshes.AddRange( data.Meshes.GetRange( _VerticalFogMeshIndex, _VerticalFogMeshCount ) );
            foreach( var mesh in VerticalFogMeshes ) mesh.Populate( data, reader, lod );
        }
    }
}
