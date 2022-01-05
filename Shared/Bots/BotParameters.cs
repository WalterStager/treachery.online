﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BotParameters
    {
        public static float PenaltyForAttackingBots = 4.0f;

        public int Bidding_ResourcesToKeepWhenCardIsPerfect { get; set; }
        public int Bidding_ResourcesToKeepWhenCardIsntPerfect { get; set; }
        public int Bidding_PassingTreshold { get; set; }

        public bool Karma_SaveCardToUseSpecialKarmaAbility { get; set; }

        public int Shipment_MinimumOtherPlayersITrustToPreventAWin { get; set; }
        public float Shipment_DialShortageToAccept { get; set; }
        public int Shipment_MinimumResourcesToKeepForBattle { get; set; }
        public float Shipment_MaxEnemyForceStrengthFightingForSpice { get; set; }
        public int Shipment_ExpectedStormMovesWhenUnknown { get; set; }
        public int Shipment_DialForExtraForcesToShip { get; set; }

        public int Battle_MaximumUnsupportedForces { get; set; }
        public float Battle_MimimumChanceToAssumeEnemyHeroSurvives { get; set; }
        public float Battle_MimimumChanceToAssumeMyLeaderSurvives { get; set; }
        public float Battle_DialShortageThresholdForThrowing { get; set; }


        public static BotParameters BlackParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 4,
            Bidding_PassingTreshold = 0,
            Karma_SaveCardToUseSpecialKarmaAbility = true,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 2,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 3,
            Shipment_ExpectedStormMovesWhenUnknown = 3,
            Battle_MaximumUnsupportedForces = 6,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.6f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.4f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters GreenParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 4,
            Bidding_PassingTreshold = 2,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 2,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
            Shipment_ExpectedStormMovesWhenUnknown = 3,
            Battle_MaximumUnsupportedForces = 6,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.5f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters YellowParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 0,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 4,
            Bidding_PassingTreshold = 0,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 8,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 8,
            Shipment_ExpectedStormMovesWhenUnknown = 0,
            Battle_MaximumUnsupportedForces = 20,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
            Battle_DialShortageThresholdForThrowing = 4
        };

        public static BotParameters RedParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
            Bidding_PassingTreshold = 3,
            Karma_SaveCardToUseSpecialKarmaAbility = true,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 8,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
            Shipment_ExpectedStormMovesWhenUnknown = 6,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters OrangeParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
            Bidding_PassingTreshold = 0,
            Karma_SaveCardToUseSpecialKarmaAbility = true,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 6,
            Shipment_DialForExtraForcesToShip = 2,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
            Shipment_ExpectedStormMovesWhenUnknown = 6,
            Battle_MaximumUnsupportedForces = 0,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
            Battle_DialShortageThresholdForThrowing = 6
        };

        public static BotParameters BlueParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 7,
            Bidding_PassingTreshold = 0,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 2,
            Shipment_MinimumResourcesToKeepForBattle = 3,
            Shipment_DialForExtraForcesToShip = 2,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
            Shipment_ExpectedStormMovesWhenUnknown = 6,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.55f,
            Battle_DialShortageThresholdForThrowing = 6
        };

        public static BotParameters GreyParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
            Bidding_PassingTreshold = 4,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 4,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
            Shipment_ExpectedStormMovesWhenUnknown = 5,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.6f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.4f,
            Battle_DialShortageThresholdForThrowing = 4
        };

        public static BotParameters PurpleParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
            Bidding_PassingTreshold = 3,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
            Shipment_DialShortageToAccept = 6,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 6,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 6,
            Shipment_ExpectedStormMovesWhenUnknown = 0,
            Battle_MaximumUnsupportedForces = 6,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.3f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.6f,
            Battle_DialShortageThresholdForThrowing = 6
        };

        public static BotParameters BrownParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 8,
            Bidding_PassingTreshold = 3,
            Karma_SaveCardToUseSpecialKarmaAbility = true,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 2,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 4,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 0,
            Shipment_ExpectedStormMovesWhenUnknown = 6,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.55f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters WhiteParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
            Bidding_PassingTreshold = 4,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 2,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
            Shipment_ExpectedStormMovesWhenUnknown = 5,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.4f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
            Battle_DialShortageThresholdForThrowing = 6
        };

        public static BotParameters PinkParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
            Bidding_PassingTreshold = 4,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 6,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
            Shipment_ExpectedStormMovesWhenUnknown = 5,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.5f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters CyanParameters = new BotParameters()
        {
            Bidding_ResourcesToKeepWhenCardIsPerfect = 1,
            Bidding_ResourcesToKeepWhenCardIsntPerfect = 5,
            Bidding_PassingTreshold = 4,
            Karma_SaveCardToUseSpecialKarmaAbility = false,
            Shipment_MinimumOtherPlayersITrustToPreventAWin = 4,
            Shipment_DialShortageToAccept = 4,
            Shipment_MinimumResourcesToKeepForBattle = 0,
            Shipment_DialForExtraForcesToShip = 6,
            Shipment_MaxEnemyForceStrengthFightingForSpice = 2,
            Shipment_ExpectedStormMovesWhenUnknown = 5,
            Battle_MaximumUnsupportedForces = 2,
            Battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.5f,
            Battle_MimimumChanceToAssumeMyLeaderSurvives = 0.5f,
            Battle_DialShortageThresholdForThrowing = 3
        };

        public static BotParameters GetDefaultParameters(Faction f)
        {
            var result = f switch
            {
                Faction.Black => BlackParameters.MemberwiseClone(),
                Faction.Blue => BlueParameters.MemberwiseClone(),
                Faction.Green => GreenParameters.MemberwiseClone(),
                Faction.Yellow => YellowParameters.MemberwiseClone(),
                Faction.Red => RedParameters.MemberwiseClone(),
                Faction.Orange => OrangeParameters.MemberwiseClone(),
                Faction.Grey => GreyParameters.MemberwiseClone(),
                Faction.Purple => PurpleParameters.MemberwiseClone(),
                Faction.Brown => BrownParameters.MemberwiseClone(),
                Faction.White => WhiteParameters.MemberwiseClone(),
                Faction.Pink => PinkParameters.MemberwiseClone(),
                Faction.Cyan => CyanParameters.MemberwiseClone(),
                _ => BlackParameters.MemberwiseClone()
            };

            return (BotParameters)result;
        }
    }
}
