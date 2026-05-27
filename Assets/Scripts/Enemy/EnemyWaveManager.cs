using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private List<SpawnGroup<EnemySpawner>> enemySpawnGroups;
    [SerializeField] private List<SpawnGroup<LootSpawner>> lootSpawnGroups;

    [Space]
    [SerializeField, Range(0f, 1f)] private float lastWaveDefeatedThreshold = 0.5f;
    [SerializeField, Min(0f)] private float maxWaveDuration = 30f;
    [SerializeField] private HealthBar progressBar;
    [Space]
    [SerializeField] private bool startOnAwake = true;

    [Space] [SerializeField, Min(-1)] private int finalWaveNumber;
    
    private int waveNumber;
    private List<IEnemy> waveEnemies;
    
    private Coroutine coroutine;
    
    public bool IsRunning => coroutine != null;


    private void Awake()
    {
        waveEnemies = new();
        
        if (startOnAwake) StartWaves(0);
    }

    
    public void StartWaves(int startWave)
    {
        if (coroutine == null)
        {
            waveNumber = startWave;
            coroutine = StartCoroutine(waveCoroutine());
        }
    }
    
    public void StopWaves()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }


    private IEnumerator waveCoroutine()
    {
        for (;; waveNumber++)
        {
            if (waveNumber > finalWaveNumber && finalWaveNumber != -1)
            {
                Debug.Log("Waves ended");
                StopWaves();
                yield break;
            }
            
            Debug.Log($"Wave #{waveNumber}");
            
            // clear old wave
            waveEnemies.Clear();

            foreach (SpawnGroup<EnemySpawner> spawnGroup in enemySpawnGroups.Where(g => waveNumber >= g.StartWave && waveNumber <= g.EndWave))
            {
                // only spawn if it should spawn
                if ((waveNumber - spawnGroup.StartWave) % spawnGroup.WaveInterval == 0)
                {
                    // get how many enemies to spawn
                    int count = spawnGroup.BaseCount +
                                (int)(spawnGroup.CountPerSpawn * ((waveNumber - spawnGroup.StartWave) / spawnGroup.WaveInterval));

                    // spawn the number of enemies at random spawners in the group
                    for (int i = 0; i < Mathf.Min(count, spawnGroup.MaxCount); i++)
                    {
                        // get which spawner to spawn an enemy from
                        int index = Random.Range(0, spawnGroup.Spawners.Count);
                        
                        // spawn an enemy and add it to the wave enemies
                        waveEnemies.Add(spawnGroup.Spawners[index].summon());
                    }
                }
            }
            
            foreach (SpawnGroup<LootSpawner> spawnGroup in lootSpawnGroups.Where(g => waveNumber >= g.StartWave && waveNumber <= g.EndWave))
            {
                // only spawn if it should spawn
                if ((waveNumber - spawnGroup.StartWave) % spawnGroup.WaveInterval == 0)
                {
                    // get how much loot to spawn
                    int count = spawnGroup.BaseCount +
                                (int)(spawnGroup.CountPerSpawn * ((waveNumber - spawnGroup.StartWave) / spawnGroup.WaveInterval));

                    // spawn the number of loot drops at random spawners in the group
                    for (int i = 0; i < Mathf.Min(count, spawnGroup.MaxCount); i++)
                    {
                        // get which spawner to spawn the loot from
                        int index = Random.Range(0, spawnGroup.Spawners.Count);
                        
                        // spawn the loot
                        spawnGroup.Spawners[index].summon();
                    }
                }
            }

            float time = Time.time;
            yield return new WaitUntil(() =>
            {
                // if wave lasted longer than the max duration
                bool timeThresholdMet = maxWaveDuration < Time.time - time;
                
                // or enough enemies have been defeated
                bool killThresholdMet = waveEnemies.Count * lastWaveDefeatedThreshold < waveEnemies.Sum(enemy =>
                { 
                    if (enemy == null) return 1;
                    return enemy.isDead() ? 1 : 0;
                });
                
                progressBar?.setHealth(
                    (1f - (Time.time - time) / maxWaveDuration) * 
                    (1f - waveEnemies.Sum(enemy => { if (enemy == null) return 1; return enemy.isDead() ? 1 : 0; }) / 
                        (waveEnemies.Count * lastWaveDefeatedThreshold)));
                
                return timeThresholdMet || killThresholdMet;
            });
        }
    }
    
    
    [Serializable]
    private struct SpawnGroup<T>
    {
        [SerializeField] private List<T> spawners;
        [Space]
        [SerializeField, Min(0)] private int startWave;
        [SerializeField, Min(0)] private int endWave;
        [SerializeField, Min(1)] private int waveInterval;
        [Space] 
        [SerializeField, Min(0)] private int baseCount;
        [SerializeField, Min(0)] private float countPerSpawn;
        [SerializeField, Min(0)] private int maxCount;
        
        public List<T> Spawners => spawners;
        
        public int StartWave => startWave;
        public int EndWave => endWave;
        public int WaveInterval => waveInterval;
        
        public int BaseCount => baseCount;
        public float CountPerSpawn => countPerSpawn;
        public int MaxCount => maxCount;
    }
}
