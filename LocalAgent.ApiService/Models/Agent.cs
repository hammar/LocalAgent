using System;

namespace LocalAgent.ApiService.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string SystemInstructions { get; set; } = string.Empty;
    }
}
