using System.Collections.Generic;
using UnityEngine;

// Единая точка воспроизведения звука. Синглтон по образцу TelemetryManager:
// один на сцену уровня (сцена одна на весь эксперимент), DontDestroyOnLoad не нужен.
// Все игровые скрипты дёргают AudioManager.Instance?.PlaySfx(...) — мягко, без жёсткой зависимости.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Один enum = один список озвучиваемых событий. Добавляешь событие — добавляешь сюда.
    public enum Sfx
    {
        Shot,           // выстрел
        AmmoEmpty,      // сухой щелчок: магазин пуст
        EnemyDeath,     // снаряд убил врага
        PlayerHit,      // игрок получил урон
        PlayerDeath,    // игрок погиб
        AmmoPickup,     // подобрал патроны
        Checkpoint,     // чекпоинт сохранён
        EnemySpawn,     // пошла волна
        LevelComplete,  // финиш
        UIClick         // кнопка интерфейса
    }

    [System.Serializable]
    public struct SfxEntry
    {
        public Sfx id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("Музыка")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Звуки (одно событие = одна запись)")]
    [SerializeField] private SfxEntry[] sfx;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private readonly Dictionary<Sfx, SfxEntry> map = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Музыкальный источник — зацикленный.
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;

        // Источник под one-shot эффекты. PlayOneShot не зависит от Time.timeScale,
        // поэтому звуки корректно играют и во время паузы/опроса (timeScale = 0).
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        foreach (var e in sfx)
            map[e.id] = e;   // последняя запись побеждает, если случайно задублировал id
    }

    public void PlaySfx(Sfx id)
    {
        if (!map.TryGetValue(id, out var e) || e.clip == null) return;
        sfxSource.PlayOneShot(e.clip, e.volume <= 0f ? 1f : e.volume);
    }

    public void PlayMusic()
    {
        if (musicClip == null) return;
        if (!musicSource.isPlaying) musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // --- Удобные обёртки для привязки к Button.onClick в инспекторе ---
    // (UnityEvent не умеет передавать enum, поэтому даём готовые методы без аргументов)
    public void PlayUIClick() => PlaySfx(Sfx.UIClick);
}
