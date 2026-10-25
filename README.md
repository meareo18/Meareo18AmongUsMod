# Meareo18 Among Us Mod (Android ARM64 & IL2CPP)

Mod del Among Us original para Android ARM64, desarrollado para partidas comunitarias privadas con roles personalizados (**Migajero** y **Apostador**) y soporte para regiones comunitarias de coste 0 €.

---

## Estructura del Repositorio

```text
Meareo18AmongUsMod/
├── build.ps1                    # Script de compilación reproducible
├── README.md                    # Documentación principal
├── docs/                        # Informes y arquitectura técnica
│   ├── AUDITORIA_VIABILIDAD.md  # Análisis exhaustivo de viabilidad (Fase A)
│   ├── ARQUITECTURA_SINCRONIZACION.md # Sincronización y lógica de roles
│   └── REGION_COMUNITARIA.md    # Estudio técnico de red y servidores privados
└── src/
    └── Meareo18Mod/             # Código fuente del plugin C#
        ├── Meareo18Mod.csproj   # Archivo de proyecto SDK net8.0
        ├── Meareo18Plugin.cs    # Ciclo de vida y punto de entrada
        ├── Diagnostic/          # Sistema de diagnóstico e información de runtime
        │   └── DiagnosticLogger.cs
        ├── Network/             # Códigos RPC de red (CustomRPC)
        │   └── CustomRpcCodes.cs
        └── Roles/               # Implementación de Migajero y Apostador
```

---

## Roles Incluidos

1. **Migajero (Impostor):** Recoge migajas que reducen el KillCooldown.
2. **Apostador (Impostor):** Apuesta periódica con efectos aleatorios (45% pos / 20% neu / 35% neg).

---

## Compilación

```powershell
.\build.ps1 -Configuration Release
```

> ⚠️ Este mod está en desarrollo. No se ha verificado compatibilidad real con Android v19a (build 7492) aún.
