using System;
using System.ComponentModel.DataAnnotations;

namespace LocalAgent.ApiService.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string SystemInstructions { get; set; } = string.Empty;
    }
}
