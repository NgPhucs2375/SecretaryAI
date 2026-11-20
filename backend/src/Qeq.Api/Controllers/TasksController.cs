using Microsoft.AspNetCore.Mvc;
using Qeq.Application.Features.Planning.Commands.AddTask;
using Qeq.Application.Features.Planning.Commands.CompleteTask;
using Qeq.Application.Features.Planning.Queries.GetTasks;

namespace Qeq.Api.Controllers;

public class TasksController : BaseApiController
{
    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddTaskCommand command)
        => Ok(await Mediator.Send(command));

    // GET: api/tasks?status=...&pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetTasksQuery query)
        => Ok(await Mediator.Send(query));

    // PATCH: api/tasks/{id}/complete
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete([FromRoute] Guid id)
        => Ok(await Mediator.Send(new CompleteTaskCommand(id)));
}