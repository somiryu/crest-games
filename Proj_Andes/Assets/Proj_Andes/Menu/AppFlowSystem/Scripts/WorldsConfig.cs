using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldsConfig", menuName = "GameSequencesList/WorldsConfig")]
public class WorldsConfig : SimpleGameSequenceItem
{
    public PickAWorld pickAWorld;
    public int gameIndex;
    public void Init()
    {
        pickAWorld.gameIdx = gameIndex;
        pickAWorld.worldsConfig = this;
    }
    public void AssignPlanetIdx()
    {
        GameSequencesList.CurrPlanetIdx = gameIndex;
    }
}
