using Microsoft.AspNetCore.Mvc;
using Qeq.Application.Features.Notes.Commands.AddIdea;
using Qeq.Application.Features.Notes.Queries.GetIdeas;

namespace Qeq.Api.Controllers;

public class IdeasController : BaseApiController
{
    // POST: api/ideas
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddIdeaCommand command)
        => Ok(await Mediator.Send(command));

    // GET: api/ideas?pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetIdeasQuery query)
        => Ok(await Mediator.Send(query));
}