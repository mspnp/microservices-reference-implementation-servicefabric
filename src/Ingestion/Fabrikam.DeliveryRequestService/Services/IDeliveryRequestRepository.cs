using DeliveryRequestService.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DeliveryRequestService.Services
{
    public interface IDeliveryRequestRepository
    {
        Task<bool> CreateAsync(InternalDeliveryRequest deliveryRequest);
    }
}
