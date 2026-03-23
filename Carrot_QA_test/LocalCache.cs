using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using IniFileManager;

namespace Carrot_QA_test
{
    #region Data Models

    /// <summary>
    /// 디바이스 데이터 모델
    /// </summary>
    public class DeviceData
    {
        public string Imei { get; set; }
        public string IccId { get; set; }
        public string BleId { get; set; }
        public string Mac { get; set; }
        public string Version { get; set; }
        public uint VersionNumber { get; set; }
        public int Rssi { get; set; }
        public QaResultData QaResult { get; set; }
        public SensorDataBlock SensorData { get; set; }
        public GoldReferenceData GoldReference { get; set; }
        public TimestampData Timestamp { get; set; }
        public SyncStatusData SyncStatus { get; set; }
    }

    /// <summary>
    /// QA 결과 데이터
    /// </summary>
    public class QaResultData
    {
        public string Qa1 { get; set; }
        public string Qa2 { get; set; }
        public string Qa3 { get; set; }
        public string Ng1Type { get; set; }
        public string Ng2Type { get; set; }
        public string Ng3Type { get; set; }
    }

    /// <summary>
    /// 센서 데이터 블록
    /// </summary>
    public class SensorDataBlock
    {
        public int GpsSnr { get; set; }
        public int Temperature { get; set; }
        public int Capacitance { get; set; }
        public int BleRssi { get; set; }
        public LteBandData LteB3 { get; set; }
        public LteBandData LteB5 { get; set; }
    }

    /// <summary>
    /// LTE 밴드 데이터
    /// </summary>
    public class LteBandData
    {
        public int Min { get; set; }
        public int Avg { get; set; }
        public int Max { get; set; }
    }

    /// <summary>
    /// Gold Reference 데이터
    /// </summary>
    public class GoldReferenceData
    {
        public int Gps { get; set; }
        public int GpsMargin { get; set; }
        public int Temp { get; set; }
        public int TempMargin { get; set; }
        public int CapMin { get; set; }
        public int CapMax { get; set; }
        public int B3 { get; set; }
        public int B3Margin { get; set; }
        public int B5 { get; set; }
        public int B5Margin { get; set; }
    }

    /// <summary>
    /// 타임스탬프 데이터
    /// </summary>
    public class TimestampData
    {
        public DateTime FirstSeen { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// 동기화 상태 데이터
    /// </summary>
    public class SyncStatusData
    {
        public bool Synced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public int SyncAttempts { get; set; }
        public string LastSyncError { get; set; }
    }

    /// <summary>
    /// 세션 데이터
    /// </summary>
    public class SessionData
    {
        public string SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string OperationMode { get; set; }
        public SessionStatistics Statistics { get; set; }
        public List<string> Devices { get; set; }
        public List<FailedDeviceInfo> FailedDevices { get; set; }
    }

    /// <summary>
    /// 세션 통계
    /// </summary>
    public class SessionStatistics
    {
        public int TotalScanned { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public double PassRate => TotalScanned > 0 ? (double)Passed / TotalScanned * 100 : 0;
    }

    /// <summary>
    /// 실패 디바이스 정보
    /// </summary>
    public class FailedDeviceInfo
    {
        public string Imei { get; set; }
        public string FailureType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 인덱스 파일 데이터
    /// </summary>
    public class IndexData
    {
        public string Version { get; set; } = "1.0";
        public DateTime LastUpdated { get; set; }
        public int TotalDevices { get; set; }
        public List<SessionSummary> Sessions { get; set; } = new List<SessionSummary>();
        public int PendingSync { get; set; }
    }

    /// <summary>
    /// 세션 요약 정보
    /// </summary>
    public class SessionSummary
    {
        public string SessionId { get; set; }
        public int DeviceCount { get; set; }
        public int PassCount { get; set; }
    }

    #endregion

    /// <summary>
    /// 로컬 캐시 관리 클래스 (Standalone 모드용)
    /// </summary>
    public class LocalCache
    {
        private readonly string cacheDir;
        private readonly ApplicationSettings settings;
        private readonly string devicesDir;
        private readonly string sessionsDir;
        private readonly string exportsDir;
        private readonly string indexPath;

        public LocalCache()
        {
            settings = ApplicationSettings.Instance();
            cacheDir = Path.GetFullPath(settings.CacheDirectory);
            devicesDir = Path.Combine(cacheDir, "devices");
            sessionsDir = Path.Combine(cacheDir, "sessions");
            exportsDir = Path.Combine(cacheDir, "exports");
            indexPath = Path.Combine(cacheDir, "index.json");

            // 디렉토리 생성
            Directory.CreateDirectory(devicesDir);
            Directory.CreateDirectory(sessionsDir);
            Directory.CreateDirectory(exportsDir);

            Trace.WriteLine($"LocalCache initialized: {cacheDir}");
        }

        /// <summary>
        /// 태그 정보를 로컬 캐시에 저장
        /// </summary>
        public void SaveTag(Taginfo tag)
        {
            if (!settings.EnableLocalCache) return;

            try
            {
                var deviceData = ConvertToDeviceData(tag);
                string filename = Path.Combine(devicesDir, $"{tag.TagIMEI}.json");

                // 기존 파일이 있으면 FirstSeen 유지
                if (File.Exists(filename))
                {
                    var existing = LoadDeviceData(tag.TagIMEI);
                    if (existing != null)
                    {
                        deviceData.Timestamp.FirstSeen = existing.Timestamp.FirstSeen;
                    }
                }
                else
                {
                    deviceData.Timestamp.FirstSeen = DateTime.UtcNow;
                }

                deviceData.Timestamp.LastUpdate = DateTime.UtcNow;

                string json = JsonConvert.SerializeObject(deviceData, Formatting.Indented);
                File.WriteAllText(filename, json, Encoding.UTF8);

                UpdateIndex();
                Trace.WriteLine($"LocalCache saved: {tag.TagIMEI}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LocalCache save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 캐시에서 디바이스 데이터 로드
        /// </summary>
        public DeviceData LoadDeviceData(string imei)
        {
            string filename = Path.Combine(devicesDir, $"{imei}.json");
            if (!File.Exists(filename)) return null;

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);
                return JsonConvert.DeserializeObject<DeviceData>(json);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LocalCache load failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 캐시에서 태그 정보 로드
        /// </summary>
        public Taginfo LoadTag(string imei)
        {
            var deviceData = LoadDeviceData(imei);
            if (deviceData == null) return null;

            return ConvertToTaginfo(deviceData);
        }

        /// <summary>
        /// 모든 캐시 데이터를 CSV로 내보내기
        /// </summary>
        public string ExportToCSV()
        {
            string filename = $"QA_Result_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filepath = Path.Combine(exportsDir, filename);

            var sb = new StringBuilder();
            sb.AppendLine("IMEI,ICC_ID,BLE_ID,VERSION,QA1,QA2,QA3,NG_TYPE,GPS_SNR,TEMP,CAP,BLE_RSSI,LTE_B3_AVG,LTE_B5_AVG,TIMESTAMP");

            var files = Directory.GetFiles(devicesDir, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file, Encoding.UTF8);
                    var device = JsonConvert.DeserializeObject<DeviceData>(json);

                    string ngType = string.Join("|", new[] {
                        device.QaResult?.Ng1Type,
                        device.QaResult?.Ng2Type,
                        device.QaResult?.Ng3Type
                    }.Where(s => !string.IsNullOrEmpty(s)));

                    sb.AppendLine($"{device.Imei}," +
                                 $"{device.IccId}," +
                                 $"{device.BleId}," +
                                 $"{device.Version}," +
                                 $"{device.QaResult?.Qa1}," +
                                 $"{device.QaResult?.Qa2}," +
                                 $"{device.QaResult?.Qa3}," +
                                 $"{ngType}," +
                                 $"{device.SensorData?.GpsSnr}," +
                                 $"{device.SensorData?.Temperature}," +
                                 $"{device.SensorData?.Capacitance}," +
                                 $"{device.SensorData?.BleRssi}," +
                                 $"{device.SensorData?.LteB3?.Avg}," +
                                 $"{device.SensorData?.LteB5?.Avg}," +
                                 $"{device.Timestamp?.LastUpdate:s}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Export error for {file}: {ex.Message}");
                }
            }

            File.WriteAllText(filepath, sb.ToString(), Encoding.UTF8);
            Trace.WriteLine($"CSV exported: {filepath}");
            return filepath;
        }

        /// <summary>
        /// 세션 정보 저장
        /// </summary>
        public void SaveSession(SessionData session)
        {
            try
            {
                string filename = Path.Combine(sessionsDir, $"{session.SessionId}.json");
                string json = JsonConvert.SerializeObject(session, Formatting.Indented);
                File.WriteAllText(filename, json, Encoding.UTF8);
                Trace.WriteLine($"Session saved: {session.SessionId}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Session save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 인덱스 파일 업데이트
        /// </summary>
        private void UpdateIndex()
        {
            try
            {
                var deviceFiles = Directory.GetFiles(devicesDir, "*.json");
                var sessionFiles = Directory.GetFiles(sessionsDir, "*.json");

                int pendingSync = 0;
                foreach (var file in deviceFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file, Encoding.UTF8);
                        var device = JsonConvert.DeserializeObject<DeviceData>(json);
                        if (device.SyncStatus != null && !device.SyncStatus.Synced)
                        {
                            pendingSync++;
                        }
                    }
                    catch { }
                }

                var indexData = new IndexData
                {
                    LastUpdated = DateTime.UtcNow,
                    TotalDevices = deviceFiles.Length,
                    PendingSync = pendingSync
                };

                foreach (var file in sessionFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file, Encoding.UTF8);
                        var session = JsonConvert.DeserializeObject<SessionData>(json);
                        indexData.Sessions.Add(new SessionSummary
                        {
                            SessionId = session.SessionId,
                            DeviceCount = session.Devices?.Count ?? 0,
                            PassCount = session.Statistics?.Passed ?? 0
                        });
                    }
                    catch { }
                }

                string indexJson = JsonConvert.SerializeObject(indexData, Formatting.Indented);
                File.WriteAllText(indexPath, indexJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Index update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 모든 캐시된 디바이스 목록 가져오기
        /// </summary>
        public List<DeviceData> GetAllDevices()
        {
            var devices = new List<DeviceData>();
            var files = Directory.GetFiles(devicesDir, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file, Encoding.UTF8);
                    var device = JsonConvert.DeserializeObject<DeviceData>(json);
                    devices.Add(device);
                }
                catch { }
            }

            return devices;
        }

        /// <summary>
        /// 동기화되지 않은 디바이스 목록 가져오기
        /// </summary>
        public List<DeviceData> GetPendingSyncDevices()
        {
            return GetAllDevices()
                .Where(d => d.SyncStatus == null || !d.SyncStatus.Synced)
                .ToList();
        }

        /// <summary>
        /// Taginfo를 DeviceData로 변환
        /// </summary>
        private DeviceData ConvertToDeviceData(Taginfo tag)
        {
            return new DeviceData
            {
                Imei = tag.TagIMEI,
                IccId = tag.TagIccID,
                BleId = tag.TagBleID,
                Mac = tag.TagMac,
                Version = tag.TagVersion,
                VersionNumber = tag.TagVersionNumber,
                Rssi = tag.TagRssi,
                QaResult = new QaResultData
                {
                    Qa1 = "",
                    Qa2 = tag.passFlag,
                    Qa3 = "",
                    Ng1Type = "",
                    Ng2Type = tag.TagFlagString,
                    Ng3Type = ""
                },
                SensorData = new SensorDataBlock
                {
                    GpsSnr = tag.ng2_gpsSnr,
                    Temperature = tag.ng2_temp,
                    Capacitance = tag.ng2_cap,
                    BleRssi = tag.ng2_ble_rssi,
                    LteB3 = new LteBandData
                    {
                        Min = tag.ng2_b3_min,
                        Avg = tag.ng2_b3_avg,
                        Max = tag.ng2_b3_max
                    },
                    LteB5 = new LteBandData
                    {
                        Min = tag.ng2_b5_min,
                        Avg = tag.ng2_b5_avg,
                        Max = tag.ng2_b5_max
                    }
                },
                GoldReference = new GoldReferenceData
                {
                    Gps = tag.ng2_Gold_GPS,
                    GpsMargin = tag.ng2_Gold_GPS_Margin,
                    Temp = tag.ng2_Gold_Temp,
                    TempMargin = tag.ng2_Gold_Temp_Margin,
                    CapMin = tag.ng2_Gold_Cap_Min,
                    CapMax = tag.ng2_Gold_Cap_Max,
                    B3 = tag.ng2_Gold_b3,
                    B3Margin = tag.ng2_Gold_b3_Margin,
                    B5 = tag.ng2_Gold_b5,
                    B5Margin = tag.ng2_Gold_b5_Margin
                },
                Timestamp = new TimestampData
                {
                    LastUpdate = DateTime.UtcNow
                },
                SyncStatus = new SyncStatusData
                {
                    Synced = false,
                    SyncAttempts = 0
                }
            };
        }

        /// <summary>
        /// DeviceData를 Taginfo로 변환
        /// </summary>
        private Taginfo ConvertToTaginfo(DeviceData device)
        {
            var tag = new Taginfo
            {
                TagMac = device.Mac,
                TagIMEI = device.Imei,
                TagIccID = device.IccId,
                TagBleID = device.BleId,
                TagVersion = device.Version,
                TagVersionNumber = device.VersionNumber,
                TagRssi = (short)device.Rssi
            };

            if (device.QaResult != null)
            {
                tag.passFlag = device.QaResult.Qa2;
                tag.TagFlagString = device.QaResult.Ng2Type;
            }

            if (device.SensorData != null)
            {
                tag.ng2_gpsSnr = device.SensorData.GpsSnr;
                tag.ng2_temp = device.SensorData.Temperature;
                tag.ng2_cap = device.SensorData.Capacitance;
                tag.ng2_ble_rssi = device.SensorData.BleRssi;

                if (device.SensorData.LteB3 != null)
                {
                    tag.ng2_b3_min = device.SensorData.LteB3.Min;
                    tag.ng2_b3_avg = device.SensorData.LteB3.Avg;
                    tag.ng2_b3_max = device.SensorData.LteB3.Max;
                }

                if (device.SensorData.LteB5 != null)
                {
                    tag.ng2_b5_min = device.SensorData.LteB5.Min;
                    tag.ng2_b5_avg = device.SensorData.LteB5.Avg;
                    tag.ng2_b5_max = device.SensorData.LteB5.Max;
                }
            }

            if (device.GoldReference != null)
            {
                tag.ng2_Gold_GPS = device.GoldReference.Gps;
                tag.ng2_Gold_GPS_Margin = device.GoldReference.GpsMargin;
                tag.ng2_Gold_Temp = device.GoldReference.Temp;
                tag.ng2_Gold_Temp_Margin = device.GoldReference.TempMargin;
                tag.ng2_Gold_Cap_Min = device.GoldReference.CapMin;
                tag.ng2_Gold_Cap_Max = device.GoldReference.CapMax;
                tag.ng2_Gold_b3 = device.GoldReference.B3;
                tag.ng2_Gold_b3_Margin = device.GoldReference.B3Margin;
                tag.ng2_Gold_b5 = device.GoldReference.B5;
                tag.ng2_Gold_b5_Margin = device.GoldReference.B5Margin;
            }

            return tag;
        }

        /// <summary>
        /// 캐시 디렉토리 경로 가져오기
        /// </summary>
        public string GetCacheDirectory() => cacheDir;

        /// <summary>
        /// 내보내기 디렉토리 경로 가져오기
        /// </summary>
        public string GetExportsDirectory() => exportsDir;
    }
}
