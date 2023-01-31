﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Treachery.Shared
{
    public class NexusPlayed : GameEvent
    {
        public NexusPlayed(Game game) : base(game)
        {
        }

        public NexusPlayed()
        {
        }

        public Faction Faction { get; set; }

        public PrescienceAspect GreenPrescienceAspect { get; set; }

        public int PurpleForces { get; set; }

        public int PurpleSpecialForces { get; set; }

        public int _purpleHeroId = -1;

        [JsonIgnore]
        public IHero PurpleHero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_purpleHeroId);
            }
            set
            {
                _purpleHeroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public bool PurpleAssignSkill { get; set; } = false;
               

        public override Message Validate()
        {
            switch (Faction)
            {
                case Faction.None: return Message.Express("Invalid Nexus faction");
                    
                case Faction.Green:
                    if (GreenPrescienceAspect == PrescienceAspect.None) return Message.Express("Invalid battle plan element");
                    break;

                case Faction.Pink:
                    break;

                case Faction.Yellow:
                    break;

                case Faction.Grey:
                    break;

                case Faction.White:
                    break;

                case Faction.Orange:
                    break;

                case Faction.Purple:
                    if (PurpleHero != null && !ValidPurpleHeroes(Game, Player).Contains(PurpleHero)) return Message.Express("Invalid leader");
                    if (PurpleForces > ValidPurpleMaxAmount(Game, Player, false)) return Message.Express("You can't revive that many ", Player.Force);
                    if (PurpleSpecialForces > ValidPurpleMaxAmount(Game, Player, true)) return Message.Express("You can't revive that many ", Player.SpecialForce);
                    if (DeterminePurpleCost() > Player.Resources) return Message.Express("You can't pay that many");
                    if (PurpleForces + PurpleSpecialForces > 5) return Message.Express("You can't revive that many forces");
                    if (PurpleAssignSkill && PurpleHero == null) return Message.Express("You must revive a leader to assign a skill to");
                    if (PurpleAssignSkill && !Revival.MayAssignSkill(Game, Player, PurpleHero)) return Message.Express("You can't assign a skill to this leader");
                    break;
            }
            

            return null;
        }

        public static bool CanUseCunning(Player p) => p.Faction == p.Nexus;

        public static bool CanUseSecretAlly(Game g, Player p) => !g.IsPlaying(p.Nexus);

        public static bool CanUseBetrayal(Game g, Player p) => !(CanUseCunning(p) || CanUseSecretAlly(g, p));

        [JsonIgnore]
        public bool Cunning => Initiator == Faction;

        [JsonIgnore]
        public bool SecretAlly => !Game.IsPlaying(Faction);

        [JsonIgnore]
        public bool Betrayal => !(Cunning && SecretAlly);

        public static bool IsApplicable(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.NexusCards || g.CurrentPhaseIsUnInterruptable)
            {
                return false;
            }

            bool cunning = CanUseCunning(p);
            bool secretAlly = CanUseSecretAlly(g,p);
            bool betrayal = !(cunning || secretAlly);

            bool isCurrentlyFormulatingBattlePlan = g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p) && (g.DefenderBattleAction == null || g.AggressorBattleAction == null);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Green when cunning || secretAlly => isCurrentlyFormulatingBattlePlan,

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentPhase == Phase.Contemplate,

                Faction.Yellow when betrayal => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Yellow when cunning => g.CurrentMainPhase == MainPhase.Blow && g.MonsterAppearedInTerritoryWithoutForces,
                Faction.Yellow when secretAlly => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.Resurrection,

                Faction.Red when betrayal => g.CurrentMainPhase == MainPhase.Bidding || g.CurrentMainPhase == MainPhase.Battle && g.Applicable(Rule.RedSpecialForces) && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(Faction.Red),
                Faction.Red when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Orange when betrayal => g.RecentlyPaid != null && g.HasRecentPaymentFor(typeof(Shipment)),
                Faction.Orange when cunning => g.CurrentPhase == Phase.OrangeMove && !g.InOrangeCunningShipment,
                Faction.Orange when secretAlly => g.CurrentPhase == Phase.NonOrangeShip,

                Faction.Blue when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Blue when cunning => g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Blue when secretAlly => g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                Faction.Grey when betrayal => g.CurrentPhase == Phase.BeginningOfBidding || g.CurrentPhase > Phase.BeginningOfBidding && g.CurrentPhase < Phase.BiddingReport,
                Faction.Grey when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Purple when betrayal => g.CurrentPhase == Phase.Facedancing,
                Faction.Purple when cunning => true,
                Faction.Purple when secretAlly => g.CurrentPhase == Phase.Resurrection,

                _ => false
            } ;



        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play a Nexus card");
        }

        public int DeterminePurpleCost()
        {
            return DeterminePurpleCost(PurpleForces, PurpleSpecialForces);
        }

        public static int DeterminePurpleCost(int Forces, int SpecialForces)
        {
            return (Forces + SpecialForces);
        }

        public static int ValidPurpleMaxAmount(Game g, Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    if (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction))
                    {
                        return 0;
                    }
                    else
                    {
                        return Math.Min(p.SpecialForcesKilled, 1);
                    }
                }
                else
                {
                    return Math.Min(p.SpecialForcesKilled, 5);
                }
            }
            else
            {
                return Math.Min(p.ForcesKilled, 5);
            }
        }

        public static IEnumerable<IHero> ValidPurpleHeroes(Game game, Player player) => game.KilledHeroes(player);

    }

}
