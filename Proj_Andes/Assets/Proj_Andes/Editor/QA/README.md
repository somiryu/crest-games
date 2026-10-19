# Bot de minijuegos (QA)

`Hi Hat Games > QA > Jugar secuencia de minijuegos (bot)` juega toda la secuencia de **Minijuegos**
en el editor como lo haría un niño, y al final deja un reporte en `Logs/QA/minigame-bot_<fecha>.md`.

## Qué hace
- Entra con un usuario de prueba **solo en memoria** (`QA_BOT`, institución `QA_BOT_LOCAL`) por el mismo
  camino que el botón "¡Listo!" del login.
- Solo toca lo que un niño podría tocar: botones visibles, activos y sin nada encima (respeta los paneles
  que bloquean mientras suena un audio). Prioriza lo que acaba de aparecer en pantalla, mantiene
  presionados los botones de "mantener" (mover el cohete, turbo) y toca/arrastra la pantalla en los juegos
  físicos (Boosters, garra, Turbo) a través de `GameInput`.
- Nunca toca reiniciar, salir, home, música ni el panel de debug.
- Si una escena no avanza en 5 minutos lo anota como **cuelgue**, guarda un diagnóstico de lo que había en
  pantalla y pasa a la siguiente.

## Qué valida
- Errores y excepciones de cada escena.
- La analítica que se subiría (la cola local del juego) contra las columnas que exporta el CMS
  (`crest-cms/src/routes/(admin)/admin/descargables`), incluidas las de emoción e "intenta de nuevo",
  y la cantidad de documentos por partida (rondas).

## Seguridad de los datos
- No escribe en Firestore: durante la secuencia el juego solo guarda en local, y el bot además fuerza
  "sin internet".
- Al terminar restaura el guardado local del editor (PlayerPrefs). Si Unity se cierra a mitad de una
  corrida, se restaura al volver a abrir el editor o al salir de Play.

## Repetir solo una parte
Desde un script de editor (o la consola del MCP):

```csharp
EditorPrefs.SetInt(MinigameBotRunner.StartIndexKey, 22); // índice en GameSequencesList
EditorPrefs.SetInt(MinigameBotRunner.SceneLimitKey, 1);  // cuántas escenas jugar
```

Bórralas (`EditorPrefs.DeleteKey`) para volver a la secuencia completa.

## Límites
Valida la lógica, el flujo y la analítica. No reemplaza una partida real en una tablet: no prueba la
sensación del toque, el rendimiento ni el audio.
