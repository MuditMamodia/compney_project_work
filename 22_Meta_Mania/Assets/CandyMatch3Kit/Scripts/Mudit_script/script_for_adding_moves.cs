using GameVanilla.Game.Common;
using UnityEngine;

public class script_for_adding_moves : MonoBehaviour
{
    private GameBoard gameBoard;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        settingthemoves_test_function();
    }

    void settingthemoves_test_function()
    {
        //AddMoves(1);
        //SetMoves(1);
    }

    // Add a number of moves to current limit
    public void AddMoves(int amount)
    {
        gameBoard.currentLimit += amount;

        // Update UI safely through public getter
        gameBoard.GetGameUi().SetLimit(gameBoard.currentLimit);
    }

    // Set the total number of moves
    public void SetMoves(int newAmount)
    {
        gameBoard.currentLimit = newAmount;

        // Update UI safely through public getter
        gameBoard.GetGameUi().SetLimit(newAmount);
    }
}