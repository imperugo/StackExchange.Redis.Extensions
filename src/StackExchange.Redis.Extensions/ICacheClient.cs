using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions
{
	public interface ICacheClient : IDisposable
	{
		IDatabase Database { get; }
		bool Exists(string key);
		Task<bool> ExistsAsync(string key);
		bool Remove(string key);
		Task<bool> RemoveAsync(string key);
		void RemoveAll(IEnumerable<string> keys);
		Task RemoveAllAsync(IEnumerable<string> keys);
		T Get<T>(string key) where T : class;
		Task<T> GetAsync<T>(string key) where T : class;
		bool Add<T>(string key, T value) where T : class;
		Task<bool> AddAsync<T>(string key, T value) where T : class;
		bool Replace<T>(string key, T value) where T : class;
		Task<bool> ReplaceAsync<T>(string key, T value) where T : class;
		bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class;
		Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class;
		bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class;
		Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class;
		bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class;
		Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class;
		bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class;
		Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class;
		IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class;
		Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys) where T : class;
		IEnumerable<string> SearchKeys(string pattern);
		Task<IEnumerable<string>> SearchKeysAsync(string pattern);
	}
}