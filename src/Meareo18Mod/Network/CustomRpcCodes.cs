namespace Meareo18AmongUsMod.Network
{
    /// <summary>
    /// Identificadores numéricos para RPCs personalizados.
    /// Usados por Reactor para enrutar mensajes entre clientes.
    /// </summary>
    public enum CustomRpcCodes : byte
    {
        HandshakeVersionCheck      = 80,
        CommunityRegionValidation  = 81,
        MigajeroSpawnCrumbs        = 96,
        MigajeroCollectCrumb       = 97,
        MigajeroUpdateCooldown     = 98,
        MigajeroSyncCrumbsState    = 99,
        ApostadorPromptBet         = 112,
        ApostadorResolveDecision   = 113,
        ApostadorApplyEffect       = 114,
        ApostadorClearTemporaryEffects = 115
    }
}
