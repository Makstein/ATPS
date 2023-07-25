using System;
using Game.Shared;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameState gameState = GameState.Run;

    public static GameManager Instance;

    public Health PlayerHealth;

    private void Awake()
    {
        Instance = this;
    }
}