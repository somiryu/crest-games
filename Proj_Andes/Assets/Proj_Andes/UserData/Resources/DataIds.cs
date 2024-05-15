using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataIds
{
    public const string usersCollection = "usuario";

    public const string TestID = "id_prueba";
    public const string GameOrderInSequence = "orden";
    public const string UserID = "id_jugador";
    public const string GameID = "id_juego";
    public const string GameType = "juego_tipo";

    //Generic IDs
    public const string totalClicks = "click_repetions";
    public const string lostBecauseOfTime = "time_over";
    public const string won = "correct";
    public const string responseTime = "time_response";
    public const string challengeType = "challenge_type";

    //General Games collection IDs
    public const string generalGamesCollID = "juegos";
    public const string wins = "logros_general";
    public const string losses = "perdidas_general";
    public const string clicks = "clicks";
    public const string averageClickTime = "frecuencia_media_clicks";
	public const string lastFrustrationLevel = "ultima_frustracion_selec";
	public const string GameModeAboutAnalytics = "game_mode";

	//Monster market
	public const string monsterMarket = "mercado_monstruos";
	public const string chestChosen = "chest_chosen";
	public const string starsSpent = "stars_spent";
	public const string starsBeforeSpend = "stars_before_spend";
	public const string unspentStars = "unspent_stars";
	public const string selectionTime = "selection_time";

	//Frustration levels
	public const string frustrationGames = "frustration_levels";
	public const string repetition = "repeticion";
    public const string timePlayed = "tiempo_juego_total";
    public const string lostByCheat = "lostByCheat";
    public const string frustrationLevel = "frustrationLevel";

    //Narratives IDs
    public const string Narratives1 = "m1_empatia";
    public const string Narratives2 = "m2_agres_confl";
    public const string Narratives3 = "m3_emoc_propias";

    //Data per game
    public const string stars = "stars";

    public const string voiceStarGame = "nubes_flores";

    public const string turboRocketGame = "turbo_estrellas";

    public const string mechanicHandGame = "frustr_garra";
    public const string mechanicHandClawThrows = "claw_Throws";

    public const string fightTheAlienGame = "monstruo_figuras";

    public const string boostersAndScapeGame = "frustr_persecusion";
    public const string boostersAndScapeTotalBoostsActivated = "boosters_activated";

    public const string magnetsGame = "buggy: los imanes y la energía";
    public const string magnetsEneryPicked = "magnet_collected";

    public const string sizeRocketsGame = "carrito_recolector";
    public const string sizeRocketsBigShips = "big_ships";
    public const string sizeRocketsMidShips = "medium_ships";
    public const string sizeRocketsSmallShips = "small_ships";

    public const string tryAgainClicksMechHand = "fg_espera_click";
    public const string tryAgainClicksBoosters = "fp_espera_click";
    public const string tryAgainClicksAfterWait = "fg_espera_click_afterWait";

    //General Test analytics
    public const string test = "prueba";
    public const string institutionCode = "id_proyecto";
    public const string created_At = "inicio";
    public const string ended_At = "fin";
    public const string age = "edad";
    public const string grade = "grado";
    public const string state = "estado";
    public const string time_Spent = "tiempo_total";

	//Size rockets analytics
	public const string frustrationMode = "cr_modo";
	public const string tryIndex = "cr_intento";
	public const string sizeRocketResponseString = "cr_respuesta";
	public const string sizeRocketResponseInt = "cr_codigo";
	public const string mouseUpCount = "cr_release";
	public const string starsPerRound = "cr_premio";

    //Hearts and flowers analytics
    public const string heartsAndStarsGame = "corazones_flores";
    public const string heartsNFlowersFrustration= "cf_modo";
    public const string heartsNFlowersSkill = "cf_habilidad";
    public const string heartsNFlowersTest = "cf_prueba";
    public const string heartsNFlowersValid = "cf_valid";
    public const string heartsNFlowersTrial = "cf_intento";
    public const string heartsNFlowersStimuli = "cf_estimulo";
    public const string heartsNFlowersAnswer = "cf_respuesta";
    public const string heartsNFlowersCode = "cf_codigo";
    public const string heartsNFlowersTime = "cf_tiempo";

    //Cloud or flower analytics
    public const string cloudNFlowerFrustrationMode = "nf_modo";
    public const string cloudNFlowerTest = "nf_prueba";
    public const string cloudNFlowerPassedTuto1 = "nf_valid_1";
    public const string cloudNFlowerPassedTuto2 = "nf_valid_2";
    public const string cloudNFlowerPassedTuto3 = "nf_valid_3";
    public const string cloudNFlowerTrial = "nf_intento";
    public const string cloudNFlowerAudibleStimuli = "nf_estimulo_aud";
    public const string cloudNFlowerVisualStimuli = "nf_estimulo_vis";
    public const string cloudNFlowerAnswer = "nf_respuesta";
    public const string cloudNFlowerCode = "nf_codigo";
    public const string cloudNFlowerTimer = "nf_tiempo";



    //Turbo rocket analytics
    public const string turboStarsFrustrationMode = "te_modo";
    public const string turboStarsStars = "te_estrellas";
    public const string turboStarsTurboUses = "te_turbo";
    public const string turboStarsTurboTime = "te_turbo_tm";
    public const string turboStarsTime= "te_tiempo_total";



    //MechHand analytics
    public const string mechHandTest = "fg_prueba";
    public const string mechHandTrial = "fg_intento";
    public const string mechHandThrown = "fg_lanzamiento";
    public const string mechHandPresicion = "fg_precision";
    public const string mechHandFeelAnswer = "fg_emoc_rta";
    public const string mechHandFeelCode = "fg_emoc_cod";
    public const string mechHandFeelTiming = "fg_emoc_tm";
    public const string mechHandWaitClick = "fg_espera_click";


    //BoostHand analytics
    public const string frustPersTest= "fp_prueba";
    public const string frustPersTrial = "fp_intento";
    public const string frustPersPresition = "fp_precision";
    public const string frustPersBoostClicks = "fp_turbo";
    public const string frustPersFeelAnswer = "fp_emoc_rta";
    public const string frustPersFeelCode = "fp_emoc_cod";
    public const string frustPersFeelTiming = "fp_emoc_tm";
    public const string frustPersWaitClick = "fp_espera_click";

    //Monster market
    public const string marketMonsterOrder = "mm_orden";
    public const string marketMonsterStarPre = "mm_estrellas_pre";
    public const string marketMonsterStarsSpent = "mm_estrellas_gast";
    public const string marketMonsterStarsAfter = "mm_estrellas_post";
    public const string marketMonsterChestTrial = "mm_cofre_intento";
    public const string marketMonsterChestAnswer = "mm_cofre_rta";
    public const string marketMonsterChestCode = "mm_cofre_cod";
    public const string marketMonsterTotalTime = "mm_cofre_tm";

}
