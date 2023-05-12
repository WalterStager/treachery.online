﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class TreacheryCalled : GameEvent
    {
        #region Construction

        public TreacheryCalled(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public TreacheryCalled()
        {
        }

        #endregion Construction

        #region Properties

        public bool TraitorCalled { get; set; }

        [JsonIgnore]
        public bool Succeeded => TraitorCalled && (Initiator != Faction.Black || !Game.BlackTraitorWasCancelled);

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!TraitorCalled) return null;

            if (!MayCallTreachery(Game, Player)) return Message.Express("You cannot call TREACHERY");

            return null;
        }

        public static bool MayCallTreachery(Game g, Player p)
        {
            if (g.AggressorPlan != null && g.DefenderPlan != null)
            {
                if (!g.CurrentBattle.Territory.IsHomeworld || p.IsNative(g.CurrentBattle.Territory))
                {
                    if (!g.DefenderPlan.Messiah)
                    {
                        if (g.AggressorPlan.Initiator == p.Faction && HasTraitor(g, p, g.DefenderPlan.Hero) ||
                             p.Is(Faction.Black) && p.Ally == g.AggressorPlan.Initiator && !g.Prevented(FactionAdvantage.BlackCallTraitorForAlly) && HasTraitor(g, p, g.DefenderPlan.Hero))

                        {
                            return true;
                        }
                    }

                    if (!g.AggressorPlan.Messiah)
                    {
                        if (g.DefenderPlan.Initiator == p.Faction && HasTraitor(g, p, g.AggressorPlan.Hero) ||
                             p.Is(Faction.Black) && p.Ally == g.DefenderPlan.Initiator && !g.Prevented(FactionAdvantage.BlackCallTraitorForAlly) && HasTraitor(g, p, g.AggressorPlan.Hero))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool HasTraitor(Game g, Player p, IHero h)
        {
            return p.Traitors.Any(t => t.IsTraitor(h)) || h != null && g.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && h.Faction == p.Faction;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Game.AggressorPlan.By(Initiator) || By(Faction.Black) && Game.AreAllies(Game.AggressorPlan.Initiator, Faction.Black))
            {
                Game.AggressorTraitorAction = this;
                if (TraitorCalled)
                {
                    Log();
                    Game.Stone(Milestone.TreacheryCalled);
                    Player.RevealedTraitors.Add(Game.DefenderPlan.Hero);
                }
            }

            if (Game.DefenderPlan.By(Initiator) || By(Faction.Black) && Game.AreAllies(Game.DefenderPlan.Initiator, Faction.Black))
            {
                Game.DefenderTraitorAction = this;
                if (TraitorCalled)
                {
                    Log();
                    Game.Stone(Milestone.TreacheryCalled);
                    Player.RevealedTraitors.Add(Game.AggressorPlan.Hero);
                }
            }

            if (Game.AggressorTraitorAction != null && Game.DefenderTraitorAction != null)
            {
                var treachery = Game.CurrentBattle.TreacheryOf(Faction.Black);

                if (Game.Applicable(Rule.NexusCards) && treachery != null && treachery.Initiator == Faction.Black && treachery.TraitorCalled)
                {
                    Game.Enter(Phase.CancellingTraitor);
                }
                else
                {
                    Game.HandleRevealedBattlePlans();
                }
            }
        }

        public override Message GetMessage()
        {
            if (TraitorCalled)
            {
                return Message.Express(Initiator, " call TREACHERY!");
            }
            else
            {
                return Message.Express(Initiator, " don't call treachery");
            }
        }

        #endregion Execution
    }
}
