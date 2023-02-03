﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Location : IIdentifiable
    {
        public virtual int Sector { get; set; }

        private Territory _territory = null;
        public virtual Territory Territory
        {
            get
            {
                return _territory;
            }
            set
            {
                _territory = value;

                if (value != null)
                {
                    _territory.AddLocation(this);
                }
            }
        }

        public virtual List<Location> Neighbours { get; set; } = new List<Location>();

        public string Orientation { get; set; } = "";

        public virtual int SpiceBlowAmount { get; set; } = 0;

        public virtual DiscoveryTokenType TokenType { get; set; } = DiscoveryTokenType.None;

        public Location(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public bool IsStronghold => Territory.IsStronghold;

        public bool IsProtectedFromStorm => Territory.IsProtectedFromStorm;

        public override bool Equals(object obj)
        {
            return obj is Location l && l.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return Message.DefaultDescriber.Describe(this) + "*";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}