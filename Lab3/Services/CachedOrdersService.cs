using Lab3.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Lab3.Data;

namespace Lab3.Services
{
    public class CachedOrdersService : ICachedOrdersService
    {
        private readonly BakeryDBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly int _savingTime;

        public CachedOrdersService(BakeryDBContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _savingTime = 2 * 27 + 240;
        }
        // получение списка материалов из базы
        public IEnumerable<Order> GetOrders(int rowsNumber = 20)
        {
            return _dbContext.Orders.Take(rowsNumber).ToList();
        }

        // добавление списка материалов в кэш
        public void AddOrders(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Order> cachedOrders))
            {
                cachedOrders = _dbContext.Orders.Take(rowsNumber).ToList();

                if (cachedOrders != null)
                {
                    _memoryCache.Set(cacheKey, cachedOrders, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_savingTime)
                    });
                }
                Console.WriteLine("Таблица занесена в кеш");
            }
            else
            {
                Console.WriteLine("Таблица уже находится в кеше");
            }
        }
        // получение списка матреиалов из кэша или из базы, если нет в кэше
        public IEnumerable<Order> GetOrdersFromCache(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Order> orders;
            if (!_memoryCache.TryGetValue(cacheKey, out orders))
            {
                orders = _dbContext.Orders.Take(rowsNumber).ToList();
                if (orders != null)
                {
                    _memoryCache.Set(cacheKey, orders,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_savingTime)));
                }
            }
            return orders;
        }
        //Получение списка уникальных цветов изделий
        public IEnumerable<string> GetTypes(IEnumerable<Order> selectedOrders)
        {
            IEnumerable<string> colors = selectedOrders.Select(o => o.ProductType).Distinct().ToList();
            return colors;
        }

    }
}