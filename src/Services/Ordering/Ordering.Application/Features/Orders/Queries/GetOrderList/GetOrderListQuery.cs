using MediatR;

namespace Ordering.Application.Features.Orders.Queries.GetOrderList
{
    public class GetOrderListQuery : IRequest<List<OrdersDto>>
    {
        public string Username { get; set; }

        public GetOrderListQuery(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }


    }
}
