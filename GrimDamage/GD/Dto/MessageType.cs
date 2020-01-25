namespace GrimDamage.GD.Dto
{
    public enum MessageType
    {
        PlayerHealthOffsetDetected = 100,
        ErrorDetectingPlayerHealthOffset = 404,

        CharacterMovement1 = 1001,
        CharacterMovement2 = 1002,
        CharacterMovement3 = 1003,
        CharacterMovement4 = 1004,
        CharacterMovement5 = 1005,
        CharacterMovement6 = 1006,
        HitpointMonitor = 1007,

        ErrorDetectingPrimaryPlayerIdOffset = 1404,

        BeginStun = 2001,
        EndStun = 2002,
        BeginTrap = 2003,
        EndTrap = 2004,
        DisableMovement = 2005,
        SetLifeState = 2006,

        Pause = 20000,
        Unpause = 20001,
        PlayerIdDetected = 20002,
        PlayerDied = 20003,

        LogUnrecognized = 45000,

        DamageToVictim = 45001,
        LifeLeech = 45002,
        TotalDamage = 45003,
        SetAttackerName = 45004,
        SetDefenderName = 45005,
        SetAttackerId = 45006,
        SetDefenderId = 45007,
        Deflect = 45008,
        Absorb = 45009,
        Reflect = 45010,
        Block = 45011,
        EndCombat = 45012,

        ResistMonitor = 10101010
    }
}
