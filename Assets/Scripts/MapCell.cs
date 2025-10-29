using UnityEngine;
using System;

[Serializable]
public class MapCell
{
    public int x;
    public int y;

    // Walls for each direction
    public bool wallNorth = true;
    public bool wallSouth = true;
    public bool wallEast = true;
    public bool wallWest = true;

    // Cell state
    public bool visited = false;
    public bool isWall = false; // NEW: True if this cell IS a wall (not just has walls)
    public bool isButtonWall = false; // True if this wall cell has a secret button
    public bool isStart = false;
    public bool isBoss = false;
    public bool isCurrentPosition = false;

    // Optional: Enemy or treasure
    public bool hasEnemy = false;
    public bool hasTreasure = false;

    // Secret rooms
    public bool isSecretRoom = false;
    public bool hasSecretButtonNorth = false;
    public bool hasSecretButtonSouth = false;
    public bool hasSecretButtonEast = false;
    public bool hasSecretButtonWest = false;

    public MapCell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // Check if this cell has any secret button
    public bool HasAnySecretButton()
    {
        return hasSecretButtonNorth || hasSecretButtonSouth || hasSecretButtonEast || hasSecretButtonWest;
    }

    // Get all walls as a bitmask for easy checking
    public int GetWallMask()
    {
        int mask = 0;
        if (wallNorth) mask |= 1;
        if (wallEast) mask |= 2;
        if (wallSouth) mask |= 4;
        if (wallWest) mask |= 8;
        return mask;
    }

    // Check if this cell is fully enclosed
    public bool IsFullyWalled()
    {
        return wallNorth && wallSouth && wallEast && wallWest;
    }

    // Check if can move in a direction
    public bool CanMoveNorth() => !wallNorth;
    public bool CanMoveSouth() => !wallSouth;
    public bool CanMoveEast() => !wallEast;
    public bool CanMoveWest() => !wallWest;
}
