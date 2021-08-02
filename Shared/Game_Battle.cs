﻿/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public BattleInitiated CurrentBattle { get; private set; } = null;
        public Battle AggressorBattleAction { get; private set; } = null;
        public TreacheryCalled AggressorTraitorAction { get; private set; }
        public Battle DefenderBattleAction { get; private set; } = null;
        public TreacheryCalled DefenderTraitorAction { get; private set; }

        public Faction BattleWinner { get; private set; }
        public Faction BattleLoser { get; private set; }
        public int GreySpecialForceLossesToTake { get; private set; }

        public int NrOfBattlesFought = 0;

        private void EnterBattlePhase()
        {
            MainPhaseStart(MainPhase.Battle);
            NrOfBattlesFought = 0;
            if (KarmaHmsMovesLeft != 2) KarmaHmsMovesLeft = 0;
            AllowMovePhaseFactionAdvantages();
            ResetBattle();
            Enter(NextPlayerToBattle == null, EnterSpiceCollectionPhase, Version >= 107, Phase.BeginningOfBattle, Phase.BattlePhase);
        }

        private void AllowMovePhaseFactionAdvantages()
        {
            Allow(FactionAdvantage.BlueAccompanies);
            Allow(FactionAdvantage.GreenSpiceBlowPrescience);
            Allow(FactionAdvantage.BlueNoFlipOnIntrusion);
            Allow(FactionAdvantage.YellowExtraMove);
            Allow(FactionAdvantage.YellowProtectedFromStorm);
            Allow(FactionAdvantage.OrangeDetermineMoveMoment);
            Allow(FactionAdvantage.OrangeSpecialShipments);
            Allow(FactionAdvantage.OrangeShipmentsDiscount);
            Allow(FactionAdvantage.OrangeShipmentsDiscountAlly);
            Allow(FactionAdvantage.PurpleRevivalDiscount);
            Allow(FactionAdvantage.PurpleRevivalDiscountAlly);
            Allow(FactionAdvantage.GreyCyborgExtraMove);
        }

        public Player NextPlayerToBattle
        {
            get
            {
                BattleSequence.Start(false, 1);

                for (int i = 0; i < Players.Count; i++)
                {
                    var playerToCheck = BattleSequence.CurrentPlayer;
                    if (Battle.MustFight(this, playerToCheck))
                    {
                        return playerToCheck;
                    }

                    BattleSequence.NextPlayer(false);
                }

                return null;
            }
        }


        public void HandleEvent(BattleInitiated b)
        {
            CurrentReport = new Report(MainPhase.Battle);
            CurrentBattle = b;
            ChosenHMSAdvantage = StrongholdAdvantage.None;
            CurrentRetreat = null;
            NrOfBattlesFought++;
            CurrentReport.Add(b.GetMessage());

            if (!Battle.ValidBattleHeroes(this, b.Defender).Any())
            {
                CurrentReport.Add(b.Target, "{0} don't have leaders available for this battle.", b.Target);
            }

            if (!Battle.ValidBattleHeroes(this, b.Player).Any())
            {
                CurrentReport.Add(b.Initiator, "{0} don't have leaders available for this battle.", b.Initiator);
            }

            HasBattleWheel.Clear();
            HasBattleWheel.Add(b.Initiator);
            HasBattleWheel.Add(b.Target);
        }

        public Voice CurrentVoice { get; private set; } = null;
        public void HandleEvent(Voice e)
        {
            CurrentVoice = e;

            if (CurrentBattle != null)
            {

                var opponent = CurrentBattle.OpponentOf(e.Player);

                if (opponent != null)
                {
                    RevokePlanIfNeeded(opponent.Faction);
                }
            }

            CurrentReport.Add(e);
            RecentMilestones.Add(Milestone.Voice);
        }

        public Prescience CurrentPrescience { get; private set; } = null;
        public void HandleEvent(Prescience e)
        {
            CurrentPrescience = e;
            CurrentReport.Add(e);
            RecentMilestones.Add(Milestone.Prescience);
        }

        public void HandleEvent(Battle b)
        {
            if (b.Initiator == CurrentBattle.Initiator)
            {
                AggressorBattleAction = b;
            }
            else if (b.Initiator == CurrentBattle.Target)
            {
                DefenderBattleAction = b;
            }

            if (AggressorBattleAction != null && DefenderBattleAction != null)
            {
                RevealCurrentNoField(AggressorBattleAction.Player, CurrentBattle.Territory);
                RevealCurrentNoField(DefenderBattleAction.Player, CurrentBattle.Territory);

                ActivateSmuggler(AggressorBattleAction.Player, AggressorBattleAction.Hero, DefenderBattleAction.Hero, CurrentBattle.Territory);
                ActivateSmuggler(DefenderBattleAction.Player, DefenderBattleAction.Hero, AggressorBattleAction.Hero, CurrentBattle.Territory);

                CurrentReport.Add(AggressorBattleAction.GetBattlePlanMessage());
                CurrentReport.Add(DefenderBattleAction.GetBattlePlanMessage());
                RegisterKnownCards(AggressorBattleAction);
                RegisterKnownCards(DefenderBattleAction);
                Enter(Phase.CallTraitorOrPass);

                if (Version >= 69)
                {
                    if (CurrentBattle.Initiator == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
                    {
                        AggressorTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
                    }
                    else if (CurrentBattle.Target == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
                    {
                        DefenderTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
                    }
                }
            }
        }

        private void ActivateSmuggler(Player player, IHero hero, IHero opponentHero, Territory territory)
        {
            if (SkilledAs(hero, LeaderSkill.Smuggler))
            {
                var locationWithResources = territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));
                if (locationWithResources != null)
                {
                    int collected = Math.Min(ResourcesOnPlanet[locationWithResources], hero.ValueInCombatAgainst(opponentHero));
                    if (collected > 0)
                    {
                        CurrentReport.Add(player.Faction, "{0} {1} collects {2} {3} from {4}", player.Faction, LeaderSkill.Smuggler, collected, Concept.Resource, territory);
                        ChangeSpiceOnPlanet(locationWithResources, -collected);
                        player.Resources += collected;
                    }
                }
            }

        }

        private void RegisterKnownCards(Battle battle)
        {
            RegisterKnown(battle.Weapon);
            RegisterKnown(battle.Defense);
        }

        private void DiscardOneTimeCardsUsedInBattle(TreacheryCalled aggressorCall, TreacheryCalled defenderCall)
        {
            bool aggressorKeepsCards = Version >= 73 && aggressorCall.TraitorCalled && !defenderCall.TraitorCalled;
            if (!aggressorKeepsCards)
            {
                DiscardOneTimeCards(AggressorBattleAction);
            }

            bool defenderKeepsCards = Version >= 73 && defenderCall.TraitorCalled && !aggressorCall.TraitorCalled;
            if (!defenderKeepsCards)
            {
                DiscardOneTimeCards(DefenderBattleAction);
            }
        }

        private void DiscardOneTimeCards(Battle plan)
        {
            if (plan.Hero != null && plan.Hero is TreacheryCard)
            {
                Discard(plan.Hero as TreacheryCard);
            }

            if (plan.Weapon != null && !(plan.Weapon.IsWeapon || plan.Weapon.IsDefense || plan.Weapon.IsUseless))
            {
                Discard(plan.Weapon);
            }
            else if (CurrentDiplomacy != null && plan.Initiator == CurrentDiplomacy.Initiator && plan.Weapon == CurrentDiplomacy.Card)
            {
                Discard(plan.Weapon);
            }
            else if (plan.Weapon != null && (
                plan.Weapon.IsArtillery ||
                plan.Weapon.IsRockmelter ||
                plan.Weapon.IsMirrorWeapon ||
                plan.Weapon.IsPoisonTooth && !PoisonToothCancelled
                ))
            {
                Discard(plan.Weapon);
            }

            if (plan.Defense != null && plan.Defense.IsPortableAntidote)
            {
                Discard(plan.Defense);
            }
            else if (CurrentDiplomacy != null && plan.Initiator == CurrentDiplomacy.Initiator && plan.Defense == CurrentDiplomacy.Card)
            {
                Discard(plan.Defense);
            }
        }

        public void HandleEvent(BattleRevision e)
        {
            if (CurrentBattle != null)
            {
                if (e.By(CurrentBattle.Initiator))
                {
                    AggressorBattleAction = null;
                }
                else if (e.By(CurrentBattle.Target))
                {
                    DefenderBattleAction = null;
                }
            }
        }

        private bool PoisonToothCancelled = false;
        public void HandleEvent(PoisonToothCancelled e)
        {
            PoisonToothCancelled = true;
            CurrentReport.Add(e);
        }

        public void HandleEvent(PortableAntidoteUsed e)
        {
            CurrentReport.Add(e);
            var plan = CurrentBattle.PlanOf(e.Initiator);
            plan.Defense = e.Player.Card(TreacheryCardType.PortableAntidote);
        }

        public Diplomacy CurrentDiplomacy { get; private set; }
        public void HandleEvent(Diplomacy e)
        {
            CurrentReport.Add(e);
            CurrentDiplomacy = e;
        }

        public void HandleEvent(ResidualPlayed e)
        {
            Discard(e.Player, TreacheryCardType.Residual);

            var opponent = CurrentBattle.OpponentOf(e.Initiator);
            var leadersToKill = new Deck<IHero>(Battle.ValidBattleHeroes(this, opponent), Random);
            leadersToKill.Shuffle();

            if (!leadersToKill.IsEmpty)
            {
                var toKill = leadersToKill.Draw();
                KillHero(toKill);
                CurrentReport.Add(e);
            }
            else
            {
                CurrentReport.Add(opponent.Faction, "{0} have no available leaders to kill");
            }
        }

        public IList<TreacheryCard> AuditedCards;
        public void HandleEvent(TreacheryCalled e)
        {
            if (AggressorBattleAction.By(e.Initiator) || (e.By(Faction.Black) && Allies(AggressorBattleAction.Initiator, Faction.Black)))
            {
                AggressorTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Add(e);
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(DefenderBattleAction.Hero);
                }
            }

            if (DefenderBattleAction.By(e.Initiator) || (e.By(Faction.Black) && Allies(DefenderBattleAction.Initiator, Faction.Black)))
            {
                DefenderTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Add(e);
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(AggressorBattleAction.Hero);
                }
            }

            if (AggressorTraitorAction != null && DefenderTraitorAction != null)
            {
                Enter(AggressorBattleAction.HasRockMelter || DefenderBattleAction.HasRockMelter, Phase.MeltingRock, HandleRevealedBattlePlans); 
            }
        }

        private bool RockMelterWasUsedToKill { get; set; }
        public void HandleEvent(RockWasMelted e)
        {
            CurrentReport.Add(e);
            RockMelterWasUsedToKill = e.Kill;
            HandleRevealedBattlePlans();
        }

        private void HandleRevealedBattlePlans()
        {
            ResolveEffectsOfOwnedStrongholds(AggressorBattleAction, DefenderBattleAction);
            ResolveEffectsOfOwnedStrongholds(DefenderBattleAction, AggressorBattleAction);

            DiscardOneTimeCardsUsedInBattle(AggressorTraitorAction, DefenderTraitorAction);
            ResolveBattle(CurrentBattle, AggressorBattleAction, DefenderBattleAction, AggressorTraitorAction, DefenderTraitorAction);

            if (AggressorBattleAction.Initiator == BattleWinner) ActivateDeciphererIfApplicable(AggressorBattleAction);
            if (DefenderBattleAction.Initiator == BattleWinner) ActivateDeciphererIfApplicable(DefenderBattleAction);

            CaptureLeaderIfApplicable();
            FlipBeneGesseritWhenAlone();
            DetermineAudit();
        }

        public List<IHero> TraitorsDeciphererCanLookAt { get; private set; } = new List<IHero>();
        public bool DeciphererMayReplaceTraitor { get; private set; } = false;

        private void ActivateDeciphererIfApplicable(Battle plan)
        {
            bool playerIsSkilled = SkilledAs(plan.Player, LeaderSkill.Decipherer);
            bool leaderIsSkilled = SkilledAs(plan.Hero, LeaderSkill.Decipherer);

            if (playerIsSkilled || leaderIsSkilled)
            {
                var traitor = TraitorDeck.Draw();
                TraitorsDeciphererCanLookAt.Add(traitor);
                plan.Player.KnownNonTraitors.Add(traitor);

                traitor = TraitorDeck.Draw();
                TraitorsDeciphererCanLookAt.Add(traitor);
                plan.Player.KnownNonTraitors.Add(traitor);

                DeciphererMayReplaceTraitor = plan.Initiator != Faction.Purple && leaderIsSkilled;
            } 
        }

        private void FinishDeciphererIfApplicable()
        {
            if (TraitorsDeciphererCanLookAt.Count > 0)
            {
                foreach (var item in TraitorsDeciphererCanLookAt)
                {
                    TraitorDeck.PutOnTop(item);
                }

                TraitorDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);

                TraitorsDeciphererCanLookAt.Clear();
            }
        }

        private void ResolveEffectsOfOwnedStrongholds(Battle playerPlan, Battle opponentPlan)
        {
            if (Map.TueksSietch.Territory == CurrentBattle.Territory && HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CollectResourcesForUseless))
            {
                if (playerPlan.Weapon?.Type == TreacheryCardType.Useless)
                {
                    CurrentReport.Add(playerPlan.Initiator, "{0} stronghold advantage: {1} collect 2 for playing {2}", Map.TueksSietch, playerPlan.Initiator, playerPlan.Weapon);
                    playerPlan.Player.Resources += 2;
                }

                if (playerPlan.Defense?.Type == TreacheryCardType.Useless)
                {
                    CurrentReport.Add(playerPlan.Initiator, "{0} stronghold advantage: {1} collect 2 for playing {2}", Map.TueksSietch, playerPlan.Initiator, playerPlan.Defense);
                    playerPlan.Player.Resources += 2;
                }
            }

            if (Map.SietchTabr.Territory == CurrentBattle.Territory && HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CollectResourcesForDial))
            {
                int collected = (int)Math.Floor(opponentPlan.Dial(this, playerPlan.Initiator));
                if (collected > 0)
                {
                    CurrentReport.Add(playerPlan.Initiator, "{0} stronghold advantage: {1} collect {2} for enemy force dial", Map.SietchTabr, playerPlan.Initiator, playerPlan.Defense);
                    playerPlan.Player.Resources += collected;
                }
            }

        }

        private void DetermineAudit()
        {
            if (Auditee != null)
            {
                var auditableCards = new Deck<TreacheryCard>(AuditCancelled.GetCardsThatMayBeAudited(this), Random);

                if (auditableCards.Items.Count > 0)
                {
                    var nrOfAuditedCards = AuditCancelled.GetNumberOfCardsThatMayBeAudited(this);
                    AuditedCards = new List<TreacheryCard>();
                    auditableCards.Shuffle();
                    for (int i = 0; i < nrOfAuditedCards; i++)
                    {
                        AuditedCards.Add(auditableCards.Draw());
                    }

                    Enter(Phase.AvoidingAudit);
                }
                else
                {
                    CurrentReport.Add(Auditee.Faction, "{0} don't have any cards to audit", Auditee.Faction);
                    Enter(BattleWinner != Faction.None, Phase.BattleConclusion, FinishBattle);
                }
            }
            else
            {
                Enter(BattleWinner != Faction.None, Phase.BattleConclusion, FinishBattle);
            }
        }

        public Player Auditee
        {
            get
            {
                if (Applicable(Rule.BrownAuditor) && !Prevented(FactionAdvantage.BrownAudit))
                {
                    if (AggressorBattleAction.Hero != null && AggressorBattleAction.Hero.HeroType == HeroType.Auditor) {

                        return DefenderBattleAction.Player;
                    }
                    else if (DefenderBattleAction.Hero != null && DefenderBattleAction.Hero.HeroType == HeroType.Auditor)
                    {
                        return AggressorBattleAction.Player;
                    }
                }

                return null;
            }
        }

        public void HandleEvent(AuditCancelled e)
        {
            CurrentReport.Add(e.GetDynamicMessage());
            if (e.Cancelled)
            {
                e.Player.Resources -= e.Cost();
                GetPlayer(Faction.Brown).Resources += e.Cost();
            }

            Enter(!e.Cancelled, Phase.Auditing, BattleWinner != Faction.None, Phase.BattleConclusion, FinishBattle);
        }

        public void HandleEvent(Audited e)
        {
            CurrentReport.Add(e);
            
            foreach (var card in AuditedCards)
            {
                RegisterKnown(e.Player, card);
            }

            Enter(BattleWinner != Faction.None, Phase.BattleConclusion, FinishBattle);
        }

        private void CaptureLeaderIfApplicable()
        {
            if (BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders))
            {
                if (!Prevented(FactionAdvantage.BlackCaptureLeader))
                {
                    if (AggressorBattleAction.By(BattleWinner))
                    {
                        SelectVictimOfBlackWinner(AggressorBattleAction, DefenderBattleAction);
                    }
                    else
                    {
                        SelectVictimOfBlackWinner(DefenderBattleAction, AggressorBattleAction);
                    }
                }
                else
                {
                    CurrentReport.Add(Faction.Black, "{0} prevents {1} from capturing a leader.", TreacheryCardType.Karma, Faction.Black);
                }
            }
        }

        public void HandleEvent(BattleConcluded e)
        {
            var winner = GetPlayer(e.Initiator);

            foreach (var c in e.DiscardedCards)
            {
                CurrentReport.Add(e.Initiator, "{0} discard {1}.", e.Initiator, c);
                winner.TreacheryCards.Remove(c);
                TreacheryDiscardPile.PutOnTop(c);
            }

            if (e.ReplacedTraitor != null && e.NewTraitor != null)
            {
                CurrentReport.Add(e.Initiator, "{0} replaced {1} by another traitor from the deck.", e.Initiator, e.ReplacedTraitor);

                e.Player.Traitors.Add(e.NewTraitor);
                TraitorsDeciphererCanLookAt.Remove(e.NewTraitor);

                e.Player.Traitors.Remove(e.ReplacedTraitor);
                TraitorDeck.PutOnTop(e.ReplacedTraitor);

                RecentMilestones.Add(Milestone.Shuffled);
                TraitorDeck.Shuffle();
            }

            DecideFateOfCapturedLeader(e);
            TakeTechToken(e, winner);
            ProcessGreyForceLossesAndSubstitutions(e, winner);

            Enter(IsPlaying(Faction.Purple) && BattleWinner != Faction.Purple, Phase.Facedancing, FinishBattle);
        }

        public void HandleEvent(FaceDanced f)
        {
            if (f.FaceDancerCalled)
            {
                var initiator = GetPlayer(f.Initiator);
                var facedancer = initiator.FaceDancers.FirstOrDefault(f => WinnerHero.IsFaceDancer(f));
                CurrentReport.Add(f.Initiator, "{0} reveal {1} as one of their Face Dancers!", f.Initiator, facedancer);

                RecentMilestones.Add(Milestone.FaceDanced);

                if (facedancer is Leader && IsAlive(facedancer))
                {
                    KillHero(facedancer);
                }

                if (BattleWinner == Faction.Black)
                {
                    ReturnCapturedLeaders(GetPlayer(BattleWinner), facedancer);
                }

                foreach (var p in Players)
                {
                    if (!p.KnownNonTraitors.Contains(facedancer)) p.KnownNonTraitors.Add(facedancer);
                }

                if (!initiator.RevealedDancers.Contains(facedancer))
                {
                    initiator.RevealedDancers.Add(facedancer);
                }

                if (!initiator.HasUnrevealedFaceDancers)
                {
                    ReplaceFacedancers(f, initiator);
                }

                ReplaceForces(f, initiator);

                if (Version >= 81)
                {
                    FlipBeneGesseritWhenAlone();
                }
            }
            else
            {
                CurrentReport.Add(f.Initiator, "{0} don't reveal a Face Dancer.", f.Initiator);
            }

            FinishBattle();
        }

        private void ReplaceForces(FaceDanced f, Player initiator)
        {
            if (Version < 56 || Version >= 80)
            {
                var winner = GetPlayer(BattleWinner);
                int nrOfRemovedForces = winner.AnyForcesIn(CurrentBattle.Territory);

                if (nrOfRemovedForces > 0)
                {
                    winner.ForcesToReserves(CurrentBattle.Territory);

                    initiator.ForcesInReserve -= f.ForcesFromReserve;
                    foreach (var fl in f.ForceLocations)
                    {
                        var location = fl.Key;
                        initiator.ChangeForces(location, -fl.Value.AmountOfForces);
                        initiator.ChangeSpecialForces(location, -fl.Value.AmountOfSpecialForces);
                    }

                    foreach (var fl in f.TargetForceLocations)
                    {
                        var location = fl.Key;
                        initiator.ChangeForces(location, fl.Value.AmountOfForces);
                        initiator.ChangeSpecialForces(location, fl.Value.AmountOfSpecialForces);
                    }

                    CurrentReport.Add(f.Initiator, "{0} {1} forces go back to reserves and are replaced by {2} {3} forces ({4} from reserves{5}).", nrOfRemovedForces, winner.Faction, f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Initiator, f.ForcesFromReserve, DetermineSourceLocations(f));
                }
            }
            else
            {
                int nrOfRemovedForces = FaceDanced.MaximumNumberOfForces(this, initiator);

                if (nrOfRemovedForces > 0)
                {
                    foreach (var player in Players.Where(p => p.Faction != f.Initiator))
                    {
                        player.ForcesToReserves(CurrentBattle.Territory);
                    }

                    initiator.ForcesInReserve -= f.ForcesFromReserve;
                    foreach (var fl in f.ForceLocations)
                    {
                        var location = fl.Key;
                        initiator.ChangeForces(location, -fl.Value.AmountOfForces);
                        initiator.ChangeSpecialForces(location, -fl.Value.AmountOfSpecialForces);
                    }

                    foreach (var fl in f.TargetForceLocations)
                    {
                        var location = fl.Key;
                        initiator.ChangeForces(location, fl.Value.AmountOfForces);
                        initiator.ChangeSpecialForces(location, fl.Value.AmountOfSpecialForces);
                    }

                    CurrentReport.Add(f.Initiator, "{0} forces go back to reserves and are replaced by {1} {2} forces ({3} from reserves{4}).", nrOfRemovedForces, f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Initiator, f.ForcesFromReserve, DetermineSourceLocations(f));
                }
            }
        }

        private void ReplaceFacedancers(FaceDanced f, Player purple)
        {
            TraitorDeck.Items.AddRange(purple.FaceDancers);
            purple.FaceDancers.Clear();
            purple.RevealedDancers.Clear();
            TraitorDeck.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            for (int i = 0; i < 3; i++)
            {
                purple.FaceDancers.Add(TraitorDeck.Draw());
            }
            CurrentReport.Add(f.Initiator, "{0} draw 3 new Face Dancers.", f.Initiator);
        }

        private MessagePart DetermineSourceLocations(FaceDanced f)
        {
            if (f.ForceLocations.Count == 0)
            {
                return new MessagePart("");
            }
            else
            {
                return new MessagePart(", {0}", string.Join(", ", f.ForceLocations.Select(fl => string.Format("{0} from {1}", fl.Value.AmountOfForces + fl.Value.AmountOfSpecialForces, fl.Key))));
            }
        }

        public IHero WinnerHero
        {
            get
            {
                if (BattleWinner != Faction.None)
                {
                    var winnerGambit = BattleWinner == AggressorBattleAction.Initiator ? AggressorBattleAction : DefenderBattleAction;
                    return winnerGambit.Hero;
                }

                return null;
            }
        }

        private void FinishBattle()
        {
            GreenKarma = false;
            if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
            if (CurrentJuice != null && CurrentJuice.Type == JuiceType.Aggressor) CurrentJuice = null;
            CurrentDiplomacy = null;
            FinishDeciphererIfApplicable();
            if (NextPlayerToBattle == null) MainPhaseEnd();
            Enter(Phase.BattleReport);
        }

        private void AllowPreventedBattleFactionAdvantages()
        {
            if (Version >= 88 || AggressorBattleAction.By(Faction.Green) || DefenderBattleAction.By(Faction.Green))
            {
                Allow(FactionAdvantage.GreenUseMessiah);
                Allow(FactionAdvantage.GreenBattlePlanPrescience);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Blue) || DefenderBattleAction.By(Faction.Blue))
            {
                Allow(FactionAdvantage.BlueUsingVoice);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Yellow) || DefenderBattleAction.By(Faction.Yellow))
            {
                Allow(FactionAdvantage.YellowSpecialForceBonus);
                Allow(FactionAdvantage.YellowNotPayingForBattles);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Red) || DefenderBattleAction.By(Faction.Red))
            {
                Allow(FactionAdvantage.RedSpecialForceBonus);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Grey) || DefenderBattleAction.By(Faction.Grey))
            {
                Allow(FactionAdvantage.GreySpecialForceBonus);
            }

            if (Version >= 88 || GetPlayer(AggressorBattleAction.Initiator).Ally == Faction.Black || GetPlayer(DefenderBattleAction.Initiator).Ally == Faction.Black)
            {
                Allow(FactionAdvantage.BlackCallTraitorForAlly);
                Allow(FactionAdvantage.BlackCaptureLeader);
            }

            Allow(FactionAdvantage.BrownReceiveForcePayment);
        }

        private void ProcessGreyForceLossesAndSubstitutions(BattleConcluded e, Player winner)
        {
            if (GreySpecialForceLossesToTake > 0)
            {
                var winnerGambit = WinnerBattleAction;
                int winnerForcesLost = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength + e.SpecialForceLossesReplaced;
                int winnerSpecialForcesLost = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength - e.SpecialForceLossesReplaced;
                HandleLosses(CurrentBattle.Territory, winner, winnerForcesLost, winnerSpecialForcesLost);
            }
        }

        private void TakeTechToken(BattleConcluded e, Player winner)
        {
            if (e.StolenToken != TechToken.None)
            {
                var loser = GetPlayer(BattleLoser);
                if (loser.TechTokens.Contains(e.StolenToken))
                {
                    loser.TechTokens.Remove(e.StolenToken);
                    winner.TechTokens.Add(e.StolenToken);
                    CurrentReport.Add(e.Initiator, "{0} take {1} from {2}.", e.Initiator, e.StolenToken, BattleLoser);
                }
            }
        }

        private void DecideFateOfCapturedLeader(BattleConcluded e)
        {
            if (e.By(Faction.Black) && Applicable(Rule.BlackCapturesOrKillsLeaders) && BlackVictim != null)
            {
                if (e.Initiator == CurrentBattle.Aggressor)
                {
                    CaptureOrAssassinateLeader(AggressorBattleAction, DefenderBattleAction, e.CaptureDecision);
                }
                else
                {
                    CaptureOrAssassinateLeader(DefenderBattleAction, AggressorBattleAction, e.CaptureDecision);
                }
            }
        }

        private void ResetBattle()
        {
            CurrentBattle = null;
            CurrentPrescience = null;
            CurrentThought = null;
            CurrentVoice = null;
            BlackVictim = null;
            AggressorBattleAction = null;
            DefenderBattleAction = null;
            AggressorTraitorAction = null;
            DefenderTraitorAction = null;
            PoisonToothCancelled = false;
            GreySpecialForceLossesToTake = 0;
            BattleWinner = Faction.None;
            BattleLoser = Faction.None;
            HasActedOrPassed.Clear();
        }

        public void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
        {
            var aggressor = GetPlayer(agg.Initiator);
            var defender = GetPlayer(def.Initiator);

            if (Version <= 93)
            {
                SetHeroLocations(agg, b.Territory);
                SetHeroLocations(def, b.Territory);
            }

            bool lasgunShield = !aggtrt.TraitorCalled && !deftrt.TraitorCalled && (agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield);

            if (CurrentRetreat != null && !lasgunShield)
            {
                ExecuteRetreat();
            }

            if (aggtrt.TraitorCalled || deftrt.TraitorCalled)
            {
                TraitorCalled(b, agg, def, deftrt, aggressor, defender, agg.Hero, def.Hero);
            }
            else if (lasgunShield)
            {
                LasgunShieldExplosion(agg, def, aggressor, defender, b.Territory, agg.Hero, def.Hero);
            }
            else
            {
                if (Version >= 94)
                {
                    SetHeroLocations(agg, b.Territory);
                    SetHeroLocations(def, b.Territory);
                }

                DetermineBattleOutcome(agg, def, aggressor, defender, b.Territory, agg.Hero, def.Hero);
            }

            if (aggressor.Is(Faction.Black))
            {
                ReturnCapturedLeaders(aggressor, agg.Hero);
            }
            else if (defender.Is(Faction.Black))
            {
                ReturnCapturedLeaders(defender, def.Hero);
            }
        }

        private void ExecuteRetreat()
        {
            CurrentReport.Add(CurrentRetreat);
            int forcesToMove = CurrentRetreat.Forces;
            foreach (var l in CurrentBattle.Territory.Locations.Where(l => CurrentRetreat.Player.ForcesIn(l) > 0).ToArray())
            {

                if (forcesToMove == 0) break;
                int toMoveFromHere = Math.Min(forcesToMove, CurrentRetreat.Player.ForcesIn(l));
                CurrentRetreat.Player.MoveForces(l, CurrentRetreat.Location, toMoveFromHere);
                forcesToMove -= toMoveFromHere;
            }

            int specialForcesToMove = CurrentRetreat.SpecialForces;
            foreach (var l in CurrentBattle.Territory.Locations.Where(l => CurrentRetreat.Player.SpecialForcesIn(l) > 0).ToArray())
            {

                if (specialForcesToMove == 0) break;
                int toMoveFromHere = Math.Min(specialForcesToMove, CurrentRetreat.Player.SpecialForcesIn(l));
                CurrentRetreat.Player.MoveSpecialForces(l, CurrentRetreat.Location, toMoveFromHere);
                specialForcesToMove -= toMoveFromHere;
            }
        }

        private void TraitorCalled(BattleInitiated b, Battle agg, Battle def, TreacheryCalled deftrt, Player aggressor, Player defender, IHero aggLeader, IHero defLeader)
        {
            if (AggressorTraitorAction.TraitorCalled && deftrt.TraitorCalled)
            {
                TwoTraitorsCalled(agg, def, aggressor, defender, b.Territory, aggLeader, defLeader);
            }
            else
            {
                var winner = AggressorTraitorAction.TraitorCalled ? aggressor : defender;
                var loser = AggressorTraitorAction.TraitorCalled ? defender : aggressor;
                var loserGambit = AggressorTraitorAction.TraitorCalled ? def : agg;
                var winnerGambit = AggressorTraitorAction.TraitorCalled ? agg : def;
                OneTraitorCalled(b.Territory, winner, loser, loserGambit, winnerGambit);
            }
        }

        private void ReturnCapturedLeaders(Player harkonnen, IHero hero)
        {
            if (harkonnen != null)
            {
                //Captured leader used in battle
                if (hero != null && hero is Leader capturedLeader && harkonnen.Leaders.Contains(capturedLeader) && CapturedLeaders.ContainsKey(capturedLeader))
                {
                    ReturnLeader(harkonnen, capturedLeader);
                }

                if (!harkonnen.Leaders.Any(l => CapturedLeaders.ContainsKey(l) && IsAlive(l)))
                {
                    Leader toReturn;
                    while ((toReturn = harkonnen.Leaders.FirstOrDefault(c => c.Faction != Faction.Black)) != null)
                    {
                        ReturnLeader(harkonnen, toReturn);
                    }
                }
            }
        }

        private void ReturnLeader(Player currentOwner, Leader toReturn)
        {
            if (CapturedLeaders.ContainsKey(toReturn))
            {
                Player originalPlayer = GetPlayer(CapturedLeaders[toReturn]);
                originalPlayer.Leaders.Add(toReturn);
                currentOwner.Leaders.Remove(toReturn);
                CapturedLeaders.Remove(toReturn);
                if (Skilled(toReturn))
                {
                    SetInFrontOfShield(toReturn, true);
                }
                CurrentReport.Add(originalPlayer.Faction, "{0} returns to {1} after service for {2}", toReturn, originalPlayer.Faction, currentOwner.Faction);
            }
        }

        private void DetermineBattleOutcome(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggHero, IHero defHero)
        {
            //Determine result

            agg.ActivateMirrorWeaponAndDiplomacy(def.Weapon, def.Defense);
            def.ActivateMirrorWeaponAndDiplomacy(agg.Weapon, agg.Defense);

            bool poisonToothUsed = !PoisonToothCancelled && (agg.HasPoisonTooth || def.HasPoisonTooth);
            bool artilleryUsed = agg.HasArtillery || def.HasArtillery;
            bool rockMelterUsed = agg.HasRockMelter || def.HasRockMelter;

            var aggHeroKilled = false;
            var aggHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(agg, def, aggHero, poisonToothUsed, artilleryUsed, rockMelterUsed && RockMelterWasUsedToKill, ref aggHeroKilled, ref aggHeroCauseOfDeath);

            bool defHeroKilled = false;
            var defHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(def, agg, defHero, poisonToothUsed, artilleryUsed, rockMelterUsed && RockMelterWasUsedToKill, ref defHeroKilled, ref defHeroCauseOfDeath);

            int aggHeroSkillBonus = DetermineSkillBonus(agg);
            int aggHeroEffectiveStrength = (aggHero != null && !artilleryUsed) ? aggHero.ValueInCombatAgainst(defHero) : 0;
            int aggHeroContribution = !aggHeroKilled && !rockMelterUsed ? aggHeroEffectiveStrength + aggHeroSkillBonus : 0;

            int defHeroSkillBonus = DetermineSkillBonus(def);
            int defHeroEffectiveStrength = (defHero != null && !artilleryUsed) ? defHero.ValueInCombatAgainst(aggHero) : 0;
            int defHeroContribution = !defHeroKilled && !rockMelterUsed ? defHeroEffectiveStrength + defHeroSkillBonus : 0;

            var aggMessiahContribution = aggressor.Is(Faction.Green) && agg.Messiah && agg.Hero != null && !aggHeroKilled && !artilleryUsed ? 2 : 0;
            var defMessiahContribution = defender.Is(Faction.Green) && def.Messiah && def.Hero != null && !defHeroKilled && !artilleryUsed  ? 2 : 0;

            var aggThinkerContribution = Skill(agg.Hero) == LeaderSkill.Thinker && !aggHeroKilled && !artilleryUsed ? 2 : 0;
            var defThinkerContribution = Skill(def.Hero) == LeaderSkill.Thinker && !defHeroKilled && !artilleryUsed ? 2 : 0;

            float aggForceDial;
            float defForceDial;

            if (!rockMelterUsed)
            {
                aggForceDial = agg.Dial(this, defender.Faction);
                defForceDial = def.Dial(this, aggressor.Faction);
            }
            else
            {
                aggForceDial = aggressor.ForcesIn(CurrentBattle.Territory) - agg.Forces - agg.ForcesAtHalfStrength + aggressor.SpecialForcesIn(CurrentBattle.Territory) - agg.SpecialForces - agg.SpecialForcesAtHalfStrength;
                defForceDial = defender.ForcesIn(CurrentBattle.Territory) - def.Forces - def.ForcesAtHalfStrength + defender.SpecialForcesIn(CurrentBattle.Territory) - def.SpecialForces - def.SpecialForcesAtHalfStrength;
            }

            int aggBureaucracyPenalty = SkilledAs(def.Hero, LeaderSkill.Bureaucrat) ? Map.Strongholds.Count(sh => aggressor.Occupies(sh)) : 0;
            int defBureaucracyPenalty = SkilledAs(agg.Hero, LeaderSkill.Bureaucrat) ? Map.Strongholds.Count(sh => defender.Occupies(sh)) : 0;

            float aggTotal = aggForceDial + aggHeroContribution + aggMessiahContribution + aggThinkerContribution - aggBureaucracyPenalty;
            float defTotal = defForceDial + defHeroContribution + defMessiahContribution + defThinkerContribution - defBureaucracyPenalty;

            agg.DeactivateMirrorWeaponAndDiplomacy();
            def.DeactivateMirrorWeaponAndDiplomacy();

            bool aggressorWinsTies = true;
            if (HasStrongholdAdvantage(defender.Faction, StrongholdAdvantage.WinTies)) aggressorWinsTies = false;
            if (IsAggressorByJuice(defender) && !HasStrongholdAdvantage(aggressor.Faction, StrongholdAdvantage.WinTies)) aggressorWinsTies = false;

            Player winner;
            if (aggressorWinsTies)
            {
                winner = (aggTotal >= defTotal) ? aggressor : defender;
            }
            else
            {
                winner = (defTotal >= aggTotal) ? defender : aggressor;
            }

            var winnerBattlePlan = (winner == aggressor) ? agg : def;

            var loser = winner == aggressor ? defender : aggressor;
            var loserBattlePlan = (loser == aggressor) ? agg : def;

            //Handle result

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            if (aggHeroKilled)
            {
                KillLeaderInBattle(aggHero, defHero, aggHeroCauseOfDeath, winner, aggHeroEffectiveStrength);
            }

            if (defHeroKilled)
            {
                KillLeaderInBattle(defHero, aggHero, defHeroCauseOfDeath, winner, defHeroEffectiveStrength);
            }

            if (IsAggressorByJuice(defender))
            {
                CurrentReport.Add(aggressor.Faction, "{0} (defending) strength: {1}.", aggressor.Faction, aggTotal);
                CurrentReport.Add(defender.Faction, "{0} (aggressor due to {2}) strength: {1}.", defender.Faction, defTotal, TreacheryCardType.Juice);
            }
            else
            {
                CurrentReport.Add(aggressor.Faction, "{0} (aggressor) strength: {1}.", aggressor.Faction, aggTotal);
                CurrentReport.Add(defender.Faction, "{0} (defending) strength: {1}.", defender.Faction, defTotal);
            }

            CurrentReport.Add(winner.Faction, "{0} WIN THE BATTLE.", winner.Faction);

            ProcessWinnerLosses(territory, winner, winnerBattlePlan);
            ProcessLoserLosses(territory, loser, loserBattlePlan);
        }

        private int DetermineSkillBonus(Battle plan)
        {
            if (plan.Weapon != null && plan.Weapon.IsUseless || plan.Defense != null && plan.Defense.IsUseless) {
                if (SkilledAs(plan.Hero, LeaderSkill.Warmaster)) return 3;
                else if (SkilledAs(plan.Player, LeaderSkill.Warmaster)) return 1;
            }

            if (plan.Defense != null && plan.Defense.IsProjectileDefense)
            {
                if (SkilledAs(plan.Hero, LeaderSkill.Adept)) return 3;
                else if (SkilledAs(plan.Player, LeaderSkill.Adept)) return 1;
            }

            if (plan.Weapon != null && plan.Weapon.IsProjectileWeapon)
            {
                if (SkilledAs(plan.Hero, LeaderSkill.Swordmaster)) return 3;
                else if (SkilledAs(plan.Player, LeaderSkill.Swordmaster)) return 1;
            }

            if (plan.Weapon != null && !(plan.Weapon.IsWeapon || plan.Weapon.IsDefense || plan.Weapon.IsUseless))
            {
                if (SkilledAs(plan.Hero, LeaderSkill.Planetologist)) return 2;
            }

            if (plan.Defense != null && plan.Defense.IsPoisonDefense)
            {
                if (SkilledAs(plan.Hero, LeaderSkill.KillerMedic)) return 3;
                else if (SkilledAs(plan.Player, LeaderSkill.KillerMedic)) return 1;
            }

            if (plan.Weapon != null && (plan.Weapon.IsPoisonWeapon || plan.Weapon.IsPoisonTooth))
            {
                if (SkilledAs(plan.Hero, LeaderSkill.MasterOfAssassins)) return 3;
                else if (SkilledAs(plan.Player, LeaderSkill.MasterOfAssassins)) return 1;
            }

            return 0;
        }

        public bool IsAggressorByJuice(Player p)
        {
            return CurrentJuice != null && CurrentJuice.Type == JuiceType.Aggressor && CurrentJuice.Player == p;
        }

        private void DetermineCauseOfDeath(Battle playerPlan, Battle opponentPlan, IHero theHero, bool poisonToothUsed, bool artilleryUsed, bool rockMelterWasUsedToKill, ref bool heroDies, ref TreacheryCardType causeOfDeath)
        {
            bool isProtectedByCarthagAdvantage = HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CountDefensesAsSnooper) && !playerPlan.HasPoison && !playerPlan.HasPoisonTooth;

            DetermineDeathBy(theHero, TreacheryCardType.Rockmelter, rockMelterWasUsedToKill, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.ArtilleryStrike, artilleryUsed && !playerPlan.HasShield, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.PoisonTooth, poisonToothUsed && !playerPlan.HasNonAntidotePoisonDefense, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Laser, opponentPlan.HasLaser, ref heroDies, ref causeOfDeath);

            if (opponentPlan.HasPoison && !playerPlan.HasAntidote && isProtectedByCarthagAdvantage)
            {
                CurrentReport.Add(playerPlan.Initiator, "{0} stronghold advantage protects {1} from death by {2}.", Map.Carthag, theHero, TreacheryCardType.Poison);
            }

            DetermineDeathBy(theHero, TreacheryCardType.Poison, opponentPlan.HasPoison && !(playerPlan.HasAntidote || isProtectedByCarthagAdvantage), ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Projectile, opponentPlan.HasProjectile && !playerPlan.HasProjectileDefense, ref heroDies, ref causeOfDeath);
        }

        private void DetermineDeathBy(IHero hero, TreacheryCardType byWeapon, bool weaponHasEffect, ref bool heroIsKilled, ref TreacheryCardType causeOfDeath)
        {
            if (!heroIsKilled && hero != null && weaponHasEffect)
            {
                heroIsKilled = true;
                causeOfDeath = byWeapon;
            }
        }

        private void ProcessLoserLosses(Territory territory, Player loser, Battle loserGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            CurrentReport.Add(loser.Faction, "{0} lose all ({1}) forces in {2}.", loser.Faction, loser.AnyForcesIn(territory), territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void PayDialedSpice(Player p, Battle b)
        {
            int cost = b.Cost(this);
            int costToBrown = p.Ally == Faction.Brown ? b.AllyContributionAmount : 0;
            int receiverProfit = 0;

            if (cost > 0)
            {
                int costForPlayer = cost - b.AllyContributionAmount;

                if (costForPlayer > 0)
                {
                    p.Resources -= costForPlayer;

                    if (HasStrongholdAdvantage(p.Faction, StrongholdAdvantage.FreeResourcesForBattles))
                    {
                        CurrentReport.Add(p.Faction, "{0} stronghold advantage: supporting forces costs 2 less.", Map.Arrakeen);
                    }
                }

                if (b.AllyContributionAmount > 0)
                {
                    p.AlliedPlayer.Resources -= b.AllyContributionAmount;
                }

                var brown = GetPlayer(Faction.Brown);
                if (brown != null && p.Faction != Faction.Brown)
                {
                    receiverProfit = (int)Math.Floor(0.5f * (cost - costToBrown));

                    if (receiverProfit > 0)
                    {
                        if (!Prevented(FactionAdvantage.BrownReceiveForcePayment))
                        {
                            brown.Resources += receiverProfit;
                            CurrentReport.Add(Faction.Brown, "{0} receive {1} from supported forces", Faction.Brown, receiverProfit);

                            if (receiverProfit >= 5)
                            {
                                ApplyBureaucracy(p.Faction, Faction.Brown);
                            }
                        }
                        else
                        {
                            CurrentReport.Add(Faction.Brown, "{0} prevents {1}", Faction.Brown, FactionAdvantage.BrownReceiveForcePayment);
                        }
                    }
                }
                
                if (cost - receiverProfit >= 4)
                {
                    ActivateBanker();
                }
            }
        }

        private void ProcessWinnerLosses(Territory territory, Player winner, Battle plan)
        {
            PayDialedSpice(winner, plan);

            int specialForcesToLose = plan.SpecialForces + plan.SpecialForcesAtHalfStrength;
            int forcesToLose = plan.Forces + plan.ForcesAtHalfStrength;

            int specialForcesToSaveToReserves = 0;
            int forcesToSaveToReserves = 0;
            int specialForcesToSaveInTerritory = 0;
            int forcesToSaveInTerritory = 0;

            if (SkilledAs(plan.Hero, LeaderSkill.Graduate))
            {
                specialForcesToSaveInTerritory = Math.Min(specialForcesToLose, 1);
                forcesToSaveInTerritory = Math.Max(0, Math.Min(forcesToLose - specialForcesToSaveInTerritory, 1));

                specialForcesToSaveToReserves = Math.Max(0, Math.Min(specialForcesToLose - specialForcesToSaveInTerritory - forcesToSaveInTerritory, 2));
                forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose - specialForcesToSaveInTerritory - forcesToSaveInTerritory - specialForcesToSaveToReserves, 2));
            }
            else if (SkilledAs(winner, LeaderSkill.Graduate))
            {
                specialForcesToSaveToReserves = Math.Min(specialForcesToLose, 1);
                forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose - specialForcesToSaveToReserves, 1));
            }

            if (specialForcesToSaveInTerritory + forcesToSaveInTerritory + specialForcesToSaveToReserves + forcesToSaveToReserves > 0)
            {
                if (SkilledAs(plan.Hero, LeaderSkill.Graduate)) RecentMilestones.Add(Milestone.AdvancedGraduate);
                else RecentMilestones.Add(Milestone.Graduate);

                if (specialForcesToSaveToReserves > 0) winner.ForcesToReserves(territory, specialForcesToSaveToReserves, true);

                if (forcesToSaveToReserves > 0) winner.ForcesToReserves(territory, forcesToSaveToReserves, false);

                CurrentReport.Add(winner.Faction, "{0} rescues {1} forces on site and rescues {2} forces to reserves.",
                    LeaderSkill.Graduate,
                    specialForcesToSaveInTerritory + forcesToSaveInTerritory,
                    specialForcesToSaveToReserves + forcesToSaveToReserves);

            }

            if (winner.Faction != Faction.Grey || specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory == 0 || winner.ForcesIn(territory) <= plan.Forces + plan.ForcesAtHalfStrength)
            {
                int winnerForcesLost = forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory;
                int winnerSpecialForcesLost = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
                HandleLosses(territory, winner, winnerForcesLost, winnerSpecialForcesLost);
            }
            else
            {
                GreySpecialForceLossesToTake = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
            }

            
        }

        private void HandleLosses(Territory territory, Player player, int forcesLost, int specialForcesLost)
        {
            bool hadMessiahBeforeLosses = player.MessiahAvailable;

            player.KillForces(territory, forcesLost, false, true);
            player.KillForces(territory, specialForcesLost, true, true);
            LogLosses(player, forcesLost, specialForcesLost);

            if (player.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void KillLeaderInBattle(IHero killedHero, IHero opposingHero, TreacheryCardType causeOfDeath, Player winner, int heroValue)
        {
            var earned = (causeOfDeath == TreacheryCardType.ArtilleryStrike) ? 0 : killedHero.ValueInCombatAgainst(opposingHero);
            if (Version >= 45) earned = heroValue;

            CurrentReport.Add(winner.Faction, "{1} kills {0}. {2} earn {3}.", killedHero, causeOfDeath, winner.Faction, earned);
            RecentMilestones.Add(Milestone.LeaderKilled);
            if (killedHero is Leader) KillHero(killedHero as Leader);
            winner.Resources += earned;
        }

        private void LogLosses(Player player, int forcesLost, int specialForcesLost)
        {
            if (specialForcesLost > 0)
            {
                CurrentReport.Add(player.Faction, "{0} lose {1} {2} and {3} {4} during battle.", player.Faction, forcesLost, player.Force, specialForcesLost, player.SpecialForce);
            }
            else if (forcesLost > 0)
            {
                CurrentReport.Add(player.Faction, "{0} lose {1} {2} during battle.", player.Faction, forcesLost, player.Force);
            }
        }

        private void SetHeroLocations(Battle b, Territory territory)
        {
            if (b.Hero != null && b.Hero is Leader)
            {
                LeaderState[b.Hero].CurrentTerritory = territory;
            }

            if (b.Messiah)
            {
                LeaderState[LeaderManager.Messiah].CurrentTerritory = territory;
            }
        }

        private void OneTraitorCalled(Territory territory, Player winner, Player loser, Battle loserGambit, Battle winnerGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            var traitor = loserGambit.Hero;
            var traitorValue = traitor.ValueInCombatAgainst(winnerGambit.Hero);
            var traitorOwner = winner.Traitors.Any(t => t.IsTraitor(traitor)) ? winner.Faction : Faction.Black;

            CurrentReport.Add(traitorOwner, "{0} is a {1} traitor! {2} lose everything.", traitor, traitorOwner, loser.Faction);

            RecentMilestones.Add(Milestone.LeaderKilled);

            if (traitor is Leader)
            {
                CurrentReport.Add(loser.Faction, "Treachery kills {0}. {1} earn {2}.", traitor, winner.Faction, traitorValue);
                KillHero(traitor);
                winner.Resources += traitorValue;
            }

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            CurrentReport.Add(loser.Faction, "{0} lose all ({1}) forces in {2}.", loser.Faction, loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void TwoTraitorsCalled(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            CurrentReport.Add("Both leaders are traitors, everyone gets killed!");
            RecentMilestones.Add(Milestone.LeaderKilled);

            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Add(defender.Faction, "Treachery kills {0}.", defLeader);
            KillHero(defLeader);
            CurrentReport.Add(aggressor.Faction, "Treachery kills {0}.", aggLeader);
            KillHero(aggLeader);
            CurrentReport.Add(defender.Faction, "{0} lose all ({1}) forces in {2}.", defender.Faction, defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), territory);
            defender.KillAllForces(territory, true);
            CurrentReport.Add(aggressor.Faction, "{0} lose all ({1}) forces in {2}.", aggressor.Faction, aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), territory);
            aggressor.KillAllForces(territory, true);

            LoseCards(def);
            PayDialedSpice(defender, def);

            LoseCards(agg);
            PayDialedSpice(aggressor, agg);

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LasgunShieldExplosion(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Add(Faction.None, "A {0}/{1} explosion occurs!", TreacheryCardType.Laser, TreacheryCardType.Shield);
            RecentMilestones.Add(Milestone.Explosion);

            if (aggLeader != null)
            {
                CurrentReport.Add(aggressor.Faction, "The explosion kills {0}.", aggLeader);
                KillHero(aggLeader);
            }

            if (defLeader != null)
            {
                CurrentReport.Add(defender.Faction, "The explosion kills {0}.", defLeader);
                KillHero(def.Hero);
            }

            if (agg.Messiah || def.Messiah)
            {
                CurrentReport.Add(aggressor.Faction, "The explosion kills the {0}.", Concept.Messiah);
                KillHero(LeaderManager.Messiah);
            }

            LoseCards(agg);
            PayDialedSpice(aggressor, agg);

            LoseCards(def);
            PayDialedSpice(defender, def);

            int removed = RemoveResources(territory);
            if (removed > 0)
            {
                CurrentReport.Add(Faction.None, "The explosion destroys {0} {1} in {2}.", removed, Concept.Resource, territory);
            }

            foreach (var p in Players)
            {
                RevealCurrentNoField(p, territory);

                int numberOfForces = p.AnyForcesIn(territory);
                if (numberOfForces > 0)
                {
                    CurrentReport.Add(p.Faction, "The explosion kills {0} {1} forces in {2}.", numberOfForces, p.Faction, territory);
                    p.KillAllForces(territory, true);
                }
            }

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LoseCards(Battle gambit)
        {
            Discard(gambit.Weapon);
            Discard(gambit.Defense);
        }

        public bool CanJoinCurrentBattle(IHero hero)
        {
            var currentTerritory = CurrentTerritory(hero);
            return currentTerritory == null || currentTerritory == CurrentBattle?.Territory;
        }

        public Leader BlackVictim = null;
        private void SelectVictimOfBlackWinner(Battle harkonnenAction, Battle victimAction)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var victim = GetPlayer(victimAction.Initiator);

            //Get all living leaders from the opponent that haven't fought in another territory this turn
            Deck<Leader> availableLeaders = new Deck<Leader>(
                victim.Leaders.Where(l => l.HeroType != HeroType.Auditor && LeaderState[l].Alive && CanJoinCurrentBattle(l)), Random);

            if (!availableLeaders.IsEmpty)
            {
                availableLeaders.Shuffle();
                BlackVictim = availableLeaders.Draw();
            }
            else
            {
                BlackVictim = null;
                CurrentReport.Add(Faction.Black, "{0} don't have any leaders {1} can capture or kill.", victim.Faction, Faction.Black);
            }
        }

        public Dictionary<Leader, Faction> CapturedLeaders { get; private set; } = new Dictionary<Leader, Faction>();
        private void CaptureOrAssassinateLeader(Battle harkonnenAction, Battle victimAction, CaptureDecision decision)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var target = GetPlayer(victimAction.Initiator);

            if (decision == CaptureDecision.Capture)
            {
                CurrentReport.Add(Faction.Black, "{0} capture a leader!", Faction.Black);
                harkonnen.Leaders.Add(BlackVictim);
                target.Leaders.Remove(BlackVictim);
                SetInFrontOfShield(BlackVictim, false);
                CapturedLeaders.Add(BlackVictim, target.Faction);
            }
            else if (decision == CaptureDecision.Kill)
            {
                CurrentReport.Add(Faction.Black, "{0} kill a leader for 2!", Faction.Black);
                RecentMilestones.Add(Milestone.LeaderKilled);
                AssassinateLeader(BlackVictim);
                harkonnen.Resources += 2;
            }
            else if (decision == CaptureDecision.DontCapture)
            {
                CurrentReport.Add(Faction.Black, "{0} decide not to capture or kill a leader.", Faction.Black);
            }
        }

        

        public void HandleEvent(SwitchedSkilledLeader e)
        {
            var leader = e.Player.Leaders.FirstOrDefault(l => Skilled(l) && !CapturedLeaders.ContainsKey(l));
            SwitchInFrontOfShield(leader);
            CurrentReport.Add(e.Initiator, "{0} place {1} {2} {3} their shield", e.Initiator, Skill(leader), leader, IsInFrontOfShield(leader) ? "in front of" : "behind");
        }

        public Battle WinnerBattleAction
        {
            get
            {
                if (AggressorBattleAction != null && AggressorBattleAction.Initiator == BattleWinner) return AggressorBattleAction;
                if (DefenderBattleAction != null && DefenderBattleAction.Initiator == BattleWinner) return DefenderBattleAction;

                return null;
            }
        }


        public Thought CurrentThought { get; private set; }
        public void HandleEvent(Thought e)
        {
            CurrentThought = e;
            var opponent = CurrentBattle.OpponentOf(e.Initiator);
            CurrentReport.Add(e.Initiator, "By {0}, {1} ask {2} if they have a {3}.", LeaderSkill.Thinker, e.Initiator, opponent, e.Card);

            Enter(Phase.Thought);
        }

        public void HandleEvent(ThoughtAnswered e)
        {
            CurrentReport.Add(e);
            if (e.Card == null)
            {
                CurrentReport.Add(e.Initiator, CurrentThought.Initiator, "In response, {0} say they don't own any cards.", e.Initiator);
            }
            else
            {
                CurrentReport.Add(e.Initiator, CurrentThought.Initiator, "In response, {0} show a {1}.", e.Initiator, e.Card);
                RegisterKnown(CurrentThought.Initiator, e.Card);
            }

            Enter(Phase.BattlePhase);
        }

        public void HandleEvent(HMSAdvantageChosen e)
        {
            CurrentReport.Add(e);
            ChosenHMSAdvantage = e.Advantage;
        }

        public Retreat CurrentRetreat { get; private set; } = null;
        public void HandleEvent(Retreat e)
        {
            CurrentRetreat = e;
        }
    }
}
