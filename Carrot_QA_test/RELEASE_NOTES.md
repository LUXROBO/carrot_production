# Carrot QA - EMT Release Notes

## Version 4.2.0 (V1 Standalone Mode)

**릴리즈 날짜**: 2026-03-25

---

### 새로운 기능

#### Standalone 모드 지원

서버 연결 없이 로컬에서 QA 검사를 진행할 수 있습니다.

- **오프라인 검사 가능**: 인터넷이나 VPN 연결 없이 검사 진행
- **로컬 데이터 저장**: 검사 결과가 자동으로 로컬에 저장됨
- **CSV 내보내기**: 검사 결과를 CSV 파일로 내보내기 가능

#### 동작 모드 선택

config.ini 설정을 통해 3가지 동작 모드를 선택할 수 있습니다:

| 모드 | 설명 | 용도 |
|------|------|------|
| **standalone** | 독립 실행 모드 | 서버 없이 로컬 검사 |
| **luckyboxSolution** | 럭키박스 솔루션 연동 | 기존 DB/API 연동 |
| **carrotAPI** | 캐롯 API 연동 | V2 API 준비 |

#### Simulation 모드

BLE 하드웨어 없이 애플리케이션 기능을 테스트할 수 있습니다.

- 개발 및 테스트 환경에서 유용
- 랜덤 테스트 데이터 생성 (80% PASS 비율)

---

### 사용 방법

#### Standalone 모드로 검사하기

1. **config.ini 설정 변경**
   ```ini
   [Application]
   OperationMode=standalone

   [LocalStorage]
   EnableLocalCache=true
   CacheDirectory=./cache
   AutoExportCSV=true
   ```

2. **프로그램 실행**
   - 타이틀에 `[Standalone]` 표시 확인
   - DB 상태가 파란색으로 "Standalone (Local Only)" 표시

3. **검사 진행**
   - 평소와 동일하게 BLE 스캔 및 검사 진행
   - 결과는 자동으로 로컬에 저장됨

4. **결과 확인**
   - `./cache/devices/` 폴더에 디바이스별 JSON 파일 저장
   - `./cache/exports/` 폴더에 CSV 파일 저장

#### CSV 내보내기

검사 결과는 다음 시점에 자동으로 CSV로 내보내집니다:
- Clear 버튼 클릭 시 (세션 종료)
- 프로그램 종료 시

**CSV 파일 위치**: `./cache/exports/LUX2_QA_Result_날짜_시간.csv`

**CSV 내용 예시**:
```
IMEI,ICC_ID,BLE_ID,VERSION,QA1,QA2,QA3,NG_TYPE,...
863593012345678,8982050123456789012,4C52...,v1.2.3,OK,OK,OK,,35,25,120,...
```

---

### 화면 표시 변경

#### 타이틀 표시
- `Carrot QA - EMT [Standalone] v4.2.0`
- `Carrot QA - EMT [LuckyBox] v4.2.0`
- `Carrot QA - EMT [CarrotAPI] v4.2.0`

#### DB 연결 상태 표시

| 상태 | 색상 | 표시 |
|------|------|------|
| Standalone 모드 | 파란색 | "Standalone (Local Only)" |
| DB 연결됨 | 초록색 | 서버 주소 |
| DB 연결 안됨 | 빨간색 | "Disconnected" |

---

### 설정 파일 (config.ini)

#### 새로운 설정 항목

```ini
[Application]
; 동작 모드: standalone | luckyboxSolution | carrotAPI
OperationMode=standalone

[DB]
; DB 연동 활성화 (true | false)
EnableDB=true

[Network]
; VPN 검증 우회 (개발/테스트용)
BypassVPNCheck=false

[LocalStorage]
; 로컬 캐시 활성화
EnableLocalCache=true
; 캐시 저장 위치
CacheDirectory=./cache
; 자동 CSV 내보내기
AutoExportCSV=true

[Simulation]
; 시뮬레이션 모드 활성화
EnableSimulation=false
```

---

### 주의사항

1. **Standalone 모드에서는 제품 조회(F2) 기능 사용 불가**
   - DB 연결이 없으므로 기존 검사 이력 조회 불가
   - "DB 미연결" 메시지 표시

2. **로컬 저장 데이터 백업 권장**
   - `./cache/` 폴더의 데이터를 정기적으로 백업하세요

3. **모드 변경 시 프로그램 재시작 필요**
   - config.ini 변경 후 프로그램을 재시작해야 적용됨

---

### 기존 버전과의 호환성

- 기존 config.ini 파일 그대로 사용 가능
- 새 설정 항목이 없으면 기본값(luckyboxSolution 모드) 적용
- 기존 기능은 모두 동일하게 동작

---

### 문의 및 지원

문제 발생 시 관리자에게 문의하세요.
