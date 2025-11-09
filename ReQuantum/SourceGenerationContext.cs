using ReQuantum.Models;
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