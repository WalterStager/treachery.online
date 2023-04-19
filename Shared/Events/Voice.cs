﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Voice : GameEvent
    {
        #region Construction

        public Voice(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Voice()
        {
        }

        #endregion Construction

        #region Properties

        public bool Must { get; set; }

        [JsonIgnore]
        public bool MayNot => !Must;


        public TreacheryCardType Type { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!MayUseVoice(Game, Player)) return Message.Express("You cannot use Voice");
            if (!ValidTypes(Game).Contains(Type)) return Message.Express("Invalid use of Voice");

            return null;
        }

        public static IEnumerable<TreacheryCardType> ValidTypes(Game g)
        {
            var result = new List<TreacheryCardType>
            {
                TreacheryCardType.Mercenary,
                TreacheryCardType.Laser,
                TreacheryCardType.Poison,
                TreacheryCardType.Projectile,
                TreacheryCardType.Shield,
                TreacheryCardType.Antidote,
                TreacheryCardType.Useless
            };

            if (g.Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal))
            {
                result.Add(TreacheryCardType.ArtilleryStrike);
                result.Add(TreacheryCardType.Chemistry);
                result.Add(TreacheryCardType.PoisonTooth);
                result.Add(TreacheryCardType.WeirdingWay);
            }

            if (g.Applicable(Rule.ExpansionTreacheryCardsPBandSS))
            {
                result.Add(TreacheryCardType.ProjectileAndPoison);
                result.Add(TreacheryCardType.ShieldAndAntidote);
            }

            if (g.IsPlaying(Faction.White))
            {
                result.Add(TreacheryCardType.Rockmelter);
                result.Add(TreacheryCardType.MirrorWeapon);
            }

            if (!g.Applicable(Rule.BlueVoiceMustNameSpecialCards))
            {
                result.Add(TreacheryCardType.PoisonDefense);
                result.Add(TreacheryCardType.ProjectileDefense);
            }

            return result;
        }

        public static bool MayUseVoice(Game g, Player p)
        {
            bool disableWhenPrescienceIsUsed = g.Version >= 108 && g.CurrentPrescience != null;

            if (!disableWhenPrescienceIsUsed && g.CurrentVoice == null && g.CurrentBattle != null && g.CurrentBattle.IsInvolved(p))
            {
                if (p.Nexus == Faction.Blue && NexusPlayed.CanUseSecretAlly(g, p))
                {
                    return g.CurrentBattle.IsAggressorOrDefender(p);
                }
                else if (!g.Prevented(FactionAdvantage.BlueUsingVoice) && !g.IsOccupiedByFactionOrTheirAlly(World.Blue, g.CurrentBattle.OpponentOf(p)))
                {
                    if (p.Faction == Faction.Blue)
                    {
                        return g.CurrentBattle.IsInvolved(p);
                    }
                    else if (p.Ally == Faction.Blue && g.BlueAllowsUseOfVoice)
                    {
                        return g.CurrentBattle.IsAggressorOrDefender(p);
                    }
                }
            }

            return false;
        }

        public static bool IsVoicedBy(Game g, bool asWeapon, bool must, TreacheryCardType cardType, TreacheryCardType voicedType)
        {
            if (cardType == voicedType)
            {
                return true;
            }
            else
            {
                if (!g.Applicable(Rule.BlueVoiceMustNameSpecialCards))
                {
                    switch (voicedType)
                    {
                        case TreacheryCardType.PoisonDefense:
                            return
                                cardType == TreacheryCardType.Antidote ||
                                (g.Version >= 140 && cardType == TreacheryCardType.PortableAntidote) ||
                                (!asWeapon && cardType == TreacheryCardType.Chemistry) ||
                                cardType == TreacheryCardType.ShieldAndAntidote;

                        case TreacheryCardType.Poison:
                            return cardType == TreacheryCardType.PoisonTooth ||
                                cardType == TreacheryCardType.ProjectileAndPoison ||
                                !must && asWeapon && cardType == TreacheryCardType.Chemistry;

                        case TreacheryCardType.Shield:
                            return cardType == TreacheryCardType.ShieldAndAntidote;

                        case TreacheryCardType.ProjectileDefense:
                            return cardType == TreacheryCardType.Shield ||
                                cardType == TreacheryCardType.ShieldAndAntidote ||
                                !must && !asWeapon && cardType == TreacheryCardType.WeirdingWay;

                        case TreacheryCardType.Projectile:
                            return (asWeapon && cardType == TreacheryCardType.WeirdingWay) ||
                                cardType == TreacheryCardType.ProjectileAndPoison;
                    }
                }
                else
                {
                    switch (voicedType)
                    {
                        case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote;
                        case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield;
                    }
                }
            }

            return false;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentVoice = this;

            if (!IsPlaying(Faction.Blue))
            {
                Game.PlayNexusCard(Player, "Secret Ally", " to use Voice");
            }

            if (Game.CurrentBattle != null)
            {
                var opponent = Game.CurrentBattle.OpponentOf(Player);

                if (opponent != null)
                {
                    Game.RevokePlanIfNeeded(opponent.Faction);
                }
            }

            Log();
            Game.Stone(Milestone.Voice);
        }

        public override Message GetMessage()
        {
            if (Must)
            {
                return Message.Express(Initiator, " use Voice to force the use of ", Type);
            }
            else
            {
                return Message.Express(Initiator, " use Voice to deny the use of ", Type);
            }
        }

        #endregion Execution
    }
}
