using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VfxEditor.AvfxFormat2.Enums;

namespace VfxEditor.AvfxFormat2 {
    public class AvfxParticleDataLaser : AvfxData {
        public readonly AvfxCurve Length = new( "Length", "Len" );
        public readonly AvfxCurve Width = new( "Width", "Wdt" );

        public AvfxParticleDataLaser() : base() {
            Parsed = new() {
                Length,
                Width
            };

            DisplayTabs.Add( Width );
            DisplayTabs.Add( Length );
        }
    }
}