using System;

[Serializable]
public class PlayersData
{
    public int _levelPassed { get; private set; }
    public int _coinsTotal { get; private set; }

    public void IncrementData(int levelPassed, int coinsCollected)
    {
        _levelPassed += levelPassed;
        _coinsTotal += coinsCollected;
    }
}
