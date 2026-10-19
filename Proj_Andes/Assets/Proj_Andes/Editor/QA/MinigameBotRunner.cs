using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

/// <summary>
/// Plays the whole Minijuegos sequence in the editor like a child would: only taps UI that is visible and not
/// blocked, holds hold-buttons, and taps/drags the screen through GameInput for the physical games.
/// At the end it checks the analytics that would be uploaded against the columns the CMS exports.
///
/// Nothing reaches Firestore: during the sequence the game only saves locally, the bot also forces "no internet",
/// plays with an in-memory QA user and restores the editor's local save (PlayerPrefs) when it finishes.
/// </summary>
[InitializeOnLoad]
public static class MinigameBotRunner
{
    const string PendingKey = "QA.MinigameBot.Pending";
    const string SnapshotKey = "QA.MinigameBot.PlayerPrefsSnapshot";
    //Optional, to replay part of the sequence: sequence index to start at and how many scenes to play
    public const string StartIndexKey = "QA.MinigameBot.StartIndex";
    public const string SceneLimitKey = "QA.MinigameBot.SceneLimit";
    const string MinijuegosFlavorPath = "Assets/Proj_Andes/Menu/AppFlowSystem/Scriptables/AppFlavors/AppFlavor_Minijuegos.asset";
    const string QaInstitution = "QA_BOT_LOCAL";
    const double MaxSecondsPerScene = 300;
    const string ReportFolder = "Logs/QA";

    static readonly string[] LocalSaveKeys = { DatabaseManager.UserDatasJSONKey, DatabaseManager.pendingUserJSONKey, DatabaseManager.pendingSessionsJSONKey };

    //Columns exported by crest-cms (src/routes/(admin)/admin/descargables) with their database names
    static readonly Dictionary<string, string[]> CmsContract = new Dictionary<string, string[]>
    {
        { DataIds.heartsAndStarsGame, new[] { "cf_codigo", "cf_estimulo", "cf_intento", "cf_modo", "cf_prueba", "cf_respuesta", "cf_tiempo", "cf_valid" } },
        { DataIds.voiceStarGame, new[] { "nf_prueba", "nf_codigo", "nf_estimulo_aud", "nf_estimulo_vis", "nf_intento", "nf_respuesta", "nf_valid_1", "nf_valid_2", "nf_valid_3", "nf_modo", "nf_tiempo" } },
        { DataIds.sizeRocketsGame, new[] { "cr_modo", "cr_codigo", "cr_intento", "cr_estrellas", "cr_release", "cr_respuesta" } },
        { DataIds.turboRocketGame, new[] { "te_estrellas", "te_modo", "te_tiempo_total", "te_turbo", "te_turbo_tm" } },
        { DataIds.monsterMarket, new[] { "mm_cofre_cod", "mm_cofre_rta", "mm_cofre_intento", "mm_cofre_tm", "mm_estrellas_gast", "mm_estrellas_post", "mm_estrellas_pre", "mm_orden" } },
        { DataIds.mechanicHandGame, new[] { "fg_partida", "fg_intento", "fg_precision", "fg_trampa_activada", "fg_emoc_rta", "fg_emoc_cod", "fg_emoc_tm", "fg_espera_click" } },
        { DataIds.boostersAndScapeGame, new[] { "fp_partida", "fp_intento", "fp_turbo", "fp_precision", "fp_trampa_activada", "fp_emoc_rta", "fp_emoc_cod", "fp_emoc_tm", "fp_espera_click" } },
    };
    static readonly string[] CommonKeys = { DataIds.institutionCode, DataIds.UserID, DataIds.TestID };
    static readonly string[] AllowedRocketAnswers = { "pequeño", "mediano", "grande" };

    static bool running;
    static Random rng;
    static int seed;
    static double startedAt, sceneStartedAt, nextActionAt;
    static string currentScene;
    static int sceneIndex;
    static bool loggedIn;
    static int startIndex, sceneLimit;
    static UserData qaUser;
    static readonly StringBuilder report = new StringBuilder();
    static readonly List<string> problems = new List<string>();
    static readonly List<string> errorsInScene = new List<string>();
    static readonly StringBuilder diagnostics = new StringBuilder();
    static Action pendingRelease;
    static double releaseAt;
    static Vector2 dragFrom, dragTo;
    static double dragStart, dragEnd;

    static MinigameBotRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        //A previous run that didn't finish (editor closed, crash) must not leave QA data in the local save
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !SessionState.GetBool(PendingKey, false)) RestoreLocalSave();
    }

    [MenuItem("Hi Hat Games/QA/Jugar secuencia de minijuegos (bot)", priority = 100)]
    static void RunFromMenu()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("QA bot: stop Play mode first"); return; }
        AppFlavorBuildMenu.Apply(AssetDatabase.LoadAssetAtPath<AppFlavor>(MinijuegosFlavorPath));
        var firstScene = EditorBuildSettings.scenes.First(s => s.enabled).path;
        if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(firstScene);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
        {
            SessionState.SetBool(PendingKey, false);
            Begin(Environment.TickCount);
        }
        else if (state == PlayModeStateChange.ExitingPlayMode && running) Finish("Play mode was stopped");
        else if (state == PlayModeStateChange.EnteredEditMode) RestoreLocalSave();
    }

    static void Begin(int withSeed)
    {
        seed = withSeed;
        rng = new Random(seed);
        running = true;
        loggedIn = false;
        sceneIndex = 0;
        currentScene = null;
        problems.Clear();
        diagnostics.Clear();
        errorsInScene.Clear();
        report.Clear();
        SnapshotLocalSave();
        Application.runInBackground = true;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        startedAt = EditorApplication.timeSinceStartup;
        startIndex = EditorPrefs.GetInt(StartIndexKey, 0);
        sceneLimit = EditorPrefs.GetInt(SceneLimitKey, 0);
        report.AppendLine($"# Bot de minijuegos — {DateTime.Now:yyyy-MM-dd HH:mm}").AppendLine().AppendLine($"Semilla: {seed}" + (startIndex > 0 || sceneLimit > 0 ? $" · desde el paso {startIndex}, máximo {sceneLimit} escenas" : "")).AppendLine();
        report.AppendLine("| # | Escena | Duración | Resultado | Errores |").AppendLine("|---|---|---|---|---|");
        Debug.Log("QA bot: started, seed " + seed);
    }

    static void OnLog(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        if (message.Contains("[AI]")) return; //Unity-MCP connection noise
        var line = message.Split('\n')[0] + (string.IsNullOrEmpty(stack) ? "" : " @ " + stack.Split('\n')[0]);
        errorsInScene.Add($"{type}: {line}");
    }

    static double Now => EditorApplication.timeSinceStartup;

    static void Tick()
    {
        if (!running || !EditorApplication.isPlaying || EditorApplication.isPaused) return;

        var scene = SceneManager.GetActiveScene().name;
        if (scene != currentScene) OnSceneChanged(scene, forced: false);
        if (!running) return;

        if (pendingRelease != null && Now >= releaseAt) { pendingRelease(); pendingRelease = null; }
        if (dragEnd > 0) ContinueDrag();

        if (Now - sceneStartedAt > MaxSecondsPerScene)
        {
            problems.Add($"Cuelgue: la escena {scene} no avanzó en {MaxSecondsPerScene:0} s");
            diagnostics.AppendLine($"### Cuelgue en {scene}").AppendLine(DescribeScreen()).AppendLine();
            EndScene("CUELGUE, se forzó el paso");
            GameSequencesList.Instance.GoToNextItemInList();
            return;
        }
        if (Now < nextActionAt || pendingRelease != null || dragEnd > 0) return;
        nextActionAt = Now + 0.5 + rng.NextDouble() * 1.1;

        if (!loggedIn) { TryLogin(); return; }
        Act(scene);
    }

    static void OnSceneChanged(string scene, bool forced)
    {
        if (currentScene != null) EndScene(forced ? "forzado" : "ok");
        currentScene = scene;
        sceneStartedAt = Now;
        nextActionAt = Now + 1.5;
        GameInput.ClearSimulation();
        previouslyTappable.Clear();
        pendingRelease = null;
        dragEnd = 0;

        //The sequence went back to the user selection screen: the whole run is over
        if (loggedIn && GameSequencesList.Instance.goToGameGroupIdx == 0 && sceneIndex > 2) Finish("secuencia completa");
        else if (sceneLimit > 0 && sceneIndex >= sceneLimit + 2) Finish("límite de escenas");
    }

    static void EndScene(string result)
    {
        sceneIndex++;
        var errors = errorsInScene.Count == 0 ? "" : string.Join("<br>", errorsInScene.Distinct().Take(5).Select(e => e.Replace("|", "/")));
        if (errorsInScene.Count > 0) problems.Add($"{errorsInScene.Count} error(es) en {currentScene}");
        report.AppendLine($"| {sceneIndex} | {currentScene} | {Now - sceneStartedAt:0} s | {result} | {errors} |");
        errorsInScene.Clear();
        sceneStartedAt = Now;
    }

    // ---------- Login: same path as a child pressing "new game" ----------

    static void TryLogin()
    {
        var ui = Object.FindObjectOfType<FirebaseAnonymousLoginUI>();
        if (ui == null || !DatabaseManager.userListDone) return;

        //Second safety net: even a save asked to upload stops before touching the database
        UserDataManager.Instance.HasInternet = false;

        qaUser = new UserData { pin = "QA_BOT", id_proyecto = QaInstitution, edad = 8, grado = 3, sexo = UserSex.Hombre, lugar_nacimiento = "QA", vive = "Mama" };
        qaUser.tipo_colegio = UserSchoolType.Privado.ToString();
        qaUser.tipo_colegio_cod = (int)UserSchoolType.Privado;
        UserDataManager.CurrInstitutionCode = QaInstitution;
        UserDataManager.Instance.RegisterNewUser(qaUser);
        UserDataManager.Instance.SetCurrUser(qaUser.id_jugador);
        loggedIn = true;
        ui.OnReadyConfirmBtnPressed();
        if (startIndex > 0) GameSequencesList.Instance.GoToSequenceIdx(startIndex, -1);
    }

    // ---------- Playing ----------

    static void Act(string scene)
    {
        //A finished game: continue like a child would, after looking at the result for a moment
        var eog = Object.FindObjectsOfType<EndOfGameManager>().FirstOrDefault(e => IsTappable(e.continueBtn));
        if (eog != null && Now - sceneStartedAt > 2) { Tap(eog.continueBtn); return; }

        switch (scene)
        {
            case "Frustration_BoostersAndScape_MainScene": PlayBoosters(); return;
            case "Frustration_MechanicHand_TestScene": if (rng.NextDouble() < 0.5) { StartDrag(); return; } break;
            case "Gratification_TurboRocket_Tutorial": if (TapWhereTheTutorialHandPoints()) return; goto case "Gratification_TurboRocket";
            case "Gratification_TurboRocket": if (rng.NextDouble() < 0.4) { TapScreen(); return; } break;
        }
        ActOnUI();
    }

    static void PlayBoosters()
    {
        var manager = MG_BoostersAndScape_Manager.Instance;
        var booster = manager != null ? manager.currentBooster : null;
        var canBoost = booster != null && booster.Boosteable();
        //Mostly taps in time, sometimes too early, like a child
        if (canBoost ? rng.NextDouble() < 0.8 : rng.NextDouble() < 0.15) TapScreen();
        else ActOnUI();
        nextActionAt = Now + 0.15 + rng.NextDouble() * 0.35;
    }

    static HashSet<Button> previouslyTappable = new HashSet<Button>();

    static void ActOnUI()
    {
        var excluded = ExcludedButtons();
        var buttons = Object.FindObjectsOfType<Button>().Where(b => !excluded.Contains(b) && IsTappable(b)).ToList();

        //Like a child following the audio instructions: what just appeared on screen is usually the next step
        var appeared = buttons.Where(b => !previouslyTappable.Contains(b)).ToList();
        previouslyTappable = new HashSet<Button>(buttons);
        if (appeared.Count > 0 && rng.NextDouble() < 0.85) { TapOrHold(appeared[rng.Next(appeared.Count)]); return; }

        //Mostly doesn't insist on chests it can't pay (the game answers with a long "no stars" message)
        buttons = buttons.Where(b =>
        {
            var chest = b.GetComponent<MonsterMarketButtonBehaviour>();
            var unaffordable = chest != null && chest.monsterMarketButton.monsterChestType != MonsterChestType.NONE && chest.monsterMarketButton.costChest > UserDataManager.CurrUser.Coins;
            return !unaffordable || rng.NextDouble() < 0.1;
        }).ToList();
        var holdables = Object.FindObjectsOfType<MonoBehaviour>()
            .Where(m => m is IPointerDownHandler && !(m is Selectable) && m.isActiveAndEnabled && IsReachable(m.gameObject))
            .ToList();

        var options = buttons.Count + holdables.Count;
        if (options == 0) return;
        var pick = rng.Next(options);
        if (pick < buttons.Count) TapOrHold(buttons[pick]);
        else Hold(holdables[pick - buttons.Count].gameObject, 0.6 + rng.NextDouble() * 1.8);
    }

    //Buttons with their own press handling (e.g. hold to move the ship) are held, not tapped
    static void TapOrHold(Button button)
    {
        var holdToUse = button.GetComponents<MonoBehaviour>().Any(m => m is IPointerDownHandler && !(m is Selectable));
        if (holdToUse) Hold(button.gameObject, 1 + rng.NextDouble() * 3);
        else Tap(button);
    }

    //Buttons a child shouldn't need to finish the sequence: restarting, leaving, menus, sound and the debug overlay
    static HashSet<Button> ExcludedButtons()
    {
        var excluded = new HashSet<Button>(Object.FindObjectsOfType<EndOfGameManager>().Select(e => e.resetBtn));
        var ui = GameUIController.Instance;
        if (ui != null) foreach (var name in new[] { "homeBtn", "continueBtn", "exitBtn", "musicBtn" }) excluded.Add(Field<Button>(ui, name));
        foreach (var b in Object.FindObjectsOfType<Button>())
        {
            var n = b.name.ToLowerInvariant();
            if (n.Contains("reset") || n.Contains("exit") || n.Contains("salir") || n.Contains("home") || n.Contains("menu") || n.Contains("music") || n.Contains("sound")) excluded.Add(b);
            if (b.GetComponentInParent<AppSkipSceneButton>() != null) excluded.Add(b);
        }
        excluded.Remove(null);
        return excluded;
    }

    static T Field<T>(object target, string name) where T : class =>
        target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetValue(target) as T;

    // ---------- Input like a finger ----------

    static Vector2 ScreenCenterOf(GameObject go)
    {
        var rect = go.transform as RectTransform;
        if (rect == null) return Vector2.zero;
        var canvas = go.GetComponentInParent<Canvas>();
        var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return RectTransformUtility.WorldToScreenPoint(cam, (corners[0] + corners[2]) / 2);
    }

    static List<RaycastResult> RaycastUI(Vector2 position)
    {
        var results = new List<RaycastResult>();
        if (EventSystem.current == null) return results;
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, results);
        return results;
    }

    //The topmost UI element under its center is itself or one of its children: nothing blocks it
    static bool IsReachable(GameObject go)
    {
        if (!go.activeInHierarchy) return false;
        var hits = RaycastUI(ScreenCenterOf(go));
        return hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(go.transform);
    }

    static bool IsTappable(Button button) => button != null && button.IsInteractable() && button.isActiveAndEnabled && IsReachable(button.gameObject);

    static PointerEventData PointerAt(Vector2 position) => new PointerEventData(EventSystem.current) { position = position, button = PointerEventData.InputButton.Left };

    static void Tap(Button button)
    {
        var data = PointerAt(ScreenCenterOf(button.gameObject));
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
    }

    static void Hold(GameObject target, double seconds)
    {
        var data = PointerAt(ScreenCenterOf(target));
        ExecuteEvents.Execute(target, data, ExecuteEvents.pointerDownHandler);
        releaseAt = Now + seconds;
        pendingRelease = () => { if (target != null) ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler); };
    }

    //A tap on the play area (not on UI), read by the games through GameInput
    static void TapScreen()
    {
        for (int i = 0; i < 10; i++)
        {
            var position = new Vector2((float)(0.2 + rng.NextDouble() * 0.6) * Screen.width, (float)(0.15 + rng.NextDouble() * 0.7) * Screen.height);
            if (RaycastUI(position).Count > 0) continue;
            GameInput.SimulatePress(position);
            releaseAt = Now + 0.08;
            pendingRelease = GameInput.SimulateRelease;
            return;
        }
    }

    //The turbo tutorial stops the rocket in front of a star and shows a hand on it: a child taps there
    static bool TapWhereTheTutorialHandPoints()
    {
        var hand = Object.FindObjectsOfType<TutorialHand_TurboRocket>().FirstOrDefault(h => h.gameObject.activeInHierarchy);
        if (hand == null || Camera.main == null || rng.NextDouble() < 0.2) return false;
        var position = (Vector2)Camera.main.WorldToScreenPoint(hand.transform.position);
        if (RaycastUI(position).Count > 0) return false;
        GameInput.SimulatePress(position);
        releaseAt = Now + 0.08;
        pendingRelease = GameInput.SimulateRelease;
        return true;
    }

    static void StartDrag()
    {
        dragFrom = new Vector2(Screen.width * 0.5f, Screen.height * (float)(0.3 + rng.NextDouble() * 0.4));
        dragTo = dragFrom + new Vector2(0, (float)((rng.NextDouble() - 0.5) * Screen.height * 0.5));
        if (RaycastUI(dragFrom).Count > 0) { ActOnUI(); return; }
        GameInput.SimulatePress(dragFrom);
        dragStart = Now;
        dragEnd = Now + 0.4 + rng.NextDouble() * 0.6;
    }

    static void ContinueDrag()
    {
        var t = Mathf.Clamp01((float)((Now - dragStart) / (dragEnd - dragStart)));
        GameInput.SimulateMove(Vector2.Lerp(dragFrom, dragTo, t));
        if (t < 1) return;
        GameInput.SimulateRelease();
        dragEnd = 0;
    }

    //What a child would see when stuck: what can be tapped, what covers the screen, whether time or audio is stopped
    static string DescribeScreen()
    {
        var sb = new StringBuilder();
        var excluded = ExcludedButtons();
        var buttons = Object.FindObjectsOfType<Button>().Where(b => b.isActiveAndEnabled).ToList();
        sb.AppendLine("- Botones tocables: " + string.Join(", ", buttons.Where(b => !excluded.Contains(b) && IsTappable(b)).Select(PathOf)));
        sb.AppendLine("- Botones visibles pero NO tocables: " + string.Join(", ", buttons.Where(b => !IsTappable(b)).Select(b => $"{PathOf(b)} (interactable={b.IsInteractable()}, tapa: {TopUnder(b.gameObject)})")));
        sb.AppendLine("- Excluidos por el bot: " + string.Join(", ", buttons.Where(b => excluded.Contains(b)).Select(PathOf)));
        sb.AppendLine("- Lo que hay en el centro de la pantalla: " + string.Join(" > ", RaycastUI(new Vector2(Screen.width / 2f, Screen.height / 2f)).Take(4).Select(r => PathOf(r.gameObject.transform))));
        var timeManager = TimeManager.Instance;
        var stopUsers = timeManager != null ? timeManager.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Where(f => typeof(System.Collections.ICollection).IsAssignableFrom(f.FieldType)).Select(f => $"{f.Name}={(f.GetValue(timeManager) as System.Collections.ICollection)?.Count}") : new string[0];
        sb.AppendLine($"- Time.timeScale={Time.timeScale}, TimeManager: {string.Join(", ", stopUsers)}");
        sb.AppendLine("- Audio sonando: " + string.Join(", ", Object.FindObjectsOfType<AudioSource>().Where(a => a.isPlaying && a.clip != null).Select(a => a.clip.name)));
        return sb.ToString();
    }

    static string PathOf(Component c) => PathOf(c.transform);
    static string PathOf(Transform t) { var p = t.name; for (var x = t.parent; x != null && p.Length < 120; x = x.parent) p = x.name + "/" + p; return p; }
    static string TopUnder(GameObject go) { var hits = RaycastUI(ScreenCenterOf(go)); return hits.Count == 0 ? "nada" : PathOf(hits[0].gameObject.transform); }

    // ---------- Results ----------

    static void Finish(string why)
    {
        if (!running) return;
        running = false;
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        if (currentScene != null && why != "secuencia completa") EndScene(why);
        GameInput.ClearSimulation();

        ValidateAnalytics();

        report.AppendLine().AppendLine($"Duración total: {(Now - startedAt) / 60:0.0} min. Fin: {why}.").AppendLine();
        report.AppendLine(problems.Count == 0 ? "## Resultado: OK, sin problemas" : $"## Resultado: {problems.Count} problema(s)");
        foreach (var p in problems) report.AppendLine("- " + p);
        if (diagnostics.Length > 0) report.AppendLine().AppendLine("## Diagnóstico de cuelgues").AppendLine().Append(diagnostics);

        Directory.CreateDirectory(ReportFolder);
        var path = Path.Combine(ReportFolder, $"minigame-bot_{DateTime.Now:yyyy-MM-dd_HHmmss}.md");
        File.WriteAllText(path, report.ToString());

        CleanUpQaData();
        var summary = $"QA bot: {why}, {sceneIndex} escenas, {problems.Count} problema(s). Reporte: {Path.GetFullPath(path)}";
        if (problems.Count == 0) Debug.Log(summary); else Debug.LogWarning(summary);
        if (EditorApplication.isPlaying) EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
    }

    //What would be uploaded: the local pending sessions, plus anything not flushed yet
    static void ValidateAnalytics()
    {
        var collections = new Dictionary<string, List<Dictionary<string, object>>>();
        void Collect(Dictionary<string, Dictionary<string, Dictionary<string, object>>> source)
        {
            foreach (var c in source)
            {
                if (!collections.TryGetValue(c.Key, out var docs)) collections[c.Key] = docs = new List<Dictionary<string, object>>();
                docs.AddRange(c.Value.Values);
            }
        }
        Collect(DatabaseManager.pendingSessionsToUpload);
        Collect(UserDataManager.userAnayticsPerGame);

        report.AppendLine().AppendLine("## Analítica generada vs. columnas del CMS").AppendLine();
        report.AppendLine("| Colección | Documentos | Columnas faltantes o vacías |").AppendLine("|---|---|---|");
        foreach (var contract in CmsContract)
        {
            if (!collections.TryGetValue(contract.Key, out var docs) || docs.Count == 0)
            {
                report.AppendLine($"| {contract.Key} | 0 | (no se generó) |");
                if (startIndex == 0 && sceneLimit == 0) problems.Add($"No se generó analítica de {contract.Key}");
                continue;
            }
            var missing = new SortedDictionary<string, int>();
            foreach (var doc in docs)
                foreach (var key in contract.Value.Concat(CommonKeys))
                    if (!doc.TryGetValue(key, out var value) || value == null || (value is string s && s.Length == 0))
                        missing[key] = missing.TryGetValue(key, out var n) ? n + 1 : 1;

            if (contract.Key == DataIds.sizeRocketsGame)
                foreach (var doc in docs)
                    if (doc.TryGetValue(DataIds.sizeRocketResponseString, out var answer) && !AllowedRocketAnswers.Contains(answer as string))
                        missing["cr_respuesta=" + answer] = missing.TryGetValue("cr_respuesta=" + answer, out var n) ? n + 1 : 1;

            var text = missing.Count == 0 ? "ninguna" : string.Join(", ", missing.Select(m => $"{m.Key} ({m.Value}/{docs.Count})"));
            //Rounds per game: documents that share an id_juego, in the order the games were played
            var perGame = docs.Where(d => d.ContainsKey(DataIds.GameID)).GroupBy(d => d[DataIds.GameID]?.ToString()).Select(g => g.Count().ToString());
            report.AppendLine($"| {contract.Key} | {docs.Count} (por partida: {string.Join(", ", perGame)}) | {text} |");
            if (missing.Count > 0) problems.Add($"{contract.Key}: faltan columnas del CMS: {text}");
        }
        var others = collections.Keys.Except(CmsContract.Keys).Select(k => $"{k} ({collections[k].Count})");
        report.AppendLine().AppendLine("Otras colecciones generadas: " + string.Join(", ", others));
    }

    static void CleanUpQaData()
    {
        if (qaUser != null) DatabaseManager.userDatas.RemoveAll(u => u.id_jugador == qaUser.id_jugador);
        DatabaseManager.pendingUserDatasToUpload.RemoveAll(u => u.id_proyecto == QaInstitution);
        DatabaseManager.pendingSessionsToUpload.Clear();
        UserDataManager.userAnayticsPerGame.Clear();
        UserDataManager.Instance.SetCurrUser(null);
        RestoreLocalSave();
    }

    // ---------- Local save snapshot (PlayerPrefs) ----------

    [Serializable] class Snapshot { public List<string> keys = new List<string>(); public List<string> values = new List<string>(); }

    static void SnapshotLocalSave()
    {
        if (EditorPrefs.HasKey(SnapshotKey)) return; //keep the oldest one if a run was interrupted
        var snapshot = new Snapshot();
        foreach (var key in LocalSaveKeys)
        {
            snapshot.keys.Add(key);
            snapshot.values.Add(PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null);
        }
        EditorPrefs.SetString(SnapshotKey, JsonUtility.ToJson(snapshot));
    }

    static void RestoreLocalSave()
    {
        if (!EditorPrefs.HasKey(SnapshotKey)) return;
        var snapshot = JsonUtility.FromJson<Snapshot>(EditorPrefs.GetString(SnapshotKey));
        for (int i = 0; i < snapshot.keys.Count; i++)
        {
            if (string.IsNullOrEmpty(snapshot.values[i])) PlayerPrefs.DeleteKey(snapshot.keys[i]);
            else PlayerPrefs.SetString(snapshot.keys[i], snapshot.values[i]);
        }
        PlayerPrefs.Save();
        EditorPrefs.DeleteKey(SnapshotKey);
        Debug.Log("QA bot: local save restored");
    }
}
