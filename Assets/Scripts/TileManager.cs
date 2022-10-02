using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : SingletonPattern<TileManager>
{
    private List<FloorTile> floorTiles = new List<FloorTile>();

    protected override void Awake()
    {
        base.Awake();

        //Set floorTiles list
        foreach (FloorTile tile in FindObjectsOfType<FloorTile>())
            floorTiles.Add(tile);
    }

    public void RemoveFloorTile(FloorTile floorTile)
    {
        floorTiles.Remove(floorTile);
    }

    public FloorTile GetRandomSpawnTile()
    {
        int randTileIndex = Random.Range(0, floorTiles.Count);
        FloorTile spawnableTile = floorTiles[randTileIndex];

        //Check that the randomly selected tile can spawn an object
        while (spawnableTile.canSpawnObject != true)
        {
            //Check the random index if it can spawn an object and can lower - if true, leave the loop
            if(floorTiles[randTileIndex].canSpawnObject && floorTiles[randTileIndex].canLower)
            {
                spawnableTile = floorTiles[randTileIndex];
            }
            else //Check next tile in the list
            {
                randTileIndex++;

                if (randTileIndex == floorTiles.Count)
                    randTileIndex = 0;
            }
        }

        return spawnableTile;
    }
}
