# 🗡️ Unity 2D Action Platformer

> 할로우 나이트(Hollow Knight) 스타일의 2D 액션 플랫포머.  
> 디자인 패턴과 컴포넌트 기반 아키텍처를 적용해 확장 가능한 구조로 설계했습니다.

<p align="center">
  <img width="1280" height="720" alt="Image" src="https://github.com/user-attachments/assets/7c53170e-3a62-4cf0-8385-d3c90f355afd" />&nbsp;&nbsp;
  <img width="1280" height="720" alt="Image" src="https://github.com/user-attachments/assets/ffe46119-8fd2-408f-a36c-0ce8bb19d269" />
</p>

<p align="center">
  📝 <a href="https://cookie-lock-dbc.notion.site/Unity-2D-34e87bfb3960805ea2b6e504c4e6c4de?source=copy_link"><b>상세 포트폴리오 (Notion)</b></a> &nbsp;·&nbsp;
  🎬 <a href="https://youtu.be/fAOMKv_2ZQY">플레이 영상</a> &nbsp;·&nbsp;
  <!-- 🎮 <a href="">플레이하기 (WebGL)</a> -->
</p>

---

## 📋 프로젝트 정보

| 항목 | 내용 |
|------|------|
| **개발 기간** | 2026.03 ~ 2026.04 (약 1개월) |
| **개발 인원** | 1인 (개인 프로젝트) |
| **장르** | 2D 액션 플랫포머 |
| **엔진 / 언어** | Unity 6 / C# |

---

## ✨ 주요 구현

### 🧠 아키텍처 & 디자인 패턴
- **컴포넌트 기반 + 상속 + 인터페이스** — `Entity` 추상 베이스에 `Health`/`Movement`/`Combat` 컴포넌트 분리, `[RequireComponent]`로 의존성 강제, `IDamageable` 인터페이스로 데미지 시스템 디커플링
- **제네릭 FSM** — Player·Enemy·Boss가 하나의 `StateMachine<T>`를 공유, null 가드와 동일 상태 재진입 방지 포함
- **Strategy Pattern** — `EnemyData`(SO)가 자신의 공격 상태를 직접 생성, 새 적 타입 추가 시 `Enemy.cs` 수정 불필요 (OCP 준수)
- **Observer Pattern** — `GameManager`/`HealthComponent` 등의 이벤트 기반 통신으로 매니저-게임객체 간 디커플링, `OnEnable`/`OnDisable` 짝 매칭으로 메모리 누수 방지

### 🎮 Game Feel (할로우 나이트 조작감 분석 및 재현)
- **플레이어** — 3단 콤보 공격, 가변 점프, 대시(무적+마나 소모), **점프 버퍼링**, **코요테 타임**, i-Frame
- **타격감** — Cinemachine Impulse 기반 카메라 셰이크, `Time.timeScale` 조절로 Hit Stop, 정확한 타격 지점에 VFX 스폰

### 👹 적 AI & 보스전
- **적 AI 2종** — 근접/원거리, Raycast 기반 시야·사거리·절벽·도주 거리 감지, `OnDrawGizmos`로 디버깅 시각화
- **다단계 페이즈 보스** — 체력 비율 기반 페이즈 전환 (`PhaseComponent`), `EntityData`(SO) 통째로 교체로 패턴/스탯 일괄 변경
- **가중치 기반 패턴 선택** — 보스 공격을 확률 분포에서 샘플링
- **보스룸 흐름** — 입장→문 잠금→거리 진입→각성(포효+카메라 줌아웃)→처치→영구 저장→문 개방

### 💾 데이터 & 시스템
- **JSON 영구 저장** — `Application.persistentDataPath` 사용 (플랫폼 독립), 손상 데이터 검증, 보스 처치 영구 기록
- **씬 전환 데이터 인계** — Export/Import 패턴으로 PlayerController가 자기 상태를 직접 직렬화
- **체크포인트 / 포탈** — 우선순위 기반 부활 위치 복원
- **Object Pooling** — Queue 순환(Dequeue→Enqueue) 구조로 반납 처리 없이 무한 재활용

> 📖 **설계 의도, 코드 스니펫, 트러블슈팅 등은 [노션 포트폴리오](https://cookie-lock-dbc.notion.site/Unity-2D-34e87bfb3960805ea2b6e504c4e6c4de?source=copy_link)에 자세히 정리했습니다.**

---

## 📁 폴더 구조

```
Assets/Scripts/
├── Core/              # GameManager, DataManager, AudioManager, FeedbackManager 등 매니저
├── EntityBase/        # 모든 캐릭터의 베이스
│   ├── Entity.cs        # 추상 클래스
│   ├── Components/      # Health, Movement, Combat, Mana, Phase
│   └── FSM/             # StateMachine<T>, State<T>
├── Player/States/     # Idle, Move, Jump, Fall, Dash, Attack, Hurt, Dead
├── Enemies/           # Enemy, EnemyData (Strategy)
│   └── States/          # Patrol, Chase, Attack, Hurt, Dead, Retreat
├── Boss/States/       # Sleep, Intro, Idle, Chase, Melee, Shockwave, Dead
├── Combat/ Environment/ Items/ UI/ VFX/ Camera/
```

## 📮 Contact

- **Email** · dlwjdals102@naver.com
- **Notion** · [노션 포트폴리오](https://cookie-lock-dbc.notion.site/Unity-2D-34e87bfb3960805ea2b6e504c4e6c4de?source=copy_link)
