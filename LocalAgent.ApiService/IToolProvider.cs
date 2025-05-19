using System;
using Microsoft.Extensions.AI;

namespace LocalAgent.ApiService;

public interface IToolProvider
{
    public IEnumerable<AITool> GetTools();
}
