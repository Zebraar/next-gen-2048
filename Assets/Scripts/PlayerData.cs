using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int highScore;
    public Dictionary<string, float> playerSettings = new Dictionary<string, float>();
}
