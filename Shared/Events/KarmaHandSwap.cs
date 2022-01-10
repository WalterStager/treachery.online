﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaHandSwap : GameEvent
    {
        public string _cardIds;

        public KarmaHandSwap(Game game) : base(game)
        {
        }

        public KarmaHandSwap()
        {
        }

        [JsonIgnore]
        public IEnumerable<TreacheryCard> ReturnedCards
        {
            get
            {
                return IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public override string Validate()
        {
            if (ReturnedCards.Count() != Game.KarmaHandSwapNumberOfCards) return string.Format("Select {0} cards to return", Game.KarmaHandSwapNumberOfCards);

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " return ", ReturnedCards.Count(), " cards");
        }
    }
}
