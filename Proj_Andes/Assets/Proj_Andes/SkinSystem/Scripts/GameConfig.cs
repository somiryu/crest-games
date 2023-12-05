using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tymski;

public abstract class GameConfig : ScriptableObject
{
    public abstract SceneReference scene { get;}
}
