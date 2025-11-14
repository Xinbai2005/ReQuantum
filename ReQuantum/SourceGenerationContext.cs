using ReQuantum.Modules.Calendar.Entities;
using ReQuantum.Modules.CoursesZju.Models;
using ReQuantum.Modules.ZjuSso.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReQuantum.Client;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ZjuSsoState))]
[JsonSerializable(typeof(List<CalendarNote>))]
[JsonSerializable(typeof(List<CalendarTodo>))]
[JsonSerializable(typeof(List<CalendarEvent>))]
[JsonSerializable(typeof(CoursesZjuTodosResponse))]
[JsonSerializable(typeof(CoursesZjuState))]
public partial class SourceGenerationContext : JsonSerializerContext;