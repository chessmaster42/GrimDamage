using System;

namespace GrimDamage.Tracking.Model
{
    interface IDamageEntry {
        double Amount { get; }
        DamageType Type { get; }
        DateTime Time { get; }
    }
}
