using MediatR;

namespace DataProcessor.Features.Messages;

public record ProcessMessageCommand(string Content, string RoutingKey) : IRequest;
