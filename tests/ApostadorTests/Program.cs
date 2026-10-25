using System;
using System.Collections.Generic;
using Meareo18AmongUsMod.Roles;

namespace Meareo18AmongUsMod.Tests
{
    class Program
    {
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("    BATERÍA DE PRUEBAS: ROL APOSTADOR v2                  ");
            Console.WriteLine("==========================================================");

            Test1_RandomIntervals_WithinBounds();
            Test2_Reject_RandomNegativePunishments();
            Test3_Timeout_RandomNegativePunishments();
            Test4_Prevention_SimultaneousBetPrompt();
            Test5_EffectLifecycle_InstantVsTemporary();
            Test6_MeetingCalled_CleansActiveEffect();
            Test7_ResetRound_CleansAllState();
            Test8_Accept_MaintainsOriginalProbabilities();

            Console.WriteLine($"\nRESULTADO: {_testsPassed} PASADAS | {_testsFailed} FALLADAS");
            if (_testsFailed > 0) Environment.Exit(1);
        }

        static void AssertTrue(bool condition, string testName, string failureReason = "")
        {
            if (condition) { Console.WriteLine($"[PASS] {testName}"); _testsPassed++; }
            else { Console.WriteLine($"[FAIL] {testName} -> {failureReason}"); _testsFailed++; }
        }

        static void Test1_RandomIntervals_WithinBounds()
        {
            var role = new ApostadorRole();
            bool allInRange = true, sawBelow90 = false, sawAbove90 = false;
            for (int i = 0; i < 1000; i++)
            {
                float v = role.GenerateNextInterval();
                if (v < 60f || v > 120f) { allInRange = false; break; }
                if (v < 90f) sawBelow90 = true;
                if (v > 90f) sawAbove90 = true;
            }
            AssertTrue(allInRange && sawBelow90 && sawAbove90, "Prueba 1: Intervalos siempre entre 60-120s (1.000 iteraciones)");
        }

        static void Test2_Reject_RandomNegativePunishments()
        {
            var role = new ApostadorRole();
            var neg = new HashSet<SpecificBetEffect> { SpecificBetEffect.IncreaseKillCooldown, SpecificBetEffect.SlowDownMovement, SpecificBetEffect.PlayAlarmSound };
            var counts = new Dictionary<SpecificBetEffect, int>();
            bool allNeg = true;
            for (int i = 0; i < 300; i++)
            {
                role.ResetRound(); role.PromptNewBet((uint)i);
                var e = role.ResolveDecision(BetDecision.Rejected);
                if (!neg.Contains(e)) { allNeg = false; break; }
                if (!counts.ContainsKey(e)) counts[e] = 0;
                counts[e]++;
            }
            AssertTrue(allNeg && counts.Count == 3, "Prueba 2: Rechazo -> solo negativos, los 3 aparecen");
        }

        static void Test3_Timeout_RandomNegativePunishments()
        {
            var role = new ApostadorRole();
            var neg = new HashSet<SpecificBetEffect> { SpecificBetEffect.IncreaseKillCooldown, SpecificBetEffect.SlowDownMovement, SpecificBetEffect.PlayAlarmSound };
            var counts = new Dictionary<SpecificBetEffect, int>();
            bool allNeg = true;
            for (int i = 0; i < 300; i++)
            {
                role.ResetRound(); role.PromptNewBet((uint)i);
                var e = role.ResolveDecision(BetDecision.TimedOut);
                if (!neg.Contains(e)) { allNeg = false; break; }
                if (!counts.ContainsKey(e)) counts[e] = 0;
                counts[e]++;
            }
            AssertTrue(allNeg && counts.Count == 3, "Prueba 3: Timeout -> solo negativos, los 3 aparecen");
        }

        static void Test4_Prevention_SimultaneousBetPrompt()
        {
            var role = new ApostadorRole();
            bool first = role.PromptNewBet(1);
            bool second = role.PromptNewBet(2);
            AssertTrue(first && !second && role.IsBetPromptOpen && role.CurrentBetId == 1, "Prueba 4: Bloqueo apuestas simultáneas");
        }

        static void Test5_EffectLifecycle_InstantVsTemporary()
        {
            var role = new ApostadorRole();
            bool alarmIsInstant = !role.IsEffectTemporary(SpecificBetEffect.PlayAlarmSound) && role.GetProvisionalEffectDuration(SpecificBetEffect.PlayAlarmSound) == 0f;
            bool invisIsTemp = role.IsEffectTemporary(SpecificBetEffect.TemporaryInvisibility) && role.GetProvisionalEffectDuration(SpecificBetEffect.TemporaryInvisibility) == 8f;
            SpecificBetEffect chosen;
            do { role.ResetRound(); role.PromptNewBet(20); chosen = role.ResolveDecision(BetDecision.Accepted); } while (!role.IsEffectTemporary(chosen));
            bool blocked = !role.CanPromptNewBet();
            role.EndCurrentEffect();
            bool unblocked = role.CanPromptNewBet();
            bool nextOk = role.NextScheduledIntervalSeconds >= 60f && role.NextScheduledIntervalSeconds <= 120f;
            AssertTrue(alarmIsInstant && invisIsTemp && blocked && unblocked && nextOk, "Prueba 5: Ciclo de vida instantáneo vs temporal");
        }

        static void Test6_MeetingCalled_CleansActiveEffect()
        {
            var role = new ApostadorRole();
            SpecificBetEffect chosen;
            do { role.ResetRound(); role.PromptNewBet(50); chosen = role.ResolveDecision(BetDecision.Accepted); } while (!role.IsEffectTemporary(chosen));
            bool wasActive = role.IsEffectActive;
            role.OnMeetingCalled();
            bool clean = !role.IsEffectActive && !role.IsBetPromptOpen && role.NextScheduledIntervalSeconds >= 60f;
            AssertTrue(wasActive && clean, "Prueba 6: Reunión limpia efectos y reprograma intervalo");
        }

        static void Test7_ResetRound_CleansAllState()
        {
            var role = new ApostadorRole();
            role.IsAssigned = true; role.PromptNewBet(77); role.ResetRound();
            bool clean = !role.IsBetPromptOpen && !role.IsEffectActive && role.CurrentActiveEffect == null
                && role.CurrentBetId == 0 && role.LastDecision == BetDecision.Pending && !role.IsAssigned && role.NextScheduledIntervalSeconds == 0f;
            AssertTrue(clean, "Prueba 7: ResetRound limpia todo el estado");
        }

        static void Test8_Accept_MaintainsOriginalProbabilities()
        {
            var role = new ApostadorRole();
            int pos = 0, neu = 0, neg = 0, n = 10000;
            var posSet = new HashSet<SpecificBetEffect> { SpecificBetEffect.TemporaryInvisibility, SpecificBetEffect.ResetKillCooldown, SpecificBetEffect.TeleportToRandomRoom };
            var neuSet = new HashSet<SpecificBetEffect> { SpecificBetEffect.ShuffleVisualNames, SpecificBetEffect.InstantElectricalSabotage };
            for (int i = 0; i < n; i++)
            {
                role.ResetRound(); role.PromptNewBet((uint)i);
                var e = role.ResolveDecision(BetDecision.Accepted);
                if (posSet.Contains(e)) pos++; else if (neuSet.Contains(e)) neu++; else neg++;
            }
            double pr = (double)pos/n*100, nr2 = (double)neu/n*100, ng = (double)neg/n*100;
            Console.WriteLine($"   Distribución ({n} tiros): Pos={pr:F2}% Neu={nr2:F2}% Neg={ng:F2}%");
            AssertTrue(pr >= 41.5 && pr <= 48.5 && nr2 >= 17 && nr2 <= 23 && ng >= 31.5 && ng <= 38.5,
                "Prueba 8: Probabilidades aceptar (45%/20%/35%) dentro de tolerancia estadística");
        }
    }
}
