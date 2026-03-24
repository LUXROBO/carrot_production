using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using IniFileManager;

namespace Carrot_QA_test
{
    /// <summary>
    /// 로컬 캐시 저장소 관리 클래스
    /// Standalone 및 CarrotAPI 모드에서 QA 결과 데이터를 로컬에 저장
    /// </summary>
    public class LocalCache
    {
        private readonly ApplicationSettings _settings;
        private readonly string _cacheDir;
        private readonly string _sessionFile;
        private readonly List<CacheEntry> _entries;
        private DateTime _sessionStartTime;
        private int _exportedCount = 0;

        /// <summary>
        /// 캐시 활성화 여부
        /// </summary>
        public bool IsEnabled => _settings.EnableLocalCache;

        /// <summary>
        /// 현재 캐시 항목 수
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// 마지막 내보내기 이후 추가된 항목 수
        /// </summary>
        public int PendingCount => _entries.Count - _exportedCount;

        /// <summary>
        /// LocalCache 생성자
        /// </summary>
        public LocalCache()
        {
            _settings = ApplicationSettings.Instance();
            _entries = new List<CacheEntry>();
            _sessionStartTime = DateTime.Now;

            // 캐시 디렉토리 설정
            _cacheDir = _settings.CacheDirectory;
            if (string.IsNullOrWhiteSpace(_cacheDir))
            {
                _cacheDir = "./cache";
            }

            // 상대 경로를 절대 경로로 변환
            if (!Path.IsPathRooted(_cacheDir))
            {
                _cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _cacheDir);
            }

            // 세션 파일명 생성
            _sessionFile = $"session_{_sessionStartTime:yyyyMMdd_HHmmss}.csv";

            // 캐시 디렉토리 생성
            EnsureCacheDirectory();
        }

        /// <summary>
        /// 캐시 디렉토리 확인 및 생성
        /// </summary>
        private void EnsureCacheDirectory()
        {
            try
            {
                if (!Directory.Exists(_cacheDir))
                {
                    Directory.CreateDirectory(_cacheDir);
                    Trace.WriteLine($"LocalCache: Created cache directory at {_cacheDir}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LocalCache: Failed to create cache directory: {ex.Message}");
            }
        }

        /// <summary>
        /// 태그 정보를 캐시에 추가
        /// </summary>
        /// <param name="tag">Taginfo 객체</param>
        public void Add(Taginfo tag)
        {
            if (!IsEnabled || tag == null)
                return;

            var entry = new CacheEntry
            {
                IMEI = tag.TagIMEI ?? "",
                ICC_ID = tag.TagIccID ?? "",
                BLE_ID = GetShortBleId(tag.TagBleID),
                Version = tag.TagVersion ?? "",
                QA1 = "",
                QA2 = tag.passFlag ?? "",
                QA3 = "",
                NG_TYPE = tag.TagFlagString?.Replace(",", "|").Replace("\"", "'") ?? "",
                GPS_SNR = tag.ng2_gpsSnr,
                TEMP = tag.ng2_temp,
                CAP = tag.ng2_cap,
                BLE_RSSI = tag.ng2_ble_rssi,
                LTE_B3_AVG = tag.ng2_b3_avg,
                LTE_B5_AVG = tag.ng2_b5_avg,
                Timestamp = DateTime.Now
            };

            // 중복 확인 (같은 IMEI가 있으면 업데이트)
            var existing = _entries.FirstOrDefault(e => e.IMEI == entry.IMEI);
            if (existing != null)
            {
                var index = _entries.IndexOf(existing);
                _entries[index] = entry;
                Trace.WriteLine($"LocalCache: Updated entry for IMEI {entry.IMEI}");
            }
            else
            {
                _entries.Add(entry);
                Trace.WriteLine($"LocalCache: Added entry for IMEI {entry.IMEI}");
            }

            // 자동 내보내기 조건 확인
            CheckAutoExportConditions();
        }

        /// <summary>
        /// BLE ID 축약
        /// </summary>
        private string GetShortBleId(string bleId)
        {
            if (string.IsNullOrEmpty(bleId))
                return "";

            return bleId.Length >= 12 ? bleId.Substring(bleId.Length - 12, 12) : bleId;
        }

        /// <summary>
        /// 자동 내보내기 조건 확인
        /// </summary>
        private void CheckAutoExportConditions()
        {
            if (!_settings.AutoExportCSV)
                return;

            // 디바이스 수 기준 자동 내보내기
            if (PendingCount >= _settings.AutoExportDeviceCount)
            {
                ExportToCSV(AutoExportType.DeviceCount);
            }
        }

        /// <summary>
        /// 시간 기반 자동 내보내기 확인 (타이머에서 호출)
        /// </summary>
        public void CheckIntervalExport()
        {
            if (!_settings.AutoExportCSV || PendingCount == 0)
                return;

            var elapsed = DateTime.Now - _sessionStartTime;
            if (elapsed.TotalMinutes >= _settings.AutoExportInterval)
            {
                ExportToCSV(AutoExportType.Interval);
                _sessionStartTime = DateTime.Now; // 타이머 리셋
            }
        }

        /// <summary>
        /// CSV 파일로 내보내기
        /// </summary>
        /// <param name="exportType">내보내기 트리거 타입</param>
        /// <param name="customPath">사용자 지정 경로 (Manual 모드용)</param>
        /// <returns>내보내기 성공 여부</returns>
        public bool ExportToCSV(AutoExportType exportType, string customPath = null)
        {
            if (_entries.Count == 0)
            {
                Trace.WriteLine("LocalCache: No entries to export");
                return false;
            }

            try
            {
                string filePath;
                if (!string.IsNullOrEmpty(customPath))
                {
                    filePath = customPath;
                }
                else
                {
                    string fileName = $"QA_Export_{DateTime.Now:yyyyMMdd_HHmmss}_{exportType}.csv";
                    filePath = Path.Combine(_cacheDir, fileName);
                }

                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // CSV 헤더
                    sw.WriteLine("IMEI,ICC_ID,BLE_ID,VERSION,QA1,QA2,QA3,NG_TYPE,GPS_SNR,TEMP,CAP,BLE_RSSI,LTE_B3_AVG,LTE_B5_AVG,TIMESTAMP");

                    // 데이터 행
                    foreach (var entry in _entries)
                    {
                        string line = string.Join(",",
                            entry.IMEI,
                            entry.ICC_ID,
                            entry.BLE_ID,
                            entry.Version,
                            entry.QA1,
                            entry.QA2,
                            entry.QA3,
                            $"\"{entry.NG_TYPE}\"",
                            entry.GPS_SNR,
                            entry.TEMP,
                            entry.CAP,
                            entry.BLE_RSSI,
                            entry.LTE_B3_AVG,
                            entry.LTE_B5_AVG,
                            entry.Timestamp.ToString("s")
                        );
                        sw.WriteLine(line);
                    }
                }

                _exportedCount = _entries.Count;
                Trace.WriteLine($"LocalCache: Exported {_entries.Count} entries to {filePath} (trigger: {exportType})");
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LocalCache: Export failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 세션 종료 시 자동 저장
        /// </summary>
        public void OnSessionEnd()
        {
            if (_settings.AutoExportCSV && PendingCount > 0)
            {
                ExportToCSV(AutoExportType.SessionEnd);
            }
        }

        /// <summary>
        /// 애플리케이션 종료 시 자동 저장
        /// </summary>
        public void OnAppClose()
        {
            if (_settings.AutoExportCSV && PendingCount > 0)
            {
                ExportToCSV(AutoExportType.AppClose);
            }
        }

        /// <summary>
        /// 캐시 초기화
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _exportedCount = 0;
            _sessionStartTime = DateTime.Now;
            Trace.WriteLine("LocalCache: Cleared all entries");
        }

        /// <summary>
        /// 현재 캐시 항목 목록 반환
        /// </summary>
        public IReadOnlyList<CacheEntry> GetEntries()
        {
            return _entries.AsReadOnly();
        }
    }

    /// <summary>
    /// 캐시 항목 데이터 구조
    /// </summary>
    public class CacheEntry
    {
        public string IMEI { get; set; }
        public string ICC_ID { get; set; }
        public string BLE_ID { get; set; }
        public string Version { get; set; }
        public string QA1 { get; set; }
        public string QA2 { get; set; }
        public string QA3 { get; set; }
        public string NG_TYPE { get; set; }
        public int GPS_SNR { get; set; }
        public int TEMP { get; set; }
        public int CAP { get; set; }
        public int BLE_RSSI { get; set; }
        public int LTE_B3_AVG { get; set; }
        public int LTE_B5_AVG { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
