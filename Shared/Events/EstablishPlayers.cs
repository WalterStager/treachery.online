﻿/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class EstablishPlayers : GameEvent
    {
        public string _players = "";

        public int Seed { get; set; }

        public int MaximumNumberOfPlayers { get; set; } = 6;

        public int MaximumTurns { get; set; } = 10;

        public Rule[] ApplicableRules { get; set; }

        public string _factionsInPlay = "";

        public string _gameName = "";

        [JsonIgnore]
        public string GameName
        {
            get
            {
                if (_gameName == null || _gameName == "")
                {
                    return string.Format("{0}'s Game", Players.FirstOrDefault());
                }
                else
                {
                    return _gameName;
                }
            }

            set
            {
                _gameName = value;
            }
        }

        public EstablishPlayers(Game game) : base(game)
        {
        }

        public EstablishPlayers()
        {
        }

        [JsonIgnore]
        public IEnumerable<string> Players
        {
            get
            {
                if (_players == "")
                {
                    return new string[] { };
                }
                else
                {
                    return _players.Split('>');
                }
            }
            set
            {
                _players = string.Join('>', value);
            }
        }

        [JsonIgnore]
        public List<Faction> FactionsInPlay
        {
            get
            {
                if (_factionsInPlay == null || _factionsInPlay.Length == 0)
                {
                    return new List<Faction>();
                }
                else
                {
                    return _factionsInPlay.Split(',').Select(f => Enum.Parse<Faction>(f)).ToList();
                }
            }
            set
            {
                _factionsInPlay = string.Join(',', value);
            }
        }

        public override Message Validate()
        {
            int extraSpotsForBots =
                (ApplicableRules.Contains(Rule.PurpleBot) && FactionsInPlay.Contains(Faction.Purple) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlackBot) && FactionsInPlay.Contains(Faction.Black) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.OrangeBot) && FactionsInPlay.Contains(Faction.Orange) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.RedBot) && FactionsInPlay.Contains(Faction.Red) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreenBot) && FactionsInPlay.Contains(Faction.Green) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlueBot) && FactionsInPlay.Contains(Faction.Blue) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.YellowBot) && FactionsInPlay.Contains(Faction.Yellow) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreyBot) && FactionsInPlay.Contains(Faction.Grey) ? 1 : 0);

            if (Players.Count() + extraSpotsForBots > FactionsInPlay.Count) return Message.Express("More factions required");
            if (ApplicableRules.Contains(Rule.FillWithBots) && FactionsInPlay.Count < MaximumNumberOfPlayers) return Message.Express("More factions required");

            int nrOfBots =
                (ApplicableRules.Contains(Rule.PurpleBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlackBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.OrangeBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.RedBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreenBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.YellowBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreyBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlueBot) ? 1 : 0);

            if (Players.Count() + nrOfBots == 0 && !ApplicableRules.Contains(Rule.FillWithBots)) return Message.Express("At least one player required");
            if (Players.Count() + nrOfBots < 2 && !ApplicableRules.Contains(Rule.FillWithBots)) return Message.Express("At least two players required");
            if (MaximumNumberOfPlayers < 2) return Message.Express("At least two players required");
            if (Players.Count() + nrOfBots > MaximumNumberOfPlayers) return Message.Express("Too many players");
            if (FactionsInPlay.Any(f => !AvailableFactions(Game).Contains(f))) return Message.Express("Invalid faction");

            return null;
        }

        public static int GetMaximumNumberOfPlayers(Game g)
        {
            return AvailableFactions(g).Count();
        }

        public static int GetMaximumNumberOfTurns()
        {
            return 20;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message Execute(bool performValidation, bool isHost)
        {
            if (performValidation)
            {
                var result = Validate();
                if (result == null)
                {
                    Game.PerformPreEventTasks(this);
                    ExecuteConcreteEvent();
                    Game.PerformPostEventTasks(this, true);
                }
                return result;
            }
            else
            {
                Game.PerformPreEventTasks(this);
                ExecuteConcreteEvent();
                Game.PerformPostEventTasks(this, true);
                return null;
            }
        }

        public static IEnumerable<Faction> AvailableFactions(Game g)
        {
            var result = new List<Faction>();

            if (g.ExpansionLevel >= 0)
            {
                result.Add(Faction.Green);
                result.Add(Faction.Black);
                result.Add(Faction.Yellow);
                result.Add(Faction.Red);
                result.Add(Faction.Orange);
                result.Add(Faction.Blue);
            }

            if (g.ExpansionLevel >= 1)
            {
                result.Add(Faction.Grey);
                result.Add(Faction.Purple);
            }

            if (g.ExpansionLevel >= 2)
            {
                result.Add(Faction.Brown);
                result.Add(Faction.White);
            }

            if (g.ExpansionLevel >= 3)
            {
                result.Add(Faction.Pink);
                result.Add(Faction.Cyan);
            }

            return result;
        }

        public static IEnumerable<Ruleset> AvailableRulesets()
        {
            return Enumerations.GetValuesExceptDefault(typeof(Ruleset), Ruleset.None);
        }

        public static IEnumerable<RuleGroup> AvailableRuleGroups()
        {
            return new RuleGroup[]
            {
                RuleGroup.CoreAdvanced,
                RuleGroup.CoreBasicExceptions,
                RuleGroup.CoreAdvancedExceptions,

                RuleGroup.ExpansionIxAndBtBasic,
                RuleGroup.ExpansionIxAndBtAdvanced,

                RuleGroup.ExpansionBrownAndWhiteBasic,
                RuleGroup.ExpansionBrownAndWhiteAdvanced,

                RuleGroup.House,
            };
        }
    }
}
