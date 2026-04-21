using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private List<SpawnGroup> spawnGroups;

    [Space]
    [SerializeField, Range(0f, 1f)] private float lastWaveDefeatedThreshold = 0.5f;
    [SerializeField, Min(0f)] private float maxWaveDuration = 30f;
    [Space]
    [SerializeField] private bool startOnAwake = true;
    
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
        waveNumber = startWave;
        
        // if coroutine is null, start new
        coroutine ??= StartCoroutine(waveCoroutine());
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
            // clear old wave
            waveEnemies.Clear();

            foreach (SpawnGroup spawnGroup in spawnGroups.Where(g => waveNumber >= g.StartWave && waveNumber < g.EndWave))
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

            float time = Time.time;
            yield return new WaitUntil(() =>
            {
                // if wave lasted longer than the max duration
                bool timeThresholdMet = Time.time - time > maxWaveDuration;
                
                // or enough enemies have been defeated
                bool killThresholdMet = waveEnemies.Count * lastWaveDefeatedThreshold < waveEnemies.Sum(enemy =>
                { 
                    if (enemy == null) return 1;
                    return enemy.isDead() ? 1 : 0;
                });
                
                return timeThresholdMet || killThresholdMet;
            });
        }
    }
    
    
    [Serializable]
    private struct SpawnGroup
    {
        [SerializeField] private List<EnemySpawner> spawners;
        [Space]
        [SerializeField, Min(0)] private int startWave;
        [SerializeField, Min(0)] private int endWave;
        [SerializeField, Min(1)] private int waveInterval;
        [Space] 
        [SerializeField, Min(0)] private int baseCount;
        [SerializeField, Min(0)] private float countPerSpawn;
        [SerializeField, Min(0)] private int maxCount;
        
        public List<EnemySpawner> Spawners => spawners;
        
        public int StartWave => startWave;
        public int EndWave => endWave;
        public int WaveInterval => waveInterval;
        
        public int BaseCount => baseCount;
        public float CountPerSpawn => countPerSpawn;
        public int MaxCount => maxCount;
    }
}
