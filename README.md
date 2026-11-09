# DreamWork

> 꿈 속 기억의 구슬(Dream Globe)을 골라 포탈을 통해 그 꿈의 세계로 들어가는 **Meta Quest 기반 VR 인터랙티브 콘텐츠**입니다.

<img width="654" height="368" alt="DreamSphere" src="https://github.com/user-attachments/assets/2cf16ab7-537f-4d53-b5e9-9aefbaa77cc8" />

📝 개발 기록: [[Unity C#] Scriptable Object 활용](https://hungrykang.tistory.com/85)

---

## 📌 프로젝트 개요

| 항목 | 내용 |
| --- | --- |
| 플랫폼 | Meta Quest (Standalone VR) |
| 엔진 | Unity 6000.2.5f1 (URP 17.2) |
| 입력 | Meta XR Interaction SDK — 컨트롤러 Ray 인터랙션 |
| 형태 | **외주 프로젝트** (기존 프로젝트를 넘겨받아 구조 개선 및 기능 개발, 5월 - 6월 진행) |

---

## 🎬 체험 흐름

```
[MainScene_DD03]
  돔 Dissolve 연출
      │  Dissolve 값이 임계점 이하 → 머티리얼 교체 · BGM 전환
      ▼
  Spike 끝에 Dream Globe 27개 동적 생성
      │  돔 중심과의 거리에 따라 나타나고 / 줄어들고 / 사라짐
      ▼
  Ray로 Globe Hover → 효과음
  Ray로 Globe Select
      ├─ Portal 텍스처가 해당 세트의 relateImage로 교체
      ├─ 세트 번호에 따른 감정 파티클 + BGM (Happy / Sad / Nervous)
      └─ Ego ↔ Globe 사이로 Spline이 자라나며 연결
      ▼
  플레이어가 Portal에 진입
      ▼
[New_DD05]
  선택한 세트의 realImage(Cubemap)를 Skybox로 페이드 인
      ▼
  15초 후 자동 복귀 or X 버튼 → MainScene_DD03
```

---

## ✨ 주요 개발 기능 

### 1. ScriptableObject 기반 이미지 세트 관리

27개 세트 × 3장(= 81장)의 텍스처를 MonoBehaviour에서 직접 들고 있던 구조를 **ScriptableObject 데이터 컨테이너**로 분리했습니다.
비슷한 이미지가 많아 배치 실수가 잦았던 문제를 세트 단위 에셋으로 묶어 해결하고, 씬과 독립적으로 데이터를 관리할 수 있게 했습니다.

```csharp
[CreateAssetMenu(fileName = "ImageSetData", menuName = "Scriptable Objects/ImageSetData")]
public class ImageSetData : ScriptableObject
{
    public string    setID;        // 세트 식별자 (01 ~ 27)
    public Texture2D mainImage;    // Dream Globe 머티리얼
    public Texture2D relateImage;  // Portal 머티리얼
    public Cubemap   realImage;    // DD05 Skybox 머티리얼
}
```

| 클래스 | 역할 |
| --- | --- |
| `ImageSetData` | 세트 하나의 텍스처 3종 보관 (`01.asset` ~ `27.asset`) |
| `ImageSetting` | 전체 세트 목록 관리. `GetByIndex` / `GetBySetID` / `GetByMainTexture` 조회 제공 (Dictionary 캐싱) |
| `SelectableObject` | 각 Globe에 부착되어 자신의 세트 정보를 반환 |

```
ImageSetting (SO) ──▶ ImageSetData 01 ~ 27 (SO)
        ▲
        │ GetImageSet()
SelectableObject (Globe)
        │ 선택 시 ImageSetData 전달
        ├──▶ PortallMaterialController  → relateImage
        ├──▶ EmotionParticlePlayer      → setID 범위별 감정 연출
        └──▶ LevelTransition (static)   → DD05 Skybox realImage
```

### 2. Dream Globe 동적 생성 & 거리 기반 스케일

- `AttachDreamGlobesToSpikeTips` : 이름이 `SpikeTip`으로 시작하는 모든 지점에 Globe 프리팹을 생성하고, 인덱스에 맞는 `mainImage`로 인스턴스 머티리얼을 만들어 적용
- `DreamGlobeScaleByCenterDistance` : 돔 중심과의 거리로 상태 전환
  - `visibleRadius` 안 → 딜레이 후 등장
  - `shrinkStartRadius ~ hiddenRadius` → 점점 작아짐
  - `hiddenRadius` 밖 → 사라지고 다음 루프 대기
- 런타임 생성된 Interaction SDK 컴포넌트가 정상 등록되도록 `ColliderSurface → Grabbable → RayInteractable` 순서로 재초기화

### 3. VR Ray 인터랙션

| 스크립트 | 역할 |
| --- | --- |
| `RayEventProvider` | `Grabbable.WhenPointerEventRaised`를 구독해 Hover / Select 이벤트 처리 |
| `DreamSphereManager` | Ray 이벤트를 모아 `OnSphereSelected` / `OnSphereClicked` / `OnSpherehover`로 전파하는 싱글톤 허브 |
| `ISDKInputBinder` | 씬 전환 후 남아 있는 이전 참조를 끊기 위해 `ControllerRef` → `EventSystem` → `RayInteractor` 순으로 재시작 |
| `LeftHandControllerUI` | 왼손 X / Y 버튼으로 종료 · 재시작 · 이전 씬 복귀 |

### 4. 포탈 & 씬 전환

- `PortallMaterialController` : 선택된 세트의 `relateImage`를 포탈 머티리얼에 적용하고 현재 세트를 보관
- `PortalEnter` : `Player` 태그 충돌 시 보관된 세트로 `LevelTransition.EnterPortal()` 호출
- `LevelTransition` : 선택 세트를 **static 필드로 넘겨** 씬 간 데이터를 전달하고, 전환 전 `RayInteractor` / `RayInteractable`을 비활성화해 등록 해제를 보장
- `DD05SkyboxController` : 런타임 머티리얼 인스턴스를 만들어 `realImage` 큐브맵을 적용, `_Tint`를 Black → 기본색으로 페이드 인 (`OnDestroy`에서 머티리얼 해제)
- `AutoSceneReturner` : DD05 진입 15초 후 메인 씬으로 자동 복귀

### 5. 연출

| 스크립트 | 역할 |
| --- | --- |
| `DissolvingEventManager` | `Dissolver.OnDissolve` 값에 따라 효과음, 돔·계단 머티리얼 교체, BGM 전환, Globe 생성 트리거 |
| `MagicBalllVisble` | Dissolve 진행에 맞춰 Magic Ball VFX 활성화, 선택 시 축소 |
| `EmotionParticlePlayer` | setID `01~09` Happy / `10~18` Sad / `19~26` Nervous 파티클 + BGM |
| `SplineConnectToEgo` · `GrowthRateController` | Ego에서 선택한 Globe까지 Spline을 연결하고 셰이더 Growth 값으로 자라나는 연출 |
| `WPOCollapseLerpController` | 촉수(Tentacle) 셰이더의 WPO 값을 커브로 보간해 수축 연출 |
| `AlembicSyncPlayer` | Alembic 애니메이션 재생 구간 제어 |
| `AvatarAppearance` | 플레이어와 Soul 사이 거리로 아바타 Alpha Clip을 전환 (히스테리시스 적용) |
| `SoundManager` | 이벤트별 BGM 크로스페이드, SFX 풀링, 마스터 볼륨 관리 |
| `IdleVideoPlayer` | 60초 동안 입력이 없으면 대기 영상 재생, 입력 시 숨김 |

---

## 🛠 개발 환경

| 구분 | 버전 |
| --- | --- |
| Unity | 6000.2.5f1 |
| Universal RP | 17.2.0 |
| Visual Effect Graph | 17.2.0 |
| Meta XR SDK (All) | 78.0.0 |
| Meta Avatars SDK | 40.0.1 |
| Unity XR Oculus / OpenXR | 4.5.4 / 1.17.1 |
| Alembic | 2.4.5 |
| UniTask | Git (Cysharp) |


### 조작

| 입력 | MainScene_DD03 | New_DD05 |
| --- | --- | --- |
| 컨트롤러 Ray Hover | Globe 하이라이트 + 효과음 | — |
| 컨트롤러 Ray Select | Globe 선택 → 포탈 이미지 변경 | — |
| 포탈 진입 | New_DD05로 이동 | — |
| 왼손 **X** | 종료 | 메인 씬으로 복귀 |
| 왼손 **Y** | 현재 씬 재시작 | 종료 |

---

## 🔧 트러블슈팅

| 문제 | 해결 |
| --- | --- |
| 81장의 텍스처를 스크립트에서 직접 관리해 비슷한 이미지끼리 배치 혼선 발생 | 세트 단위 `ImageSetData` 에셋으로 묶고 `ImageSetting`에서 일괄 관리 |
| 씬 전환 후 Ray 입력이 동작하지 않음 (이전 씬의 참조가 남아 있음) | 전환 전 Interactor/Interactable을 비활성화해 등록 해제, 새 씬에서 `ISDKInputBinder`로 입력 축 재시작 |
| 런타임에 생성한 Globe가 Ray에 잡히지 않음 | 생성 후 Interaction SDK 컴포넌트를 의존 순서대로 껐다 켜서 재초기화 |
| 런타임 머티리얼이 메모리에 남음 | Skybox / Avatar 머티리얼 인스턴스를 `OnDestroy`에서 `Destroy` |


## 📌 회고 & 개선 방향

- 빌드 불가 문제: 애네미에션 파트에서 사용된 Alembic 패키지가 Android(Quest) 빌드를 지원하지 않았음 (추후 빌드가 필요하다면 애니메이션 파트에서 수정이 필요함)
- **`setID` 기반 조회(`GetBySetID`)** 로 바꿔 순서 의존을 없애는 것입니다. 자세한 내용은 [블로그 글](https://hungrykang.tistory.com/85)에 정리했습니다.

