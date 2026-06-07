# 돌 굴러와유 (Rock Falldown)

> 3D 등반형 회피 서바이벌 + 수학 퍼즐 하이브리드
> **"머리는 차갑게, 발은 빠르게! 굴러오는 돌 사이에서 살아남아라!"**
> Unity 6 (6000.0.x) · URP · New Input System

경사로를 아래에서 위로 올라가며 굴러오는 돌을 피하고, 수학 게이트를 통과해 정상에 도달하는 게임.
오래 살아남아 IQ가 오르면 캐릭터(초상)가 **원시인 → 학생 → 교수 → 천재** 로 진화한다.

📄 기획서: `돌 굴러와유_기획문서_v2 (2).docx` · 상세 진행: [PROGRESS.md](PROGRESS.md) · 코드 규칙: [GUIDELINES.md](GUIDELINES.md)

---

## 🚀 빠르게 시작 (팀원용)

1. 저장소 클론 → **Unity 6 (6000.0.69f1)** 로 열기
2. 최초 1회: 메뉴 **Window ▸ TextMeshPro ▸ Import TMP Essential Resources**
3. `Assets/Scenes/SampleScene.unity` 열기
4. **▶ Play** — 바로 플레이 가능 (모든 셋업은 씬에 저장돼 있음)

**조작**: `A/D` 좌우, `W/S` 앞/뒤

> ⚠️ 만약 IQ/HUD/스테이지 셋업이 비어 보이면, 아래 [에디터 도구](#-에디터-도구-tools--rock-falldown)로 재구성할 수 있습니다.

---

## ✅ 현재 진행 상황

### 스테이지 (경사로를 이어 붙인 구조, 아래→위)
| 스테이지 | 테마 / 바닥 | 굴러오는 돌 | 비고 |
|---|---|---|---|
| **1** | 초원 (잔디+판석) | 일반 돌(회색 바위) | 튜토리얼 |
| **2** | 얼음 빙판 | **거대 눈덩이(분열)** + 일반 강한 돌 + 랜덤 기둥 장애물 | 수학 게이트 |
| **3** | 화산 고원 | **빠른 바위(빨강)** + 일반 돌 | 고속·짧은 반응시간 |
| 4·5 | (예정) | 분열/혼합 / 보스 | Stage 5 = 바닥 구멍 컨셉(메모) |

### 핵심 시스템 (구현됨)
- **이동/물리**: Rigidbody, 돌 충돌 시 임펄스 넉백 → 짧게 감속 후 정지 ([PlayerMove](Assets/PlayerMove.cs)/[PlayerHit](Assets/PlayerHit.cs))
- **돌**: 레인 기반 스폰(폭÷돌크기), 여러 종류 혼합 스폰 ([RockSpawner](Assets/RockSpawner.cs)), 분열바위([SplittingRock](Assets/Scripts/Rock/SplittingRock.cs))·빠른바위([FastRock](Assets/Scripts/Rock/FastRock.cs))
- **수학 게이트**: 2지선다 문(정답 통과 / 오답 밀림) ([MathGate](Assets/Scripts/Stage/MathGate.cs)/[GateDoor](Assets/Scripts/Stage/GateDoor.cs)), 문제 데이터 [MathQuestion](Assets/Scripts/ScriptableObjects/MathQuestion.cs)(ScriptableObject)
- **IQ 시스템**: 기본 60, 게이트 정답마다 **+30** ([IQManager](Assets/Scripts/System/IQManager.cs)), 좌상단 HUD 숫자([IQDisplay](Assets/Scripts/UI/IQDisplay.cs))
- **캐릭터 진화**: IQ 구간 **60/90/120/150** → 좌상단 HUD 초상(2프레임 깜빡임, [PortraitDisplay](Assets/Scripts/UI/PortraitDisplay.cs)) + 플레이어 3D 색 변화([PlayerTint](Assets/Scripts/Player/PlayerTint.cs))
- **스테이지별 스포너 제어**: 플레이어 위치에 따라 현재 스테이지 스포너만 작동(내려오면 재작동) ([StageManager](Assets/Scripts/System/StageManager.cs))
- **장애물**: 표면 위 랜덤 배치 원기둥 ([ObstacleSpawner](Assets/Scripts/Stage/ObstacleSpawner.cs))
- **아트**: 바닥/돌/초상 이미지를 GPT(gpt-image-2)로 생성해 적용 (아래 참고)

### 아직 없음 / 예정
- Stage 4·5, 사운드, 시작/클리어/게임오버 화면, 최고점수 저장

---

## 🛠 에디터 도구 (Tools ▸ Rock Falldown)

씬 콘텐츠는 코드로 생성하는 메뉴로 구성한다. (재구성/새 스테이지 추가 시 사용)

| 메뉴 | 역할 |
|------|------|
| Setup Game Systems | IQ매니저 · 좌상단 HUD(숫자+초상) · StageManager · 플레이어 색/넉백 값 일괄 셋업 |
| Create Next Stage (map) | 마지막 스테이지 위로 다음 스테이지 **맵(평면)** 생성 (Stage_4, 5…) |
| Create Math Gate at End of Stage 1 / 2 | 해당 스테이지 끝에 수학 게이트 배치 |
| Setup Stage 2 Content | 거대 눈덩이(분열) + 랜덤 기둥 장애물 + 혼합 스폰 |
| Setup Stage 3 Content (Fast Rocks) | 빠른 바위(빨강) + 혼합 스폰 |
| Import & Assign Ground Textures | 잔디/얼음/고원 바닥 텍스처를 각 스테이지에 적용 |
| Import & Assign Portraits | 진화 초상 PNG를 HUD에 연결 |
| Apply Rock Texture | 돌/눈덩이에 바위·눈 텍스처 입히기(무광) |

> 인스펙터 값(넉백 세기 등)을 바꿀 땐 **스크립트 기본값이 아니라 컴포넌트 인스펙터**에서 수정해야 적용됨. (Play 중 실시간 조절 가능)

---

## 🎨 이미지(아트) 생성 — 선택

바닥/돌/초상 이미지는 OpenAI 이미지 API로 생성한다. (이미 생성된 PNG가 저장소에 포함돼 있어 **평소엔 불필요**)

재생성이 필요할 때만:
1. `.env.example` → `.env` 복사 후 `OPENAI_API_KEY` 입력 (`.gitignore`로 커밋 제외됨)
2. `pip install openai`
3. 스크립트 실행:
   - `python Tools/generate_ground.py` — 바닥(잔디/얼음/고원)
   - `python Tools/generate_rock.py` — 바위/눈
   - `python Tools/generate_portraits.py` — 진화 초상
4. Unity에서 해당 `Import/Apply` 메뉴 실행

> 모델: `gpt-image-2` (`.env`의 `IMAGE_MODEL`). 이 모델은 투명 배경 미지원이라 흰/일반 배경으로 생성됨.

---

## 📁 폴더 구조

```
Assets/
├── Scenes/SampleScene.unity      # 메인 씬 (모든 셋업 포함)
├── Scripts/
│   ├── System/   (IQManager, StageManager)
│   ├── UI/       (IQDisplay, PortraitDisplay)
│   ├── Player/   (PlayerTint, CharacterEvolution*)
│   ├── Rock/     (Rock, SplittingRock, FastRock)
│   ├── Stage/    (MathGate, GateDoor, Obstacle, ObstacleSpawner)
│   ├── ScriptableObjects/ (MathQuestion)
│   └── Editor/   (각종 Builder/Importer 도구)
├── Prefabs/      (Rock, Rock_Splitting, Rock_Fragment, Rock_Fast, Obstacle, Evolution/)
├── Materials/    (Stage_Grass/Ice/Plateau, Rock_Boulder/Snow/Fast …)
├── Art/          (Ground/, Rocks/, Portraits/  ← 생성된 이미지)
└── ScriptableObjects/ (Q_Stage2_01, Q_Stage2_02 …  수학 문제)
* Player 루트의 PlayerMove/PlayerHit/PlayerDeath/CameraFollow, RockSpawner 는 Assets 루트에 위치
```

---

## 🤝 협업 메모
- **씬 변경은 편집 모드에서 Ctrl+S 저장** 후 커밋해야 다른 사람에게 반영됨 (플레이 모드 변경은 저장 안 됨).
- `.meta` 파일은 에셋과 **함께** 커밋.
- main 브랜치는 빌드/플레이 가능한 상태 유지.
