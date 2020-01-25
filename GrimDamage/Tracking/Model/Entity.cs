using System;
using System.Collections.Concurrent;
using System.Linq;

namespace GrimDamage.Tracking.Model
{
    public class Entity {
        public Entity() {
            DamageDealt = new ConcurrentBag<DamageDealtEntry>();
            DamageTaken = new ConcurrentBag<DamageTakenEntry>();
            DamageBlocked = new ConcurrentBag<DamageBlockedEntry>();
            Health = new ConcurrentBag<EntityHealthEntry>();
            Resists = new ConcurrentBag<ResistUpdatedEntry>();
        }

        public int Id { get; set; }

        public bool IsPrimary { get; set; }

        public string Name { get; set; }

        public EntityType Type { get; set; }

        public ConcurrentBag<EntityHealthEntry> Health { get; set; }

        public ConcurrentBag<DamageDealtEntry> DamageDealt { get; }
        public ConcurrentBag<DamageTakenEntry> DamageTaken { get; }
        public ConcurrentBag<DamageBlockedEntry> DamageBlocked { get; }
        public ConcurrentBag<ResistUpdatedEntry> Resists { get; }

        public DateTime LastSeen {
            get {
                var lastDealt = DamageDealt.DefaultIfEmpty().Max(m => m?.Time)?? DateTime.MinValue;
                var lastTaken = DamageTaken.DefaultIfEmpty().Max(m => m?.Time) ?? DateTime.MinValue;
                if (lastDealt > lastTaken)
                    return lastDealt;
                else {
                    return lastTaken;
                }
            }
        }
    }
}
