namespace GrimDamage.Statistics.dto
{
    public class PetDamageDealt {
        public int EntityId { get; set; }
        public int VictimId { get; set; }
        public string DamageType { get; set; }
        public double Amount { get; set; }
        public long Timestamp { get; set; }
    }
}
