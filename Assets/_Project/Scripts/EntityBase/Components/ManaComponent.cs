using System;
using UnityEngine;

public class ManaComponent : MonoBehaviour
{
    // 마나량이 바뀔 때마다 UI에 신호를 보낼 확성기(Event)
    public event Action<float, float> OnManaChanged;

    public float MaxMana { get; private set; }
    public float CurrentMana { get; private set; }

    private float regenRate;

    // 게임 시작 시 PlayerController가 데이터(PlayerData)를 넣어주며 초기화할 함수
    public void Initialize(float maxMana, float regenRate)
    {
        this.MaxMana = maxMana;
        this.CurrentMana = maxMana; // 시작할 때는 마나를 꽉 채워줍니다.
        this.regenRate = regenRate;

        NotifyManaChanged();
    }

    private void Update()
    {
        // 마나가 꽉 차지 않았다면, 시간에 따라 부드럽게 자동 회복시킵니다.
        if (CurrentMana < MaxMana)
        {
            CurrentMana += regenRate * Time.deltaTime;
            CurrentMana = Mathf.Clamp(CurrentMana, 0, MaxMana); // 최대치 초과 방지

            NotifyManaChanged();
        }
    }

    // 스킬(대시)을 사용할 때 마나를 깎는 함수
    // 마나가 충분해서 깎았다면 true, 부족하면 false를 반환합니다.
    public bool UseMana(float amount)
    {
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
            NotifyManaChanged();
            return true;
        }
        return false;
    }

    // 마나 물약 등을 먹었을 때 강제로 채워주는 함수
    public void RestoreMana(float amount)
    {
        CurrentMana += amount;
        CurrentMana = Mathf.Clamp(CurrentMana, 0, MaxMana);
        NotifyManaChanged();
    }

    public void LoadSavedMana(float savedMana)
    {
        // 전역 창고에서 가져온 마나 수치를 적용합니다.
        CurrentMana = savedMana;
        NotifyManaChanged(); // UI 업데이트 방송
    }

    // UI 동기화를 위한 이벤트 호출 헬퍼
    private void NotifyManaChanged()
    {
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }
}
