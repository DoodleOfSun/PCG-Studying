using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using Completed;

public enum TileType
{
    essential, random, empty, chest, enemy
}

public class DungeonManager : MonoBehaviour
{
    [Serializable]
    public class PathTile
    {
        public TileType type;
        public Vector2 position;
        public List<Vector2> adjacentPathTiles;

        // 생성자
        public PathTile(TileType t, Vector2 p, int min, int max, Dictionary<Vector2, TileType> currentTiles)
        {
            //type = (TileType)Enum.Parse(typeof(TileType),t);
            type = t;
            position = p;
            adjacentPathTiles = getAdjacentPath(min,max,currentTiles); 
        }


        // 어느 타일이 currentTiles에 접촉하는지 조사하여 List에 추가
        public List<Vector2> getAdjacentPath(int minBound, int maxBound, Dictionary<Vector2, TileType> currentTiles)
        {
            List<Vector2> result = new List<Vector2>();
            if (position.y + 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x, position.y + 1)))
            {
                result.Add(new Vector2(position.x, position.y + 1));
            }
            if (position.x + 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x + 1, position.y)))
            {
                result.Add(new Vector2(position.x + 1, position.y));
            }
            if (position.y - 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x, position.y - 1)))
            {
                result.Add(new Vector2(position.x, position.y - 1));
            }
            if (position.x - 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x - 1, position.y)))
            {
                result.Add(new Vector2(position.x - 1, position.y));
            }
            return result;
        }
    } // 여기까지 PathTile 클래스

    public Dictionary<Vector2, TileType> gridPositions = new Dictionary<Vector2, TileType>();

    public int minBound = 0, maxBound;

    public static Vector2 startPos;

    public Vector2 endPos;

    public void StartDungeon()
    {
        gridPositions.Clear();
        maxBound = Random.Range(50 , 101);
        BuildEssentialPath();
        BuildRandomPath();
    }

    private void BuildEssentialPath()
    {
        int randomY = Random.Range(0, maxBound + 1);
        PathTile ePath = new PathTile(TileType.essential, new Vector2(0, randomY), minBound, maxBound, gridPositions);
        startPos = ePath.position;

        int boundTracker = 0;

        while (boundTracker < maxBound)
        {
            gridPositions.Add(ePath.position, TileType.empty);
            int adjacentTileCount = ePath.adjacentPathTiles.Count;
            int randomIndex = Random.Range(0, adjacentTileCount);
            Vector2 nextEPathPos;
            if (adjacentTileCount > 0)
            {
                nextEPathPos = ePath.adjacentPathTiles[randomIndex];
            }
            else
            {
                break;
            }

            PathTile nextEPath = new PathTile(TileType.essential, nextEPathPos, minBound, maxBound, gridPositions);
            if (nextEPath.position.x > ePath.position.x || nextEPath.position.x == maxBound - 1 && Random.Range(0,2) == 1)
            {
                ++boundTracker;
            }
            ePath = nextEPath;
        }
        if (!gridPositions.ContainsKey (ePath.position))
        {
            gridPositions.Add(ePath.position, TileType.empty);
        }
        endPos = new Vector2(ePath.position.x, ePath.position.y);
    }

    private void BuildRandomPath()
    {

        // pathQueue를 선언하고 foreach를 사용하여 반복문 시행
        List<PathTile> pathQueueExecute = new List<PathTile>();

        // pathQueue에 새로운 타일을 저장
        List<PathTile> pathQueue = new List<PathTile>();

        foreach (KeyValuePair<Vector2, TileType> tile in gridPositions)
        {
            Vector2 tilePos = new Vector2(tile.Key.x, tile.Key.y);
            pathQueueExecute.Add(new PathTile(TileType.random, tilePos, minBound, maxBound, gridPositions));
            pathQueue.Add(new PathTile(TileType.random, tilePos, minBound, maxBound, gridPositions));
        }

        pathQueueExecute.ForEach(delegate (PathTile tile)
        {
            int adjacentTileCount = tile.adjacentPathTiles.Count;
            if (adjacentTileCount != 0)
            {
                if (Random.Range(0, 5) == 1)
                {
                    BuildRandomChamber(tile);  
                }
                else if (Random.Range(0, 5) == 1 || (tile.type == TileType.random && adjacentTileCount > 1))
                {
                    int randomIndex = Random.Range(0, adjacentTileCount);

                    Vector2 newRPathPos = tile.adjacentPathTiles[randomIndex];
                    if (!gridPositions.ContainsKey(newRPathPos))
                    {
                        if (!gridPositions.ContainsKey(newRPathPos))
                        {
                            if (Random.Range(0, GameManager.instance.enemySpawnRatio) == 1)
                            {
                                gridPositions.Add(newRPathPos, TileType.enemy);
                            }
                            else
                            {
                                gridPositions.Add(newRPathPos, TileType.empty);
                            }
                        }
                        //gridPositions.Add(newRPathPos, TileType.random);
                        PathTile newRPath = new PathTile(TileType.random, newRPathPos, minBound, maxBound, gridPositions);
                        pathQueue.Add(newRPath);
                    }
                }
            }
        }
        );

    }

    private void BuildRandomChamber(PathTile tile)
    {
        Debug.Log("랜덤한 맵 생성!");
        int chamberSize = 3,
            adjacentTileCount = tile.adjacentPathTiles.Count,
            randomIndex = Random.Range(0, adjacentTileCount);
        Vector2 chamberOrigin = tile.adjacentPathTiles[randomIndex];

        for (int x = (int) chamberOrigin.x; x < chamberOrigin.x + chamberSize; x++)
        {
            for (int y = (int) chamberOrigin.y; y < chamberOrigin.y + chamberSize; y++)
            {
                Vector2 chamberTilePos = new Vector2(x, y);
                if (!gridPositions.ContainsKey(chamberTilePos) && 
                    chamberTilePos.x < maxBound && chamberTilePos.x > 0 &&
                    chamberTilePos.y < maxBound && chamberTilePos.y > 0)
                {
                    //gridPositions.Add(chamberTilePos, TileType.empty);
                    // 70분의 1 확률로 아이템 상자 생성
                    if (Random.Range(0, 70) == 1)
                    {
                        Debug.Log("상자 생성");
                        gridPositions.Add(chamberTilePos, TileType.chest);
                    }
                    else
                    {
                        gridPositions.Add(chamberTilePos, TileType.empty);
                    }
                }
            }
        }
    }
}
