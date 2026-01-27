using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//todo: largely untested
public class WaveManager : MonoBehaviour
{
    [Serializable]
    public class Wave
    {
        public int waveNumber;
        public List<EnemySpawnInfo> enemies;
        public float spawnInterval = 1f;
        public float waveDelay = 5f;
    }

    [Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [Serializable]
    public class WaveModifier
    {
        public string description;
        public Action<GameObject> applyModifier;
    }

    public List<Wave> waves;
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabPool; // Optional: For random wave generation
    public float timeBetweenWaves = 10f;
    public List<WaveModifier> globalModifiers;

    private int _currentWaveIndex = 0;
    private bool _isWaveInProgress = false;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action<int> OnAllWavesCompleted;

    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (_currentWaveIndex >= waves.Count)
        {
            DLog.Log("All waves completed!");
            OnAllWavesCompleted?.Invoke(_currentWaveIndex);
            return;
        }

        StartCoroutine(HandleWave(waves[_currentWaveIndex]));
    }

    private IEnumerator HandleWave(Wave wave)
    {
        _isWaveInProgress = true;
        OnWaveStarted?.Invoke(wave.waveNumber);

        foreach (var spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnEnemy(spawnInfo.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        _isWaveInProgress = false;
        OnWaveCompleted?.Invoke(wave.waveNumber);

        _currentWaveIndex++;
        yield return new WaitForSeconds(wave.waveDelay);

        StartNextWave();
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        var enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        foreach (var modifier in globalModifiers)
        {
            modifier.applyModifier?.Invoke(enemy);
        }

        ScaleEnemy(enemy, _currentWaveIndex);
    }

    private void ScaleEnemy(GameObject enemy, int waveNumber)
    {
        if (enemy.TryGetComponent<Health>(out var health))
            health.SetHP(health.MaxHPEffective + waveNumber * 10);

        // if (enemy.TryGetComponent<MovementController>(out var movement))
        //     movement.ModifySpeed(waveNumber * 0.2f);
    }

    public void TriggerNextWave()
    {
        if (!_isWaveInProgress)
            StartNextWave();
    }

    public Wave GenerateRandomWave(int waveNumber)
    {
        var wave = new Wave
        {
            waveNumber = waveNumber,
            enemies = new List<EnemySpawnInfo>(),
            spawnInterval = 1f,
            waveDelay = 5f
        };

        int enemyCount = Mathf.FloorToInt(10 + waveNumber * 2);
        wave.enemies.Add(new EnemySpawnInfo
        {
            enemyPrefab = enemyPrefabPool[UnityEngine.Random.Range(0, enemyPrefabPool.Length)],
            count = enemyCount
        });

        return wave;
    }

    public bool IsWaveInProgress() => _isWaveInProgress;
    public int GetCurrentWaveIndex() => _currentWaveIndex + 1;

    public void AddModifier(WaveModifier modifier)
    {
        globalModifiers.Add(modifier);
    }

    public void RemoveModifier(WaveModifier modifier)
    {
        globalModifiers.Remove(modifier);
    }
}
