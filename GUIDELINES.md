# 돌 굴러와유 — 개발 가이드라인

> 이 문서는 **앞으로 코드/씬/에셋을 만들 때 반복적으로 따라야 할 규칙**을 모은 곳입니다.
> 진행률·할 일 큐는 [PROGRESS.md](PROGRESS.md), 게임 디자인 원본은 `돌 굴러와유_기획문서_v2 (2).docx` 를 참고하세요.

---

## 1. 폴더 구조 컨벤션

현재 `Assets/` 루트에 `.cs` 파일이 흩어져 있는데, 코드가 늘기 전에 다음 구조로 정리하기를 권장합니다.

```
Assets/
├── Scenes/
│   ├── Title.unity          ← 시작 화면
│   ├── Game.unity           ← 본 게임
│   └── Clear.unity          ← 클리어 화면
├── Scripts/
│   ├── Player/              ← PlayerMove, PlayerHit, PlayerDeath
│   ├── Rock/                ← RockSpawner, RockDestroyer, Rock 변종들
│   ├── Stage/               ← Checkpoint, StageManager, MathGate
│   ├── UI/                  ← IQDisplay, GameOverPanel, DangerOverlay
│   ├── System/              ← GameManager, SceneLoader, AudioManager
│   └── ScriptableObjects/   ← RockConfig, StageConfig 등 데이터
├── Prefabs/
│   ├── Player/
│   ├── Rocks/               ← Rock_Normal, Rock_Bouncy, Rock_Giant, …
│   ├── Stage/               ← Checkpoint, MathGate, KillZone
│   └── UI/
├── Materials/
├── Audio/
│   ├── BGM/
│   └── SFX/
├── Animations/
└── Settings/                ← (이미 존재) URP 에셋
```

**규칙**
- 스크립트는 *기능 그룹* 폴더로 분류. 1파일/1클래스 유지.
- 프리팹은 *접두어*로 분류: `Rock_*`, `UI_*`, `Stage_*`.
- 새 폴더에 첫 파일을 넣을 때 Unity가 `.meta`를 자동 생성하니, **이동은 반드시 Unity 에디터 안에서** 수행 (`.meta` GUID 보존).

---

## 2. 네이밍 컨벤션

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 / 파일 | PascalCase | `RockSpawner`, `MathGate` |
| public 필드 / 프로퍼티 | PascalCase | `public float Speed` |
| private 필드 | camelCase, 접두어 없음 | `Rigidbody rb`, `float knockbackTimer` |
| 상수 | UPPER_SNAKE | `const float MAX_IQ` |
| 씬 | PascalCase | `Title`, `Game`, `Clear` |
| Prefab | PascalCase + 카테고리 접두어 | `Rock_Bouncy`, `UI_GameOver` |
| 태그 | PascalCase 단수 | `Rock`, `Player`, `KillZone` |
| 레이어 | PascalCase | `Player`, `Rock`, `Stage` |

**한글 주석은 허용**하되, 식별자(클래스·메서드·변수)는 영문 유지.

---

## 3. 코드 스타일

### 3.1 항상 지켜야 할 것

- **`Invoke`/`StartCoroutine`은 `nameof` 사용**: 문자열 하드코딩 금지.
  ```csharp
  Invoke(nameof(Restart), 1f);       // ✅
  Invoke("Restart", 1f);             // ❌ 리네임 시 침묵 실패
  ```
- **`GetComponent`는 `Awake`에서 캐싱**: `Update`/`FixedUpdate`에서 매번 호출 금지.
- **`CompareTag` 사용**: `gameObject.tag == "Rock"` 대신 `gameObject.CompareTag("Rock")` (GC 없음).
- **물리 코드는 `FixedUpdate`**: `Rigidbody.MovePosition`, `AddForce` 등.
- **인스펙터 필드는 `[SerializeField] private`**: `public` 남용 금지.
  ```csharp
  [SerializeField] private float speed = 5f;   // ✅
  public float speed = 5f;                     // ❌ 외부에서 마음대로 바꿈
  ```
- **null 가드는 boundary에서만**: 인스펙터로 받은 참조는 시작 시 한 번만 검사하고, 이후 호출부에서 매번 검사하지 않음.

### 3.2 입력 시스템

기획서가 *New Input System* 채택을 명시했으므로, 신규 입력은 `InputActionAsset` 경유로 작성합니다.

- 임시·프로토타이핑 단계에서는 `Keyboard.current` 직접 폴링 허용 (현재 `PlayerMove`가 그 상태).
- 정식 진입 시 `PlayerInput` 컴포넌트 + `Actions` 에셋(`InputActions/Player.inputactions`)로 전환.
- 키 리바인딩 / 게임패드 지원 / 슬로우 모션 시 입력 분리(Ch08의 `Time.unscaledDeltaTime`) 를 위해서 필수.

### 3.3 매직 넘버 금지

```csharp
if (transform.position.y <= 0f) Destroy(gameObject);   // ❌ 0f가 뭔지 모름

[SerializeField] private float killY = -5f;
if (transform.position.y <= killY) Destroy(gameObject); // ✅
```

장기적으로는 **KillZone Trigger Collider** 한 개로 통합해 `OnTriggerEnter` 처리.

---

## 4. 씬 구성 원칙

- **씬 분리**: `Title` → `Game` → `Clear` 3개. 씬 전환은 `SceneLoader`에 중앙화.
- **씬 간 데이터 전달**: 최종 IQ·플레이 시간 등은 `GameManager`(DontDestroyOnLoad) 또는 `ScriptableObject` 사용. `PlayerPrefs`는 *최고점*에만 사용.
- **하나의 씬에는 하나의 책임**. `SampleScene`은 Phase 1 마무리 시점에 삭제.

---

## 5. Prefab/물리 표준값

| 항목 | 값 | 비고 |
|------|---|------|
| Player Rigidbody | mass 1, drag 0, freezeRotation = true | 회전은 `CameraFollow` 회전과 분리 |
| Rock Rigidbody | mass 2, drag 0.1, angular drag 0.2 | 굴러가는 느낌 |
| Player Collider | Capsule | 좌우 폭 좁게 |
| Rock Collider | Sphere | 굴러감 보장 |
| 경사로 PhysicMaterial (기본) | friction 0.5, bounce 0 | |
| 얼음 구간 PhysicMaterial | friction 0.05, bounce 0 | Phase 5 |
| 바위끼리 PhysicMaterial | friction 0.3, bounce 0.4 | 기획서 4-3 (자연 밀어내기) |

**Layer/Collision Matrix**: `Player` ↔ `Rock` ↔ `Stage` 만 충돌하도록 설정. UI/이펙트 레이어는 분리.

---

## 6. 새 시스템을 만들 때의 순서

새 기능을 추가할 때 항상 이 순서를 따릅니다.

1. **기획서에서 해당 절을 다시 읽는다** (Phase 번호로 식별).
2. **PROGRESS.md의 해당 항목을 in-progress로 표시**.
3. **데이터부터**: 수치/구간/문제 등은 `ScriptableObject`로 분리.
4. **프리팹 — 컴포넌트 — 매니저 순서**로 구현.
5. **단일 씬에서 동작 확인** → 다른 씬에 영향 없는지 확인.
6. **PROGRESS.md 갱신** + 마이크로 커밋.

---

## 7. 커밋 / 브랜치 규칙

- main 브랜치는 **빌드 가능한 상태**만 머지.
- 작업은 `feat/<phase>-<feature>` 브랜치에서: 예) `feat/p2-iq-ui`, `feat/p3-math-gate`.
- 커밋 메시지 형식:
  ```
  feat(p2): IQ 점수 UI 추가
  fix(player): nameof로 Invoke 리네임 안전화
  refactor(rock): 스폰 로직을 Y 진행 기반으로 변경
  ```
- **메타 파일(`.meta`)은 같은 커밋에 함께 포함**. Unity가 GUID로 참조하므로 누락 시 다른 PC에서 깨짐.

---

## 8. 기획서와 코드의 동기화

기획서가 v2이므로, 향후 v3·v4가 나오면:

1. `PROGRESS.md` 상단의 "기준 문서" 줄을 갱신.
2. 변경된 절만 diff로 정리해 PR 설명에 첨부.
3. 영향 받는 Phase 체크리스트만 재작성 (전체 재작성 금지 — 진행 이력 보존).

---

## 9. 작업할 때 헷갈리면 이 순서로 확인

1. **기획서에 답이 있나?** → `_plan.txt` (자동 추출본) 또는 원본 docx 확인.
2. **이미 비슷한 코드가 있나?** → `Assets/Scripts/` grep.
3. **레퍼런스 게임에선 어떻게?** → The Game of Sisyphus / Getting Over It / Jump King 영상 참고.
4. **여전히 막히면** → PROGRESS.md §6 "미정 결정 사항"에 추가하고 기획자에게 질문.
