using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataIds
{
    public const string usersCollection = "users";

    public const string TestID = "test ID";
    public const string GameOrderInSequence = "game_order";
    public const string UserID = "id_jugador";
    public const string GameID = "id_juego";
    public const string GameType = "game Type";

    //Generic IDs
    public const string totalClicks = "click_repetions";
    public const string lostBecauseOfTime = "time_over";
    public const string won = "correct";
    public const string responseTime = "time_response";
    public const string challengeType = "challenge_type";

    //General Games collection IDs
    public const string generalGamesCollID = "games";
    public const string wins = "general_Achievements";
    public const string losses = "general_Losses";
    public const string clicks = "clicks";
    public const string averageClickTime = "avg_click_frequency";
	public const string lastFrustrationLevel = "last_frustration_selected";
	public const string GameModeAboutAnalytics = "game_mode";

	//Monster market
	public const string monsterMarket = "market";
	public const string chestChosen = "chest_chosen";
	public const string starsSpent = "stars_spent";
	public const string starsBeforeSpend = "stars_before_spend";
	public const string unspentStars = "unspent_stars";
	public const string selectionTime = "selection_time";

	//Frustration levels
	public const string frustrationGames = "frustration_levels";
    public const string timePlayed = "time_played";
    public const string lostByCheat = "lostByCheat";
    public const string frustrationLevel = "frustrationLevel";

    //Narratives IDs
    public const string Narratives = "narrative";

    //Data per game
    public const string stars = "stars";

    public const string voiceStarGame = "campo_nubes";
    public const string voiceStarImage = "image";
    public const string voiceStarSound = "audio";
    public const string voiceStarLostTutorial = "lostTutorial";

    public const string heartsAndStarsGame = "corazones_flores";

    public const string turboRocketGame = "star_booster";
    public const string turboRocketturboUsedTimes = "boosts";

    public const string mechanicHandGame = "frustr_garra";
    public const string mechanicHandClawThrows = "claw_Throws";

    public const string fightTheAlienGame = "monstruo_figuras";

    public const string boostersAndScapeGame = "delay: boosters y escape";
    public const string boostersAndScapeTotalBoostsActivated = "boosters_activated";

    public const string magnetsGame = "buggy: los imanes y la energía";
    public const string magnetsEneryPicked = "magnet_collected";

    public const string sizeRocketsGame = "carrito_recolector";
    public const string sizeRocketsBigShips = "big_ships";
    public const string sizeRocketsMidShips = "medium_ships";
    public const string sizeRocketsSmallShips = "small_ships";

    public const string tryAgainClicks = "retry_clicks";

    //General Test analytics
    public const string test = "test";
    public const string institutionCode = "id_proyecto";
    public const string created_At = "created_At";
    public const string age = "edad";
    public const string grade = "grado";
    public const string state = "state";
    public const string time_Spent = "time_Spent";

	//Size rockets analytics
	public const string frustrationMode = "cr_modo";
	public const string tryIndex = "cr_intento";
	public const string sizeRocketResponseString = "cr_respuesta";
	public const string sizeRocketResponseInt = "cr_codigo";
	public const string mouseUpCount = "cr_release";
	public const string starsPerRound = "cr_premio";

    //MechHand analytics
    public const string mechHandTrial = "fg_intento";
    public const string mechHandThrown = "fg_lanzamiento";
    public const string mechHandPresition = "fg_precision";
    public const string mechHandFeelAnswer = "fg_emoc_rta";
    public const string mechHandFeelCode = "fg_emoc_cod";
    public const string mechHandFeelTiming = "fg_emoc_tm";
    public const string mechHandWaitClick = "fg_espera_click";


    //BoostHand analytics
    public const string frustPersTrial = "fp_intento";
    public const string frustPersPresition = "fp_precision";
    public const string frustPersBoostClicks = "fp_turbo";
    public const string frustPersFeelAnswer = "fp_emoc_rta";
    public const string frustPersFeelCode = "fp_emoc_cod";
    public const string frustPersFeelTiming = "fp_emoc_tm";
    public const string frustPersWaitClick = "fp_espera_click";

}
