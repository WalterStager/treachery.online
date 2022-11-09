﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        protected virtual AllianceByTerror DetermineAllianceByTerror()
        {
            var cyan = Game.GetPlayer(Faction.Cyan);
            var accept = !HasAlly || PlayerStanding(cyan) > 0.75f * PlayerStanding(AlliedPlayer);
            return new AllianceByTerror(Game) { Initiator = Faction, Passed = accept };
        }

        protected virtual AllianceOffered DetermineAllianceOffered()
        {
            int nrOfPlayers = Game.Players.Count();
            int nrOfBots = Game.Players.Count(p => p.IsBot);
            int nrOfUnalliedBots = Game.Players.Count(p => p.IsBot && p.Ally == Faction.None);

            if (nrOfPlayers == nrOfBots && nrOfUnalliedBots > 2)
            {
                int nrOfUnalliedHumans = Game.Players.Count(p => !(p.IsBot) && p.Ally == Faction.None);

                var offer = Game.CurrentAllianceOffers.Where(offer => offer.Target == Faction && !offer.Player.IsBot).HighestOrDefault(offer => PlayerStanding(offer.Player));
                if (offer == null) offer = Game.CurrentAllianceOffers.Where(offer => offer.Target == Faction).HighestOrDefault(offer => PlayerStanding(offer.Player));

                if (offer != null)
                {
                    return new AllianceOffered(Game) { Initiator = Faction, Target = offer.Initiator };
                }
                else if (
                    nrOfPlayers > 2 &&
                    !Game.Applicable(Rule.BotsCannotAlly) &&
                    (nrOfUnalliedHumans == 0 || nrOfUnalliedHumans < nrOfUnalliedBots - 1) &&
                    !Game.CurrentAllianceOffers.Any(o => o.Initiator == Faction && Game.GetPlayer(o.Target).Ally == Faction.None))
                {
                    var mostInterestingOpponentBotWithoutAlly = Game.Players.Where(p => p != this && p.IsBot && p.Ally == Faction.None).HighestOrDefault(p => PlayerStanding(p));

                    if (mostInterestingOpponentBotWithoutAlly != null)
                    {
                        return new AllianceOffered(Game) { Initiator = Faction, Target = mostInterestingOpponentBotWithoutAlly.Faction };
                    }
                }
            }

            return null;
        }

        private int PlayerStanding(Player p) =>
                    2 * p.TreacheryCards.Count() +
                    p.Resources +
                    6 * p.LocationsWithAnyForces.Count(l => l.Territory.IsStronghold) +
                    p.ForcesOnPlanet.Sum(b => b.Value.TotalAmountOfForces) +
                    p.ForcesInReserve +
                    2 * p.SpecialForcesInReserve +
                    2 * p.Leaders.Count(l => Game.IsAlive(l));

        protected virtual AllianceBroken DetermineAllianceBroken()
        {
            var offer = Game.CurrentAllianceOffers.Where(offer => offer.Target == Faction && !offer.Player.IsBot).HighestOrDefault(offer => PlayerStanding(offer.Player));

            if (offer != null && PlayerStanding(offer.Player) > PlayerStanding(AlliedPlayer))
            {
                return new AllianceBroken(Game) { Initiator = Faction };
            }

            return null;
        }

        protected virtual NexusCardDrawn DetermineNexusCardDrawn()
        {
            if (NexusCardDrawn.MayDraw(this))
            {
                return new NexusCardDrawn(Game) { Initiator = Faction, Passed = false };
            }
            else
            {
                return new NexusCardDrawn(Game) { Initiator = Faction, Passed = true };
            }
        }

        protected virtual AllyPermission DetermineAlliancePermissions()
        {
            if (Ally == Faction.None) return null;

            return Faction switch
            {

                Faction.Yellow => DetermineAlliancePermissions_Yellow(),
                Faction.Green => DetermineAlliancePermissions_Green(),
                Faction.Black => DetermineAlliancePermissions_Black(),
                Faction.Red => DetermineAlliancePermissions_Red(),
                Faction.Orange => DetermineAlliancePermissions_Orange(),
                Faction.Blue => DetermineAlliancePermissions_Blue(),
                Faction.Grey => DetermineAlliancePermissions_Grey(),
                Faction.Purple => DetermineAlliancePermissions_Purple(),
                Faction.White => DetermineAlliancePermissions_White(),
                Faction.Brown => DetermineAlliancePermissions_Brown(),
                _ => null
            };
        }

        protected AllyPermission DetermineAlliancePermissions_Black()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = SpecialKarmaPowerUsed ? TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma) : null;

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Blue()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.BlueAllyMayUseVoice || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources, BlueAllowsUseOfVoice = true };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }
        protected AllyPermission DetermineAlliancePermissions_Green()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.GreenSharesPrescience || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources, GreenSharesPrescience = true };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Grey()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.GreyAllyMayReplaceCards || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources, AllyMayReplaceCards = true };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Brown()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove || Game.CurrentMainPhase == MainPhase.Battle)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase >= MainPhase.Battle)
                {
                    allowedResources = Resources;
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_White()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.WhiteAllyMayUseNoField || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources, WhiteAllowsUseOfNoField = true };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Orange()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.OrangeAllyMayShipAsGuild || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, AllyMayShipAsOrange = true, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Purple()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.PurpleAllyMayReviveAsPurple || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, AllyMayReviveAsPurple = true, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Red()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.Bidding)
                {
                    allowedResources = Resources;
                }
                else if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                {
                    allowedResources = Math.Max(Resources - 5, 0);
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (Game.RedWillPayForExtraRevival != 3 || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, RedWillPayForExtraRevival = 3, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected AllyPermission DetermineAlliancePermissions_Yellow()
        {
            if (Game.CurrentMainPhase == MainPhase.Bidding || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
            {
                var karmaCard = TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);

                int allowedResources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove || TreacheryCards.Count() == 4)
                {
                    allowedResources = Resources;
                }
                else
                {
                    allowedResources = Math.Max(Resources - 10, 0);
                }

                if (!Game.YellowAllowsThreeFreeRevivals || !Game.YellowSharesPrescience || !Game.YellowWillProtectFromShaiHulud || Game.GetPermittedUseOfAllySpice(Ally) != allowedResources || Game.GetPermittedUseOfAllyKarma(Ally) != karmaCard)
                {
                    LogInfo("Allowing use of resources: {0} and Karama card: {1}", allowedResources, karmaCard);
                    var permission = new AllyPermission(Game) { Initiator = Faction, PermittedKarmaCard = karmaCard, PermittedResources = allowedResources, YellowAllowsThreeFreeRevivals = true, YellowSharesPrescience = true, YellowWillProtectFromMonster = true };
                    LogInfo(permission.GetMessage());
                    return permission;
                }
            }

            return null;
        }

        protected virtual CardTraded DetermineCardTraded()
        {
            if (Game.CurrentCardTradeOffer != null)
            {
                CardTraded result;

                if (Game.CurrentCardTradeOffer.RequestedCard != null)
                {
                    result = new CardTraded(Game) { Initiator = Faction, Target = Game.CurrentCardTradeOffer.Initiator, Card = Game.CurrentCardTradeOffer.RequestedCard, RequestedCard = null };
                }
                else
                {
                    result = new CardTraded(Game) { Initiator = Faction, Target = Game.CurrentCardTradeOffer.Initiator, Card = TreacheryCards.OrderBy(c => CardQuality(c)).FirstOrDefault(), RequestedCard = null };
                }

                return result;
            }

            return null;
        }

    }

}
