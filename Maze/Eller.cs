using System;
using System.Collections.Generic;
using UnityEngine;

//1) Change List<int> to simple 1-dimension array

public class Eller : AbstractAlgorithm
{
    private const int _step = 2; //step between to cells
    public override int col => _col;

    public override int rows => _rows;

    public override ALGORITHM algorothm => _algorithm;

    public override List<int> mazeTable => _mazeTable;

    public override List<int> GetMazeTable(int col, int rows, ALGORITHM alg = ALGORITHM.Eller)
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        //+2 for borders
        int _col = col + (col - 1) + 2;
        int _row = rows + (rows - 1) + 2;

        List<int> mazeTable = new List<int>(_col * _row); 

        for (int i = 0; i < mazeTable.Capacity; i++)
            mazeTable.Add(0);

        //set borders

        for(int i = 0; i < _row; i+=_step) //set -1 from up to bottom
        {
            for(int j = 0; j < _col; j++)
            mazeTable[i * _col + j] = MAZEWALL;
        }
           
        for(int i = (_row - 1)*_col; i < mazeTable.Capacity; i++) //bottom
            mazeTable[i] = MAZEWALL;
        for(int i = _col; i < (_row - 1) * _col; i += _col) //left
            mazeTable[i] = MAZEWALL;
        for(int i = _col*2 - 1; i < (_row - 1) * _col; i += _col) //right
            mazeTable[i] = MAZEWALL;

        

        mazeTable = proccessRow(1, _col, mazeTable, _row-2);

        string fordisplay = string.Empty;
        for(int i =0; i < _row; i++)
        {
            for(int j = 0; j<_col; j++)
            {
                fordisplay += Convert.ToInt32(mazeTable[_col * i + j]) + " ";
            }
       
            fordisplay += "\n";
        }
       
        Debug.Log(fordisplay);
            
        SetDefault(col, rows, mazeTable, alg);

        return mazeTable;
    }

    private List<int> proccessRow2(int currentRow, int col, List<int> table, int stoprow)
    {
        List<KeyValuePair<int, int>> set = new List<KeyValuePair<int, int>>();

        int lastInRowIndex = (currentRow + 1) * col - 2;
        int firstInRowIndex = currentRow * col + 1;

        int currentSet = table[firstInRowIndex]; //== 0 ? 1 : table[firstInRowIndex];
        for (int j = 1; j < col - 1; j+=_step)
            if (table[currentRow * col + j] >= currentSet)
                currentSet = table[currentRow * col + j] + 1;

        //set sets
        for (int j = 1; j < col - 1; j += _step)
        {
            if (table[currentRow * col + j] == 0)
            {
                table[currentRow * col + j] = currentSet;
                currentSet++;
            }
        }

        //right borders
        for (int j = 1; j + _step < col - 1; j += _step)
        {
            bool isBorderNeeded = UnityEngine.Random.value >= 0.5f;

            //if neighbours have same set right border needed
            if (table[currentRow * col + j] == table[currentRow * col + j + _step])
            {
                table[currentRow * col + j + 1] = MAZEWALL;
                if(currentRow + 1 < stoprow)
                    table[(currentRow + 1) * col + j + 1] = MAZEWALL;
                continue;
            }

            //decide to make border or not
            if (isBorderNeeded) //create right border 
            {
                table[currentRow * col + j + 1] = MAZEWALL;
                if (currentRow + 1 < stoprow)
                    table[(currentRow + 1) * col + j + 1] = MAZEWALL;
                continue;
            }
            //unite if no border created
            else
            {
                table[currentRow * col + j + _step] = table[currentRow * col + j];
                continue;
            }
        }

        if (currentRow >= stoprow)
        {
            int setIndex = table[currentRow * col + 1];
            for (int j = 1; j < col - 2; j += _step)
            {
                if (table[currentRow * col + j + _step] != setIndex)
                {
                    table[currentRow * col + j + 1] = MAZEWAY;
                    setIndex = table[currentRow * col + j + _step];
                }
            }
            set.Clear();

            string fordisplay = string.Empty;
            for (int i = 0; i < table.Count / col; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    fordisplay += Convert.ToInt32(table[col * i + j]) + " ";
                }

                fordisplay += "\n";
            }

            Debug.Log(fordisplay);

            for (int i = 0; i < table.Count; i++)
                table[i] = table[i] >= 0 ? 0 : 1;
            return table;
        }

        set.Clear();

        List<int> set1 = new List<int>();
        int set1value = 0;
        //bottom borders
        for (int j = 1; j < col - 1; j += _step)
        {
            set1.Add(j);
            set1value = table[currentRow * col + j];

            //process set
            if (currentRow * col + j == lastInRowIndex ||
                table[currentRow * col + j + _step] != set1value)
            {
                int membersWithoutBorders = set1.Count;
                for (int i = 0; i < set1.Count; i++)
                {
                    bool isBottomborderNeeded = UnityEngine.Random.value > 0.5f;

                    if (isBottomborderNeeded && membersWithoutBorders > 1)
                    {
                        membersWithoutBorders--;
                        table[(currentRow + 1) * col + set1[i]] = MAZEWALL;
                    }
                }
                set1.Clear();
            }
        }

        //copy row with changes for next iteration
        if (table.Count - col + 1 > currentRow * col) //not in last row
        {
            for (int j = 1; j < col - 1; j++)
            {
                table[(currentRow + 2) * col + j] = table[currentRow * col + j];
            }
            for (int j = 2; j < col - 1; j += _step)
            {
                table[(currentRow + 2) * col + j] = 0;
            }
            for (int j = 1; j < col - 1; j++)
            {
                if (table[(currentRow + 1) * col + j] == MAZEWALL)
                    table[(currentRow + 2) * col + j] = 0;
            }
        }

        return proccessRow2(currentRow += _step, col, table, stoprow);
    }

    private List<int> proccessRow(int currentRow, int col, List<int> table, int stoprow)
    {
        List<KeyValuePair<int, int>> set = new List<KeyValuePair<int, int>>();

        int lastInRowIndex = (currentRow + 1) * col - 2;
        int firstInRowIndex = currentRow * col + 1;
        int currentSet = table[firstInRowIndex]; //== 0 ? 1 : table[firstInRowIndex];//table[currentRow * col];
        for (int j = 0; j < col; j++)
            if (table[currentRow * col + j] >= currentSet)
                currentSet = table[currentRow * col + j] + 1;


        //set sets
        for (int j = 1; j < col-1; j += _step)
        {
            if (table[currentRow * col + j] == 0)
            {
                table[currentRow * col + j] = currentSet;
                currentSet++;
            } 
            //else if(table[currentRow * col + j] > 0)
            //{
            //    currentSet = table[currentRow * col + j] + 1;
            //}
        }

        //right borders
        for (int j = 1; j + _step < col - 1; j += _step)
        {
            //if (currentRow * col + j == lastInRowIndex) continue;

            bool isBorderNeeded = UnityEngine.Random.value >= 0.5f;
            
            //if neighbours have same set right border needed
            if (table[currentRow * col + j] == table[currentRow * col + j + _step])
            {
                table[currentRow * col + j + 1] = MAZEWALL;
                //table[(currentRow + 1) * col + j + 1] = MAZEWAY;
                continue;
            }

            //decide to make border or not
            if (isBorderNeeded) //create right border 
            {
                table[currentRow * col + j + 1] = MAZEWALL;
                //table[(currentRow + 1) * col + j + 1] = MAZEWAY;
                continue;
            }
            //unite if no border created
            else
            {
                table[currentRow * col + j + _step] = table[currentRow * col + j];
                continue;
            }
        }

        if (currentRow >= stoprow)
        {
            //for (int j = 1; j < col - 2; j++)
            //{
            //    table[currentRow * col + j] = table[(currentRow - 2) * col + j];
            //}
            
            int setIndex = table[currentRow * col + 1];
            for (int j = 1; j < col - 2; j += _step)
            {
                if (table[currentRow * col + j + _step] != setIndex)
                {
                    table[currentRow * col + j + 1] = MAZEWAY;
                    setIndex = table[currentRow * col + j + _step];
                }
            }
            set.Clear();

            for (int i = 0; i < table.Count; i++)
                table[i] = table[i] >= 0 ? 0 : 1;
            return table;
        }

        set.Clear();
        //bottom borders
        for (int j = 1; j < col - 1; j += _step)
        {
            set.Add(new KeyValuePair<int,int>(j,table[currentRow * col + j]));

            //process set
            if (currentRow * col + j == lastInRowIndex ||
                table[currentRow * col + j + _step] != set[0].Value)
            {
                //bool noBottomBorder = true;
                int membersWithoutBorders = set.Count;
                for (int i = 0; i < set.Count; i++)
                {
                    bool isBottomborderNeeded = UnityEngine.Random.value > 0.5f;

                    if (isBottomborderNeeded && membersWithoutBorders > 1)
                    //   (!isBottomborderNeeded && !noBottomBorder) ||
                    //   (set.Count == 1 && !noBottomBorder))
                    {
                        
                        membersWithoutBorders--;
                        //noBottomBorder = false;
                    }
                    else
                    {
                        table[(currentRow + 1) * col + set[i].Key] = MAZEWAY;
                    }
                }

                set.Clear();
            }
        }

        //copy row with changes for next iteration
        if (table.Count - col + 1 > currentRow * col) //not in last row
        {
            for (int j = 1; j < col - 1; j ++)
            {
                if (table[(currentRow + 1) * col + j] == MAZEWALL)
                    continue;
                else table[(currentRow + 2) * col + j] = table[currentRow * col + j];
            }
            //for (int j = 1; j < col - 1; j++)
            //{
            //    table[(currentRow + 2) * col + j] = table[currentRow * col + j];
            //    if (table[(currentRow + 1) * col + j] == MAZEWALL)
            //        table[(currentRow + 2) * col + j] = 0;
            //}
            //for (int j = 2; j < col - 1; j+=_step)
            //{
            //    table[(currentRow + 2) * col + j] = 0;
            //}
        }

        return proccessRow(currentRow += _step, col, table, stoprow);
    }

    protected override void SetDefault(int col, int rows, List<int> mazeTable, ALGORITHM alg)
    {
        _col = col;
        _rows = rows;
        _mazeTable = mazeTable;
        _algorithm = alg;
    }
}
