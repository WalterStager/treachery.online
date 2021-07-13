﻿/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public string _gameName { get; set; }

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

        public override string Validate()
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

            if (Players.Count() + extraSpotsForBots > FactionsInPlay.Count) return "More factions required";
            if (ApplicableRules.Contains(Rule.FillWithBots) && FactionsInPlay.Count < MaximumNumberOfPlayers) return "More factions required";

            int nrOfBots =
                (ApplicableRules.Contains(Rule.PurpleBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlackBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.OrangeBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.RedBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreenBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.YellowBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreyBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlueBot) ? 1 : 0);

            if (Players.Count() + nrOfBots == 0 && !ApplicableRules.Contains(Rule.FillWithBots)) return "At least one player required";
            if (Players.Count() + nrOfBots > MaximumNumberOfPlayers) return "Too many players";

            return "";
        }

        public static int GetMaximumNumberOfPlayers()
        {
            return 8;
        }

        public static int GetMaximumNumberOfTurns()
        {
            return 20;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override string Execute(bool performValidation, bool isHost)
        {
            if (performValidation)
            {
                var result = Validate();
                if (result == "")
                {
                    ExecuteConcreteEvent();
                    Game.PerformPostEventTasks(this);
                }
                return result;
            }
            else
            {
                ExecuteConcreteEvent();
                Game.PerformPostEventTasks(this);
                return "";
            }
        }

        public static IEnumerable<Faction> AvailableFactions()
        {
            return new Faction[] { Faction.Green, Faction.Black, Faction.Yellow, Faction.Red, Faction.Orange, Faction.Blue, Faction.Grey, Faction.Purple, Faction.Brown };
            //return new Faction[] { Faction.Green, Faction.Black, Faction.Yellow, Faction.Red, Faction.Orange, Faction.Blue, Faction.Grey, Faction.Purple };
        }

        public static IEnumerable<Ruleset> AvailableRulesets()
        {
            return Enumerations.GetValuesExceptDefault(typeof(Ruleset), Ruleset.None);
            
            /*return new Ruleset[] { 
                Ruleset.BasicGame, 
                Ruleset.AdvancedGame, 
                Ruleset.ExpansionBasicGame, 
                Ruleset.ExpansionAdvancedGame, 
                Ruleset.ServerClassic, 
                Ruleset.Custom };*/
            
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

            /*
            return new RuleGroup[]
            {
                RuleGroup.CoreAdvanced,
                RuleGroup.CoreBasicExceptions,
                RuleGroup.CoreAdvancedExceptions,

                RuleGroup.ExpansionIxAndBtBasic,
                RuleGroup.ExpansionIxAndBtAdvanced,

                RuleGroup.House,
            };
            */
        }

    }
}
