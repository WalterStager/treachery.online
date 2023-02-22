﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Map
    {
        public const int NUMBER_OF_SECTORS = 18;
        private List<Location> _locations;

        public readonly LocationFetcher LocationLookup;
        public readonly TerritoryFetcher TerritoryLookup;

        public Location TheGreaterFlat { get; set; }

        public Location PolarSink { get; set; }

        public Location Arrakeen { get; set; }

        public Location Carthag { get; set; }

        public Location TueksSietch { get; set; }

        public Location SietchTabr { get; set; }

        public Location HabbanyaSietch { get; set; }

        public Territory FalseWallSouth { get; set; }

        public Territory Meridan { get; set; }

        public Territory FalseWallWest { get; set; }

        public Territory ImperialBasin { get; set; }

        public Territory ShieldWall { get; set; }

        public Territory HoleInTheRock { get; set; }

        public Territory FalseWallEast { get; set; }

        public Territory TheMinorErg { get; set; }

        public Territory PastyMesa { get; set; }

        public Territory GaraKulon { get; set; }

        public Territory OldGap { get; set; }

        public Territory SihayaRidge { get; set; }

        public Location FuneralPlain { get; set; }

        public Territory BightOfTheCliff { get; set; }

        public Territory PlasticBasin { get; set; }

        public Territory RockOutcroppings { get; set; }

        public Territory BrokenLand { get; set; }

        public Territory Tsimpo { get; set; }

        public Territory HaggaBasin { get; set; }

        public Territory WindPass { get; set; }

        public Territory WindPassNorth { get; set; }

        public Territory CielagoEast { get; set; }

        public Territory CielagoWest { get; set; }

        public Territory HabbanyaErg { get; private set; }

        public Location TheGreatFlat { get; set; }

        public HiddenMobileStronghold HiddenMobileStronghold { get; set; }

        public Map()
        {
            LocationLookup = new LocationFetcher(this);
            TerritoryLookup = new TerritoryFetcher(this);
            Initialize();
        }

        public void Initialize()
        {
            InitializeLocations();
            InitializeLocationNeighbours();
        }

        public IEnumerable<Location> Locations(bool includeHomeworlds) => _locations.Where(l => includeHomeworlds || l is not Homeworld);

        public IEnumerable<Homeworld> Homeworlds => _locations.Where(l => l is Homeworld).Select(l => l as Homeworld);

        public IEnumerable<Territory> Territories(bool includeHomeworlds) => Locations(includeHomeworlds).Select(l => l.Territory).Distinct();

        public IEnumerable<Location> Strongholds => _locations.Where(l => l.Territory.IsStronghold);

        public static IEnumerable<ResourceCard> GetResourceCardsInAndOutsidePlay(Map m)
        {
            var result = new List<ResourceCard>();
            foreach (var location in m._locations.Where(l => l.SpiceBlowAmount > 0))
            {
                result.Add(new ResourceCard(location.Territory.Id) { Location = location });
            }

            for (int i = 1; i <= 6; i++)
            {
                result.Add(new ResourceCard(98));
            }

            result.Add(new ResourceCard(99) { IsSandTrout = true });

            result.Add(new ResourceCard(100) { IsGreatMaker = true });

            result.Add(new ResourceCard(40) { Location = m.SihayaRidge.ResourceBlowLocation, DiscoveryLocation = m.CielagoEast.DiscoveryTokenLocation });
            result.Add(new ResourceCard(41) { Location = m.RockOutcroppings.ResourceBlowLocation, DiscoveryLocation = m.Meridan.DiscoveryTokenLocation });
            result.Add(new ResourceCard(42) { Location = m.HaggaBasin.ResourceBlowLocation, DiscoveryLocation = m.GaraKulon.DiscoveryTokenLocation });
            result.Add(new ResourceCard(43) { Location = m.FuneralPlain, DiscoveryLocation = m.PastyMesa.DiscoveryTokenLocation });
            result.Add(new ResourceCard(44) { Location = m.WindPassNorth.ResourceBlowLocation, DiscoveryLocation = m.PlasticBasin.DiscoveryTokenLocation });
            result.Add(new ResourceCard(45) { Location = m.OldGap.ResourceBlowLocation, DiscoveryLocation = m.FalseWallWest.DiscoveryTokenLocation });

            return result;
        }

        public static IEnumerable<ResourceCard> GetResourceCardsInPlay(Game g)
        {
            var result = new List<ResourceCard>();
            foreach (var location in g.Map._locations.Where(l => l.SpiceBlowAmount > 0))
            {
                result.Add(new ResourceCard(location.Territory.Id) { Location = location });
            }

            for (int i = 1; i <= 6; i++)
            {
                result.Add(new ResourceCard(98));
            }

            if (g.Applicable(Rule.SandTrout))
            {
                result.Add(new ResourceCard(99) { IsSandTrout = true });
            }

            return result;
        }

        private void InitializeLocations()
        {
            int id = 0;
            _locations = new();

            {
                var t = new Territory(0)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                PolarSink = (new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = -1,
                    SpiceBlowAmount = 0
                });
                _locations.Add(PolarSink);
            }

            {
                ImperialBasin = new Territory(1)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {//1
                    Territory = ImperialBasin,
                    Orientation = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {//2
                    Territory = ImperialBasin,
                    Orientation = "Center",
                    Sector = 9,
                    SpiceBlowAmount = 0

                });
                _locations.Add(new Location(id++)
                {//3
                    Territory = ImperialBasin,
                    Orientation = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(2)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Carthag = new Location(id++)
                {//4
                    Territory = t,
                    Orientation = "",
                    Sector = 10,
                    SpiceBlowAmount = 0
                };
                _locations.Add(Carthag);
            }

            {
                var t = new Territory(3)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Arrakeen = new Location(id++)
                {//5
                    Territory = t,
                    Orientation = "",
                    Sector = 9,
                    SpiceBlowAmount = 0

                };
                _locations.Add(Arrakeen);
            }

            {
                var t = new Territory(4)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                TueksSietch = new Location(id++)
                {//6
                    Territory = t,
                    Orientation = "",
                    Sector = 4,
                    SpiceBlowAmount = 0
                };
                _locations.Add(TueksSietch);
            }

            {
                var t = new Territory(5)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                SietchTabr = new Location(id++)
                {//7
                    Territory = t,
                    Orientation = "",
                    Sector = 13,
                    SpiceBlowAmount = 0
                };
                _locations.Add(SietchTabr);
            }

            {
                var t = new Territory(6)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                HabbanyaSietch = new Location(id++)
                {//8
                    Territory = t,
                    Orientation = "",
                    Sector = 16,
                    SpiceBlowAmount = 0
                };
                _locations.Add(HabbanyaSietch);
            }

            {
                var t = new Territory(7)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 2,
                    SpiceBlowAmount = 8
                });
            }

            {
                var t = new Territory(8)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0
                });
            }

            {
                Meridan = new Territory(9)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = Meridan,
                    Orientation = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Hiereg
                });
                _locations.Add(new Location(id++)
                {
                    Territory = Meridan,
                    Orientation = "East",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(10)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 1,
                    SpiceBlowAmount = 12
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0
                });
            }

            {
                CielagoEast = new Territory(11)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = CielagoEast,
                    Orientation = "West",
                    Sector = 2,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Hiereg
                });
                _locations.Add(new Location(id++)
                {
                    Territory = CielagoEast,
                    Orientation = "East",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(12)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallSouth = new Territory(13)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Orientation = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Orientation = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallEast = new Territory(14)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Orientation = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Orientation = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Orientation = "Middle",
                    Sector = 6,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Orientation = "North",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Orientation = "Far North",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                TheMinorErg = new Territory(15)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Orientation = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Orientation = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Orientation = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Orientation = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 8
                });
            }

            {
                PastyMesa = new Territory(16)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Orientation = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Orientation = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Orientation = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Smuggler
                });
                _locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Orientation = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(17)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 6,
                    SpiceBlowAmount = 8
                });
            }

            {
                var t = new Territory(18)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "South",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "Middle",
                    Sector = 4,
                    SpiceBlowAmount = 10
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "North",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(19)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(20)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                HoleInTheRock = new Territory(21)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = HoleInTheRock,
                    Orientation = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                SihayaRidge = new Territory(22)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = SihayaRidge,
                    Orientation = "",
                    Sector = 8,
                    SpiceBlowAmount = 6
                });
            }

            {
                ShieldWall = new Territory(23)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Orientation = "South",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Orientation = "North",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                GaraKulon = new Territory(24)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = GaraKulon,
                    Orientation = "",
                    Sector = 7,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Hiereg
                });
            }

            {
                OldGap = new Territory(25)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = OldGap,
                    Orientation = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = OldGap,
                    Orientation = "Middle",
                    Sector = 9,
                    SpiceBlowAmount = 6
                });
                _locations.Add(new Location(id++)
                {
                    Territory = OldGap,
                    Orientation = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
            }

            {
                BrokenLand = new Territory(26)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Orientation = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Orientation = "West",
                    Sector = 11,
                    SpiceBlowAmount = 8
                });
            }

            {
                Tsimpo = new Territory(27)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Orientation = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Orientation = "Middle",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Orientation = "West",
                    Sector = 12,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(28)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
            }

            {
                RockOutcroppings = new Territory(29)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Orientation = "North",
                    Sector = 12,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Orientation = "South",
                    Sector = 13,
                    SpiceBlowAmount = 6
                });
            }

            {
                PlasticBasin = new Territory(30)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Orientation = "North",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Orientation = "Middle",
                    Sector = 12,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Smuggler
                });
                _locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Orientation = "South",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
            }

            {
                HaggaBasin = new Territory(31)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Orientation = "East",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Orientation = "West",
                    Sector = 12,
                    SpiceBlowAmount = 6
                });
            }

            {
                BightOfTheCliff = new Territory(32)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Orientation = "North",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Orientation = "South",
                    Sector = 14,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(33)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                FuneralPlain = new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 14,
                    SpiceBlowAmount = 6
                };
                _locations.Add(FuneralPlain);
            }

            {
                var t = new Territory(34)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                TheGreatFlat = new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 14,
                    SpiceBlowAmount = 10
                };
                _locations.Add(TheGreatFlat);
            }

            {
                WindPass = new Territory(35)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Orientation = "Far North",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Orientation = "North",
                    Sector = 14,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Orientation = "South",
                    Sector = 15,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Orientation = "Far South",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(36)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                TheGreaterFlat = new Location(id++)
                {
                    Territory = t,
                    Orientation = "",
                    Sector = 15,
                    SpiceBlowAmount = 0
                };

                _locations.Add(TheGreaterFlat);
            }

            {
                HabbanyaErg = new Territory(37)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Orientation = "West",
                    Sector = 15,
                    SpiceBlowAmount = 8
                });
                _locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Orientation = "East",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallWest = new Territory(38)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Orientation = "North",
                    Sector = 15,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Orientation = "Middle",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    TokenType = DiscoveryTokenType.Smuggler
                });
                _locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Orientation = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
            }

            {
                WindPassNorth = new Territory(39)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Orientation = "North",
                    Sector = 16,
                    SpiceBlowAmount = 6
                });
                _locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Orientation = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(40)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "West",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = t,
                    Orientation = "East",
                    Sector = 17,
                    SpiceBlowAmount = 10
                });
            }

            {
                CielagoWest = new Territory(41)
                {
                    IsHomeworld = false,
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                _locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Orientation = "North",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
                _locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Orientation = "South",
                    Sector = 00,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(42)
                {
                    IsHomeworld = false,
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };

                HiddenMobileStronghold = new HiddenMobileStronghold(t, id++) { SpiceBlowAmount = 0 };
                _locations.Add(HiddenMobileStronghold);
            }

            AddHomeworld(43, id++, World.Green, Faction.Green, true, false, 6, 2, 2);
            AddHomeworld(44, id++, World.Black, Faction.Black, true, false, 7, 2, 2);
            AddHomeworld(45, id++, World.Yellow, Faction.Yellow, true, true, 3, 2, 2);
            AddHomeworld(46, id++, World.Red, Faction.Red, true, false, 5, 2, 3);
            AddHomeworld(47, id++, World.RedStar, Faction.Red, false, true, 2, 3, 2);
            AddHomeworld(48, id++, World.Orange, Faction.Orange, true, false, 5, 2, 2);
            AddHomeworld(49, id++, World.Blue, Faction.Blue, true, false, 11, 3, 2);
            AddHomeworld(50, id++, World.Grey, Faction.Grey, true, true, 5, 2, 2);
            AddHomeworld(51, id++, World.Purple, Faction.Purple, true, false, 9, 2, 2);
            AddHomeworld(52, id++, World.Brown, Faction.Brown, true, false, 11, 2, 2);
            AddHomeworld(53, id++, World.White, Faction.White, true, false, 10, 2, 2);
            AddHomeworld(54, id++, World.Pink, Faction.Pink, true, false, 7, 2, 2);
            AddHomeworld(55, id++, World.Cyan, Faction.Cyan, true, false, 8, 2, 2);
        }

        private void AddHomeworld(int territoryId, int locationId, World world, Faction faction, bool isHomeOfNormalForces, bool isHomeOfSpecialForces, int threshold, int battleBonusAtHighThreshold, int battleBonusAtLowThreshold)
        {
            _locations.Add(new Homeworld(world, faction, new Territory(territoryId) { IsHomeworld = true, IsStronghold = false, IsProtectedFromStorm = false, IsProtectedFromWorm = false }, isHomeOfNormalForces, isHomeOfSpecialForces, threshold, battleBonusAtHighThreshold, battleBonusAtLowThreshold, locationId));
        }

        public void InitializeLocationNeighbours()
        {
            _locations[5].Neighbours.Add(_locations[2]);
            _locations[5].Neighbours.Add(_locations[50]);
            _locations[5].Neighbours.Add(_locations[43]);
            _locations[57].Neighbours.Add(_locations[58]);
            _locations[57].Neighbours.Add(_locations[64]);
            _locations[57].Neighbours.Add(_locations[2]);
            _locations[57].Neighbours.Add(_locations[3]);
            _locations[57].Neighbours.Add(_locations[0]);
            _locations[57].Neighbours.Add(_locations[4]);
            _locations[14].Neighbours.Add(_locations[19]);
            _locations[14].Neighbours.Add(_locations[11]);
            _locations[14].Neighbours.Add(_locations[18]);
            _locations[14].Neighbours.Add(_locations[13]);
            _locations[12].Neighbours.Add(_locations[9]);
            _locations[12].Neighbours.Add(_locations[85]);
            _locations[12].Neighbours.Add(_locations[15]);
            _locations[12].Neighbours.Add(_locations[13]);
            _locations[20].Neighbours.Add(_locations[19]);
            _locations[20].Neighbours.Add(_locations[23]);
            _locations[20].Neighbours.Add(_locations[39]);
            _locations[19].Neighbours.Add(_locations[14]);
            _locations[19].Neighbours.Add(_locations[20]);
            _locations[19].Neighbours.Add(_locations[11]);
            _locations[19].Neighbours.Add(_locations[18]);
            _locations[19].Neighbours.Add(_locations[23]);
            _locations[10].Neighbours.Add(_locations[11]);
            _locations[10].Neighbours.Add(_locations[9]);
            _locations[10].Neighbours.Add(_locations[0]);
            _locations[10].Neighbours.Add(_locations[13]);
            _locations[11].Neighbours.Add(_locations[14]);
            _locations[11].Neighbours.Add(_locations[19]);
            _locations[11].Neighbours.Add(_locations[10]);
            _locations[11].Neighbours.Add(_locations[23]);
            _locations[11].Neighbours.Add(_locations[21]);
            _locations[11].Neighbours.Add(_locations[0]);
            _locations[9].Neighbours.Add(_locations[12]);
            _locations[9].Neighbours.Add(_locations[10]);
            _locations[9].Neighbours.Add(_locations[84]);
            _locations[9].Neighbours.Add(_locations[85]);
            _locations[9].Neighbours.Add(_locations[0]);
            _locations[9].Neighbours.Add(_locations[81]);
            _locations[18].Neighbours.Add(_locations[14]);
            _locations[18].Neighbours.Add(_locations[19]);
            _locations[18].Neighbours.Add(_locations[17]);
            _locations[18].Neighbours.Add(_locations[13]);
            _locations[17].Neighbours.Add(_locations[18]);
            _locations[17].Neighbours.Add(_locations[16]);
            _locations[17].Neighbours.Add(_locations[13]);
            _locations[84].Neighbours.Add(_locations[9]);
            _locations[84].Neighbours.Add(_locations[85]);
            _locations[84].Neighbours.Add(_locations[79]);
            _locations[84].Neighbours.Add(_locations[83]);
            _locations[84].Neighbours.Add(_locations[73]);
            _locations[84].Neighbours.Add(_locations[81]);
            _locations[58].Neighbours.Add(_locations[57]);
            _locations[58].Neighbours.Add(_locations[64]);
            _locations[58].Neighbours.Add(_locations[65]);
            _locations[58].Neighbours.Add(_locations[0]);
            _locations[85].Neighbours.Add(_locations[12]);
            _locations[85].Neighbours.Add(_locations[9]);
            _locations[85].Neighbours.Add(_locations[84]);
            _locations[85].Neighbours.Add(_locations[15]);
            _locations[29].Neighbours.Add(_locations[28]);
            _locations[29].Neighbours.Add(_locations[1]);
            _locations[29].Neighbours.Add(_locations[0]);
            _locations[29].Neighbours.Add(_locations[47]);
            _locations[29].Neighbours.Add(_locations[46]);
            _locations[25].Neighbours.Add(_locations[26]);
            _locations[25].Neighbours.Add(_locations[22]);
            _locations[25].Neighbours.Add(_locations[21]);
            _locations[25].Neighbours.Add(_locations[0]);
            _locations[25].Neighbours.Add(_locations[30]);
            _locations[27].Neighbours.Add(_locations[28]);
            _locations[27].Neighbours.Add(_locations[26]);
            _locations[27].Neighbours.Add(_locations[0]);
            _locations[27].Neighbours.Add(_locations[32]);
            _locations[28].Neighbours.Add(_locations[29]);
            _locations[28].Neighbours.Add(_locations[27]);
            _locations[28].Neighbours.Add(_locations[0]);
            _locations[28].Neighbours.Add(_locations[46]);
            _locations[28].Neighbours.Add(_locations[33]);
            _locations[26].Neighbours.Add(_locations[25]);
            _locations[26].Neighbours.Add(_locations[27]);
            _locations[26].Neighbours.Add(_locations[0]);
            _locations[26].Neighbours.Add(_locations[31]);
            _locations[24].Neighbours.Add(_locations[23]);
            _locations[24].Neighbours.Add(_locations[22]);
            _locations[24].Neighbours.Add(_locations[34]);
            _locations[24].Neighbours.Add(_locations[40]);
            _locations[24].Neighbours.Add(_locations[30]);
            _locations[24].Neighbours.Add(_locations[6]);
            _locations[23].Neighbours.Add(_locations[20]);
            _locations[23].Neighbours.Add(_locations[19]);
            _locations[23].Neighbours.Add(_locations[11]);
            _locations[23].Neighbours.Add(_locations[24]);
            _locations[23].Neighbours.Add(_locations[21]);
            _locations[23].Neighbours.Add(_locations[39]);
            _locations[78].Neighbours.Add(_locations[77]);
            _locations[78].Neighbours.Add(_locations[79]);
            _locations[78].Neighbours.Add(_locations[76]);
            _locations[78].Neighbours.Add(_locations[82]);
            _locations[78].Neighbours.Add(_locations[73]);
            _locations[77].Neighbours.Add(_locations[78]);
            _locations[77].Neighbours.Add(_locations[74]);
            _locations[77].Neighbours.Add(_locations[72]);
            _locations[42].Neighbours.Add(_locations[44]);
            _locations[42].Neighbours.Add(_locations[49]);
            _locations[42].Neighbours.Add(_locations[43]);
            _locations[42].Neighbours.Add(_locations[45]);
            _locations[79].Neighbours.Add(_locations[84]);
            _locations[79].Neighbours.Add(_locations[78]);
            _locations[79].Neighbours.Add(_locations[83]);
            _locations[68].Neighbours.Add(_locations[67]);
            _locations[68].Neighbours.Add(_locations[63]);
            _locations[68].Neighbours.Add(_locations[69]);
            _locations[48].Neighbours.Add(_locations[37]);
            _locations[48].Neighbours.Add(_locations[46]);
            _locations[48].Neighbours.Add(_locations[45]);
            _locations[76].Neighbours.Add(_locations[78]);
            _locations[76].Neighbours.Add(_locations[75]);
            _locations[76].Neighbours.Add(_locations[82]);
            _locations[75].Neighbours.Add(_locations[76]);
            _locations[75].Neighbours.Add(_locations[82]);
            _locations[75].Neighbours.Add(_locations[74]);
            _locations[83].Neighbours.Add(_locations[84]);
            _locations[83].Neighbours.Add(_locations[82]);
            _locations[83].Neighbours.Add(_locations[8]);
            _locations[83].Neighbours.Add(_locations[15]);
            _locations[82].Neighbours.Add(_locations[78]);
            _locations[82].Neighbours.Add(_locations[76]);
            _locations[82].Neighbours.Add(_locations[75]);
            _locations[82].Neighbours.Add(_locations[83]);
            _locations[82].Neighbours.Add(_locations[8]);
            _locations[8].Neighbours.Add(_locations[83]);
            _locations[8].Neighbours.Add(_locations[82]);
            _locations[64].Neighbours.Add(_locations[57]);
            _locations[64].Neighbours.Add(_locations[58]);
            _locations[64].Neighbours.Add(_locations[65]);
            _locations[64].Neighbours.Add(_locations[55]);
            _locations[64].Neighbours.Add(_locations[4]);
            _locations[65].Neighbours.Add(_locations[58]);
            _locations[65].Neighbours.Add(_locations[64]);
            _locations[65].Neighbours.Add(_locations[62]);
            _locations[65].Neighbours.Add(_locations[63]);
            _locations[65].Neighbours.Add(_locations[0]);
            _locations[65].Neighbours.Add(_locations[56]);
            _locations[65].Neighbours.Add(_locations[70]);
            _locations[66].Neighbours.Add(_locations[67]);
            _locations[66].Neighbours.Add(_locations[63]);
            _locations[66].Neighbours.Add(_locations[60]);
            _locations[66].Neighbours.Add(_locations[7]);
            _locations[22].Neighbours.Add(_locations[25]);
            _locations[22].Neighbours.Add(_locations[24]);
            _locations[22].Neighbours.Add(_locations[21]);
            _locations[22].Neighbours.Add(_locations[30]);
            _locations[21].Neighbours.Add(_locations[11]);
            _locations[21].Neighbours.Add(_locations[25]);
            _locations[21].Neighbours.Add(_locations[23]);
            _locations[21].Neighbours.Add(_locations[22]);
            _locations[21].Neighbours.Add(_locations[0]);
            _locations[44].Neighbours.Add(_locations[42]);
            _locations[44].Neighbours.Add(_locations[1]);
            _locations[44].Neighbours.Add(_locations[43]);
            _locations[44].Neighbours.Add(_locations[47]);
            _locations[44].Neighbours.Add(_locations[45]);
            _locations[2].Neighbours.Add(_locations[5]);
            _locations[2].Neighbours.Add(_locations[57]);
            _locations[2].Neighbours.Add(_locations[1]);
            _locations[2].Neighbours.Add(_locations[3]);
            _locations[2].Neighbours.Add(_locations[50]);
            _locations[2].Neighbours.Add(_locations[0]);
            _locations[2].Neighbours.Add(_locations[43]);
            _locations[1].Neighbours.Add(_locations[29]);
            _locations[1].Neighbours.Add(_locations[44]);
            _locations[1].Neighbours.Add(_locations[2]);
            _locations[1].Neighbours.Add(_locations[0]);
            _locations[1].Neighbours.Add(_locations[43]);
            _locations[1].Neighbours.Add(_locations[47]);
            _locations[3].Neighbours.Add(_locations[57]);
            _locations[3].Neighbours.Add(_locations[2]);
            _locations[3].Neighbours.Add(_locations[54]);
            _locations[3].Neighbours.Add(_locations[4]);
            _locations[16].Neighbours.Add(_locations[17]);
            _locations[16].Neighbours.Add(_locations[15]);
            _locations[16].Neighbours.Add(_locations[13]);
            _locations[15].Neighbours.Add(_locations[12]);
            _locations[15].Neighbours.Add(_locations[85]);
            _locations[15].Neighbours.Add(_locations[83]);
            _locations[15].Neighbours.Add(_locations[16]);
            _locations[49].Neighbours.Add(_locations[42]);
            _locations[49].Neighbours.Add(_locations[50]);
            _locations[49].Neighbours.Add(_locations[43]);
            _locations[50].Neighbours.Add(_locations[5]);
            _locations[50].Neighbours.Add(_locations[2]);
            _locations[50].Neighbours.Add(_locations[49]);
            _locations[50].Neighbours.Add(_locations[51]);
            _locations[67].Neighbours.Add(_locations[68]);
            _locations[67].Neighbours.Add(_locations[66]);
            _locations[51].Neighbours.Add(_locations[50]);
            _locations[51].Neighbours.Add(_locations[52]);
            _locations[51].Neighbours.Add(_locations[54]);
            _locations[37].Neighbours.Add(_locations[48]);
            _locations[37].Neighbours.Add(_locations[36]);
            _locations[37].Neighbours.Add(_locations[46]);
            _locations[37].Neighbours.Add(_locations[33]);
            _locations[34].Neighbours.Add(_locations[24]);
            _locations[34].Neighbours.Add(_locations[35]);
            _locations[34].Neighbours.Add(_locations[40]);
            _locations[34].Neighbours.Add(_locations[30]);
            _locations[34].Neighbours.Add(_locations[6]);
            _locations[36].Neighbours.Add(_locations[37]);
            _locations[36].Neighbours.Add(_locations[35]);
            _locations[36].Neighbours.Add(_locations[38]);
            _locations[36].Neighbours.Add(_locations[32]);
            _locations[35].Neighbours.Add(_locations[34]);
            _locations[35].Neighbours.Add(_locations[36]);
            _locations[35].Neighbours.Add(_locations[41]);
            _locations[35].Neighbours.Add(_locations[31]);
            _locations[62].Neighbours.Add(_locations[65]);
            _locations[62].Neighbours.Add(_locations[61]);
            _locations[62].Neighbours.Add(_locations[63]);
            _locations[62].Neighbours.Add(_locations[59]);
            _locations[62].Neighbours.Add(_locations[56]);
            _locations[61].Neighbours.Add(_locations[62]);
            _locations[61].Neighbours.Add(_locations[53]);
            _locations[61].Neighbours.Add(_locations[55]);
            _locations[63].Neighbours.Add(_locations[68]);
            _locations[63].Neighbours.Add(_locations[65]);
            _locations[63].Neighbours.Add(_locations[66]);
            _locations[63].Neighbours.Add(_locations[62]);
            _locations[63].Neighbours.Add(_locations[60]);
            _locations[63].Neighbours.Add(_locations[7]);
            _locations[63].Neighbours.Add(_locations[69]);
            _locations[63].Neighbours.Add(_locations[70]);
            _locations[0].Neighbours.Add(_locations[57]);
            _locations[0].Neighbours.Add(_locations[10]);
            _locations[0].Neighbours.Add(_locations[11]);
            _locations[0].Neighbours.Add(_locations[9]);
            _locations[0].Neighbours.Add(_locations[58]);
            _locations[0].Neighbours.Add(_locations[29]);
            _locations[0].Neighbours.Add(_locations[25]);
            _locations[0].Neighbours.Add(_locations[27]);
            _locations[0].Neighbours.Add(_locations[28]);
            _locations[0].Neighbours.Add(_locations[26]);
            _locations[0].Neighbours.Add(_locations[65]);
            _locations[0].Neighbours.Add(_locations[21]);
            _locations[0].Neighbours.Add(_locations[2]);
            _locations[0].Neighbours.Add(_locations[1]);
            _locations[0].Neighbours.Add(_locations[70]);
            _locations[0].Neighbours.Add(_locations[71]);
            _locations[0].Neighbours.Add(_locations[72]);
            _locations[0].Neighbours.Add(_locations[80]);
            _locations[0].Neighbours.Add(_locations[81]);
            _locations[38].Neighbours.Add(_locations[36]);
            _locations[38].Neighbours.Add(_locations[41]);
            _locations[52].Neighbours.Add(_locations[51]);
            _locations[52].Neighbours.Add(_locations[53]);
            _locations[52].Neighbours.Add(_locations[54]);
            _locations[43].Neighbours.Add(_locations[5]);
            _locations[43].Neighbours.Add(_locations[42]);
            _locations[43].Neighbours.Add(_locations[44]);
            _locations[43].Neighbours.Add(_locations[2]);
            _locations[43].Neighbours.Add(_locations[1]);
            _locations[43].Neighbours.Add(_locations[49]);
            _locations[59].Neighbours.Add(_locations[62]);
            _locations[59].Neighbours.Add(_locations[60]);
            _locations[59].Neighbours.Add(_locations[53]);
            _locations[60].Neighbours.Add(_locations[66]);
            _locations[60].Neighbours.Add(_locations[63]);
            _locations[60].Neighbours.Add(_locations[59]);
            _locations[60].Neighbours.Add(_locations[7]);
            _locations[47].Neighbours.Add(_locations[29]);
            _locations[47].Neighbours.Add(_locations[44]);
            _locations[47].Neighbours.Add(_locations[1]);
            _locations[47].Neighbours.Add(_locations[46]);
            _locations[47].Neighbours.Add(_locations[45]);
            _locations[46].Neighbours.Add(_locations[28]);
            _locations[46].Neighbours.Add(_locations[48]);
            _locations[46].Neighbours.Add(_locations[37]);
            _locations[46].Neighbours.Add(_locations[47]);
            _locations[46].Neighbours.Add(_locations[33]);
            _locations[7].Neighbours.Add(_locations[66]);
            _locations[7].Neighbours.Add(_locations[63]);
            _locations[7].Neighbours.Add(_locations[60]);
            _locations[45].Neighbours.Add(_locations[42]);
            _locations[45].Neighbours.Add(_locations[48]);
            _locations[45].Neighbours.Add(_locations[44]);
            _locations[45].Neighbours.Add(_locations[47]);
            _locations[40].Neighbours.Add(_locations[24]);
            _locations[40].Neighbours.Add(_locations[34]);
            _locations[40].Neighbours.Add(_locations[41]);
            _locations[40].Neighbours.Add(_locations[39]);
            _locations[40].Neighbours.Add(_locations[6]);
            _locations[41].Neighbours.Add(_locations[35]);
            _locations[41].Neighbours.Add(_locations[38]);
            _locations[41].Neighbours.Add(_locations[40]);
            _locations[39].Neighbours.Add(_locations[20]);
            _locations[39].Neighbours.Add(_locations[23]);
            _locations[39].Neighbours.Add(_locations[40]);
            _locations[53].Neighbours.Add(_locations[61]);
            _locations[53].Neighbours.Add(_locations[52]);
            _locations[53].Neighbours.Add(_locations[59]);
            _locations[53].Neighbours.Add(_locations[55]);
            _locations[69].Neighbours.Add(_locations[68]);
            _locations[69].Neighbours.Add(_locations[63]);
            _locations[69].Neighbours.Add(_locations[74]);
            _locations[69].Neighbours.Add(_locations[71]);
            _locations[74].Neighbours.Add(_locations[77]);
            _locations[74].Neighbours.Add(_locations[75]);
            _locations[74].Neighbours.Add(_locations[69]);
            _locations[74].Neighbours.Add(_locations[72]);
            _locations[33].Neighbours.Add(_locations[28]);
            _locations[33].Neighbours.Add(_locations[37]);
            _locations[33].Neighbours.Add(_locations[46]);
            _locations[33].Neighbours.Add(_locations[32]);
            _locations[30].Neighbours.Add(_locations[25]);
            _locations[30].Neighbours.Add(_locations[24]);
            _locations[30].Neighbours.Add(_locations[22]);
            _locations[30].Neighbours.Add(_locations[34]);
            _locations[30].Neighbours.Add(_locations[31]);
            _locations[32].Neighbours.Add(_locations[27]);
            _locations[32].Neighbours.Add(_locations[36]);
            _locations[32].Neighbours.Add(_locations[33]);
            _locations[32].Neighbours.Add(_locations[31]);
            _locations[31].Neighbours.Add(_locations[26]);
            _locations[31].Neighbours.Add(_locations[35]);
            _locations[31].Neighbours.Add(_locations[30]);
            _locations[31].Neighbours.Add(_locations[32]);
            _locations[54].Neighbours.Add(_locations[3]);
            _locations[54].Neighbours.Add(_locations[51]);
            _locations[54].Neighbours.Add(_locations[52]);
            _locations[54].Neighbours.Add(_locations[55]);
            _locations[54].Neighbours.Add(_locations[4]);
            _locations[55].Neighbours.Add(_locations[64]);
            _locations[55].Neighbours.Add(_locations[61]);
            _locations[55].Neighbours.Add(_locations[53]);
            _locations[55].Neighbours.Add(_locations[54]);
            _locations[55].Neighbours.Add(_locations[56]);
            _locations[55].Neighbours.Add(_locations[4]);
            _locations[56].Neighbours.Add(_locations[65]);
            _locations[56].Neighbours.Add(_locations[62]);
            _locations[56].Neighbours.Add(_locations[55]);
            _locations[6].Neighbours.Add(_locations[24]);
            _locations[6].Neighbours.Add(_locations[34]);
            _locations[6].Neighbours.Add(_locations[40]);
            _locations[4].Neighbours.Add(_locations[57]);
            _locations[4].Neighbours.Add(_locations[64]);
            _locations[4].Neighbours.Add(_locations[3]);
            _locations[4].Neighbours.Add(_locations[54]);
            _locations[4].Neighbours.Add(_locations[55]);
            _locations[70].Neighbours.Add(_locations[65]);
            _locations[70].Neighbours.Add(_locations[63]);
            _locations[70].Neighbours.Add(_locations[0]);
            _locations[70].Neighbours.Add(_locations[71]);
            _locations[73].Neighbours.Add(_locations[84]);
            _locations[73].Neighbours.Add(_locations[78]);
            _locations[73].Neighbours.Add(_locations[72]);
            _locations[73].Neighbours.Add(_locations[80]);
            _locations[71].Neighbours.Add(_locations[0]);
            _locations[71].Neighbours.Add(_locations[69]);
            _locations[71].Neighbours.Add(_locations[70]);
            _locations[71].Neighbours.Add(_locations[72]);
            _locations[72].Neighbours.Add(_locations[77]);
            _locations[72].Neighbours.Add(_locations[0]);
            _locations[72].Neighbours.Add(_locations[74]);
            _locations[72].Neighbours.Add(_locations[73]);
            _locations[72].Neighbours.Add(_locations[71]);
            _locations[72].Neighbours.Add(_locations[80]);
            _locations[80].Neighbours.Add(_locations[0]);
            _locations[80].Neighbours.Add(_locations[73]);
            _locations[80].Neighbours.Add(_locations[72]);
            _locations[80].Neighbours.Add(_locations[81]);
            _locations[81].Neighbours.Add(_locations[9]);
            _locations[81].Neighbours.Add(_locations[84]);
            _locations[81].Neighbours.Add(_locations[0]);
            _locations[81].Neighbours.Add(_locations[80]);
            _locations[13].Neighbours.Add(_locations[14]);
            _locations[13].Neighbours.Add(_locations[12]);
            _locations[13].Neighbours.Add(_locations[10]);
            _locations[13].Neighbours.Add(_locations[17]);
            _locations[13].Neighbours.Add(_locations[16]);
        }

        struct NeighbourCacheKey
        {
            internal Location start;
            internal int distance;
            internal Faction faction;
            internal bool ignoreStorm;

            public override bool Equals(object obj)
            {
                return
                    obj is NeighbourCacheKey c &&
                    c.start == this.start &&
                    c.distance == this.distance &&
                    c.faction == this.faction &&
                    c.ignoreStorm == this.ignoreStorm;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(start, distance, faction);
            }
        }

        private int NeighbourCacheTimestamp = -1;
        private readonly Dictionary<NeighbourCacheKey, List<Location>> NeighbourCache = new();

        public List<Location> FindNeighbours(Location start, int distance, bool ignoreStorm, Faction f, Game game, bool checkForceObstacles = true)
        {
            var cacheKey = new NeighbourCacheKey() { start = start, distance = distance, faction = f, ignoreStorm = ignoreStorm };

            if (checkForceObstacles)
            {
                if (NeighbourCacheTimestamp != game.History.Count)
                {
                    NeighbourCache.Clear();
                    NeighbourCacheTimestamp = game.History.Count;
                }
                else if (NeighbourCache.TryGetValue(cacheKey, out List<Location> value))
                {
                    return value;
                }
            }

            var forceObstacles = new List<Location>();
            if (checkForceObstacles)
            {
                forceObstacles = DetermineForceObstacles(f, game);
            }

            List<Location> neighbours = new();
            FindNeighbours(neighbours, start, null, 0, distance, f, ignoreStorm ? 99 : game.SectorInStorm, forceObstacles);

            neighbours.Remove(start);

            if (checkForceObstacles)
            {
                NeighbourCache.Add(cacheKey, neighbours);
            }

            return neighbours;
        }

        private static bool BelongsTo(Game game, Battalion b, Faction f) => b.Faction == f || game.GetPlayer(f).Ally == Faction.Pink && b.Faction == Faction.Pink;

        private static List<Location> DetermineForceObstacles(Faction f, Game game)
        {
            return game.Forces(false).Where(kvp =>
                kvp.Key.IsStronghold &&
                !kvp.Value.Any(b => BelongsTo(game, b, f)) &&
                kvp.Value.Count(b => b.CanOccupy) >= 2)
                .Select(kvp => kvp.Key)
                .Distinct()
                .Union(game.CurrentBlockedTerritories.SelectMany(t => t.Locations))
                .ToList();
        }

        private static void FindNeighbours(
            List<Location> found,
            Location current,
            Location previous,
            int currentDistance,
            int maxDistance,
            Faction f,
            int sectorInStorm,
            List<Location> forceObstacles)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours.Where(n => n != previous && n.Sector != sectorInStorm && !forceObstacles.Contains(n)))
            {
                int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                if (currentDistance + distance <= maxDistance)
                {
                    FindNeighbours(found, neighbour, current, currentDistance + distance, maxDistance, f, sectorInStorm, forceObstacles);
                }
            }
        }

        public static List<List<Location>> FindPaths(Location start, Location destination, int distance, bool ignoreStorm, Faction f, Game game)
        {
            var paths = new List<List<Location>>();
            var route = new Stack<Location>();
            var obstacles = DetermineForceObstacles(f, game);
            FindPaths(paths, route, start, destination, null, 0, distance, f, ignoreStorm ? 99 : game.SectorInStorm, obstacles);
            return paths;
        }

        private static void FindPaths(List<List<Location>> foundPaths, Stack<Location> currentPath, Location current, Location destination, Location previous, int currentDistance, int maxDistance, Faction f, int sectorInStorm, List<Location> obstacles)
        {
            currentPath.Push(current);

            if (current.Equals(destination))
            {
                foundPaths.Add(currentPath.ToList());
            }
            else
            {
                foreach (var neighbour in current.Neighbours.Where(neighbour =>
                    neighbour != previous &&
                    neighbour.Sector != sectorInStorm &&
                    !currentPath.Contains(neighbour) &&
                    !obstacles.Contains(neighbour)))
                {
                    if (neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            FindPaths(foundPaths, currentPath, neighbour, destination, current, currentDistance + distance, maxDistance, f, sectorInStorm, obstacles);
                        }
                    }
                }
            }

            currentPath.Pop();
        }

        public static List<Location> FindFirstShortestPath(Location start, Location destination, bool ignoreStorm, Faction f, Game game)
        {
            var route = new Stack<Location>();
            var obstacles = DetermineForceObstacles(f, game);
            for (int i = 0; i <= 4; i++)
            {
                var path = FindPath(route, start, destination, null, 0, i, f, ignoreStorm ? 99 : game.SectorInStorm, obstacles);
                if (path != null) return path;
            }
            return null;
        }

        private static List<Location> FindPath(Stack<Location> currentRoute, Location current, Location destination, Location previous, int currentDistance, int maxDistance, Faction f, int sectorInStorm, List<Location> obstacles)
        {
            currentRoute.Push(current);

            if (current.Equals(destination))
            {
                return currentRoute.Reverse().ToList();
            }
            else
            {
                foreach (var neighbour in current.Neighbours.Where(neighbour =>
                    neighbour != previous &&
                    neighbour.Sector != sectorInStorm &&
                    !currentRoute.Contains(neighbour) &&
                    !obstacles.Contains(neighbour)))
                {
                    if (neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            var found = FindPath(currentRoute, neighbour, destination, current, currentDistance + distance, maxDistance, f, sectorInStorm, obstacles);
                            if (found != null) return found;
                        }
                    }
                }
            }

            currentRoute.Pop();
            return null;
        }

        public static List<Location> FindNeighboursForHmsMovement(Location start, int distance, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new();
            FindNeighboursForHmsMovement(neighbours, start, null, 0, distance, ignoreStorm, sectorInStorm);
            neighbours.Remove(start);
            return neighbours;
        }

        private static void FindNeighboursForHmsMovement(List<Location> found, Location current, Location previous, int currentDistance, int maxDistance, bool ignoreStorm, int sectorInStorm)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours)
            {
                if (neighbour != previous)
                {
                    if (ignoreStorm || neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            FindNeighboursForHmsMovement(found, neighbour, current, currentDistance + distance, maxDistance, ignoreStorm, sectorInStorm);
                        }
                    }
                }
            }
        }

        public static List<Location> FindNeighboursWithinTerritory(Location start, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new();
            FindNeighboursWithinTerritory(neighbours, start, null, ignoreStorm, sectorInStorm);
            return neighbours;
        }

        private static void FindNeighboursWithinTerritory(List<Location> found, Location current, Location previous, bool ignoreStorm, int sectorInStorm)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours.Where(l => l.Territory == current.Territory))
            {
                if (neighbour != previous)
                {
                    if (ignoreStorm || neighbour.Sector != sectorInStorm)
                    {
                        FindNeighboursWithinTerritory(found, neighbour, current, ignoreStorm, sectorInStorm);
                    }
                }
            }
        }

        public class TerritoryFetcher : IFetcher<Territory>
        {
            private readonly Map _map;

            public TerritoryFetcher(Map map)
            {
                _map = map;
            }

            public Territory Find(int id)
            {
                if (id == -1)
                {
                    return null;
                }
                else
                {
                    return _map.Territories(true).SingleOrDefault(t => t.Id == id);
                }
            }

            public int GetId(Territory obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }

        public class LocationFetcher : IFetcher<Location>
        {
            private readonly Map _map;

            public LocationFetcher(Map map)
            {
                _map = map;
            }

            public Location Find(int id)
            {
                if (id == -1)
                {
                    return null;
                }
                else
                {
                    return _map._locations[id];
                }
            }

            public int GetId(Location obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }
    }
}