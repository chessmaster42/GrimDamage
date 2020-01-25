namespace GrimDamage.Parser.Model
{
    public enum GrimState {
        Pause, Unpause, Dying,
        BeginStun, EndStun,
        BeginTrap, EndTrap,
        DisableMovement,
        Unknown,
        Initializing,
        Alive,
        Dead,
        Respawning
    }
}
