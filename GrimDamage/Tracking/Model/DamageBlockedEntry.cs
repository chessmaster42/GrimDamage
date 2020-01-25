using System;

namespace GrimDamage.Tracking.Model
{
    public class DamageBlockedEntry {
        public int Attacker { get; set; }
        public double Amount { get; set; }
        public DateTime Time { get; set; }
    }
}
