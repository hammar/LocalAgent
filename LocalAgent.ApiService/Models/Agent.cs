using System;

namespace LocalAgent.ApiService.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SystemInstructions { get; set; } = string.Empty;
    }
}
