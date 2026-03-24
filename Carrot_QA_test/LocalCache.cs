using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using IniFileManager;

namespace Carrot_QA_test
{
    /// <summary>
    /// Standalone 모드에서 QA 데이터를 로컬에 캐싱하고 관리하는 클래스
    /// </summary>
    public class LocalCache
    {
        private readonly string cacheFilePath;
        private readonly ApplicationSettings settings;
        private List<QARecord> records;
        private readonly object lockObj = new object();

        public LocalCache()
        {
            this.settings = ApplicationSettings.Instance();
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarrotQA_EMT"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            this.cacheFilePath = Path.Combine(appDataPath, "qa_cache.json");
            this.records = new List<QARecord>();

            LoadCache();
        }

        public LocalCache(string customPath)
        {
            this.settings = ApplicationSettings.Instance();
            this.cacheFilePath = customPath;
            this.records = new List<QARecord>();

            string directory = Path.GetDirectoryName(customPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LoadCache();
        }

        /// <summary>
        /// QA 레코드 추가
        /// </summary>
        public void AddRecord(QARecord record)
        {
            if (!settings.EnableLocalCache) return;

            lock (lockObj)
            {
                record.Timestamp = DateTime.Now;
                record.IsSynced = false;
                records.Add(record);
                SaveCache();
            }
        }

        /// <summary>
        /// QA2 결과 저장
        /// </summary>
        public void SaveQA2Result(string imei, string iccId, string passFlag, string flagString, string bleId)
        {
            if (!settings.EnableLocalCache) return;

            var record = new QARecord
            {
                IMEI = imei,
                IccID = iccId,
                PassFlag = passFlag,
                FlagString = flagString,
                BleID = bleId,
                QAStage = "QA2",
                Timestamp = DateTime.Now,
                IsSynced = false
            };

            AddRecord(record);
        }

        /// <summary>
        /// QA3 결과 저장
        /// </summary>
        public void SaveQA3Result(string imei, string iccId, string passFlag, string flagString, string bleId)
        {
            if (!settings.EnableLocalCache) return;

            var record = new QARecord
            {
                IMEI = imei,
                IccID = iccId,
                PassFlag = passFlag,
                FlagString = flagString,
                BleID = bleId,
                QAStage = "QA3",
                Timestamp = DateTime.Now,
                IsSynced = false
            };

            AddRecord(record);
        }

        /// <summary>
        /// 동기화되지 않은 레코드 조회
        /// </summary>
        public List<QARecord> GetUnsyncedRecords()
        {
            lock (lockObj)
            {
                return records.Where(r => !r.IsSynced).ToList();
            }
        }

        /// <summary>
        /// 전체 레코드 조회
        /// </summary>
        public List<QARecord> GetAllRecords()
        {
            lock (lockObj)
            {
                return new List<QARecord>(records);
            }
        }

        /// <summary>
        /// 레코드 동기화 상태 업데이트
        /// </summary>
        public void MarkAsSynced(string imei)
        {
            lock (lockObj)
            {
                var record = records.FirstOrDefault(r => r.IMEI == imei && !r.IsSynced);
                if (record != null)
                {
                    record.IsSynced = true;
                    record.SyncedAt = DateTime.Now;
                    SaveCache();
                }
            }
        }

        /// <summary>
        /// 모든 레코드 동기화 상태 업데이트
        /// </summary>
        public void MarkAllAsSynced()
        {
            lock (lockObj)
            {
                foreach (var record in records.Where(r => !r.IsSynced))
                {
                    record.IsSynced = true;
                    record.SyncedAt = DateTime.Now;
                }
                SaveCache();
            }
        }

        /// <summary>
        /// 캐시 파일에서 로드
        /// </summary>
        private void LoadCache()
        {
            try
            {
                if (File.Exists(cacheFilePath))
                {
                    string json = File.ReadAllText(cacheFilePath);
                    records = JsonConvert.DeserializeObject<List<QARecord>>(json) ?? new List<QARecord>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocalCache LoadCache error: {ex.Message}");
                records = new List<QARecord>();
            }
        }

        /// <summary>
        /// 캐시 파일로 저장
        /// </summary>
        private void SaveCache()
        {
            try
            {
                string json = JsonConvert.SerializeObject(records, Formatting.Indented);
                File.WriteAllText(cacheFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocalCache SaveCache error: {ex.Message}");
            }
        }

        /// <summary>
        /// 캐시 비우기
        /// </summary>
        public void ClearCache()
        {
            lock (lockObj)
            {
                records.Clear();
                SaveCache();
            }
        }

        /// <summary>
        /// 동기화된 오래된 레코드 정리 (기본 30일)
        /// </summary>
        public void CleanupOldRecords(int daysToKeep = 30)
        {
            lock (lockObj)
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                records.RemoveAll(r => r.IsSynced && r.SyncedAt.HasValue && r.SyncedAt.Value < cutoffDate);
                SaveCache();
            }
        }

        /// <summary>
        /// CSV로 내보내기
        /// </summary>
        public void ExportToCsv(string filePath)
        {
            lock (lockObj)
            {
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("IMEI,IccID,PassFlag,FlagString,BleID,QAStage,Timestamp,IsSynced,SyncedAt");
                    foreach (var record in records)
                    {
                        writer.WriteLine($"{record.IMEI},{record.IccID},{record.PassFlag},{record.FlagString},{record.BleID},{record.QAStage},{record.Timestamp:yyyy-MM-dd HH:mm:ss},{record.IsSynced},{record.SyncedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}");
                    }
                }
            }
        }

        /// <summary>
        /// 캐시된 레코드 수
        /// </summary>
        public int Count => records.Count;

        /// <summary>
        /// 동기화되지 않은 레코드 수
        /// </summary>
        public int UnsyncedCount => records.Count(r => !r.IsSynced);
    }

    /// <summary>
    /// QA 레코드 데이터 모델
    /// </summary>
    public class QARecord
    {
        public string IMEI { get; set; }
        public string IccID { get; set; }
        public string PassFlag { get; set; }
        public string FlagString { get; set; }
        public string BleID { get; set; }
        public string QAStage { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
    }
}
