using MediatR;

namespace ShiftMate.Application.Demo.Commands.StartDemo;

// Skapar en ny demo-sandlåda och returnerar en JWT för vald roll ("Manager" eller "Employee").
public record StartDemoCommand(string Role) : IRequest<string>;
