using Microsoft.Graph;
using Microsoft.Graph.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace LocalAgent.McpServer.Tools;

/// <summary>
/// MCP Tool for querying Microsoft To-Do tasks through Microsoft Graph API.
/// </summary>
[McpServerToolType]
public sealed class ToDoTools
{
    private readonly ILogger<ToDoTools> _logger;
    private readonly GraphServiceClient _graphClient;

    public ToDoTools(ILogger<ToDoTools> logger, GraphServiceClient graphClient)
    {
        _logger = logger;
        _graphClient = graphClient;
    }

    [McpServerTool, Description("Get all To-Do task lists for the user.")]
    public async Task<string> GetTaskLists()
    {
        _logger.LogInformation("Fetching To-Do task lists");

        try
        {
            var taskLists = await _graphClient.Me.Todo.Lists
                .GetAsync();

            if (taskLists?.Value == null || !taskLists.Value.Any())
            {
                return "No task lists found.";
            }

            var result = new StringBuilder();
            foreach (var list in taskLists.Value)
            {
                result.AppendLine($"ID: {list.Id}");
                result.AppendLine($"Name: {list.DisplayName}");
                result.AppendLine($"Is Owner: {list.IsOwner}");
                result.AppendLine($"Is Shared: {list.IsShared}");
                result.AppendLine("---");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching To-Do task lists");
            throw;
        }
    }

    [McpServerTool, Description("Get tasks from a specific To-Do list.")]
    public async Task<string> GetTasksByList(
        [Description("The unique alphanumeric ID of the task list. DO NOT use the display name (e.g., 'Work'). This MUST be the 'id' property returned by the GetTaskLists tool. Example: 'AAMkAGI3...'")] string listId)
    {
        _logger.LogInformation("Fetching tasks from list: {ListId}", listId);

        try
        {
            var tasks = await _graphClient.Me.Todo.Lists[listId].Tasks
                .GetAsync();

            if (tasks?.Value == null || !tasks.Value.Any())
            {
                return $"No tasks found in list with ID: {listId}";
            }

            return FormatTasks(tasks.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tasks from list: {ListId}", listId);
            throw;
        }
    }

    [McpServerTool, Description("Get tasks due by a specific date across all lists. Date should be in ISO 8601 format (e.g., 2024-12-31).")]
    public async Task<string> GetTasksByDueDate(
        [Description("The due date in ISO 8601 format (e.g., 2024-12-31 or 2024-12-31T23:59:59Z).")] string dueDate)
    {
        _logger.LogInformation("Fetching tasks due by: {DueDate}", dueDate);

        try
        {
            // Parse the date using invariant culture for consistent behavior
            if (!DateTime.TryParse(dueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return $"Invalid date format: {dueDate}. Please use ISO 8601 format (e.g., 2024-12-31).";
            }

            // Get all task lists
            var taskLists = await _graphClient.Me.Todo.Lists.GetAsync();
            
            if (taskLists?.Value == null || !taskLists.Value.Any())
            {
                return "No task lists found.";
            }

            var allTasks = new List<TodoTask>();

            // Get tasks from each list
            foreach (var list in taskLists.Value)
            {
                var tasks = await _graphClient.Me.Todo.Lists[list.Id].Tasks.GetAsync();
                
                if (tasks?.Value != null)
                {
                    // Filter tasks by due date, parsing with invariant culture
                    var filteredTasks = tasks.Value.Where(task => 
                    {
                        if (task.DueDateTime?.DateTime == null) return false;
                        return DateTime.TryParse(task.DueDateTime.DateTime, CultureInfo.InvariantCulture, 
                            DateTimeStyles.None, out var taskDueDate) && taskDueDate <= parsedDate;
                    });
                    
                    allTasks.AddRange(filteredTasks);
                }
            }

            if (!allTasks.Any())
            {
                return $"No tasks found due by {parsedDate:yyyy-MM-dd}.";
            }

            return FormatTasks(allTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tasks by due date: {DueDate}", dueDate);
            throw;
        }
    }

    private static string FormatTasks(IEnumerable<TodoTask> tasks)
    {
        var result = new StringBuilder();
        
        foreach (var task in tasks)
        {
            result.AppendLine($"Title: {task.Title}");
            result.AppendLine($"ID: {task.Id}");
            result.AppendLine($"Status: {task.Status}");
            result.AppendLine($"Importance: {task.Importance}");
            
            if (task.DueDateTime?.DateTime != null)
            {
                result.AppendLine($"Due Date: {task.DueDateTime.DateTime}");
            }
            
            if (task.CreatedDateTime.HasValue)
            {
                result.AppendLine($"Created: {task.CreatedDateTime.Value:yyyy-MM-dd HH:mm:ss}");
            }
            
            if (!string.IsNullOrWhiteSpace(task.Body?.Content))
            {
                result.AppendLine($"Body: {task.Body.Content}");
            }
            
            result.AppendLine("---");
        }

        return result.ToString();
    }
}
