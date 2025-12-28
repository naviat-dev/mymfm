using Firebase.Database;
using Firebase.Database.Query;

namespace mymfm.Services;

public class FirebaseService
{
	private readonly FirebaseConfig _config;
	private readonly FirebaseAuthService _authService;

	public FirebaseService(FirebaseConfig config, FirebaseAuthService authService)
	{
		_config = config;
		_authService = authService;
	}

	private FirebaseClient GetAuthenticatedClient()
	{
		var auth = _authService.GetCurrentAuth();
		if (auth != null)
		{
			return new FirebaseClient(_config.DatabaseUrl, new FirebaseOptions
			{
				AuthTokenAsyncFactory = () => Task.FromResult(auth)
			});
		}
		return new FirebaseClient(_config.DatabaseUrl);
	}

	// Save data to Firebase
	public async Task<string> SaveDataAsync<T>(string path, T data)
	{
		var client = GetAuthenticatedClient();
		var result = await client
			.Child(path)
			.PostAsync(data);
		return result.Key;
	}

	// Get all data from a path
	public async Task<IReadOnlyCollection<T>> GetAllDataAsync<T>(string path)
	{
		var client = GetAuthenticatedClient();
		var items = await client
			.Child(path)
			.OnceAsync<T>();
		return items.Select(x => x.Object).ToList();
	}

	// Get single item by key
	public async Task<T?> GetDataByKeyAsync<T>(string path, string key)
	{
		var client = GetAuthenticatedClient();
		var item = await client
			.Child(path)
			.Child(key)
			.OnceSingleAsync<T>();
		return item;
	}

	// Update data
	public async Task UpdateDataAsync<T>(string path, string key, T data)
	{
		var client = GetAuthenticatedClient();
		await client
			.Child(path)
			.Child(key)
			.PutAsync(data);
	}

	// Delete data
	public async Task DeleteDataAsync(string path, string key)
	{
		var client = GetAuthenticatedClient();
		await client
			.Child(path)
			.Child(key)
			.DeleteAsync();
	}
}
