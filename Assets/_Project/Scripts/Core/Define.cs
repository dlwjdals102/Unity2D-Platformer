using UnityEngine;

/// <summary>
/// 프로젝트 전역에서 사용되는 Enum, 상수, 태그, 레이어를 정의합니다.
/// 매직 넘버와 하드코딩 문자열을 방지합니다.
/// </summary>
public static class Define
{
    /// <summary>
    /// 프로젝트의 씬 이름을 한곳에서 관리합니다.
    /// 빌드 세팅의 씬 이름과 정확히 일치해야 합니다.
    /// </summary>
    public static class SceneNames
    {
        public const string Title = "Title";
        public const string Test = "Test";        // 새 게임 시작 시 진입할 첫 스테이지
        public const string Stage_1 = "Stage_1";  // 이어하기 데이터가 손상되었을 때의 폴백 씬
        public const string Stage_2 = "Stage_2";  // 이어하기 데이터가 손상되었을 때의 폴백 씬
    }

    /// <summary>
    /// 프로젝트에서 사용하는 레이어 이름을 한곳에서 관리합니다.
    /// 런타임에는 LayerMask.NameToLayer()를 통해 인덱스로 변환하여 사용합니다.
    /// </summary>
    public static class LayerNames
    {
        public const string Ground = "Ground";
        public const string IgnoreRaycast = "Ignore Raycast"; // Unity 내장 레이어
    }

    /// <summary>
    /// 프로젝트에서 사용하는 모든 Unity Tag 문자열을 한곳에서 관리합니다.
    /// 매직 스트링을 상수로 묶어 오타 방지 및 유지보수성을 향상시킵니다.
    /// </summary>
    public static class GameTags
    {
        public const string Player = "Player";
        // 추후 태그 추가 시 여기에 정의
        // public const string Enemy = "Enemy";
        // public const string Boss = "Boss";
    }

    /// <summary>
    /// Animator의 파라미터들을 해시로 미리 캐싱하여 매 호출 시 문자열 변환을 방지합니다.
    /// Animator.StringToHash는 내부적으로 문자열 해싱을 수행하므로,
    /// 매 프레임 호출되는 코드에서는 이 정적 클래스를 통해 캐싱된 해시 값을 사용합니다.
    ///
    /// 사용 예: anim.SetInteger(AnimatorParameters.ComboCounter, 1);
    /// </summary>
    public static class AnimatorParameters
    {
        // ==========================================
        // Player
        // ==========================================
        public static readonly int ComboCounter = Animator.StringToHash("ComboCounter");

        // ==========================================
        // 공통 (필요 시 추가)
        // ==========================================
        public static readonly int XVelocity = Animator.StringToHash("xVelocity");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    }

    /// <summary>
    /// AudioMixer Exposed Parameter 이름들을 모아둔 상수 클래스.
    /// </summary>
    public static class MixerParameters
    {
        public const string BGMVolume = "BGM_Volume";
        public const string SFXVolume = "SFX_Volume";
    }

    /// <summary>
    /// PlayerPrefs 키를 모아둔 상수 클래스 (오타 방지).
    /// </summary>
    public static class PrefsKeys
    {
        public const string SavedBGMVolume = "Saved_BGM_Volume";
        public const string SavedSFXVolume = "Saved_SFX_Volume";
    }

}
