using System;
using System.Collections.Generic;
using Meareo18AmongUsMod.Diagnostic;

namespace Meareo18AmongUsMod.Roles
{
    public struct CrumbData
    {
        public uint Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool IsActive { get; set; }

        public CrumbData(uint id, float x, float y)
        {
            Id = id; X = x; Y = y; IsActive = true;
        }
    }

    /// <summary>
    /// Lógica central del rol impostor MIGAJERO.
    /// Recoge migajas para reducir el KillCooldown. Solo el Migajero las ve.
    /// </summary>
    public class MigajeroRole
    {
        public bool IsEnabled { get; set; } = true;
        public float SpawnChancePercentage { get; set; } = 100f;
        public int TotalCrumbsAvailable { get; set; } = 20;
        public int MaxSimultaneousCrumbs { get; set; } = 5;
        public float SpawnIntervalSeconds { get; set; } = 15f;
        public float CooldownReductionPerCrumb { get; set; } = 2.5f;
        public float MinKillCooldown { get; set; } = 10.0f;
        public int MaxCrumbsPerPlayer { get; set; } = 10;

        public int CollectedCount { get; private set; }
        public bool IsAssigned { get; set; }

        private readonly Dictionary<uint, CrumbData> _activeCrumbs = new();
        private readonly HashSet<uint> _collectedIds = new();
        private readonly object _lock = new object();

        public void RegisterCrumb(uint id, float x, float y)
        {
            lock (_lock)
            {
                if (!_activeCrumbs.ContainsKey(id) && !_collectedIds.Contains(id))
                {
                    _activeCrumbs[id] = new CrumbData(id, x, y);
                    DiagnosticLogger.Info($"[MIGAJERO] Migaja #{id} registrada en ({x:F2}, {y:F2}).");
                }
            }
        }

        public bool TryCollectCrumb(uint id, float currentCooldown, out float newCooldown)
        {
            lock (_lock)
            {
                newCooldown = currentCooldown;
                if (!IsAssigned || CollectedCount >= MaxCrumbsPerPlayer) return false;
                if (_collectedIds.Contains(id) || !_activeCrumbs.TryGetValue(id, out var crumb) || !crumb.IsActive) return false;

                crumb.IsActive = false;
                _activeCrumbs.Remove(id);
                _collectedIds.Add(id);
                CollectedCount++;
                newCooldown = Math.Max(currentCooldown - CooldownReductionPerCrumb, MinKillCooldown);
                DiagnosticLogger.Info($"[MIGAJERO] Migaja #{id} recogida. Cooldown: {currentCooldown:F1}s -> {newCooldown:F1}s");
                return true;
            }
        }

        public void ResetRound()
        {
            lock (_lock)
            {
                _activeCrumbs.Clear();
                _collectedIds.Clear();
                CollectedCount = 0;
                IsAssigned = false;
                DiagnosticLogger.Info("[MIGAJERO] Ronda reiniciada.");
            }
        }
    }
}
