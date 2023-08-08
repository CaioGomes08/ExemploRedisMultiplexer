using System;
using Basket.API.Entities;
using Basket.API.Repositories.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IConnectionMultiplexer _redisCache;

        public BasketRepository(IConnectionMultiplexer cache)
        {
            _redisCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        public async Task<ShoppingCart?> GetBasket(string userName)
        {
            IDatabase database = _redisCache.GetDatabase();
            var basket = await database.StringGetAsync(userName);

            if (string.IsNullOrEmpty(basket))
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart?> UpdateBasket(ShoppingCart basket)
        {
            IDatabase database = _redisCache.GetDatabase();

            await database.StringSetAsync(basket.UserName, JsonConvert.SerializeObject(basket), TimeSpan.FromMinutes(2));

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            IDatabase database = _redisCache.GetDatabase();
            await database.KeyDeleteAsync(userName);
        }
    }
}

