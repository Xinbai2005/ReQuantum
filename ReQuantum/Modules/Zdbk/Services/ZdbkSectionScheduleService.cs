using Microsoft.Extensions.Logging;
using ReQuantum.Attributes;
using ReQuantum.Infrastructure.Abstractions;
using ReQuantum.Infrastructure.Models;
using ReQuantum.Infrastructure.Services;
using ReQuantum.Modules.Calendar.Entities;
using ReQuantum.Modules.Zdbk.Models;
using ReQuantum.Modules.ZjuSso.Services;
using ReQuantum.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ReQuantum.Modules.Zdbk.Services;

public interface IZdbkSectionScheduleService
{
    /// <summary>
    /// 获取课程表原始数据
    /// </summary>
    /// <param name="academicYear">学年，如 "2025-2026"</param>
    /// <param name="semester">学期，如 "秋"、"冬"、"春"、"夏"</param>
    Task<Result<ZdbkSectionScheduleResponse>> GetCourseScheduleAsync(string academicYear, string semester);

    /// <summary>
    /// 获取当前学期的课程表
    /// </summary>
    Task<Result<ZdbkSectionScheduleResponse>> GetCurrentSemesterScheduleAsync();
}

[AutoInject(Lifetime.Singleton)]
public class ZdbkSectionScheduleService : IZdbkSectionScheduleService, IDaemonService
{
    private readonly IZjuSsoService _zjuSsoService;
    private readonly IStorage _storage;
    private readonly ILogger<ZdbkSectionScheduleService> _logger;
    private ZdbkState? _state;

    private const string StateKey = "Zdbk:State";
    private const string BaseUrl = "https://zdbk.zju.edu.cn";
    private const string SsoLoginUrl = "https://zjuam.zju.edu.cn/cas/login";
    private const string SsoRedirectUrl = "/jwglxt/xtgl/login_ssologin.html";
    private const string CourseScheduleApiBase = "https://zdbk.zju.edu.cn/jwglxt/kbcx/xskbcx_cxXsKb.html";

    public ZdbkSectionScheduleService(
        IZjuSsoService zjuSsoService,
        IStorage storage,
        ILogger<ZdbkSectionScheduleService> logger)
    {
        _zjuSsoService = zjuSsoService;
        _storage = storage;
        _logger = logger;
        _zjuSsoService.OnLogout += () => _state = null;
        LoadState();
    }

    public async Task<Result<ZdbkSectionScheduleResponse>> GetCurrentSemesterScheduleAsync()
    {
        var now = DateTime.Now;
        var (academicYear, semester) = GetCurrentSemester(now);

        _logger.LogInformation("Fetching current semester schedule: {AcademicYear} {Semester}", academicYear, semester);

        return await GetCourseScheduleAsync(academicYear, semester);
    }

    public async Task<Result<ZdbkSectionScheduleResponse>> GetCourseScheduleAsync(string academicYear, string semester)
    {
        var clientResult = await GetAuthenticatedClient();
        if (!clientResult.IsSuccess)
        {
            return Result.Fail(clientResult.Message);
        }

        var client = clientResult.Value;

        if (!_zjuSsoService.IsAuthenticated || string.IsNullOrEmpty(_zjuSsoService.Id))
        {
            return Result.Fail("未获取到学号信息");
        }

        var studentId = _zjuSsoService.Id;

        try
        {
            var semesterCode = MapSemesterToCode(semester);
            var apiUrl = $"{CourseScheduleApiBase}?gnmkdm=N253508&su={studentId}";

            var formData = new Dictionary<string, string>
            {
                { "xnm", academicYear },
                { "xqm", $"{semesterCode}|{semester}" },
            };

            var content = new FormUrlEncodedContent(formData);

            content.Headers.ContentType?.CharSet = "utf-8";

            _logger.LogDebug(
                "Requesting course schedule with URL: {Url}, FormData: xnm={Xnm}, xqm={Xqm}, xqmmc={Xqmmc}",
                apiUrl, academicYear, $"{semesterCode}|{semester}", semester);

            var response = await client.PostAsync(apiUrl, content);
            var str = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _state = null;
                _logger.LogWarning("Failed to fetch course schedule: {StatusCode}", response.StatusCode);
                return Result.Fail($"获取课程表失败: {response.StatusCode}");
            }

            var scheduleResponse = await response.Content.ReadFromJsonAsync(SourceGenerationContext.Default.ZdbkSectionScheduleResponse);

            if (scheduleResponse is null)
            {
                _logger.LogError("Failed to parse course schedule response");
                return Result.Fail("解析课程表数据失败");
            }

            _logger.LogInformation("Successfully fetched course schedule with {Count} sections",
                scheduleResponse.SectionList.Count);

            return Result.Success(scheduleResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when getting course schedule from zdbk.zju.edu.cn");
            return Result.Fail($"获取课程表失败：{ex.Message}");
        }
    }

    private async Task<Result<RequestClient>> GetAuthenticatedClient()
    {
        // 如果已有缓存的 Cookie，直接使用
        if (_state is not null)
        {
            _logger.LogDebug("Using cached session cookie");
            return Result.Success(RequestClient.Create(new RequestOptions
            {
                Cookies = [_state.SessionCookie, _state.RouteCookie]
            }));
        }

        // 通过 SSO 认证获取新 Cookie
        var clientResult = await _zjuSsoService.GetAuthenticatedClientAsync(new RequestOptions { AllowRedirects = true });

        if (!clientResult.IsSuccess)
        {
            _logger.LogWarning("Failed to get authenticated client: {Message}", clientResult.Message);
            return Result.Fail(clientResult.Message);
        }

        var client = clientResult.Value;

        try
        {
            var ssoUrl = $"{SsoLoginUrl}?service={Uri.EscapeDataString($"{BaseUrl}{SsoRedirectUrl}")}";
            _logger.LogDebug("Accessing SSO login URL: {SsoUrl}", ssoUrl);

            await client.GetAsync(ssoUrl);
            var allCookies = client.CookieContainer.GetAllCookies();
            var sessionCookie = allCookies.Last(ck => ck is { Name: "JSESSIONID", Domain: "zdbk.zju.edu.cn" });
            var route = allCookies.Last(ck => ck is { Name: "route" });

            _state = new ZdbkState(sessionCookie, route);
            SaveState();

            _logger.LogInformation("Successfully obtained new session cookie via SSO redirect");

            return Result.Success(RequestClient.Create(new RequestOptions()
            {
                Cookies = [sessionCookie, route]
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during SSO authentication");
            return Result.Fail($"SSO 认证失败：{ex.Message}");
        }
    }

    private static string MapSemesterToCode(string semester)
    {
        return semester switch
        {
            "秋" => "1",
            "冬" => "1",
            "春" => "2",
            "夏" => "2",
            _ => throw new ArgumentOutOfRangeException(nameof(semester), semester, null)
        };
    }

    private static (string AcademicYear, string Semester) GetCurrentSemester(DateTime now)
    {
        int year = now.Year;
        int month = now.Month;

        return month switch
        {
            >= 9 and <= 11 => ($"{year}-{year + 1}", "秋"),
            12 => ($"{year}-{year + 1}", "冬"),
            1 => ($"{year - 1}-{year}", "冬"),
            >= 2 and <= 5 => ($"{year - 1}-{year}", "春"),
            _ => ($"{year - 1}-{year}", "夏")
        };
    }

    private void LoadState()
    {
        _storage.TryGetWithEncryption(StateKey, out _state);
    }

    private void SaveState()
    {
        if (_state is null)
        {
            _storage.Remove(StateKey);
            return;
        }

        _storage.SetWithEncryption(StateKey, _state);
    }
}

public static class ZdbkSectionExtensions
{
    /// <param name="section">课程信息</param>
    extension(ZdbkSectionDto section)
    {
        /// <summary>
        /// 将课程表中的课程转换为日历事件列表
        /// </summary>
        /// <param name="semesterStartDate">学期起始日期</param>
        /// <returns>日历事件列表</returns>
        public List<CalendarEvent> ToCalendarEvents(DateOnly semesterStartDate)
        {
            var (courseInfo, startTime, endTime) = section.Parse();
            var events = new List<CalendarEvent>();

            for (var weekNumber = courseInfo.WeekStart; weekNumber <= courseInfo.WeekEnd; weekNumber++)
            {
                if (!ShouldHaveCourseInWeek(weekNumber, int.Parse(section.WeekType)))
                {
                    continue;
                }

                var courseDate = CalculateCourseDate(semesterStartDate, weekNumber, int.Parse(section.DayOfWeek));
                var eventId = $"{section.CourseId}_{weekNumber}_{section.DayOfWeek}_{section.StartSection}".ToGuid();

                var startDateTime = courseDate.ToDateTime(startTime);
                var endDateTime = courseDate.ToDateTime(endTime);

                events.Add(new CalendarEvent
                {
                    Id = eventId,
                    Content = $"{courseInfo.CourseName}\n{courseInfo.Teacher}\n{courseInfo.Location}",
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    CreatedAt = DateTime.Now
                });
            }

            return events;
        }
    }

    /// <summary>
    /// 转换课程表为日历事件
    /// </summary>
    public static List<CalendarEvent> ToCalendarEvents(this IEnumerable<ZdbkSectionDto> sections, DateOnly semesterStartDate)
    {
        var events = new List<CalendarEvent>();
        foreach (var section in sections)
        {
            events.AddRange(section.ToCalendarEvents(semesterStartDate));
        }
        return events;
    }

    private static bool ShouldHaveCourseInWeek(int weekNumber, int weekType)
    {
        return weekType switch
        {
            0 => true,
            1 => weekNumber % 2 == 0,
            2 => weekNumber % 2 == 1,
            _ => false
        };
    }

    private static DateOnly CalculateCourseDate(DateOnly semesterStartDate, int weekNumber, int dayOfWeek)
    {
        var daysToAdd = (weekNumber - 1) * 7 + (dayOfWeek - 1);
        return semesterStartDate.AddDays(daysToAdd);
    }
}

public static class CalendarEventExtensions
{
    private const string Zdbk = "Zdbk";

    extension(CalendarEvent evt)
    {
        public bool IsFromZdbk
        {
            get => evt.From == Zdbk;
            set
            {
                if (evt.From == Zdbk && !value)
                {
                    evt.From = string.Empty;
                }
                else if (evt.From != Zdbk && value)
                {
                    evt.From = Zdbk;
                }
            }
        }
    }
}
