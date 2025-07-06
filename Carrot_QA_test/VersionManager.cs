using System;
using System.IO;
using System.Reflection;


// 2. 버전 정보를 관리하는 유틸리티 클래스
public static class VersionManager
{
    private static Assembly _assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// 애플리케이션 버전 (예: 1.2.3)
    /// </summary>
    public static string Version
    {
        get
        {
            var version = _assembly.GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    /// <summary>
    /// 전체 버전 정보 (예: 1.2.3.0)
    /// </summary>
    public static string FullVersion
    {
        get
        {
            return _assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        }
    }

    /// <summary>
    /// 파일 버전
    /// </summary>
    public static string FileVersion
    {
        get
        {
            var fileVersionAttribute = _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            return fileVersionAttribute?.Version ?? "0.0.0.0";
        }
    }

    /// <summary>
    /// 제품명
    /// </summary>
    public static string ProductName
    {
        get
        {
            var productAttribute = _assembly.GetCustomAttribute<AssemblyProductAttribute>();
            return productAttribute?.Product ?? "Unknown Product";
        }
    }

    /// <summary>
    /// 회사명
    /// </summary>
    public static string CompanyName
    {
        get
        {
            var companyAttribute = _assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            return companyAttribute?.Company ?? "Unknown Company";
        }
    }

    /// <summary>
    /// 설명
    /// </summary>
    public static string Description
    {
        get
        {
            var descriptionAttribute = _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            return descriptionAttribute?.Description ?? "No Description";
        }
    }

    /// <summary>
    /// 저작권 정보
    /// </summary>
    public static string Copyright
    {
        get
        {
            var copyrightAttribute = _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            return copyrightAttribute?.Copyright ?? "No Copyright";
        }
    }

    /// <summary>
    /// 빌드 날짜 (어셈블리가 컴파일된 날짜)
    /// </summary>
    public static DateTime BuildDate
    {
        get
        {
            var location = _assembly.Location;
            if (string.IsNullOrEmpty(location))
                return DateTime.MinValue;

            return new FileInfo(location).CreationTime;
        }
    }

    /// <summary>
    /// 전체 버전 정보를 문자열로 반환
    /// </summary>
    public static string GetFullVersionInfo()
    {
        return $"{ProductName} v{Version}\n" +
               $"파일 버전: {FileVersion}\n" +
               $"빌드 날짜: {BuildDate:yyyy-MM-dd HH:mm:ss}\n" +
               $"{Copyright}";
    }
}