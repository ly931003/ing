﻿using Sakuno.KanColle.Amatsukaze.Game.Models.Battle.Phases;
using Sakuno.KanColle.Amatsukaze.Game.Models.Raw.Battle;
using Sakuno.KanColle.Amatsukaze.Game.Parsers;
using System.Collections.Generic;

namespace Sakuno.KanColle.Amatsukaze.Game.Models.Battle.Stages
{
    class CombinedFleetCTFDayNormalStage : CombinedFleetDay
    {
        public override BattleStageType Type => BattleStageType.Day;

        public override IList<BattlePhase> Phases => new BattlePhase[]
        {
            LandBaseJetAircraftAerialSupport,
            JetAircraftAerialCombat,
            LandBaseAerialSupport,
            AerialCombat,
            SupportingFire,
            OpeningASW,
            OpeningTorpedo,

            ShellingFirstRound,
            ClosingTorpedo,

            ShellingSecondRound,
            ShellingThirdRound,
        };

        internal protected CombinedFleetCTFDayNormalStage(BattleInfo rpOwner, ApiInfo rpInfo) : base(rpOwner)
        {
            var rRawData = rpInfo.Data as RawEnemyCombinedFleetDay;

            LandBaseJetAircraftAerialSupport = new LandBaseJetAircraftAerialSupport(this, rRawData.LandBaseJetAircraftAerialSupport);
            JetAircraftAerialCombat = new AerialCombatPhase(this, rRawData.JetAircraftAerialCombat);
            LandBaseAerialSupport = new LandBaseAerialSupportPhase(this, rRawData.LandBaseAerialSupport);
            AerialCombat = new AerialCombatPhase(this, rRawData.AerialCombat);
            SupportingFire = new SupportingFirePhase(this, rRawData.SupportingFire);
            OpeningASW = new ShellingPhase(this, rRawData.OpeningASW);
            OpeningTorpedo = new TorpedoSalvoPhase(this, rRawData.OpeningTorpedoSalvo);

            ShellingFirstRound = new ShellingPhase(this, rRawData.ShellingFirstRound);
            ClosingTorpedo = new TorpedoSalvoPhase(this, rRawData.ClosingTorpedoSalvo);

            ShellingSecondRound = new ShellingPhase(this, rRawData.ShellingSecondRound);
            ShellingThirdRound = new ShellingPhase(this, rRawData.ShellingThirdRound);
        }
    }
}
