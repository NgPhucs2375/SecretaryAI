using MediatR;
using Qeq.Application.DTOs.Notes;
using Qeq.Application.Interfaces;
using Qeq.Application.Wrappers;
using Qeq.Domain.Entities;

namespace Qeq.Application.Features.Notes.Commands.AddIdea;

public record AddIdeaCommand(
    string Content ): IRequest<Response<IdeaNoteDto>>;

public class AddIdeaCommandHandler : IRequestHandler<AddIdeaCommand, Response<IdeaNoteDto>>
{
    private readonly IApplicationDbContext _context;

    public AddIdeaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<IdeaNoteDto>> Handle(AddIdeaCommand request, CancellationToken cancellationToken)
    {
        var ideaNote = new IdeaNote
        {
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.IdeaNotes.Add(ideaNote);
        await _context.SaveChangesAsync(cancellationToken);

        var ideaNoteDto = new IdeaNoteDto(
            ideaNote.Id,
            ideaNote.Content,
            ideaNote.CreatedAt
        );

        return new Response<IdeaNoteDto>(ideaNoteDto, "Ý tưởng đã được thêm thành công.");
    }
}