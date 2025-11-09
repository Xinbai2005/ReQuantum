using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReQuantum.Models;

public class CoursesZjuTodosResponse
{
    [JsonPropertyName("todo_list")]
    public required List<CoursesZjuTodoDto> TodoList { get; set; }
}