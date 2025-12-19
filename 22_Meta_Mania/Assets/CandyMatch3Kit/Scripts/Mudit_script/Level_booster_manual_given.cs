using UnityEngine;
using GameVanilla.Game.Common;
using GameVanilla.Core;   // IMPORTANT
using System.Collections.Generic;

public class Level_booster_manual_given : MonoBehaviour
{
    private GameBoard gameBoard;
    private void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();

        if (Booster_manager_singleton_m.Instance != null &&
            Booster_manager_singleton_m.Instance.giveRandomBooster)
        {
            GiveRandomBoosters(Booster_manager_singleton_m.Instance.boostersToGive);
        }
    }

    // -------------------------
    // ?? Random booster giver (CANDY ONLY)
    // -------------------------
    public void GiveRandomBoosters(int count)
    {
        int attemptsLimit = 50; // safety

        for (int i = 0; i < count; i++)
        {
            int attempts = 0;
            GameObject validCandy = null;

            while (validCandy == null && attempts < attemptsLimit)
            {
                int x = Random.Range(0, gameBoard.level.width);
                int y = Random.Range(0, gameBoard.level.height);

                var tile = gameBoard.GetTile(x, y);

                if (IsNormalCandy(tile))
                {
                    validCandy = tile;

                    int boosterType = Random.Range(0, 3);
                    switch (boosterType)
                    {
                        case 0: GiveStripedBooster(x, y); break;
                        case 1: GiveWrappedBooster(x, y); break;
                        case 2: GiveColorBombBooster(x, y); break;
                    }
                }

                attempts++;
            }
        }
    }

    // -------------------------
    // Booster creation
    // -------------------------
    public void GiveStripedBooster(int x, int y)
    {
        ReplaceTileWithBooster(x, y);
        gameBoard.CreateHorizontalStripedTile(x, y, GetRandomColor());
    }

    public void GiveWrappedBooster(int x, int y)
    {
        ReplaceTileWithBooster(x, y);
        gameBoard.CreateWrappedTile(x, y, GetRandomColor());
    }

    public void GiveColorBombBooster(int x, int y)
    {
        ReplaceTileWithBooster(x, y);
        gameBoard.CreateColorBomb(x, y);
    }

    // -------------------------
    // Helpers
    // -------------------------
    private void ReplaceTileWithBooster(int x, int y)
    {
        int idx = x + (y * gameBoard.level.width);

        var oldTile = gameBoard.GetTile(x, y);
        if (oldTile == null)
            return;

        // Return tile to pool
        var pooled = oldTile.GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.pool.ReturnObject(oldTile.gameObject);
        }

        // ?? Clear internal tiles[] reference safely
        var tilesField = typeof(GameBoard).GetField(
            "tiles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (tilesField != null)
        {
            var tilesList = tilesField.GetValue(gameBoard) as List<GameObject>;
            if (tilesList != null && idx >= 0 && idx < tilesList.Count)
            {
                tilesList[idx] = null;
            }
        }
    }

    private CandyColor GetRandomColor()
    {
        var colors = gameBoard.level.availableColors;
        return colors[Random.Range(0, colors.Count)];
    }

    // ? KEY FILTER FUNCTION
    private bool IsNormalCandy(GameObject tile)
    {
        if (tile == null) return false;

        return tile.GetComponent<Candy>() != null &&
               tile.GetComponent<StripedCandy>() == null &&
               tile.GetComponent<WrappedCandy>() == null &&
               tile.GetComponent<ColorBomb>() == null;
    }
}