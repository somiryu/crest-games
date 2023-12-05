using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_FightTheAlienGameConfigs", menuName = "MiniGames/MG_FightTheAlienGameConfigs")]
public class MG_FightTheAlienGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int PlayerHealth = 10;
	public int EnemyHealth = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;

	public int playerHealthLostOnWrongAnswer = -1;
	public int EnemyHealthLostOnRightAnswer = -1;
	public SceneReference gameScene;
	public override SceneReference scene => gameScene;
}
