using Application.Features.Orders.Create;
using Onspay.AspNetCore.Endpoints;
using Onspay.AspNetCore.Extensions;
using Onspay.AspNetCore.Responses;
using Onspay.Cqrs.Messaging;

namespace Web.Api.Endpoints.Orders;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
                CreateOrderRequest request,
                ICommandHandler<CreateOrderCommand, Guid> commandHandler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateOrderCommand(
                    request.OrderNumber,
                    request.Description,
                    request.Items
                        .Select(item => new CreateOrderItemCommand(item.ProductName, item.Quantity, item.Price))
                        .ToList());

                var result = await commandHandler.Handle(command, cancellationToken);

                return result.ToCreatedApiResponse(id => $"orders/{id}");
            })
            .WithTags(Tags.Orders)
            .WithName("CreateOrder")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);
    }
}

internal sealed record CreateOrderRequest(
    int OrderNumber,
    string? Description,
    List<CreateOrderItemRequest> Items);

internal sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal Price);
