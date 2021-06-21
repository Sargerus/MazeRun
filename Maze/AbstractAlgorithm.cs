using System.Collections.Generic;

public enum ALGORITHM
{
    Eller = 1
}

public abstract class AbstractAlgorithm
{
    public const int MAZEWALL = -1;
    public const int MAZEWAY = 0;

    protected int _col, _rows;
    protected List<int> _mazeTable;
    protected ALGORITHM _algorithm;

    public abstract int col { get; }
    public abstract int rows { get; }
    public abstract ALGORITHM algorothm { get; }
    public abstract List<int> mazeTable { get; }

    public abstract List<int> GetMazeTable(int col, int rows, ALGORITHM alg = ALGORITHM.Eller);
    protected abstract void SetDefault(int col, int rows, List<int> mazeTable, ALGORITHM alg);
}
