using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using VfxEditor.Select2.Scd.BgmQuest;
using VfxEditor.Select2.Shared.Zone;

namespace VfxEditor.Select2.Scd.Zone {
    public class ZoneRowSelected {
        public readonly BgmSituationStruct Situation;
        public readonly Dictionary<string, BgmSituationStruct> Quests = new();

        public ZoneRowSelected( ZoneRow zone ) {
            Situation = BgmQuestRowSelected.GetBgmSituation( zone.BgmId );

            if( zone.BgmId <= 50000 ) return;

            foreach( var bgmSwitch in Plugin.DataManager.GetExcelSheet<BGMSwitch>().Where( x => x.RowId == zone.BgmId ) ) {
                var questName = bgmSwitch.Quest.Value?.Name.ToString();
                var situation = BgmQuestRowSelected.GetBgmSituation( bgmSwitch.BGM );
                Quests[string.IsNullOrEmpty( questName ) ? zone.Name : questName] = situation;
            }
        }
    }
}
