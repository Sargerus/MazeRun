using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LevelMode:byte
{
    Standard=0
}

public class LevelConstructor : MonoBehaviour
{
    private GameObject _wallPrefab;
    private GameObject _floorPrefab;

    private GameObject _wallsContainer;
    private GameObject _miscContainer;
    private GameObject _gateObject;

    private Vector3 _wallPrefabExtent;
    private Vector3 _floorPrefabExtent;

    private GameObject _keyPrefab;
    private GameObject _gatePrefab;
    private GameObject _obstacleMovingWall;
    private GameObject _obstacleHole;

    //current lvl info
    private int _keyCount;//only for mode with keys
    private int _keyCollected;
    private LevelMode _lvlMode;
    private Text _keyCollectedText;
    private Text _coinsCollectedText;
    private int _coinsCollectedOnLevel;

    private Vector3 _cameraTopLeft;
    private Vector3 _cameraBottomRight;

    private void Awake()
    {
        _wallPrefab = (GameObject)Resources.Load("Prefabs/Environment/Wall", typeof(GameObject));
        _floorPrefab = (GameObject)Resources.Load("Prefabs/Environment/Floor", typeof(GameObject));
        _gatePrefab = (GameObject)Resources.Load("Prefabs/Environment/Gate", typeof(GameObject));
        _keyPrefab = (GameObject)Resources.Load("Prefabs/Collectable/Donut", typeof(GameObject));
        _obstacleMovingWall = (GameObject)Resources.Load("Prefabs/Obstacles/MovingWall", typeof(GameObject));
        _obstacleHole = (GameObject)Resources.Load("Prefabs/Obstacles/Hole", typeof(GameObject));

        _wallsContainer = GameObject.FindWithTag("WallsContainer");
        _miscContainer = GameObject.FindWithTag("MiscContainer");

        //_floorContainer = GameObject.FindWithTag("FloorContainer");

        if (_wallPrefab != null) _wallPrefabExtent = _wallPrefab.GetComponent<MeshRenderer>().bounds.extents;
        if (_floorPrefabExtent != null) _floorPrefabExtent = _floorPrefab.GetComponent<MeshRenderer>().bounds.extents;

        StartCoroutine(LoadTextElements());
    }

    private System.Collections.IEnumerator LoadTextElements()
    {
        while(_keyCollectedText == null)
        {
            var go = GameObject.Find("KeyCollectedText");
            if (go)
            {
                _keyCollectedText = go.GetComponent<Text>();
                UpdateKeyCollectedText();
            }                

            yield return new WaitForSeconds(0.5f);
        }

        while (_coinsCollectedText == null)
        {
            var go2 = GameObject.Find("CoinsCollectedText");
            if (go2)
            {
                _coinsCollectedText = go2.GetComponent<Text>();
                UpdateCoinsCollected(0);
            }                

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Start()
    {
        _coinsCollectedOnLevel = 0;
    }

    public void KeyCollected()
    {
        _keyCollected++;
        UpdateKeyCollectedText();
        UpdateCoinsCollected(10);
        switch (_lvlMode)
        {
            case LevelMode.Standard:
                if (_keyCount == _keyCollected)
                    OpenFinishGate();
                break;
        }
    }

    public void UpdateKeyCollectedText()
    {
        if (_keyCollectedText)
            _keyCollectedText.text = _keyCollected + " / 3";
    }

    public void UpdateCoinsCollected(int count)
    {
        _coinsCollectedOnLevel += count;
        if (_coinsCollectedText)
            _coinsCollectedText.text = _coinsCollectedOnLevel.ToString();
    }

    private void OpenFinishGate()
    {            
        Instantiate(_gatePrefab, _gateObject.transform.localPosition, Quaternion.identity);
    }

    public void BuildMaze(ALGORITHM alg, int col, int rows, LevelMode lvlMode, int keyCount = 3)
    {
        _cameraTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 10f));
        _cameraBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 10f));

        float actualWidth = _cameraBottomRight.x - _cameraTopLeft.x;
        float actualHeight = _cameraTopLeft.y - _cameraBottomRight.y;
        int playersSpawnIndex = 0;

        Dictionary<int, Vector3> cells = new Dictionary<int, Vector3>();
        List<int> table = new List<int>(), tableContent = new List<int>();
        List<(Vector3, Vector3)> coords = new List<(Vector3, Vector3)>();

        int _col = col + (col - 1) + 2;
        int _row = rows + (rows - 1) + 2;

        Vector3 scaleVector = new Vector3(actualWidth / _col, actualHeight / _row, 1);

        GameObject player = null, plane = null;

        UpdateKeyCollectedText();
        UpdateCoinsCollected(0);

        switch (alg)
        {
            case ALGORITHM.Eller:

                Eller eller = new Eller();

                table = eller.GetMazeTable(col, rows); tableContent = new List<int>(table);
                Dictionary<int, GameObject> gameObjectIndex = new Dictionary<int, GameObject>();
                Vector3 ext = new Vector3();

                GameObject g;
                for (int i = 0, iStep = 1; i < _row; i++, iStep += 2)
                {
                    for (int j = 0, jStep = 1; j < _col; j++, jStep += 2)
                    {
                        Vector3 spawnPoint = new Vector3(_cameraTopLeft.x, _cameraTopLeft.y, _cameraTopLeft.z);
                        spawnPoint.x += jStep * _wallPrefabExtent.x * scaleVector.x;
                        spawnPoint.y -= iStep * _wallPrefabExtent.y * scaleVector.y;
                        spawnPoint.z = 10f;

                        if (table[i * _col + j] == 1)
                        {
                            g = Instantiate(_wallPrefab, spawnPoint, Quaternion.identity, _wallsContainer.transform);
                            g.transform.localScale = new Vector3(g.transform.localScale.x * scaleVector.x, g.transform.localScale.y * scaleVector.y, g.transform.localScale.z);
                            gameObjectIndex.Add(i * _col + j, g);
                            ext = new Vector3(_wallPrefabExtent.x * scaleVector.x, _wallPrefabExtent.y * scaleVector.y, 0);
                        }

                        else
                        {
                            cells.Add(i * _col + j, spawnPoint);
                            if (player == null) 
                            { 
                                player = Instantiate((GameObject)Resources.Load("Prefabs/Player/Ball", typeof(GameObject)), new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z), Quaternion.identity); tableContent[i * _col + j] = -1;
                                playersSpawnIndex = i * _col + j;
                            }
                            //player.transform.localPosition = new Vector3(player.transform.localPosition.x, player.transform.localPosition.y, player.transform.localPosition.z - player.transform.localScale.z);

                        }
                        coords.Add(((spawnPoint - ext), (spawnPoint + ext)));
                    }
                }

                int bufGate;
                Vector3 cellPos;
                //if no gate was chosen choose one manually
                while (_gateObject == null)
                {
                    Random.InitState((int)System.DateTime.Now.Ticks);
                    int ind = Random.Range(0, tableContent.Count);
                    bufGate = tableContent[ind];
                    if(bufGate == 0)
                    {
                        _gateObject = new GameObject("Gates");
                        cells.TryGetValue(ind, out cellPos);
                        _gateObject.transform.position = cellPos;
                        tableContent[ind] = -1;
                    }
                }
                #region need to scale walls to make them more tight
              
                //vertical algorithm
                List<GameObject> tightGameObjects = new List<GameObject>();
                List<int> copyTable = new List<int>(table);
                Vector3 shift = Vector3.zero;
                int saveR = 0;
                GameObject block;
              
                for (int r = 1; r < _row - 1; r++)
                {
                    for (int k = 1; k < _col - 1; k++)
                    {
                        if (copyTable[r * _col + k] == 1 && copyTable[(r + 1) * _col + k] == 1)
                        {
                            if (copyTable[(r - 1) * _col + k] == 1)
                                shift.y += _wallPrefabExtent.y * scaleVector.y * 0.2f;
              
                            gameObjectIndex.TryGetValue(r * _col + k, out block);
                            tightGameObjects.Add(block);
                            copyTable[r * _col + k] = -1;
              
                            saveR = r;
              
                            while (copyTable[(saveR + 1) * _col + k] != 0 && saveR + 1 < _row - 1)
                            {
                                gameObjectIndex.TryGetValue((saveR + 1) * _col + k, out block);
                                tightGameObjects.Add(block);
                                copyTable[(saveR + 1) * _col + k] = -1;
              
                                saveR++;
                            }
              
                           if (copyTable[(saveR + 1) * _col + k] == 1)
                               shift.y -= _wallPrefabExtent.y * scaleVector.y * 0.2f;
              
                            foreach (var item in tightGameObjects)
                            {
                                item.transform.localScale = new Vector3(item.transform.localScale.x * 0.3f, item.transform.localScale.y, item.transform.localScale.z);
                                //item.transform.localPosition = new Vector3(item.transform.localPosition.x + shift.x, item.transform.localPosition.y + shift.y, item.transform.localPosition.z);
                            }
                                
              
                            shift = Vector3.zero;
                            tightGameObjects.Clear();
                        }
                    }
                }
              
               Vector3 startPos, endPos;
               GameObject obj, neighbor;

               for (int r = 1; r < _row - 1; r++)
               {
                   for (int k = 1; k < _col - 1; k++)
                   {
                        if(copyTable[r * _col + k] == 1)
                        {
                            if (copyTable[(r + 1) * _col + k] == 0 && copyTable[(r - 1) * _col + k] == 0 &&
                                copyTable[r * _col + k + 1] == 0 && copyTable[r * _col + k - 1] == 0)
                            {
                                gameObjectIndex.TryGetValue(r * _col + k, out block);
                                block.transform.localScale = new Vector3(block.transform.localScale.x * 0.35f, block.transform.localScale.y * 0.35f, block.transform.localScale.z);
                                continue;
                            }
               
                            gameObjectIndex.TryGetValue(r * _col + k, out obj);
                            float extentX = obj.GetComponent<Collider>().bounds.extents.x;
                            startPos = endPos = obj.transform.localPosition;

                            if ((copyTable[r * _col + k - 1] == 1 || copyTable[r * _col + k - 1] == -1) && (copyTable[r * _col + k + 1] == 1 || copyTable[r * _col + k + 1] == -1))
                            {
                                gameObjectIndex.TryGetValue(r * _col + k - 1, out neighbor);
                                startPos = neighbor.transform.localPosition;
                                gameObjectIndex.TryGetValue(r * _col + k + 1, out neighbor);
                                endPos = neighbor.transform.localPosition;
                            }
                            else if (copyTable[r * _col + k - 1] == 1 || copyTable[r * _col + k - 1] == -1)
                            {
                                gameObjectIndex.TryGetValue(r * _col + k - 1, out neighbor);
                                startPos = neighbor.transform.localPosition;
                                endPos = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z);
                            }
                            else if (copyTable[r * _col + k + 1] == 1 || copyTable[r * _col + k + 1] == -1)
                            {
                                gameObjectIndex.TryGetValue(r * _col + k + 1, out neighbor);
                                endPos = neighbor.transform.localPosition;
                                startPos = new Vector3(obj.transform.localPosition.x, obj.transform.transform.localPosition.y, obj.transform.transform.localPosition.z);
                            }
                            else continue;
                            
                            //float z = obj.transform.localPosition.z;
                            gameObjectIndex.Remove(r * _col + k);
                            Destroy(obj);
                            obj = Instantiate(_wallPrefab, new Vector3((startPos.x + endPos.x) / 2, startPos.y, startPos.z), Quaternion.identity, _wallsContainer.transform);
                            obj.transform.localScale = new Vector3(endPos.x - startPos.x, obj.transform.localScale.y * scaleVector.y * 0.3f, obj.transform.localScale.z);
                            gameObjectIndex.Add(r * _col + k, obj);
                        }
                    }
               }
              
                #endregion
                
                CombineBlocks(table, _row, _col, gameObjectIndex, scaleVector);

                plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plane.transform.localScale = new Vector3(_col * scaleVector.x, _row * scaleVector.y, 0.1f);
                plane.transform.position = new Vector3(_cameraTopLeft.x + (_cameraBottomRight.x - _cameraTopLeft.x) / 2, _cameraBottomRight.y + (_cameraTopLeft.y - _cameraBottomRight.y) / 2, 10.2f); //new Vector3(plane.transform.localScale.x / 2, 0, plane.transform.localScale.z / 2);
                plane.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/tstst", typeof(Material)); //(Material)Resources.Load("Materials/FloorMaterial", typeof(Material));

                break;
        }

        Graph graph = new Graph(table, coords, _row, _col);
        List<int> indexOfKey;

        switch (lvlMode)
        {
            case LevelMode.Standard:

                int obstaclesWallCount = (keyCount + 1) * keyCount / 4; //n(n-1)/2 - vertices count in graph (keycount + player). Then divide by 2 because only 2 obstacles here and they must be in half
                int obstacleHoleCount = obstaclesWallCount;
                indexOfKey = new List<int>(keyCount + 1);
                indexOfKey.Add(playersSpawnIndex);
                _keyCollected = 0;
                _keyCount = keyCount;
                _lvlMode = lvlMode;
                Vector3 cellPos;
                int ind;
                while(keyCount > 0)
                {
                    ind = Random.Range(0, tableContent.Count);
                    if(tableContent[ind] == 0)
                    {
                        cells.TryGetValue(ind, out cellPos);
                        if (cellPos.x == player.transform.localPosition.x && cellPos.y == player.transform.localPosition.y) continue;
                        cellPos.z -= 1;
                        Instantiate(_keyPrefab, cellPos, Quaternion.Euler(_keyPrefab.transform.localEulerAngles.x, _keyPrefab.transform.localEulerAngles.y, _keyPrefab.transform.localEulerAngles.z), _miscContainer.transform);
                        tableContent[ind] = -1;
                        keyCount--;
                        indexOfKey.Add(ind);
                    }
                }

                //Obstacles
                GameObject copyOfobstacleMovingWall = _obstacleMovingWall, copyOfobstacleHole = _obstacleHole;

                copyOfobstacleMovingWall.transform.localScale = new Vector3(copyOfobstacleMovingWall.transform.localScale.x * scaleVector.x * 0.8f, copyOfobstacleMovingWall.transform.localScale.y * scaleVector.y * 0.8f, copyOfobstacleMovingWall.transform.localScale.z * ((scaleVector.y + scaleVector.x) * 0.8f / 2));
                //copyOfobstacleMovingWall.transform.Rotate(-90, 0, 0);

                copyOfobstacleHole.transform.localScale = new Vector3(player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z) * 0.8f;

                var obstaclesInRandom = PutObstaclesRadomlyIntoArray(new List<(int, GameObject)> { (obstaclesWallCount, copyOfobstacleMovingWall), (obstacleHoleCount, copyOfobstacleHole) });

                var indexesForObstacles = graph.GetRandomIndexesBeetweenIndexes(indexOfKey, obstaclesInRandom.Count / indexOfKey.Count);

                for(int i = 0; i < obstaclesInRandom.Count; i++)
                {
                    cells.TryGetValue(indexesForObstacles[i], out cellPos);
                    Instantiate(obstaclesInRandom[i], cellPos, obstaclesInRandom[i].transform.rotation, _miscContainer.transform);
                }

                //Vector3 rotation = Vector3.zero;
                //int randomI, randomJ;
                //GameObject g, obstacle;
                //while (obstaclesWallCount > 0 || obstacleHoleCount > 0)
                //{
                //    randomI = Random.Range(1, _row - 1);
                //    randomJ = Random.Range(1, _col - 1);
                //
                //    cells.TryGetValue(randomI * _col + randomJ, out cellPos);
                //
                //    if (tableContent[randomI * _col + randomJ] == 0)
                //    {
                //        if (obstaclesWallCount > 0)
                //        {
                //            g = Instantiate(_obstacleMovingWall, cellPos, Quaternion.identity, _miscContainer.transform);
                //            g.transform.localScale = new Vector3(g.transform.localScale.x * scaleVector.x * 0.8f, g.transform.localScale.y * scaleVector.y * 0.8f, g.transform.localScale.z * ((scaleVector.y + scaleVector.x) * 0.8f / 2));
                //            g.transform.Rotate(-90, 0, 0);
                //            obstaclesWallCount--;
                //        }
                //        else
                //        {
                //            if ((tableContent[randomI * _col + randomJ + 1] == 1 && tableContent[randomI * _col + randomJ - 1] == 1) ||
                //                (tableContent[(randomI + 1) * _col + randomJ] == 1 && tableContent[(randomI - 1) * _col + randomJ] == 1))
                //                continue;
                //            g = Instantiate(_obstacleHole, cellPos, Quaternion.identity, _miscContainer.transform);
                //            g.transform.localScale = new Vector3(player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z);
                //            g.transform.localPosition = new Vector3(g.transform.localPosition.x, g.transform.localPosition.y, plane.transform.localPosition.z);
                //            
                //            obstacleHoleCount--;
                //        }
                //
                //        tableContent[randomI * _col + randomJ] = -1;
                //    }
                //}
                break;
        }
    }

    private List<GameObject> PutObstaclesRadomlyIntoArray(List<(int, GameObject)> obstacleAndCount)
    {
        List<GameObject> returnArray = new List<GameObject>();
        List<GameObject> sortedArray = new List<GameObject>();

        foreach (var item in obstacleAndCount)
            for (int i = 0; i < item.Item1; i++)
                sortedArray.Add(item.Item2);

        while(sortedArray.Count > 0)
        {
            int randomIndex = Random.Range(0, sortedArray.Count);
            returnArray.Add(sortedArray[randomIndex]);
            sortedArray.RemoveAt(randomIndex);
        }

        return returnArray;
    }

    private void CombineBlocks(List<int> table, int row, int col, Dictionary<int, GameObject> gameObjectIndex, Vector3 scaleVector)
    {
        List<GameObject> combineBlock = new List<GameObject>();
        Vector3 startPosition, endPosition, initialScale;
        GameObject g, neighbor;
        int saveI = 0, saveJ = 0;

        //borders
        //top
        gameObjectIndex.TryGetValue(0, out g);
        startPosition = g.transform.localPosition;
        initialScale = g.transform.localScale;
        for (int j = 0; j < col; j++)
        {
            gameObjectIndex.TryGetValue(j, out g);
            combineBlock.Add(g);
        }
        endPosition = g.transform.localPosition;
        g = Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(1, 0, 0));
        LiftUp(ref g, 0.1f);

        //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(1, 0, 0));

        //left
        gameObjectIndex.TryGetValue(col, out g);
        startPosition = g.transform.localPosition;
        initialScale = g.transform.localScale;
        for (int i = 1; i < row - 1; i++)
        {
            gameObjectIndex.TryGetValue(i * col, out g);
            combineBlock.Add(g);
        }
        endPosition = g.transform.localPosition;
        g = Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(0, 1, 0));
        LiftUp(ref g, 0.1f);
        //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(0, 1, 0));

        //bottom
        gameObjectIndex.TryGetValue((row - 1) * col + 1, out g);
        startPosition = g.transform.localPosition;
        initialScale = g.transform.localScale;
        for (int j = 0; j < col; j++)
        {
            gameObjectIndex.TryGetValue((row - 1) * col + j, out g);
            combineBlock.Add(g);
        }
        endPosition = g.transform.localPosition;
        g = Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(1, 0, 0));
        LiftUp(ref g, 0.1f);
        //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(1, 0, 0));
        
        //right
        gameObjectIndex.TryGetValue((col * 2) - 1, out g);
        startPosition = g.transform.localPosition;
        initialScale = g.transform.localScale;
        for (int i = 2; i < row; i++)
        {
            gameObjectIndex.TryGetValue((i * col) - 1, out g);
            combineBlock.Add(g);
        }
        endPosition = g.transform.localPosition;
        g = Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(0, 1, 0));
        LiftUp(ref g, 0.1f);
        //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(0, 1, 0));

        //within maze: vertical
        for (int i = 1; i < row - 1; i++)
        {
            for (int j = 1; j < col - 1; j++)
            {
                if (table[i * col + j] == 1 && table[(i + 1) * col + j] == 1)
                {
                    gameObjectIndex.TryGetValue(i * col + j, out g);
                    //startPosition = g.transform.localPosition;
                    combineBlock.Add(g);
                    initialScale = g.transform.localScale;
                    table[i * col + j] = -1;
                    saveI = i;

                    while (saveI + 1 < row - 1 && table[(saveI + 1) * col + j] != 0)
                    {
                        gameObjectIndex.TryGetValue((saveI + 1) * col + j, out g);
                        combineBlock.Add(g);
                        table[(saveI + 1) * col + j] = -1;
                        saveI++;
                    }

                    //endPosition = g.transform.localPosition;

                    g = Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(0, 1, 0), _wallsContainer.transform);
                    LiftUp(ref g, 0.1f);
                    //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(0, 1, 0), _wallsContainer.transform);
                }
            }
        }

        //within maze: horizontal
        combineBlock.Clear();
        
        for (int i = 1; i < row - 1; i++)
        {
            for (int j = 1; j < col - 1; j++)
            {
                if(table[i * col + j] == 1 && (table[i * col + j + 1] == 1 || (table[i * col + j + 1] == -1 && table[i * col + j + 2] == 1)))
                {
                    gameObjectIndex.TryGetValue(i * col + j, out g);
                    
                    combineBlock.Add(g);
                    initialScale = g.transform.localScale;
                    table[i * col + j] = -1;
                    saveJ = j;
        
                    while(saveJ + 1 < col - 1 && 
                        ((table[i * col + saveJ + 1] != 0 && table[i * col + saveJ + 1] != -1) || 
                            (table[i * col + saveJ + 2] != 0 && table[i * col + saveJ + 2] != -1 && table[i * col + saveJ + 1] == -1)))
                    {
                        if(table[i * col + saveJ + 1] == 1)
                        {
                            gameObjectIndex.TryGetValue(i * col + saveJ + 1, out g);
                            table[i * col + saveJ + 1] = -1;
                            saveJ += 1;
                        } 
                        else
                        {
                            gameObjectIndex.TryGetValue(i * col + saveJ + 2, out g);
                            table[i * col + saveJ + 2] = -1;
                            saveJ += 2;
                        }
                        //gameObjectIndex.TryGetValue(i * col + saveJ + 1, out g);
                        combineBlock.Add(g);
                        //table[i * col + saveJ + 1] = -1;
                        //saveJ++;
                    }
        
                    Create1NonUniformBlock(ref combineBlock, initialScale, new Vector3(1, 0, 0), _wallsContainer.transform);
                }
            }
        }

        //for (int i = 1; i < row - 1; i++)
        //{
        //    for (int j = 1; j < col - 1; j++)
        //    {
        //        if (table[i * col + j] == 1 && (table[i * col + j + 1] == 1 || table[i * col + j + 1] == -1) && table[i * col + j - 1] == 0)
        //        {
        //            gameObjectIndex.TryGetValue(i * col + j, out g);
        //            g.transform.localPosition = new Vector3(g.transform.localPosition.x + g.transform.localScale.x, g.transform.localPosition.y, g.transform.localPosition.z);
        //            g.transform.localScale = new Vector3(g.transform.localScale.x * 0.9f, g.transform.localScale.y, g.transform.localScale.z);
        //        }
        //        else if(table[i * col + j] == 1 && table[i * col + j + 1] == 0 && (table[i * col + j - 1] == 1 && table[i * col + j - 1] == -1))
        //        {
        //            gameObjectIndex.TryGetValue(i * col + j, out g);
        //            g.transform.localPosition = new Vector3(g.transform.localPosition.x - g.transform.localScale.x, g.transform.localPosition.y, g.transform.localPosition.z);
        //            g.transform.localScale = new Vector3(g.transform.localScale.x * 0.9f, g.transform.localScale.y, g.transform.localScale.z);                    
        //        }
        //    }
        //}
        //TODO

        ///for (int i = 1; i < row - 1; i++)
        ///{
        ///    for (int j = 1; j < col - 1; j++)
        ///    {
        ///        if (table[i * col + j] == 1 && table[i * col + j + 1] == 1)// || (table[i * col + j + 1] == -1 && table[i * col + j + 2] == 1)))
        ///        {
        ///
        ///            //else if(j + 2 < col - 1 && table[i * col + j + 1] == -1 && table[i * col + j + 2] == 1)
        ///            //    saveI = j + 1;
        ///            //else continue;
        ///            int saveJ = 0;
        ///
        ///             gameObjectIndex.TryGetValue(i * col + j, out g);
        ///             startPosition = g.transform.localPosition;
        ///             startPosition.x -= g.GetComponent<MeshRenderer>().bounds.extents.x;
        ///             combineBlock.Add(g);
        ///             initialScale = g.transform.localScale;
        ///             table[i * col + j] = -1;
        ///             saveJ = j;
        ///
        ///             while (saveJ + 1 < col - 1 && table[i * col + saveJ + 1] == 1) // || table[i * col + saveJ + 2] == 1))
        ///             {
        ///                 int index = 0, saveJInc = 0;
        ///
        ///                 //if (table[i * col + saveJ + 1] == 1) { index = i * col + saveJ + 1; saveJInc = 1; }
        ///                 //else if (table[i * col + saveJ + 2] == 1) { index = i * col + saveJ + 2; saveJInc = 2; } // table[i * col + saveJ + 1] == -1 && 
        ///
        ///                 gameObjectIndex.TryGetValue(i* col +saveJ + 1, out g);
        ///                 combineBlock.Add(g);
        ///                 table[i * col + saveJ + 1] = -1;
        ///                 saveJ++;
        ///
        ///                 //if (saveI + 2 < col - 1 && table[(i * col) + saveI + 1] == -1 && table[i * col + (saveI + 2)] == 1)
        ///                 //    saveI++;
        ///             }
        ///
        ///             //if(saveI + 1 < col - 1 && table[(i * col) + saveI + 1] == -1)
        ///             //    startPosition.x += initialScale.x;
        ///             endPosition = g.transform.localPosition;
        ///             endPosition.x += g.GetComponent<MeshRenderer>().bounds.extents.x;
        ///
        ///             g = Instantiate(_wallPrefab, new Vector3(endPosition.x - startPosition.x, endPosition.y, endPosition.z), Quaternion.identity, _wallsContainer.transform);
        ///             g.transform.localScale = new Vector3((endPosition.x - startPosition.x) / 2, initialScale.y, initialScale.z);
        ///             //Create1Block(startPosition, endPosition, ref combineBlock, initialScale, new Vector3(1, 0, 0), _wallsContainer.transform);
        ///         }
        ///    }
        ///}
    }
    private void Create1Block(Vector3 startPosition, Vector3 endPosition, ref List<GameObject> combineBlock, Vector3 initialScale, Vector3 scaleAxis, Transform parent = null)
    {
        int scaleCount = combineBlock.Count;

        foreach (var item in combineBlock)
            Destroy(item);

        float scaleX, scaleY, scaleZ, startPosX, startPosY, startPosZ;

        startPosX = scaleAxis.x > 0 ? endPosition.x - (endPosition.x - startPosition.x) / 2 : startPosition.x;
        startPosY = scaleAxis.y > 0 ? startPosition.y - (startPosition.y - endPosition.y) / 2 : startPosition.y;
        startPosZ = startPosition.z;

        scaleX = scaleAxis.x > 0 ? initialScale.x * scaleCount : initialScale.x;
        scaleY = scaleAxis.y > 0 ? initialScale.y * scaleCount : initialScale.y;
        scaleZ = scaleAxis.z > 0 ? initialScale.z * scaleCount : initialScale.z;

        GameObject g = Instantiate(_wallPrefab, new Vector3(startPosX, startPosY, startPosZ), Quaternion.identity, parent);
        g.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        combineBlock.Clear();
    }
    private GameObject Create1NonUniformBlock(ref List<GameObject> combineBlock, Vector3 initialScale, Vector3 scaleAxis, Transform parent = null)
    {
        
        float scaleX, scaleY, startPosX, startPosY, endPosX, endPosY;
        //float middleZ, endPosZ, startPosZ;

        //x starts from left, y starts form top
        startPosX = scaleAxis.x > 0 ? combineBlock[0].transform.localPosition.x - (combineBlock[0].transform.localScale.x / 2) : combineBlock[0].transform.localPosition.x; //endPosition.x - (endPosition.x - startPosition.x) / 2 : startPosition.x;
        startPosY = scaleAxis.y > 0 ? combineBlock[0].transform.localPosition.y + (combineBlock[0].transform.localScale.y / 2) : combineBlock[0].transform.localPosition.y; //startPosition.y - (startPosition.y - endPosition.y) / 2 : startPosition.y;
        //startPosZ = combineBlock[0].transform.localPosition.z;

        endPosX = scaleAxis.x > 0 ? combineBlock[combineBlock.Count - 1].transform.localPosition.x + (combineBlock[combineBlock.Count - 1].transform.localScale.x / 2) : combineBlock[combineBlock.Count - 1].transform.localPosition.x;
        endPosY = scaleAxis.y > 0 ? combineBlock[combineBlock.Count - 1].transform.localPosition.y - (combineBlock[combineBlock.Count - 1].transform.localScale.y / 2) : combineBlock[combineBlock.Count - 1].transform.localPosition.y;
        //endPosZ = combineBlock[combineBlock.Count - 1].transform.localPosition.z;

        scaleX = scaleAxis.x > 0 ? Mathf.Abs(endPosX - startPosX) : initialScale.x;
        scaleY = scaleAxis.y > 0 ? Mathf.Abs(startPosY - endPosY) : initialScale.y;
        //middleZ = 1;

        GameObject g = Instantiate(_wallPrefab, new Vector3(
            scaleAxis.x > 0 ? startPosX + scaleX / 2 : startPosX,
            scaleAxis.y > 0 ? startPosY - scaleY / 2 : startPosY,
            combineBlock[0].transform.localPosition.z),
            Quaternion.identity, parent);

        g.transform.localScale = new Vector3(scaleX, scaleY, combineBlock[0].transform.localScale.z);

        foreach (var item in combineBlock)
            Destroy(item);

        combineBlock.Clear();

        return g;
    }

    private void LiftUp(ref GameObject go, float z)
    {
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z - z);
    }

    public int GetCoinsCollected() => _coinsCollectedOnLevel;
}
