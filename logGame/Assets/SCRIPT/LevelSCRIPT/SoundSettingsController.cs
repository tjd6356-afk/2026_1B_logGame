using UnityEngine;
using UnityEngine.UI;          // UI 컴포넌트 제어를 위해 필수
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

public class SoundSettingsController : MonoBehaviour
{
    [Header("─ UI 버튼 및 슬라이더 연결")]
    public Button titleButton;
    public Button audioCheckButton;
    public Button fxAudioCheckButton;
    public Slider audioSlider;
    public Slider fxAudioSlider;

    [Header("─ 오디오 소스 (음원 출력 장치)")]
    public AudioSource bgmSource;  // 배경음(Audio)용 소스
    public AudioSource sfxSource;  // 효과음(FxAudio)용 소스

    [Header("─ 랜덤 테스트 음원 목록 (여러 개 등록 가능)")]
    public AudioClip[] bgmClips;   // AudioCheck용 배경음 음원들
    public AudioClip[] sfxClips;   // FxAudioCheck용 효과음 음원들

    void Start()
    {
        // 기존에 저장된 볼륨 불러오기 (저장된 값이 없으면 기본값 0.5f)
        float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // UI 슬라이더 값 초기화
        if (audioSlider != null) audioSlider.value = savedBGMVolume;
        if (fxAudioSlider != null) fxAudioSlider.value = savedSFXVolume;

        // 실제 오디오 소스 볼륨 적용
        if (bgmSource != null) bgmSource.volume = savedBGMVolume;
        if (sfxSource != null) sfxSource.volume = savedSFXVolume;

        // UI 이벤트 리스너 코드 연결 (인펙터에서 일일이 드래그 안 해도 되게 자동화)
        if (titleButton != null) titleButton.onClick.AddListener(GoToTitleScene);
        if (audioCheckButton != null) audioCheckButton.onClick.AddListener(PlayRandomBGM);
        if (fxAudioCheckButton != null) fxAudioCheckButton.onClick.AddListener(PlayRandomSFX);

        if (audioSlider != null) audioSlider.onValueChanged.AddListener(SetBGMVolume);
        if (fxAudioSlider != null) fxAudioSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // ────────────────────────────────────────────────────────
    // 타이틀 씬으로 이동 기능
    // ────────────────────────────────────────────────────────
    void GoToTitleScene()
    {
        Debug.Log("Title 씬으로 이동합니다.");
        SceneManager.LoadScene("Title");
    }

    // ────────────────────────────────────────────────────────
    // 랜덤 음원 재생 기능 (슬라이더 크기 반영)
    // ────────────────────────────────────────────────────────
    void PlayRandomBGM()
    {
        if (bgmSource == null || bgmClips == null || bgmClips.Length == 0)
        {
            Debug.LogWarning("BGM 소스 또는 음원이 등록되지 않았습니다.");
            return;
        }

        // 배열에서 랜덤하게 음원 하나 선택
        int randomIndex = Random.Range(0, bgmClips.Length);
        AudioClip selectedClip = bgmClips[randomIndex];

        // 현재 슬라이더 볼륨으로 재생
        bgmSource.clip = selectedClip;
        bgmSource.Play();
        Debug.Log($"랜덤 BGM 재생: {selectedClip.name} (볼륨: {bgmSource.volume})");
    }

    void PlayRandomSFX()
    {
        if (sfxSource == null || sfxClips == null || sfxClips.Length == 0)
        {
            Debug.LogWarning("SFX 소스 또는 음원이 등록되지 않았습니다.");
            return;
        }

        // 배열에서 랜덤하게 음원 하나 선택
        int randomIndex = Random.Range(0, sfxClips.Length);
        AudioClip selectedClip = sfxClips[randomIndex];

        // 효과음은 겹쳐서 날 수 있도록 PlayOneShot으로 현재 슬라이더 볼륨에 맞춰 재생
        sfxSource.PlayOneShot(selectedClip, fxAudioSlider.value);
        Debug.Log($"랜덤 SFX 재생: {selectedClip.name} (볼륨: {fxAudioSlider.value})");
    }

    // ────────────────────────────────────────────────────────
    // 슬라이더 볼륨 조절 및 데이터 실시간 저장
    // ────────────────────────────────────────────────────────
    void SetBGMVolume(float value)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = value;
            PlayerPrefs.SetFloat("BGMVolume", value); // 볼륨 값 임시 저장
        }
    }

    void SetSFXVolume(float value)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = value;
            PlayerPrefs.SetFloat("SFXVolume", value); // 볼륨 값 임시 저장
        }
    }

    private void OnDestroy()
    {
        // 씬이 바뀌거나 게임이 꺼질 때 볼륨 세팅을 디스크에 최종 저장
        PlayerPrefs.Save();
    }
}