using System;
using System.Collections.Generic;
using AVFXLib.Models;
using System.IO;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System.IO.Compression;
using VFXEditor.Data.Texture;
using VFXEditor.Data;
using Dalamud.Logging;

namespace VFXEditor.External {
    public static class TexTools {

        /*
         * TTMPL.mpl ->
         *  {
         *      "TTMPVersion":"1.0s",
         *      "Name":"Ultimate Manatrigger",
         *      "Author":"Gabster",
         *      "Version":"1.0.0",
         *      "Description":null,
         *      "ModPackPages":null,
         *      "SimpleModsList":[
         *          {
         *              "Name":"Ultimate Anarchy", // "Name":"ve0009.avfx","Category":"Raw File Copy"
         *              "Category":"Two Handed",
         *              "FullPath":"chara/weapon/w2501/obj/body/b0027/material/v0001/mt_w2501b0027_a.mtrl",
         *              "IsDefault":false,
         *              "ModOffset":0,
         *              "ModSize":768,
         *              "DatFile":"040000",
         *              "ModPackEntry":null
         *         }
         *     ]
         *  }
         */

        private struct TTMPL {
            public string TTMPVersion;
            public string Name;
            public string Author;
            public string Version;
#nullable enable
            public string? Description;
            public string? ModPackPages;
#nullable disable
            public TTMPL_Simple[] SimpleModsList;
        }

        private struct TTMPL_Simple {
            public string Name;
            public string Category;
            public string FullPath;
            public bool IsDefault;
            public int ModOffset;
            public int ModSize;
            public string DatFile;
#nullable enable
            public string? ModPackEntry;
#nullable disable
        }


        public static void Export( string name, string author, string version, string saveLocation, bool exportAll, bool exportTex ) {
            try {
                var simpleParts = new List<TTMPL_Simple>();
                byte[] newData;
                var ModOffset = 0;

                using( var ms = new MemoryStream() )
                using( var writer = new BinaryWriter( ms ) ) {
                    void AddMod( AVFXBase avfx, string _path ) {
                        if( !string.IsNullOrEmpty( _path ) && avfx != null ) {
                            var modData = SquishAVFX( avfx );
                            simpleParts.Add( CreateModResource( _path, ModOffset, modData.Length ) );
                            writer.Write( modData );
                            ModOffset += modData.Length;
                        }
                    }

                    void AddTex( TexReplace tex, string _path ) {
                        if( !string.IsNullOrEmpty( _path ) && !string.IsNullOrEmpty( tex.LocalPath ) ) {
                            using var file = File.Open( tex.LocalPath, FileMode.Open );
                            using var texReader = new BinaryReader( file );
                            using var texMs = new MemoryStream();
                            using var texWriter = new BinaryWriter( texMs );
                            texWriter.Write( CreateType2Data( texReader.ReadBytes( ( int )file.Length ) ) );
                            var modData = texMs.ToArray();
                            simpleParts.Add( CreateModResource( _path, ModOffset, modData.Length ) );
                            writer.Write( modData );
                            ModOffset += modData.Length;
                        }
                    }

                    if( exportAll ) {
                        foreach( var doc in DocumentManager.CurrentDocs ) {
                            AddMod( doc.Main?.AVFX, doc.Replace.Path );
                        }
                    }
                    else {
                        AddMod( DocumentManager.CurrentActiveDoc.Main?.AVFX, DocumentManager.CurrentActiveDoc.Replace.Path );
                    }

                    if( exportTex ) {
                        foreach( var entry in TextureManager.Manager.PathToTextureReplace ) {
                            AddTex( entry.Value, entry.Key );
                        }
                    }
                    newData = ms.ToArray();
                }

                var mod = new TTMPL {
                    TTMPVersion = "1.3s",
                    Name = name,
                    Author = author,
                    Version = version,
                    Description = null,
                    ModPackPages = null,
                    SimpleModsList = simpleParts.ToArray()
                };

                var saveDir = Path.GetDirectoryName( saveLocation );
                var tempDir = Path.Combine( saveDir, "VFXEDITOR_TEXTOOLS_TEMP" );
                Directory.CreateDirectory( tempDir );
                var mdpPath = Path.Combine( tempDir, "TTMPD.mpd" );
                var mplPath = Path.Combine( tempDir, "TTMPL.mpl" );
                var mplString = JsonConvert.SerializeObject( mod );
                File.WriteAllText( mplPath, mplString );
                File.WriteAllBytes( mdpPath, newData );

                if( File.Exists( saveLocation ) ) File.Delete( saveLocation );
                ZipFile.CreateFromDirectory( tempDir, saveLocation );
                Directory.Delete( tempDir, true );
                PluginLog.Log( "Exported To: " + saveLocation );
            }
            catch( Exception e ) {
                PluginLog.Error( e, "Could not export to TexTools" );
            }
        }

        private static TTMPL_Simple CreateModResource( string path, int modOffset, int modSize ) {
            var simple = new TTMPL_Simple();
            var split = path.Split( '/' );
            simple.Name = split[^1];
            simple.Category = "Raw File Import";
            simple.FullPath = path;
            simple.IsDefault = false;
            simple.ModOffset = modOffset;
            simple.ModSize = modSize;

            switch( split[0] ) {
                case "vfx":
                    simple.DatFile = "080000";
                    break;
                case "chara":
                    simple.DatFile = "040000";
                    break;
                case "bgcommon":
                    simple.DatFile = "010000";
                    break;
                case "bg":
                    simple.DatFile = "02";
                    if( split[1] == "ffxiv" ) {
                        simple.DatFile += "0000"; // ok, good to go
                    }
                    else { // like ex1      bg/ex1/03_abr_a2/dun/a2d1/texture/a2d1_b0_silv02_n.tex
                        var exNumber = split[1].Replace( "ex", "" ).PadLeft( 2, '0' );
                        var zoneNumber = split[2].Split( '_' )[0].PadLeft( 2, '0' );
                        simple.DatFile += exNumber;
                        simple.DatFile += zoneNumber;
                    }
                    break;
                default:
                    PluginLog.Error( "Invalid path! Could not find DatFile" );
                    break;
            }
            simple.ModPackEntry = null;
            return simple;
        }

        public static byte[] SquishAVFX( AVFXBase avfx ) {
            return CreateType2Data( avfx.ToAVFX().ToBytes() );
        }

        // https://github.com/TexTools/xivModdingFramework/blob/288478772146df085f0d661b09ce89acec6cf72a/xivModdingFramework/SqPack/FileTypes/Dat.cs#L584
        private static byte[] CreateType2Data( byte[] dataToCreate ) {
            var newData = new List<byte>();
            var headerData = new List<byte>();
            var dataBlocks = new List<byte>();
            // Header size is defaulted to 128, but may need to change if the data being imported is very large.
            headerData.AddRange( BitConverter.GetBytes( 128 ) );
            headerData.AddRange( BitConverter.GetBytes( 2 ) );
            headerData.AddRange( BitConverter.GetBytes( dataToCreate.Length ) );
            var dataOffset = 0;
            var totalCompSize = 0;
            var uncompressedLength = dataToCreate.Length;
            var partCount = ( int )Math.Ceiling( uncompressedLength / 16000f );
            headerData.AddRange( BitConverter.GetBytes( partCount ) );
            var remainder = uncompressedLength;
            using( var binaryReader = new BinaryReader( new MemoryStream( dataToCreate ) ) ) {
                binaryReader.BaseStream.Seek( 0, SeekOrigin.Begin );
                for( var i = 1; i <= partCount; i++ ) {
                    if( i == partCount ) {
                        var compressedData = Compressor( binaryReader.ReadBytes( remainder ) );
                        var padding = 128 - ( ( compressedData.Length + 16 ) % 128 );
                        dataBlocks.AddRange( BitConverter.GetBytes( 16 ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( 0 ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( compressedData.Length ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( remainder ) );
                        dataBlocks.AddRange( compressedData );
                        dataBlocks.AddRange( new byte[padding] );
                        headerData.AddRange( BitConverter.GetBytes( dataOffset ) );
                        headerData.AddRange( BitConverter.GetBytes( ( short )( ( compressedData.Length + 16 ) + padding ) ) );
                        headerData.AddRange( BitConverter.GetBytes( ( short )remainder ) );
                        totalCompSize = dataOffset + ( ( compressedData.Length + 16 ) + padding );
                    }
                    else {
                        var compressedData = Compressor( binaryReader.ReadBytes( 16000 ) );
                        var padding = 128 - ( ( compressedData.Length + 16 ) % 128 );
                        dataBlocks.AddRange( BitConverter.GetBytes( 16 ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( 0 ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( compressedData.Length ) );
                        dataBlocks.AddRange( BitConverter.GetBytes( 16000 ) );
                        dataBlocks.AddRange( compressedData );
                        dataBlocks.AddRange( new byte[padding] );
                        headerData.AddRange( BitConverter.GetBytes( dataOffset ) );
                        headerData.AddRange( BitConverter.GetBytes( ( short )( ( compressedData.Length + 16 ) + padding ) ) );
                        headerData.AddRange( BitConverter.GetBytes( ( short )16000 ) );
                        dataOffset += ( ( compressedData.Length + 16 ) + padding );
                        remainder -= 16000;
                    }
                }
            }
            headerData.InsertRange( 12, BitConverter.GetBytes( totalCompSize / 128 ) );
            headerData.InsertRange( 16, BitConverter.GetBytes( totalCompSize / 128 ) );
            var headerSize = headerData.Count;
            var rem = headerSize % 128;
            if( rem != 0 ) {
                headerSize += ( 128 - rem );
            }
            headerData.RemoveRange( 0, 4 );
            headerData.InsertRange( 0, BitConverter.GetBytes( headerSize ) );
            var headerPadding = rem == 0 ? 0 : 128 - rem;
            headerData.AddRange( new byte[headerPadding] );
            newData.AddRange( headerData );
            newData.AddRange( dataBlocks );
            return newData.ToArray();
        }

        // https://github.com/TexTools/xivModdingFramework/blob/288478772146df085f0d661b09ce89acec6cf72a/xivModdingFramework/Helpers/IOUtil.cs#L40
        private static byte[] Compressor( byte[] uncompressedBytes ) {
            using var uMemoryStream = new MemoryStream( uncompressedBytes );
            byte[] compbytes = null;
            using( var cMemoryStream = new MemoryStream() )
            using( var deflateStream = new DeflateStream( cMemoryStream, CompressionMode.Compress ) ) {
                uMemoryStream.CopyTo( deflateStream );
                deflateStream.Close();
                compbytes = cMemoryStream.ToArray();
            }
            return compbytes;
        }
    }
}
