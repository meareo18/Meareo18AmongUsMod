using System;
using Meareo18AmongUsMod.Diagnostic;

namespace Meareo18AmongUsMod.Roles
{
    public enum BetDecision { Pending, Accepted, Rejected, TimedOut }
    public enum BetEffectType { Positive, Neutral, Negative }
    public enum SpecificBetEffect
    {
        // Positivos
        TemporaryInvisibility = 1,
        ResetKillCooldown = 2,
        TeleportToRandomRoom = 3,
        // Neutrales
        ShuffleVisualNames = 10,
        InstantElectricalSabotage = 11,
        // Negativos
        IncreaseKillCooldown = 20,
        SlowDownMovement = 21,
        PlayAlarmSound = 22
    }

    /// <summary>
    /// Lógica central del rol impostor APOSTADOR.
    /// Reglas de diseño v2 (10 reglas aprobadas por el usuario).
    /// </summary>
    public class ApostadorRole
    {
        public bool IsEnabled { get; set; } = true;
        public float PositivePercent { get; set; } = 45f;
        public float NeutralPercent { get; set; } = 20f;
        public float NegativePercent { get; set; } = 35f;
        public float MinBetIntervalSeconds { get; set; } = 60f;
        public float MaxBetIntervalSeconds { get; set; } = 120f;
        public float ResponseTimeoutSeconds { get; set; } = 10f;

        // Duraciones provisionales documentadas (sujetas a balance)
        public float InvisibilityDurationSeconds { get; set; } = 8f;
        public float SlowDownDurationSeconds { get; set; } = 8f;
        public float ShuffleNamesDurationSeconds { get; set; } = 10f;

        public bool IsAssigned { get; set; }
        public bool IsBetPromptOpen { get; private set; }
        public uint CurrentBetId { get; private set; }
        public BetDecision LastDecision { get; private set; } = BetDecision.Pending;
        public bool IsEffectActive { get; private set; }
        public SpecificBetEffect? CurrentActiveEffect { get; private set; }
        public float CurrentEffectRemainingDuration { get; private set; }
        public float NextScheduledIntervalSeconds { get; private set; }

        private readonly Random _rng = new Random();

        public bool ValidateConfiguration(out string errorMessage)
        {
            float total = PositivePercent + NeutralPercent + NegativePercent;
            if (Math.Abs(total - 100f) > 0.01f)
            {
                errorMessage = $"Porcentajes suman {total:F1}% (debe ser 100%)."; return false;
            }
            errorMessage = string.Empty; return true;
        }

        public bool CanPromptNewBet() => !IsBetPromptOpen && !IsEffectActive;

        public float GenerateNextInterval()
        {
            NextScheduledIntervalSeconds = (float)(_rng.NextDouble() * (MaxBetIntervalSeconds - MinBetIntervalSeconds) + MinBetIntervalSeconds);
            DiagnosticLogger.Info($"[APOSTADOR] Siguiente apuesta en {NextScheduledIntervalSeconds:F1}s.");
            return NextScheduledIntervalSeconds;
        }

        public bool PromptNewBet(uint betId)
        {
            if (!CanPromptNewBet()) return false;
            CurrentBetId = betId;
            IsBetPromptOpen = true;
            LastDecision = BetDecision.Pending;
            DiagnosticLogger.Info($"[APOSTADOR] Apuesta #{betId} iniciada. Tiempo: {ResponseTimeoutSeconds:F1}s.");
            return true;
        }

        public SpecificBetEffect ResolveDecision(BetDecision decision)
        {
            IsBetPromptOpen = false;
            LastDecision = decision;
            SpecificBetEffect chosenEffect;

            if (decision == BetDecision.Rejected || decision == BetDecision.TimedOut)
            {
                chosenEffect = SelectRandomEffect(BetEffectType.Negative);
                DiagnosticLogger.Info($"[APOSTADOR] {decision} -> Castigo negativo: {chosenEffect}.");
            }
            else
            {
                double roll = _rng.NextDouble() * 100.0;
                BetEffectType category = roll < PositivePercent ? BetEffectType.Positive
                    : roll < PositivePercent + NeutralPercent ? BetEffectType.Neutral
                    : BetEffectType.Negative;
                chosenEffect = SelectRandomEffect(category);
                DiagnosticLogger.Info($"[APOSTADOR] ACEPTADA (roll {roll:F1}%) -> {category}: {chosenEffect}.");
            }

            ApplyEffectState(chosenEffect);
            return chosenEffect;
        }

        public bool IsEffectTemporary(SpecificBetEffect effect) => effect switch
        {
            SpecificBetEffect.TemporaryInvisibility => true,
            SpecificBetEffect.SlowDownMovement => true,
            SpecificBetEffect.ShuffleVisualNames => true,
            _ => false
        };

        public float GetProvisionalEffectDuration(SpecificBetEffect effect) => effect switch
        {
            SpecificBetEffect.TemporaryInvisibility => InvisibilityDurationSeconds,
            SpecificBetEffect.SlowDownMovement => SlowDownDurationSeconds,
            SpecificBetEffect.ShuffleVisualNames => ShuffleNamesDurationSeconds,
            _ => 0f
        };

        private void ApplyEffectState(SpecificBetEffect effect)
        {
            if (IsEffectTemporary(effect))
            {
                IsEffectActive = true;
                CurrentActiveEffect = effect;
                CurrentEffectRemainingDuration = GetProvisionalEffectDuration(effect);
                DiagnosticLogger.Info($"[APOSTADOR] Efecto temporal: {effect} por {CurrentEffectRemainingDuration:F1}s.");
            }
            else
            {
                IsEffectActive = false;
                CurrentActiveEffect = null;
                CurrentEffectRemainingDuration = 0f;
                GenerateNextInterval();
            }
        }

        public void EndCurrentEffect()
        {
            if (IsEffectActive)
            {
                DiagnosticLogger.Info($"[APOSTADOR] Efecto {CurrentActiveEffect} finalizado.");
                IsEffectActive = false;
                CurrentActiveEffect = null;
                CurrentEffectRemainingDuration = 0f;
                GenerateNextInterval();
            }
        }

        public void OnMeetingCalled()
        {
            IsBetPromptOpen = false;
            IsEffectActive = false;
            CurrentActiveEffect = null;
            CurrentEffectRemainingDuration = 0f;
            GenerateNextInterval();
        }

        public void ResetRound()
        {
            IsBetPromptOpen = false;
            IsEffectActive = false;
            CurrentActiveEffect = null;
            CurrentEffectRemainingDuration = 0f;
            CurrentBetId = 0;
            LastDecision = BetDecision.Pending;
            IsAssigned = false;
            NextScheduledIntervalSeconds = 0f;
        }

        private SpecificBetEffect SelectRandomEffect(BetEffectType category)
        {
            switch (category)
            {
                case BetEffectType.Positive:
                    SpecificBetEffect[] pos = { SpecificBetEffect.TemporaryInvisibility, SpecificBetEffect.ResetKillCooldown, SpecificBetEffect.TeleportToRandomRoom };
                    return pos[_rng.Next(pos.Length)];
                case BetEffectType.Neutral:
                    SpecificBetEffect[] neu = { SpecificBetEffect.ShuffleVisualNames, SpecificBetEffect.InstantElectricalSabotage };
                    return neu[_rng.Next(neu.Length)];
                default:
                    SpecificBetEffect[] neg = { SpecificBetEffect.IncreaseKillCooldown, SpecificBetEffect.SlowDownMovement, SpecificBetEffect.PlayAlarmSound };
                    return neg[_rng.Next(neg.Length)];
            }
        }
    }
}
