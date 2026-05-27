# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6000.3.10f1 기반 3D 탄막/아케이드 액션 게임. C#으로 작성, 주석은 한국어.

## Build & Development

Unity Editor에서 직접 열어 개발. 코드 편집은 Visual Studio 또는 Rider 사용.

- **솔루션 파일**: `D:/30905/rlsmdTest/rlsmdTest.sln`
- **빌드**: Unity Editor → File → Build Settings → Build
- **테스트**: 자동화 테스트 없음. Unity Editor → Window → General → Test Runner
- **패키지 관리**: `Packages/manifest.json` 편집 후 Unity Editor에서 자동 설치

### 개발 중 치트 키 (CheatManager.cs — 게임 씬에서만 동작)

| 키 | 기능 |
|----|------|
| F1 | 디버그 모드 토글 (FPS·적 수·상태 표시) |
| F2 | 무적 토글 |
| F3 | 적 전체 즉사 |
| F4 | 코인 +1000 |
| F5/F6/F7 | Stage1/2/3 즉시 이동 |
| F8 | 보스 즉시 소환 (StageManager) |
| F9 | 스테이지 클리어 (보스 즉사) |

## Architecture

### 씬 흐름

```
Start → Shop → Stage1/2 → (포탈) → Shop (허브 복귀)
             → Stage3   → (포탈) → End 씬
                        ↘ GameOver → 3초 카운트 → Shop
```

Shop이 허브 역할. 스테이지는 일반 웨이브(킬 누적) → 보스 → 포탈 클리어 구조.  
포탈은 `GameManager.currentStage == 2`(Stage3)이면 "End" 씬으로, 아니면 `GoToMenu()`(Shop)로 이동.  
`ClearScene.unity`·`RankingScene.unity` 파일은 존재하지만 포탈 흐름에서 직접 사용되지 않음.

### 싱글톤 매니저 계층 (모두 DontDestroyOnLoad)

| 매니저 | 파일 | 역할 |
|--------|------|------|
| GameManager | `Script/GameManager.cs` | 점수·코인·스테이지 진행·게임오버·씬 전환. 씬 로드 시 태그로 UI 재연결 |
| SkillManager | `Script/SkillManager.cs` | 스킬 구매·슬롯(Q=slot1, R=slot2) 할당·쿨다운(8초) |
| InventoryManager | `Script/InventoryManager.cs` | 가방·아이템(1~5키 사용)·폭탄(Q키)·파츠. 씬 로드 시 파츠 효과 재적용 |
| SaveManager | `Script/SaveManager.cs` | JSON 파일 저장(`Application.persistentDataPath/save.json`) |
| FadeManager | `Script/FadeManager.cs` | 씬 전환 페이드 |

> **씬-로컬 싱글톤**: `StageManager`는 `DontDestroyOnLoad` 없이 `Instance`를 노출하는 씬-로컬 싱글톤임. 씬 전환 시 자동 소멸되며, 위 영속 매니저들과 달리 스테이지 씬에만 존재함.

> **주의**: GameManager는 PlayerPrefs로도 저장하고, SaveManager는 별도 JSON 파일을 사용한다. 두 시스템이 병존함.

### 플레이어 시스템 (`Script/PlayerCs/`)

| 파일 | 클래스 | 역할 |
|------|--------|------|
| `PlayerController.cs` | `PlayerMovement` | WASD 이동, 가속/감속(`speedMultiplier` 필드로 외부 조작 가능) |
| `PlayerHealth.cs` | `PlayerHealth` | HP, 무적(스택 방식 — 여러 소스 동시 무적 지원), 방어력 부스트 |
| `PlayerDash.cs` | `PlayerDash` | 대시/회피, `slamDamage` 필드. `GhostTrail` 컴포넌트 자동 연동 |
| `PlayerReflect.cs` | `PlayerReflect` | 범위 내 탄 반사 (`reflectRadius`, `cooldown` 필드) |
| `GhostTrail.cs` | `GhostTrail` | 대시 중 잔상(고스트 트레일) 이펙트. `PlayerDash`가 `StartTrail()`/`StopTrail()` 호출 |

> **파일명 주의**: `PlayerController.cs` 파일 안의 클래스 이름은 `PlayerMovement`임.

### 적 시스템 (`Script/EnemyCs/`)

- **EnemyData** (ScriptableObject) — 이동 패턴(Chase/Zigzag/Charge/Strafe), 공격 패턴(Single/Spread/Circle/Burst/Spiral), 외형, 드롭 등 전부 데이터로 구성. 새 적은 코드 수정 없이 에셋 생성만으로 추가.  
  우클릭 → Create → Enemy → EnemyData
- **EnemyController** — EnemyData를 읽어 런타임 동작 결정. 사망 시 `StageManager.OnEnemyKilled()` 호출
- **EnemyHealth** (`EnemyHealth.cs`) — HP·점수·타격/사망 이펙트·루트 드롭을 담당하는 독립 컴포넌트. `EnemyController`가 없는 단순 적에도 단독으로 붙일 수 있음. `lootDropChance`·`lootName`·`lootSellValue`·`lootWeight` Inspector 필드로 드롭 설정.
- **EnemyShooter** — EnemyData 의존 없이 독립 동작하는 단순 적 컴포넌트. 자체 이동(`speed`, `stopDistance`)·발사(`fireInterval`, `projectilePrefab`) 로직 포함. StageManager와 연동 안 됨(킬 카운트 미기여).
- **EnemySpawner** — 웨이브 설정에 따라 적 생성
- **BossController / BossHealth** — 보스 전용 로직. 사망 시 포탈 스폰
  - 보스 페이즈: `GameManager.currentStage`(스테이지 인덱스)에 따라 페이즈 수 다름
    - Stage1: 페이즈 1만 / Stage2: HP 50% 이하 → 페이즈 2 / Stage3: HP 60% 이하 → 2, 30% 이하 → 3(Enrage)
  - 낙하 연출 0.6초 후 공격 시작. `bulletPrefab`(추적탄)과 `straightBulletPrefab`(직선탄) 두 프리팹 필요

### 스테이지 진행 흐름 (`Script/StageCs/StageManager.cs`)

1. EnemyController.Die() → StageManager.OnEnemyKilled() 호출
2. 킬 수 ≥ `killsToSpawnBoss`(기본 30) → 일반 적 스폰 중단, EnemySpawner 비활성화
3. 플레이어 앞에 `BossSpawnIndicator` 프리팹 소환 → 보스 등장
4. 보스 처치 → BossHealth.Die() → 포탈 스폰 → 플레이어가 포탈 터치 → 씬 전환 (위 씬 흐름 참조)

### 장애물 시스템 (`Script/ObstacleSpawner.cs`, `Script/FlyingObstacle.cs`)

맵 외곽에서 플레이어 방향으로 장애물을 날려보내는 환경 위험 요소. `EnemyController`·`StageManager`와 무관하게 독립 동작.

- **ObstacleSpawner** — 지정된 스폰 포인트 배열에서 랜덤 장애물 프리팹을 주기적으로 생성. 방향은 플레이어 위치 기준 + `spreadAngle` 랜덤 편차
- **FlyingObstacle** — 설정된 방향으로 직진, `lifetime`초 후 자동 제거. `destroyOnHit=true`면 플레이어 충돌 시 즉시 파괴

### 탄환 시스템 (`Script/Bullet.cs`)

- 기본: 플레이어 추적(호밍), `isStraight=true`면 직진
- `Bullet.Reflect(Transform newTarget)` — PlayerReflect에서 호출, 적 방향으로 즉시 전환
- `isBossBullet` 플래그로 보스 탄 구분 (반사탄만 보스에게 피해)
- LineRenderer로 꼬리 궤적 표현 (URP Particles/Unlit 셰이더)

### Shop 씬 UI (`Script/MenuUI.cs`)

Shop(허브) 씬의 메인 UI 컨트롤러. 씬 내 Canvas 루트에 부착.

- 패널 전환: main / stageSelect / settings / shop / skillShop / ranking. `ShowPanel()`이 한 번에 하나만 활성화.
- `CamAnimator` — 패널 이동 시 카메라 애니메이션 트리거(`Move`, `ShopMove`, `SettMove`, `SettBack`)
- 스테이지 버튼 잠금: `GameManager.unlockedStages`를 읽어 미해금 스테이지 버튼을 `interactable=false` + 🔒 표시
- `SkillShopUI.Close()`를 통해 스킬 상점 패널과 통신 (직접 `SetActive` 하지 않음)

### 상점 시스템 (`Script/Shop/`)

- **ShopItemData** (ScriptableObject) — 파츠/소모품/폭탄 구분. 우클릭 → Create → Shop → ShopItemData
- **파츠 종류** (PartType): MoveSpeedUp, MaxHpUp, ReflectRadiusUp, ReflectCooldownDown, DashDamageUp
- **소모품 종류** (ItemType): Heal, Invincibility, DefenseBoost
- 파츠는 1회 구매, 씬 전환 후에도 InventoryManager가 재적용
- **ShopTabController** — Shop 씬 UI의 탭 전환. 탭 3개: 파츠(0) / 아이템(1) / 가방(2). 가방 탭은 `InventoryManager`의 루트 목록을 표시.
- **FishAndChips** — `SkillShopUI`에서 참조하는 구매 연출 전용 컴포넌트. `PlayPart()`(애니메이션 재생 후 마지막 프레임 고정) / `SetPartImmediate()`(씬 복원 시 즉시 최종 상태 고정) 두 진입점
- **DraggableSkill** — 스킬 아이콘을 드래그해 Q/R 슬롯에 배정하는 UI 컴포넌트. `SkillType` 필드로 어떤 스킬인지 지정.
- **SkillShopPanelBuilder** — Shop씬 Canvas에 부착하면 `Awake()`에서 스킬 상점 패널 UI 전체를 코드로 자동 생성. `OpenPanel()`/`ClosePanel()` 메서드를 버튼 OnClick에 연결해 사용.

> **파츠 이중 경로 주의**: `PlayerParts.cs`(플레이어 오브젝트에 부착, 씬 전환 시 초기화)와 `InventoryManager.BuyPart()`가 동일한 파츠 적용 로직을 중복 보유. 새 파츠 종류 추가 시 두 곳(`PlayerParts.Apply()`, `InventoryManager.ApplyPartToPlayer()`) 모두 수정 필요.

### 스킬 시스템 (`Script/SkillManager.cs`)

- **스킬 종류** (SkillType): SpeedBoost(200C, 4초), Shield(300C, 3초), Reflect(400C, 즉발)
- Q키=슬롯1, R키=슬롯2. Shop 씬에서는 입력 무시
- SpeedBoost는 PlayerMovement.speedMultiplier 직접 조작

> **Q키 충돌**: `InventoryManager.Update()`(Q=폭탄 사용)와 `SkillManager.Update()`(Q=슬롯1 사용)가 동시에 Q키를 처리함. 게임 씬에서 Q를 누르면 두 동작 모두 발동.

### 랭킹 시스템

랭킹 관련 파일이 3개 병존하며 같은 PlayerPrefs 키(`"RankWrapper"`)를 공유:
- `RankingSystem.cs` (`SimpleRanking`) — 기본 랭킹 저장/불러오기 클래스 (오름차순 정렬, 상위 5개)
- `RankingManager.cs` — RankingScene에서 UI 표시·이름 입력·등록 담당 (내림차순, 상위 10개)
- `RankingViewer.cs` — 읽기 전용 조회용
- `Leaderboard.cs` — 현재 빈 파일(스텁), 미구현

### UI 태그 의존 패턴

`GameManager.OnSceneLoaded()`가 씬 로드 후 태그로 UI를 재연결함. 게임 씬에 다음 태그가 설정된 오브젝트가 없으면 UI가 갱신되지 않음:

| 태그 | 용도 |
|------|------|
| `ScoreText` | 점수 Text |
| `CoinText` | 코인 Text |
| `GameOverPanel` | 게임오버 패널 |
| `FinalScoreText` | 게임오버 최종 점수 Text |
| `FinalCoinText` | 게임오버 최종 코인 Text |

### 인게임 HUD (`Script/Inventory.cs`)

`Inventory.cs`(`Inventory` 클래스)는 `InventoryManager`와 이름이 유사하지만 완전히 별개. OnGUI로 화면 우측 상단에 가방 수, 아이템 슬롯(1~5키), 폭탄 수를 표시하는 **읽기 전용** HUD. 게임 씬 오브젝트에 붙이며, 데이터 쓰기는 전혀 하지 않음.

### 루트/가방 시스템 (`Script/LootItem.cs`, `Script/BagUI.cs`)

적 사망 시 월드에 `LootItem` 오브젝트가 스폰되고, 플레이어가 닿으면 `InventoryManager`의 가방에 추가.

- **LootEntry** — 루트 데이터 클래스(`lootName`, `sellValue`, `weight`). `EnemyHealth` Inspector에서 드롭 확률/내용 설정.
- **BagUI** — B키로 BagPanel 열고닫기. 싱글톤 `BagUI.Instance`. 패널 이름 `"BagPanel"`으로 자동 탐색(Inspector 연결 불필요). `BagUI.Instance?.Refresh()`로 갱신.
- **InventoryManager 한도**: `bagMaxSlots=10`, `itemMaxSlots=5`, `maxBombs=9`. 가방이 가득 찬 상태로 루트를 줍으면 조용히 버려짐.

### 씬 전환 유틸리티

- **StageIntro** (`Script/StageCs/StageIntro.cs`) — 스테이지 씬 진입 시 `FadeManager.FadeIn()` 자동 호출. 씬 내 아무 오브젝트에나 붙이면 됨.
- **SceneTrigger** (`Script/SceneTrigger.cs`) — Collider(IsTrigger) 접촉 시 지정 씬으로 이동. **태그 필터 없음** — 어떤 Collider든 닿으면 씬 전환 발생. Player 전용으로 쓰려면 태그 체크 코드를 추가해야 함.
- **Portal** (`Script/Portal.cs`) — 보스 처치 후 스폰되는 포탈. `CompareTag("Player")` 체크 있음(SceneTrigger와 달리 Player만 반응). `UnlockNextStage()` 후 Stage3이면 "End" 씬, 아니면 `GoToMenu()`(Shop)로 이동.

### 스텁 파일 (삭제 가능)

`Shop.cs`, `ShopScene.cs`, `MenuScene.cs` — 기능이 다른 클래스로 이전되어 현재 빈 스텁임. 삭제해도 무방.

### SaveManager 사용 현황

`SaveManager`는 싱글톤으로 등록되지만 **어디에서도 `Save()`/`Load()`를 자동 호출하지 않음**. 실질적 저장은 `GameManager.SaveGame()`(PlayerPrefs)이 담당. `SaveManager`를 실제로 활용하려면 호출 지점을 명시적으로 추가해야 함.

## 주요 패키지

- `com.unity.inputsystem` (1.18.0) — 모던 입력 처리 (일부 레거시 Input.GetKey 혼용)
- `com.unity.render-pipelines.universal` (17.0.0) — URP 렌더링
- `com.unity.ai.navigation` (2.0.10) — NavMesh
