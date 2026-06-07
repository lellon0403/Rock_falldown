# 돌 굴러와유 — 개발 진행 현황

> 기준 문서: `돌 굴러와유_기획문서_v2 (2).docx`
> 최종 갱신: 2026-06-07
> 엔진: Unity 6 (6000.x) · URP 17.0.4 · New Input System 1.19.0

---

## 1. 한 줄 요약

기획서의 **Phase 1(기본 구조)** 가 거의 끝났고, **Phase 2(바위 시스템)** 의 초기 단계에 진입한 상태입니다. 단일 씬(`SampleScene`)에서 캐릭터 이동·카메라 추적·기본 바위 스폰·충돌 넉백·낙사 리스폰까지 동작하지만, 경사로/구간/체크포인트/수학 게이트/IQ 시스템 등 기획서 본문의 핵심 시스템은 아직 미착수입니다.

---

## 2. 구현 완료 항목

| # | 기획 항목 | 구현 파일 | 메모 |
|---|----------|----------|------|
| 1 | 캐릭터 좌우/전후 이동 | [PlayerMove.cs](Assets/PlayerMove.cs) | `Keyboard.current` 직접 폴링 방식 (Player Input 컴포넌트 아님) |
| 2 | 넉백 입력 잠금 | [PlayerMove.cs:19-29](Assets/PlayerMove.cs:19) | `knockbackDuration` 동안 입력 차단 |
| 3 | 바위 충돌 → 물리 밀려남 | [PlayerHit.cs](Assets/PlayerHit.cs) | `Rock` 태그 감지 → `AddForce(Impulse)` |
| 4 | 백뷰 카메라 추적 | [CameraFollow.cs](Assets/CameraFollow.cs) | 고정 offset + 고정 회전 |
| 5 | 바위 스폰 (X 3구역 랜덤) | [RockSpawner.cs](Assets/RockSpawner.cs) | `InvokeRepeating` 1.5s 간격 |
| 6 | 바위 자동 제거 (낙하 후) | [RockDestroyer.cs](Assets/Prefabs/RockDestroyer.cs) | y ≤ 0 진입 시 `Destroy` |
| 7 | 추락 리스폰 | [PlayerDeath.cs](Assets/PlayerDeath.cs) | y ≤ 0 → 현재 씬 재로딩 |
| 8 | Rock 프리팹 | [Rock.prefab](Assets/Prefabs/Rock.prefab) | 일반 바위 1종 |
| 9 | URP 파이프라인 셋업 | `Assets/Settings/` | PC/Mobile RP 에셋 분리 완료 |

---

## 3. 기획서 항목별 진행률

진행률 표기: ⬛⬛⬛⬛⬛ 0% / ⬛⬛⬛⬛⬜ 20% / … / 🟩🟩🟩🟩🟩 100%

### Phase 1 — 기본 구조 및 맵 세팅
| 항목 | 진행률 | 상태 |
|------|--------|------|
| 3D 프로젝트 생성 (URP) | 🟩🟩🟩🟩🟩 | 완료 |
| Scene 구성 (시작/게임/클리어) | ⬛⬛⬛⬛⬛ | **미착수** — `SampleScene` 1개뿐 |
| 경사로 맵 (5구간) | ⬛⬛⬛⬛⬛ | **미착수** — 현재 평면 추정 |
| 좌우/전진 이동 | 🟩🟩🟩🟩🟩 | 완료 |
| 백뷰 카메라 추적 | 🟩🟩🟩🟩⬜ | 완료, 경사 추종 시 보정 필요 가능 |

### Phase 2 — 바위 시스템 및 충돌
| 항목 | 진행률 | 상태 |
|------|--------|------|
| 일반 바위 Prefab | 🟩🟩🟩🟩🟩 | 완료 |
| 바위 스폰 로직 | 🟩🟩🟩⬜⬜ | 시간 기반만 구현, 구간 기반 미구현 |
| 충돌 판정 + 넉백 | 🟩🟩🟩🟩🟩 | 완료 |
| 바위 처리 시스템 (3단계 안전장치) | 🟩⬜⬜⬜⬜ | 3차(타임아웃)만 부분 — y≤0 기반, 시간 기반 X |
| IQ 점수 시스템 + UI | 🟩🟩🟩🟩⬜ | [IQManager](Assets/Scripts/System/IQManager.cs) + 좌상단 [IQDisplay](Assets/Scripts/UI/IQDisplay.cs). 정답 시 +10 |
| 게임오버/다시하기 UI | 🟩⬜⬜⬜⬜ | 추락 시 자동 재로딩만 있음, UI 없음 |
| 구역 기반 스포너 제어 | 🟩🟩🟩🟩⬜ | [StageManager](Assets/Scripts/System/StageManager.cs) — 현재 스테이지 스포너만 작동(내려오면 재작동) |

### Phase 3 — 수학 퍼즐 게이트
| 항목 | 진행률 | 상태 |
|------|--------|------|
| 수학 문제 벽 (벽+2문+텍스트) | 🟩🟩🟩🟩⬜ | 에디터 빌더로 생성 가능 ([MathGateBuilder.cs](Assets/Scripts/Editor/MathGateBuilder.cs)) |
| 정답/오답 판정 | 🟩🟩🟩🟩⬜ | [MathGate.cs](Assets/Scripts/Stage/MathGate.cs) + [GateDoor.cs](Assets/Scripts/Stage/GateDoor.cs) |
| 오답 미끄러짐 | 🟩🟩🟩🟩⬜ | 벽 충돌 + 경사 미끄러짐 (자동) |
| 문제 데이터 (ScriptableObject) | 🟩🟩🟩⬜⬜ | [MathQuestion.cs](Assets/Scripts/ScriptableObjects/MathQuestion.cs) + 샘플 1개 |
| 구간별 문제 배열 | ⬛⬛⬛⬛⬛ | **미착수** — 현재 단일 문제 |
| IQ 보너스 연동 | 🟩🟩🟩🟩🟩 | 정답 시 IQManager.Add(+10) 연결 완료 |

### Phase 4 — 연출 및 캐릭터 진화
| 항목 | 진행률 | 상태 |
|------|--------|------|
| 캐릭터 진화 (원시인→천재) | 🟩🟩🟩🟩⬜ | **3D 유지 + 좌상단 HUD 초상** ([PortraitDisplay](Assets/Scripts/UI/PortraitDisplay.cs)). 4단계(IQ 60/80/100/120), 단계당 2프레임. 초상 8장 GPT 생성 완료(`Assets/Art/Portraits/`), [임포터](Assets/Scripts/Editor/PortraitImporter.cs)로 연결 |
| 두뇌 풀가동 슬로우 모션 | ⬛⬛⬛⬛⬛ | **미착수** |
| 위험 알림 UI 오버레이 | ⬛⬛⬛⬛⬛ | **미착수** |
| 바위 종류 다양화 (5종) | 🟩🟩⬜⬜⬜ | 2/5 — 일반 + [분열 바위](Assets/Scripts/Rock/SplittingRock.cs)(장애물 충돌 시 조각화). 밀치기 [바위별 설정](Assets/Scripts/Rock/Rock.cs) |
| 캐릭터 표정 Animator | ⬛⬛⬛⬛⬛ | **미착수** |

### Phase 5 — 폴리싱
| 항목 | 진행률 | 상태 |
|------|--------|------|
| 고정 장애물 (돌벽 등) | 🟩🟩🟩⬜⬜ | [ObstacleSpawner](Assets/Scripts/Stage/ObstacleSpawner.cs) — 표면 위 랜덤 배치 (매 판 다름) |
| 미끄러운 구간 (PhysicMaterial) | ⬛⬛⬛⬛⬛ | **미착수** |
| 효과음/BGM | ⬛⬛⬛⬛⬛ | **미착수** |
| 구간별 Material 테마 | 🟩🟩🟩⬜⬜ | Stage1 잔디 / Stage2 얼음 **타일 텍스처**(GPT 생성, `Assets/Art/Ground/`) — 올라갈 때 움직임 느낌. [GroundImporter](Assets/Scripts/Editor/GroundImporter.cs)로 적용 |
| 밸런싱/난이도 곡선 | ⬛⬛⬛⬛⬛ | **미착수** |
| PlayerPrefs 최고점 | ⬛⬛⬛⬛⬛ | **미착수** |

---

## 4. 코드 리뷰 — 지금 손볼 만한 것

기획 진행과 별개로, 다음 단계로 가기 전 정리해두면 좋은 부분입니다.

1. **`PlayerMove`의 입력 방식**: 기획서는 *Player Input 컴포넌트(Ch06)* 사용을 명시하지만 현재는 `Keyboard.current` 직접 폴링입니다. Phase 2 끝나기 전에 `InputActionAsset`로 전환하면 키 리바인딩·게임패드 지원이 쉬워집니다.
2. **`PlayerDeath`의 추락 판정**: y ≤ 0 하드코딩이라 경사로가 도입되면 즉시 깨집니다. *체크포인트 Y* 또는 *Trigger Collider* 방식으로 교체 필요.
3. **`RockSpawner`의 들여쓰기**: [RockSpawner.cs:20-22](Assets/RockSpawner.cs:20) — `if/else` 들여쓰기가 망가져 있어 코드 정리 권장.
4. **`Invoke("Restart", 1f)`**: 문자열 기반 Invoke는 리네임 시 깨집니다. `Invoke(nameof(Restart), 1f)` 권장.
5. **`CameraFollow.target` null 가드 없음**: 인스펙터 미할당 시 NRE. `if (target == null) return;` 한 줄.
6. **씬이 `SampleScene` 그대로**: Phase 1 마무리하면서 `Game`, `Title`, `Clear` 3개 씬으로 분리 권장.

> 위 항목들은 **빨리 만들기**보다 **다음 작업의 발판**입니다. 지금 안 고쳐도 게임은 돌아가지만, Phase 3(수학 게이트)부터는 씬 분리·Trigger 패턴이 필수가 됩니다.

---

## 5. 바로 다음에 할 일 (추천 순서)

기획서 우선순위와 실제 코드 상태를 반영한 **2주치 작업 큐**입니다.

### Week 1 — Phase 1 마무리
- [ ] 경사로 맵 v0: 길고 폭 좁은 Plane 1개 + 좌우 벽 Cube (5구간 색만 다르게)
- [ ] 카메라가 경사를 따라 추종하도록 `offset`을 로컬 기준으로 보정
- [ ] `PlayerDeath`를 Trigger Collider 기반 KillZone으로 교체
- [ ] 씬 분리: `Title`, `Game`, `Clear` 3개 씬 + 씬 전환 매니저
- [ ] `nameof(Restart)`, `target` null 가드 등 위 코드 리뷰 마이크로픽스

### Week 2 — Phase 2 본격 진입
- [ ] IQ 점수 매니저 (싱글톤 또는 ScriptableObject)
- [ ] IQ UI (TextMeshPro)
- [ ] 바위 처리 시스템 3단계 완성 (양옆 낙하 Trigger + 8~10초 타임아웃)
- [ ] 게임오버/리스타트 UI (Canvas + Button)
- [ ] `RockSpawner` 정리: 시간 기반 → **플레이어 Y 진행 기반** 스폰으로 전환 (기획서의 "구간별 난이도" 토대)

### Backlog — Phase 3 이후
수학 게이트, 캐릭터 진화, 슬로우 모션, 5종 바위, 사운드, 미끄러운 구간 등은 위 두 주가 끝나면 다시 정리합니다.

---

## 6. 미정 결정 사항 (기획자 확인 필요)

| # | 항목 | 옵션 |
|---|------|------|
| A | 게임 정식 명칭 | "데굴데굴 브레인 서바이벌" / "돌 굴러와유" / "돌 굴러가유" — 폴더명·문서명 혼재 |
| B | 한 구간 길이 (월드 좌표) | 기획서에 수치 없음 — 카메라 거리·바위 속도와 함께 결정 필요 |
| C | IQ 증가율 | "높이 비례"만 명시. m당 +1? 초당 +0.5? 수치 합의 필요 |
| D | 캐릭터 진화 — Prefab 교체 vs Animator 전환 | 기획서가 둘 다 제시 |
| E | 사운드 에셋 출처 | "무료 에셋"만 명시 — 라이선스/경로 합의 필요 |
