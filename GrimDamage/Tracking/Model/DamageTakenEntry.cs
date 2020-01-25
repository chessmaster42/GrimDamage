using System;

namespace GrimDamage.Tracking.Model
{
    public class DamageTakenEntry : IDamageEntry {
        public int Attacker { get; set; }
        public double Amount { get; set; }
        public DamageType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
