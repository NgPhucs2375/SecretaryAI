using Microsoft.AspNetCore.Mvc;
using Qeq.Application.Features.Expenses.Commands.AddTransaction;
using Qeq.Application.Features.Expenses.Queries.GetTransactions;

namespace Qeq.Api.Controllers;

public class TransactionsController : BaseApiController
{
    // POST: api/transactions
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddTransactionCommand command)
        => Ok(await Mediator.Send(command));

    // GET: api/transactions?from=...&to=...&category=food&pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetTransactionsQuery query)
        => Ok(await Mediator.Send(query));
}