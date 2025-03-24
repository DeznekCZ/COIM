﻿using Mafi;
using Mafi.Core.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class EntityInfo
    {
        public int Id { get; set; }
        public string Prototype { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        [JsonIgnore]
        public RelTile2f Relative => new RelTile2f(Fix32.FromRaw(X), Fix32.FromRaw(Y));

        public EntityInfo() { } // serializer constructor

        public EntityInfo(IEntity entity, RelTile2f position)
        {
            Id = entity.Id.Value;
            Prototype = entity.Prototype.Id.Value;
            X = position.X.RawValue;
            Y = position.Y.RawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityInfo other &&
                   Id == other.Id &&
                   Prototype == other.Prototype &&
                   X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = -1866251748;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Prototype);
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}