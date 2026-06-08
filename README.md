# 돌 굴러와유 (Rock Falldown)

> 3D 등반형 회피 서바이벌 + 수학 퍼즐 하이브리드
> **"머리는 차갑게, 발은 빠르게! 굴러오는 돌 사이에서 살아남아라!"**
> Unity 6 (6000.0.69f1) · URP · New Input System

경사로를 아래에서 위로 올라가며 굴러오는 돌을 피하고, 수학 게이트를 통과해 정상(천국)에 도달하는 게임.
게이트 정답으로 IQ가 오르면 캐릭터(초상)가 **원시인 → 학생 → 교수 → 천재** 로 진화한다.

📄 기획서: `돌 굴러와유_기획문서_v2 (2).docx` · 상세 진행: [PROGRESS.md](PROGRESS.md) · 코드 규칙: [GUIDELINES.md](GUIDELINES.md)

---

## 🚀 빠르게 시작 (팀원용)

1. 저장소 클론 → **Unity 6 (6000.0.69f1)** 로 열기 (첫 실행 시 Library 재생성 임포트가 한 번 돈다)
2. `Assets/Scenes/SampleScene.unity` 열기
3. **▶ Play** — 바로 플레이 가능 (모든 셋업·폰트·아트가 씬/에셋에 저장돼 있음)

**조작**: `A/D` 좌우, `W/S` 앞/뒤

> 메뉴를 다시 돌릴 필요 없음. 폰트/구멍 메시/스포너/머티리얼까지 전부 커밋되어 클론 즉시 동작한다.
> 혹시 셋업이 비어 보이면 아래 [에디터 도구](#-에디터-도구-tools--rock-falldown)로 재구성할 수 있다.

---

## ✅ 현재 진행 상황 (완성)

### 스테이지 (경사로를 이어 붙인 구조, 아래→위로 등반)
| 스테이지 | 테마 / 바닥 | 굴러오는 돌 | 비고 |
|---|---|---|---|
| **1** | 초원 (잔디+판석) | 일반 돌 + 빠른 돌 | 튜토리얼 |
| **2** | 얼음 빙판 | **거대 눈덩이(분열)** + 랜덤 기둥 장애물 | 수학 게이트 |
| **3** | 고원 | **빠른 바위(빨강)** + 일반 돌 | 고속·짧은 반응시간 |
| **4** | 우주 (은하 바닥) | **투명 바위 전용** (보임 2초/은신 3초, 물리·넉백은 일반과 동일) | 위치를 기억해 회피 |
| **5** | 천국 (천상계 하늘 바닥) | **빠른 바위 전용** + **바닥 구멍** | 2차방정식 게이트(두 근 모두 통과) → 평지 결승로 + 트로피 완주(타이머 종료) |

> **5스테이지 구멍 컨셉**: 바닥에 불규칙한 구멍이 뚫려 있고, 빠지면 아래로 떨어져(=땅으로 추락) 처음부터 재시작. 별도 즉사 처리 없이 추락→`PlayerDeath`로 재시작된다.

### 핵심 시스템 (구현됨)
- **이동/물리**: Rigidbody, 돌 충돌 시 임펄스 넉백 → 짧게 감속 후 정지 ([PlayerMove](Assets/PlayerMove.cs)/[PlayerHit](Assets/PlayerHit.cs))
- **돌**: 레인 기반 스폰(폭÷돌크기), 스테이지별 종류 지정 ([RockSpawner](Assets/RockSpawner.cs)). 분열바위([SplittingRock](Assets/Scripts/Rock/SplittingRock.cs))·빠른바위([FastRock](Assets/Scripts/Rock/FastRock.cs))·투명바위([PhasingRock](Assets/Scripts/Rock/PhasingRock.cs)). 스테이지 경계 아래로 굴러가면 자동 제거([RockDestroyer](Assets/Prefabs/RockDestroyer.cs))
- **넉백 세기**: 기본/빠른/투명 바위 모두 동일하게 강하게 밀침(40). 바위별로 [Rock.knockbackForce](Assets/Scripts/Rock/Rock.cs)로 조절 가능(없으면 PlayerHit 기본값 40)
- **수학 게이트**: 2지선다 문(정답 통과 / 오답 밀림) ([MathGate](Assets/Scripts/Stage/MathGate.cs)/[GateDoor](Assets/Scripts/Stage/GateDoor.cs)), 문제 데이터 [MathQuestion](Assets/Scripts/ScriptableObjects/MathQuestion.cs)(ScriptableObject). 2차방정식은 `anyAnswerCorrect`로 두 근 모두 통과 처리
- **IQ 시스템**: 기본 60, 게이트 정답마다 **+30**, **천재 상한 150에서 클램프**(게이트가 많아도 초과 안 됨) ([IQManager](Assets/Scripts/System/IQManager.cs)), 좌상단 HUD 숫자([IQDisplay](Assets/Scripts/UI/IQDisplay.cs))
- **캐릭터 진화**: IQ 구간 **60/90/120/150** → 좌상단 HUD 초상([PortraitDisplay](Assets/Scripts/UI/PortraitDisplay.cs)) + 플레이어 3D 색 변화([PlayerTint](Assets/Scripts/Player/PlayerTint.cs)/[CharacterEvolution](Assets/Scripts/Player/CharacterEvolution.cs))
- **스테이지별 스포너 제어**: 플레이어 위치에 따라 현재 스테이지 스포너만 작동(내려오면 재작동) ([StageManager](Assets/Scripts/System/StageManager.cs))
- **장애물**: 표면 위 랜덤 배치 원기둥 ([ObstacleSpawner](Assets/Scripts/Stage/ObstacleSpawner.cs))
- **완주/타이머**: 씬 시작부터 시간 측정([GameTimer](Assets/Scripts/System/GameTimer.cs)/[GameTimerDisplay](Assets/Scripts/UI/GameTimerDisplay.cs)), 트로피 닿으면 종료+기록 표시([FinishTrophy](Assets/Scripts/Stage/FinishTrophy.cs)/[FinishScreen](Assets/Scripts/UI/FinishScreen.cs))
- **한글 폰트**: 맑은 고딕을 동적 TMP 폰트로 만들어 기본 폰트 폴백에 등록 → 게임 내 모든 한글 UI 정상 표시 ([KoreanFontSetup](Assets/Scripts/Editor/KoreanFontSetup.cs))
- **아트**: 바닥/돌/초상 이미지를 GPT(**gpt-image-2**)로 생성해 적용 (아래 참고)

### 예정 / 향후
- 사운드(BGM/효과음), 시작·게임오버 화면, 최고기록 저장, 난이도 곡선 튜닝

---

## 🛠 에디터 도구 (Tools ▸ Rock Falldown)

씬 콘텐츠는 코드로 생성하는 메뉴로 구성한다. (재구성/새 스테이지 추가 시 사용 — 평소엔 불필요)

| 메뉴 | 역할 |
|------|------|
| Setup Game Systems | IQ매니저 · 좌상단 HUD(숫자+초상) · StageManager · 플레이어 색/넉백 값 일괄 셋업 |
| Setup Korean Font (Fallback) | 맑은 고딕을 동적 TMP 폰트로 가져와 기본 폰트 폴백에 등록(한글 깨짐 해결) |
| Create Next Stage (map) | 마지막 스테이지 위로 다음 스테이지 **맵(평면)** 생성 |
| Create Math Gate at End of Stage 1 ~ 5 | 해당 스테이지 끝에 수학 게이트 배치 |
| Setup Stage 2 Content (Obstacles + Splitting Rock) | 거대 눈덩이(분열) + 랜덤 기둥 장애물 |
| Setup Stage 3 Content (Fast Rocks) | 빠른 바위(빨강) + 일반 혼합 |
| Setup Stage 4 Content (Phasing Rocks) | **투명 바위 전용** 스폰 (넉백 40) |
| Setup Stage 5 Content (Holes + Mixed Rocks) | 바닥 불규칙 구멍 메시 + **빠른 바위 전용** 스폰 |
| Rebuild Stage 5 (Fresh) | Stage_5를 삭제 후 천국 바닥으로 새로 생성(구멍/스포너 포함) |
| Setup Stage 5 Finish (Trophy + Timer) | 평지 결승로 + 완주 레일 + 트로피 + 타이머/완주 UI |
| Import & Assign Ground Textures | 잔디/얼음/고원/우주/천국 + 결승선 바닥 텍스처 적용 |
| Import & Assign Portraits | 진화 초상 PNG를 HUD에 연결 |
| Apply Rock Texture | 돌/눈덩이에 바위·눈 텍스처 입히기(무광) |

> 인스펙터 값(넉백 세기 등)을 바꿀 땐 **스크립트 기본값이 아니라 컴포넌트 인스펙터**에서 수정해야 적용됨. (Play 중 실시간 조절 가능)

---

## 🎨 이미지(아트) 생성 — 선택

바닥/돌/초상 이미지는 OpenAI 이미지 API로 생성한다. (이미 생성된 PNG가 저장소에 포함돼 있어 **평소엔 불필요**)

> ⚠️ **모델은 반드시 `gpt-image-2`** 사용 (`gpt-image-1` 금지). 각 생성 스크립트에 고정되어 있음.

재생성이 필요할 때만:
1. `.env.example` → `.env` 복사 후 `OPENAI_API_KEY` 입력 (`.gitignore`로 커밋 제외됨)
2. `pip install openai`
3. 스크립트 실행:
   - `python Tools/generate_ground.py` — 바닥(잔디/얼음/고원)
   - `python Tools/generate_rock.py` — 바위/눈
   - `python Tools/generate_stage4.py` — 우주 바닥(space) + 별 바위(star) + 결승선(finish)
   - `python Tools/generate_heaven.py` — 천국(천상계) 바닥(heaven)
   - `python Tools/generate_portraits.py` — 진화 초상
4. Unity에서 해당 `Import/Apply` 메뉴 실행

---

## 📁 폴더 구조

```
Assets/
├── Scenes/SampleScene.unity      # 메인 씬 (모든 셋업 포함)
├── Fonts/                        # malgun.ttf + Malgun SDF(동적 한글 TMP 폰트)
├── Meshes/Stage5_Floor.mesh      # 5스테이지 구멍 뚫린 바닥 메시
├── Scripts/
│   ├── System/   (IQManager, StageManager, GameTimer)
│   ├── UI/       (IQDisplay, PortraitDisplay, GameTimerDisplay, FinishScreen)
│   ├── Player/   (PlayerTint, CharacterEvolution)
│   ├── Rock/     (Rock, SplittingRock, FastRock, PhasingRock)
│   ├── Stage/    (MathGate, GateDoor, Obstacle, ObstacleSpawner, KillZone, FinishTrophy)
│   ├── ScriptableObjects/ (MathQuestion)
│   └── Editor/   (각종 Builder/Importer 도구 + KoreanFontSetup)
├── Prefabs/      (Rock, Rock_Fast, Rock_Splitting, Rock_Fragment, Rock_Phasing, Obstacle, Evolution/)
├── Materials/    (Stage_Grass/Ice/Plateau/Space/Heaven/Finish, Rock_Boulder/Snow/Fast/Star …)
├── Art/          (Ground/, Rocks/, Portraits/  ← 생성된 이미지)
└── ScriptableObjects/ (Q_Stage2~5  수학 문제)
* Player 루트의 PlayerMove/PlayerHit/PlayerDeath/CameraFollow, RockSpawner 는 Assets 루트에 위치
```

---

## 🤝 협업 메모
- **씬 변경은 편집 모드에서 Ctrl+S 저장** 후 커밋해야 다른 사람에게 반영됨 (플레이 모드 변경은 저장 안 됨).
- `.meta` 파일은 에셋과 **함께** 커밋. `Library/`·`Temp/`·`UserSettings/`는 `.gitignore`로 제외(Unity가 자동 재생성).
- main 브랜치는 빌드/플레이 가능한 상태 유지.
