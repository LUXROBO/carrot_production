using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace IniFileManager
{
    /// <summary>
    /// INI 파일을 읽고 쓰기 위한 관리 클래스
    /// </summary>
    public class IniFile
    {
        private readonly string _filePath;
        private readonly Encoding _encoding;
        private Dictionary<string, Dictionary<string, string>> _iniData;

        /// <summary>
        /// INI 파일 경로
        /// </summary>
        public string FilePath => _filePath;

        /// <summary>
        /// 파일이 존재하는지 여부
        /// </summary>
        public bool FileExists => File.Exists(_filePath);

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="filePath">INI 파일 경로</param>
        /// <param name="encoding">파일 인코딩 (기본값: UTF-8)</param>
        public IniFile(string filePath, Encoding encoding = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _encoding = encoding ?? Encoding.UTF8;
            _iniData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            LoadFromFile();
        }

        #region Read Methods

        /// <summary>
        /// 특정 섹션의 키 값을 읽기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="defaultValue">기본값</param>
        /// <returns>읽은 값 또는 기본값</returns>
        public string ReadValue(string section, string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                return defaultValue;

            if (_iniData.ContainsKey(section) && _iniData[section].ContainsKey(key))
                return _iniData[section][key];

            return defaultValue;
        }

        /// <summary>
        /// 정수 값 읽기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="defaultValue">기본값</param>
        /// <returns>정수 값</returns>
        public int ReadInt(string section, string key, int defaultValue = 0)
        {
            var value = ReadValue(section, key);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// 부울 값 읽기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="defaultValue">기본값</param>
        /// <returns>부울 값</returns>
        public bool ReadBool(string section, string key, bool defaultValue = false)
        {
            var value = ReadValue(section, key).ToLower();

            if (value == "true" || value == "1" || value == "yes" || value == "on")
                return true;
            if (value == "false" || value == "0" || value == "no" || value == "off")
                return false;

            return defaultValue;
        }

        /// <summary>
        /// 실수 값 읽기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="defaultValue">기본값</param>
        /// <returns>실수 값</returns>
        public double ReadDouble(string section, string key, double defaultValue = 0.0)
        {
            var value = ReadValue(section, key);
            return double.TryParse(value, out double result) ? result : defaultValue;
        }

        /// <summary>
        /// 전체 섹션 읽기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <returns>섹션의 모든 키-값 쌍</returns>
        public Dictionary<string, string> ReadSection(string section)
        {
            if (string.IsNullOrEmpty(section) || !_iniData.ContainsKey(section))
                return new Dictionary<string, string>();

            return new Dictionary<string, string>(_iniData[section]);
        }

        /// <summary>
        /// 모든 섹션명 가져오기
        /// </summary>
        /// <returns>섹션명 목록</returns>
        public List<string> GetSectionNames()
        {
            return _iniData.Keys.ToList();
        }

        /// <summary>
        /// 특정 섹션의 모든 키명 가져오기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <returns>키명 목록</returns>
        public List<string> GetKeyNames(string section)
        {
            if (string.IsNullOrEmpty(section) || !_iniData.ContainsKey(section))
                return new List<string>();

            return _iniData[section].Keys.ToList();
        }

        // 구분자를 사용한 리스트 읽기
        public List<string> ReadList(string section, string key, char separator = ',')
        {
            string value = ReadValue(section, key);
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            return value.Split(separator)
                       .Select(item => item.Trim())
                       .Where(item => !string.IsNullOrEmpty(item))
                       .ToList();
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// 값 쓰기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="value">값</param>
        public void WriteValue(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                return;

            if (!_iniData.ContainsKey(section))
                _iniData[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            _iniData[section][key] = value ?? "";
        }

        /// <summary>
        /// 정수 값 쓰기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="value">정수 값</param>
        public void WriteInt(string section, string key, int value)
        {
            WriteValue(section, key, value.ToString());
        }

        /// <summary>
        /// 부울 값 쓰기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="value">부울 값</param>
        public void WriteBool(string section, string key, bool value)
        {
            WriteValue(section, key, value.ToString().ToLower());
        }

        /// <summary>
        /// 실수 값 쓰기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <param name="value">실수 값</param>
        public void WriteDouble(string section, string key, double value)
        {
            WriteValue(section, key, value.ToString("F2"));
        }

        /// <summary>
        /// 전체 섹션 쓰기
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="keyValues">키-값 딕셔너리</param>
        public void WriteSection(string section, Dictionary<string, string> keyValues)
        {
            if (string.IsNullOrEmpty(section) || keyValues == null)
                return;

            _iniData[section] = new Dictionary<string, string>(keyValues, StringComparer.OrdinalIgnoreCase);
        }


        // 구분자 방식으로 리스트 쓰기
        public void WriteListWithSeparator(string section, string key, List<string> values, char separator = ',')
        {
            string value = values?.Count > 0 ? string.Join(separator.ToString(), values) : "";
            WriteValue(section, key, value);
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// 키 삭제
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <returns>삭제 성공 여부</returns>
        public bool DeleteKey(string section, string key)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                return false;

            if (_iniData.ContainsKey(section) && _iniData[section].ContainsKey(key))
            {
                _iniData[section].Remove(key);

                // 섹션이 비어있으면 섹션도 삭제
                if (_iniData[section].Count == 0)
                    _iniData.Remove(section);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 섹션 삭제
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <returns>삭제 성공 여부</returns>
        public bool DeleteSection(string section)
        {
            if (string.IsNullOrEmpty(section))
                return false;

            return _iniData.Remove(section);
        }

        /// <summary>
        /// 모든 데이터 삭제
        /// </summary>
        public void Clear()
        {
            _iniData.Clear();
        }

        #endregion

        #region File Operations

        /// <summary>
        /// 파일에서 데이터 로드
        /// </summary>
        /// <returns>로드 성공 여부</returns>
        public bool LoadFromFile()
        {
            try
            {
                _iniData.Clear();

                if (!File.Exists(_filePath))
                    return false;

                var lines = File.ReadAllLines(_filePath, _encoding);
                string currentSection = "";

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // 빈 줄이나 주석 건너뛰기
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") ||
                        trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
                        continue;

                    // 섹션 헤더 처리
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                        if (!_iniData.ContainsKey(currentSection))
                            _iniData[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        continue;
                    }

                    // 키=값 쌍 처리
                    var equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex > 0 && !string.IsNullOrEmpty(currentSection))
                    {
                        var key = trimmedLine.Substring(0, equalIndex).Trim();
                        var value = trimmedLine.Substring(equalIndex + 1).Trim();

                        // 따옴표 제거 (선택적)
                        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                            (value.StartsWith("'") && value.EndsWith("'")))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }

                        _iniData[currentSection][key] = value;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INI 파일 로드 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일에 데이터 저장
        /// </summary>
        /// <returns>저장 성공 여부</returns>
        public bool SaveToFile()
        {
            try
            {
                var sb = new StringBuilder();

                // 헤더 추가
                sb.AppendLine($"; INI 파일");
                sb.AppendLine($"; 생성일시: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"; 파일경로: {_filePath}");
                sb.AppendLine();

                // 섹션별로 데이터 저장
                foreach (var section in _iniData)
                {
                    sb.AppendLine($"[{section.Key}]");

                    foreach (var keyValue in section.Value)
                    {
                        sb.AppendLine($"{keyValue.Key}={keyValue.Value}");
                    }

                    sb.AppendLine();
                }

                // 디렉토리 생성 (없는 경우)
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(_filePath, sb.ToString(), _encoding);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INI 파일 저장 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 백업 파일 생성
        /// </summary>
        /// <param name="backupPath">백업 파일 경로 (null이면 자동 생성)</param>
        /// <returns>백업 성공 여부</returns>
        public bool CreateBackup(string backupPath = null)
        {
            try
            {
                if (!File.Exists(_filePath))
                    return false;

                if (string.IsNullOrEmpty(backupPath))
                {
                    var extension = Path.GetExtension(_filePath);
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(_filePath);
                    var directory = Path.GetDirectoryName(_filePath);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    backupPath = Path.Combine(directory ?? "", $"{nameWithoutExt}_backup_{timestamp}{extension}");
                }

                File.Copy(_filePath, backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"백업 파일 생성 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일 새로 고침 (파일에서 다시 로드)
        /// </summary>
        /// <returns>새로 고침 성공 여부</returns>
        public bool Refresh()
        {
            return LoadFromFile();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 키가 존재하는지 확인
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <param name="key">키명</param>
        /// <returns>존재 여부</returns>
        public bool KeyExists(string section, string key)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                return false;

            return _iniData.ContainsKey(section) && _iniData[section].ContainsKey(key);
        }

        /// <summary>
        /// 섹션이 존재하는지 확인
        /// </summary>
        /// <param name="section">섹션명</param>
        /// <returns>존재 여부</returns>
        public bool SectionExists(string section)
        {
            if (string.IsNullOrEmpty(section))
                return false;

            return _iniData.ContainsKey(section);
        }

        /// <summary>
        /// 전체 INI 데이터를 딕셔너리로 가져오기
        /// </summary>
        /// <returns>전체 데이터</returns>
        public Dictionary<string, Dictionary<string, string>> GetAllData()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var section in _iniData)
            {
                result[section.Key] = new Dictionary<string, string>(section.Value);
            }

            return result;
        }

        /// <summary>
        /// INI 파일 내용을 문자열로 가져오기
        /// </summary>
        /// <returns>INI 파일 내용</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var section in _iniData)
            {
                sb.AppendLine($"[{section.Key}]");

                foreach (var keyValue in section.Value)
                {
                    sb.AppendLine($"{keyValue.Key}={keyValue.Value}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion
    }


    // 실제 애플리케이션에서 사용할 수 있는 설정 관리 클래스
    public class ApplicationSettings
    {
        private readonly IniFile _ini;
        private static ApplicationSettings _instance = null;
        private static readonly object _lockObject = new object();

        // Default 값은 private static readonly로 선언
        private static readonly string DefaultDatabaseServer = "115.68.195.106";
        private static readonly int DefaultDatabasePort = 3306;
        private static readonly string DefaultDatabaseName = "carrotpluglist";
        private static readonly string DefaultDatabaseUser = "luxrobo";
        private static readonly string DefaultDatabasePassword = "fjrtmfhqh123$";
        private static readonly bool DefaultVPNEnable = true;
        private static readonly string DefaultVPNServer = "";
        private static readonly bool DefaultEnableLogging = false;

        private ApplicationSettings(string configFile)
        {
            _ini = new IniFile(configFile);
            LoadDefaultSettings();
        }

        /// <summary>
        /// ApplicationSettings 인스턴스 가져오기 (파일명 지정)
        /// </summary>
        /// <param name="configFile">설정 파일 경로</param>
        /// <returns>ApplicationSettings 인스턴스</returns>
        public static ApplicationSettings Instance(string configFile = "config.ini")
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ApplicationSettings(configFile);
                    }
                }
            }
            return _instance;
        }


        // 데이터베이스 설정
        public string DatabaseServer
        {
            get => _ini.ReadValue("DB", "Server", DefaultDatabaseServer);
            set => _ini.WriteValue("DB", "Server", value);
        }

        public int DatabasePort
        {
            get => _ini.ReadInt("DB", "Port", DefaultDatabasePort);    // MySQL PORT
            set => _ini.WriteInt("DB", "Port", value);
        }

        public string DatabaseName
        {
            get => _ini.ReadValue("DB", "Database", DefaultDatabaseName);
            set => _ini.WriteValue("DB", "Database", value);
        }

        public string DatabaseUser
        {
            get => _ini.ReadValue("DB", "User", DefaultDatabaseUser);
            set => _ini.WriteValue("DB", "User", value);
        }

        public string DatabasePassword
        {
            get => _ini.ReadValue("DB", "Password", DefaultDatabasePassword);
            set => _ini.WriteValue("DB", "Password", value);
        }

        // 네트워크 설정
        public bool VPNEnable
        {
            get => _ini.ReadBool("Network", "VPNEnable", DefaultVPNEnable);
            set => _ini.WriteBool("Network", "VPNEnable", value);
        }

        public string VPNServer
        {
            get => _ini.ReadValue("Network", "VPNServer", DefaultVPNServer);
            set => _ini.WriteValue("Network", "VPNServer", value);
        }

        public bool EnableLogging
        {
            get => _ini.ReadBool("Application", "EnableLogging", DefaultEnableLogging);
            set => _ini.WriteBool("Application", "EnableLogging", value);
        }

        /* // 애플리케이션 설정
        public string LogLevel
        {
            get => _ini.ReadValue("Application", "LogLevel", "Info");
            set => _ini.WriteValue("Application", "LogLevel", value);
        }*/

        private void LoadDefaultSettings()
        {
            if (!_ini.FileExists)
            {
                this.DatabaseServer = DefaultDatabaseServer;
                this.DatabasePort = DefaultDatabasePort;
                this.DatabaseName = DefaultDatabaseName;
                this.DatabaseUser = DefaultDatabaseUser;
                this.DatabasePassword = DefaultDatabasePassword;
                this.VPNEnable = DefaultVPNEnable;
                this.VPNServer = DefaultVPNServer;
                this.EnableLogging = DefaultEnableLogging;

                // 기본값으로 초기화
                SaveSettings();
            }
        }

        public bool SaveSettings()
        {
            return _ini.SaveToFile();
        }

        public bool LoadSettings()
        {
            return _ini.LoadFromFile();
        }

        public bool CreateBackup()
        {
            return _ini.CreateBackup();
        }

        public void DisplaySettings()
        {
            Console.WriteLine("=== 현재 설정 ===");
            Console.WriteLine($"데이터베이스 서버: {DatabaseServer}:{DatabasePort}");
            Console.WriteLine($"데이터베이스명: {DatabaseName}");
            Console.WriteLine($"사용자: {DatabaseUser}");
            Console.WriteLine($"VPNEnable: {VPNEnable}");
            Console.WriteLine($"VPNServer: {VPNServer}");
            Console.WriteLine($"로깅: {EnableLogging}");
        }
    }
}