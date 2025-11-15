using ReQuantum.Modules.Calendar.Entities;
using ReQuantum.Modules.CoursesZju.Models;
using ReQuantum.Modules.Zdbk.Models;
using ReQuantum.Modules.ZjuSso.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReQuantum;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ZjuSsoState))]
[JsonSerializable(typeof(List<CalendarNote>))]
[JsonSerializable(typeof(List<CalendarTodo>))]
[JsonSerializable(typeof(List<CalendarEvent>))]
[JsonSerializable(typeof(CoursesZjuTodosResponse))]
[JsonSerializable(typeof(CoursesZjuState))]
[JsonSerializable(typeof(ZdbkSectionScheduleResponse))]
[JsonSerializable(typeof(ZdbkState))]
public partial class SourceGenerationContext : JsonSerializerContext;public partial class SourceGenerationContext : JsonSerializerContext;