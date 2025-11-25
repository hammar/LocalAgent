using System;
using System.ComponentModel.DataAnnotations;

namespace LocalAgent.ApiService.Models
{
    public class Agent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "System Instructions is required.")]
        public string SystemInstructions { get; set; } = string.Empty;
    }
}
