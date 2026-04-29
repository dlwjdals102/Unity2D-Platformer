using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;             // 소리 호출용 이름 (예: "Hit", "Swing")
    public AudioClip clip;          // 실제 오디오 파일

    // 추가 1: 이 소리가 BGM인지 SFX인지 구분해 줄 배관(Mixer Group)
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    public float volume = 1f;       // 기본 볼륨

    [Range(0.1f, 3f)]
    public float pitch = 1f;        // 기본 피치 (음높이)

    // 추가 2: 다중 타격 굉음 방지용 쿨타임 세팅
    [Tooltip("이 시간 안에는 똑같은 소리가 겹쳐서 나지 않습니다.")]
    public float cooldown = 0.05f;
    
    [HideInInspector] public float lastPlayTime; // 마지막으로 재생된 시간 추적
    [HideInInspector] public AudioSource source; // AudioManager가 런타임에 자동으로 붙여줄 재생기
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound List")]
    public Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary;

    // BGM 전용 턴테이블 2대와 믹서 그룹
    [Header("BGM Settings")]
    public AudioMixerGroup bgmMixerGroup;
    private AudioSource bgmSourceA;
    private AudioSource bgmSourceB;
    private bool isPlayingA = true; // 현재 A가 재생 중인지 추적

    // 현재 재생 중인 BGM 이름 추적 (같은 BGM 중복 재생 방지용)
    private string currentBGMName = "";

    [Header("Main Settings")]
    public AudioMixer mainMixer;

    private void Awake()
    {
        // 표준 싱글톤 패턴 (DontDestroyOnLoad는 부모 CoreManager가 처리)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 게임이 시작될 때, 리스트에 등록된 모든 소리에 대해 AudioSource를 생성하고 세팅합니다.
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            // 핵심 추가: 생성된 AudioSource에 믹서 파이프라인 연결!
            s.source.outputAudioMixerGroup = s.mixerGroup;
        }

        soundDictionary = new Dictionary<string, Sound>(sounds.Length);
        foreach (Sound s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.name))
                soundDictionary.Add(s.name, s);
            else
                Debug.LogWarning($"[AudioManager] 이미 존재합니다.: {s.name}");
        }

        // 게임 시작 시 BGM 전용 플레이어 2대를 몸체에 부착하고 기본 세팅
        bgmSourceA = gameObject.AddComponent<AudioSource>();
        bgmSourceB = gameObject.AddComponent<AudioSource>();

        bgmSourceA.loop = true; // BGM은 무조건 반복 재생
        bgmSourceB.loop = true;

        bgmSourceA.outputAudioMixerGroup = bgmMixerGroup;
        bgmSourceB.outputAudioMixerGroup = bgmMixerGroup;
    }

    private void Start()
    {
        // 씬 시작 시 저장된 볼륨 불러오기
        LoadVolumeSettings();
    }

    // 슬라이더(0.0001 ~ 1) 값을 받아 믹서에 적용 (dB 변환 공식 사용)
    public void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(Define.MixerParameters.BGMVolume, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(Define.PrefsKeys.SavedBGMVolume, volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(Define.MixerParameters.SFXVolume, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(Define.PrefsKeys.SavedSFXVolume, volume);
    }

    private void LoadVolumeSettings()
    {
        float bgm = PlayerPrefs.GetFloat(Define.PrefsKeys.SavedBGMVolume, 1f);
        float sfx = PlayerPrefs.GetFloat(Define.PrefsKeys.SavedSFXVolume, 1f);

        SetBGMVolume(bgm);
        SetSFXVolume(sfx);
    }

    /// <summary>
    /// 어디서든 AudioManager.Instance.Play("소리이름"); 으로 호출하면 됩니다!
    /// </summary>
    public void Play(string name)
    {
        // 이름이 일치하는 사운드를 찾습니다.
        /* Sound s = System.Array.Find(sounds, sound => sound.name == name);

         if (s == null)
         {
             Debug.LogWarning("사운드를 찾을 수 없습니다: " + name);
             return;
         }*/

        // 이름이 일치하는 사운드를 찾습니다.
        if (!soundDictionary.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("사운드를 찾을 수 없습니다: " + name);
            return;
        }

        // 다중 타격 버그 방어: 마지막으로 재생된 지 cooldown(예: 0.05초)이 안 지났으면 재생 무시!
        if (Time.time < s.lastPlayTime + s.cooldown)
            return;

        // 재생이 허락되었다면 현재 시간을 기록
        s.lastPlayTime = Time.time;

        // 똑같은 칼질 소리라도 매번 피치(음높이)를 미세하게 다르게 주면 훨씬 자연스럽고 찰집니다! (선택 사항)
        s.source.pitch = s.pitch * Random.Range(0.9f, 1.1f);

        s.source.PlayOneShot(s.clip);
    }

    // ==========================================
    // BGM 크로스페이드 재생 함수
    // ==========================================
    public void PlayBGM(string name, float fadeDuration = 1.5f)
    {
        // 0. 같은 BGM이 이미 재생 중이면 무시 (씬 전환 시 끊김 방지)
        if (currentBGMName == name) return;

        /*// 1. 재생할 BGM 데이터를 찾습니다.
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;*/

        // 1. 재생할 BGM 데이터를 찾습니다.
        if (!soundDictionary.TryGetValue(name, out Sound s)) return;

        // 2. 현재 재생 중인 소스와 새롭게 재생할 소스를 결정합니다.
        AudioSource activeSource = isPlayingA ? bgmSourceA : bgmSourceB;
        AudioSource newSource = isPlayingA ? bgmSourceB : bgmSourceA;

        // 3. 새 소스에 클립을 끼우고 재생을 시작합니다. (볼륨은 0부터 시작)
        newSource.clip = s.clip;
        newSource.volume = 0f;
        newSource.Play();

        // 4. 서서히 볼륨을 교차시키는 코루틴을 실행합니다.
        StartCoroutine(Crossfade(activeSource, newSource, s.volume, fadeDuration));

        // 5. 턴테이블 교체
        isPlayingA = !isPlayingA;
        currentBGMName = name;
    }

    // 서서히 볼륨을 조절하는 마법의 코루틴
    private IEnumerator Crossfade(AudioSource activeSource, AudioSource newSource, float targetVolume, float duration)
    {
        float time = 0;
        float startVolume = activeSource.volume; // 기존 음악의 현재 볼륨

        while (time < duration)
        {
            time += Time.deltaTime;

            // 기존 음악은 서서히 0으로
            activeSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);

            // 새 음악은 서서히 목표 볼륨으로
            newSource.volume = Mathf.Lerp(0f, targetVolume, time / duration);

            yield return null; // 1프레임 대기
        }

        // 교체가 완전히 끝나면 찌꺼기 방지를 위해 완벽하게 값을 고정하고 기존 음악 정지
        activeSource.volume = 0f;
        activeSource.Stop();
        newSource.volume = targetVolume;
    }
}
