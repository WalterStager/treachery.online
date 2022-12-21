﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RockWasMelted : GameEvent
    {
        public RockWasMelted(Game game) : base(game)
        {
        }

        public RockWasMelted()
        {
        }

        public bool Kill { get; set; }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Kill)
            {
                return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to kill both leaders");
            }
            else
            {
                return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to reduce both leaders to 0 strength");
            }
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var plan = g.CurrentBattle.PlanOf(p);
            return plan != null && plan.Weapon != null && plan.Weapon.IsRockmelter;
        }
    }
}
