using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;

    public static TimeManager Instance
    {
        get
        {
            if (!instance)
            {
                GameObject go = new GameObject("[Time Manager]");
                instance = go.AddComponent<TimeManager>();
            }
            
            return instance;
        }
    }
    
    private bool paused = false;
    private float hitstopTimer = 0f;
    
    public bool IsPaused => paused;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!paused && hitstopTimer > 0f)
        {
            hitstopTimer -= Time.unscaledDeltaTime;
        }

        Time.timeScale = paused || hitstopTimer > 0f ? 0f : 1f;
    }

    public void SetPaused(bool paused)
    {
        this.paused = paused;
        OnTogglePause?.Invoke(paused);
    }

    public void AddHitstop(float time)
    {
        if (hitstopTimer <= 0f) hitstopTimer = 0f;
        
        hitstopTimer += time;
    }


    public event Action<bool> OnTogglePause;
}
