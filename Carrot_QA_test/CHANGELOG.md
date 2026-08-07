# Changelog

All notable changes to the Carrot QA - EMT project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [4.2.0] - 2026-03-25

### V1 Standalone Mode Implementation

API 연동 2차 버전 준비를 위한 1단계 리팩토링. 하드코딩된 외부 연동(MySQL DB, dtag API)을 설정 기반 구조로 전환.

### Added

#### Config.cs - 설정 구조 확장
- `OperationMode` enum 추가 (`standalone`, `luckyboxSolution`, `carrotAPI`)
- `AutoExportType` flags enum 추가 (CSV 자동 내보내기 트리거 타입)
- V1 기본값 및 속성 추가:
  - `EnableDB`: DB 연동 활성화 여부
  - `EnableAPI`: API 연동 활성화 여부
  - `BypassVPNCheck`: VPN 검증 우회 옵션
  - `EnableLocalCache`: 로컬 캐시 활성화
  - `CacheDirectory`: 캐시 저장 경로
  - `AutoExportCSV`: 자동 CSV 내보내기
  - `AutoExportInterval`: 자동 내보내기 간격 (분)
  - `AutoExportDeviceCount`: 디바이스 수 기준 내보내기
  - `EnableSimulation`: 시뮬레이션 모드 활성화
  - `SimulationInterval`: 시뮬레이션 간격 (ms)
- 헬퍼 프로퍼티 추가:
  - `IsOnlineMode`: 온라인 모드 여부 (luckyboxSolution || carrotAPI)
  - `IsStandaloneMode`: Standalone 모드 여부
  - `IsCarrotMode`: CarrotAPI 모드 여부
  - `ActiveServerUrl`, `ActiveServerHost`, `ActiveServerBearer`: 현재 서버 설정

#### Taginfo.cs - Mydb 클래스 리팩토링
- `settings` 필드 추가: ApplicationSettings 인스턴스 참조
- `IsConnected` 프로퍼티 추가: DB 연결 상태 확인
- 생성자에서 설정 기반 API 정보 로드
- 조건부 가드 추가:
  - `UpdateQuery()`: EnableDB 검사
  - `UpdateQuery_qa2()`: EnableDB && IsConnected 검사
  - `UpdateQuery_qa3()`: EnableDB && IsConnected 검사
  - `UpdateQuery_dtag()`: EnableDB && IsConnected 검사
  - `GetProduct()`: EnableDB && IsConnected 검사
  - `ReflashList()`: EnableDB && IsConnected 검사
  - `regist_server()`: EnableAPI 검사

#### Form1.cs - 메인 폼 수정
- 생성자에서 모드별 DB 초기화 분기
- `GetModeDisplayText()` 메서드 추가: 모드별 표시 텍스트 반환
- `DbConnetUpdate()` 수정: Standalone 모드 파란색/LightCyan 표시
- `DbTimeUp()` 수정:
  - VPN 우회 로직 (`BypassVPNCheck`)
  - DB 업데이트 조건부 실행
  - LocalCache 저장 통합
- Simulation 관련 필드 및 메서드 추가:
  - `simulationEngine`: SimulationEngine 인스턴스
  - `localCache`: LocalCache 인스턴스
  - `simulationTimer`: 시뮬레이션 타이머
  - `SimulationTimer_Elapsed()`: 시뮬레이션 타이머 이벤트
  - `StartSimulation()`, `StopSimulation()`: 시뮬레이션 제어
  - `RefreshListView()`: ListView 갱신

#### Form2.cs - 제품 조회 폼 수정
- 생성자에서 DB 연결 조건부 실행
- `SearchImei()` 수정: Standalone 모드 알림 표시

#### LocalCache.cs - 로컬 캐시 클래스 (신규)
- 디렉토리 구조: `devices/`, `sessions/`, `exports/`
- 데이터 모델 클래스:
  - `DeviceData`: 디바이스 정보
  - `QaResultData`: QA 결과
  - `SensorDataBlock`: 센서 데이터
  - `LteBandData`: LTE 밴드 데이터
  - `GoldReferenceData`: Gold Reference 값
  - `TimestampData`: 타임스탬프
  - `SyncStatusData`: 동기화 상태
  - `SessionData`: 세션 정보
  - `SessionStatistics`: 세션 통계
  - `FailedDeviceInfo`: 실패 디바이스 정보
  - `IndexData`: 인덱스 파일 데이터
  - `SessionSummary`: 세션 요약
- 주요 메서드:
  - `SaveTag()`: Taginfo를 JSON으로 저장
  - `LoadTag()`: JSON에서 Taginfo 로드
  - `LoadDeviceData()`: DeviceData 로드
  - `ExportToCSV()`: 전체 데이터 CSV 내보내기
  - `SaveSession()`: 세션 정보 저장
  - `GetAllDevices()`: 전체 디바이스 목록
  - `GetPendingSyncDevices()`: 동기화 대기 디바이스 목록

#### Simulation.cs - 시뮬레이션 엔진 (프로젝트 추가)
- `SimulationEngine` 클래스
- `Reset()`: 시퀀스 초기화
- `Tick()`: 랜덤 태그 생성 (80% PASS 비율)
- `BuildTag()`: 랜덤 태그 데이터 생성
- `RandomizeResult()`: 결과 랜덤화

### Changed

#### Config.cs
- INI 로드 로직 확장: 신규 섹션 및 속성 로드
- `LoadDefaultSettings()`: 신규 기본값 초기화

#### Taginfo.cs
- API 서버 정보를 하드코딩에서 설정 참조로 변경
- DB 연결 로직에 조건부 가드 추가

#### Form1.cs
- 타이틀 형식 변경: `Carrot QA - EMT [{Mode}] v{Version}`
- DB 상태 표시 색상 추가 (Standalone: 파란색)

#### Form2.cs
- DB 미연결 시 메시지 박스 표시

### Project Files

#### Carrot_QA_test.csproj
- `LocalCache.cs` 추가
- `Simulation.cs` 추가

---

## File Changes Summary

| 파일 | 변경 유형 | 주요 변경 내용 |
|------|----------|---------------|
| `Config.cs` | Modified | V1 설정 구조 확장 (enum, 속성, 헬퍼) |
| `Taginfo.cs` | Modified | Mydb 클래스 리팩토링 (설정 기반, 조건부 가드) |
| `Form1.cs` | Modified | 모드별 초기화, VPN 우회, 조건부 DB, Simulation 통합 |
| `Form2.cs` | Modified | DB 조건부 연결, Standalone 알림 |
| `LocalCache.cs` | Added | 로컬 캐시 관리 클래스 |
| `Simulation.cs` | Added (project) | 시뮬레이션 엔진 (프로젝트 파일에 추가) |
| `Carrot_QA_test.csproj` | Modified | 신규 파일 참조 추가 |

---

## Migration Guide

### 기존 → V1 업그레이드

1. **config.ini 백업**
   ```bash
   copy config.ini config.ini.backup
   ```

2. **신규 설정 추가** (선택사항 - 기본값 자동 적용)
   ```ini
   [Application]
   OperationMode=luckyboxSolution

   [DB]
   EnableDB=true

   [LocalStorage]
   EnableLocalCache=false
   ```

3. **프로그램 재시작**

### Standalone 모드 전환

```ini
[Application]
OperationMode=standalone

[DB]
EnableDB=false

[LocalStorage]
EnableLocalCache=true
CacheDirectory=./cache
AutoExportCSV=true
```

---

## Architecture

### 모드별 동작 흐름

```
┌─────────────────────────────────────────────────────────────────┐
│                      Form1 Constructor                          │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │ IsOnlineMode │ → │   EnableDB   │ → │ Mydb(connURL) │      │
│  │    true      │    │    true      │    │              │      │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│         │                                                       │
│         ↓ false                                                 │
│  ┌──────────────┐    ┌──────────────┐                          │
│  │ IsStandalone │ → │   Mydb()     │ (no DB connection)        │
│  └──────────────┘    └──────────────┘                          │
├─────────────────────────────────────────────────────────────────┤
│                      DbTimeUp (Timer)                           │
├─────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────┐                    │
│  │ BypassVPNCheck || !IsOnlineMode       │                    │
│  │   → Skip VPN validation               │                    │
│  └────────────────────────────────────────┘                    │
│  ┌────────────────────────────────────────┐                    │
│  │ EnableDB && mydb.IsConnected          │                    │
│  │   → UpdateQuery_qa2/qa3               │                    │
│  │ else                                   │                    │
│  │   → tag.dbString = "Local"            │                    │
│  │   → localCache.SaveTag()              │                    │
│  └────────────────────────────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
```

### LocalCache 디렉토리 구조

```
{CacheDirectory}/
├── devices/                      # 디바이스별 JSON
│   ├── 863593012345678.json
│   └── ...
├── sessions/                     # 세션별 결과
│   └── 20260325_143022.json
├── exports/                      # CSV 내보내기
│   └── LUX2_QA_Result_20260325_143022.csv
└── index.json                    # 인덱스
```

---

## Dependencies

- .NET Framework 4.7.2
- Newtonsoft.Json (JSON 직렬화)
- MySql.Data.MySqlClient (DB 연결)
- Windows.Devices.Bluetooth (BLE)

---

## Testing Checklist

- [ ] Standalone 모드 동작 확인
- [ ] luckyboxSolution 모드 동작 확인 (기존 기능)
- [ ] VPN 우회 옵션 확인
- [ ] LocalCache JSON 저장 확인
- [ ] CSV 내보내기 확인
- [ ] Simulation 모드 확인
- [ ] 모드 전환 시 재시작 테스트
- [ ] Form2 Standalone 알림 확인
