using System.Collections.Generic;
using AI;
using Game;
using Game.Managers;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyController> Enemies { get; private set; }
    public int NumberOfEnemiesTotal { get; private set; }
    public int NumberOfEnemiesRemaining => Enemies.Count;

    private void Awake()
    {
        Enemies = new List<EnemyController>();
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        Enemies.Add(enemy);
        NumberOfEnemiesTotal++;
    }

    public void UnRegisterEnemy(EnemyController enemyKilled)
    {
        var enemiesRemainingNotification = NumberOfEnemiesRemaining - 1;

        var evt = Events.EnemyKillEvent;
        evt.Enemy = enemyKilled.gameObject;
        evt.RemainingEnemyCount = enemiesRemainingNotification;
        EventManager.Broadcast(evt);

        Enemies.Remove(enemyKilled);
    }
}