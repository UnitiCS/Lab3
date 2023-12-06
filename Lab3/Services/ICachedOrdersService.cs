using Lab3.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab3.Services
{
    public interface ICachedOrdersService
    {
        public IEnumerable<Order> GetOrders(int rowsNumber = 20);
        public void AddOrders(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Order> GetOrdersFromCache(string cacheKey, int rowsNumber = 20);
        public IEnumerable<string> GetTypes(IEnumerable<Order> selectedOrders);
    }
}