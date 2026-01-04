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
		string? auth = _authService.GetCurrentAuth();
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
		FirebaseClient client = GetAuthenticatedClient();
		FirebaseObject<T> result = await client
			.Child(path)
			.PostAsync(data);
		return result.Key;
	}

	// Get all data from a path
	public async Task<IReadOnlyCollection<T>> GetAllDataAsync<T>(string path)
	{
		FirebaseClient client = GetAuthenticatedClient();
		IReadOnlyCollection<FirebaseObject<T>> items = await client
			.Child(path)
			.OnceAsync<T>();
		return [.. items.Select(x => x.Object)];
	}

	// Get single item by key
	public async Task<T?> GetDataByKeyAsync<T>(string path, string key)
	{
		FirebaseClient client = GetAuthenticatedClient();
		T? item = await client
			.Child(path)
			.Child(key)
			.OnceSingleAsync<T>();
		return item;
	}

	// Update data
	public async Task UpdateDataAsync<T>(string path, string key, T data)
	{
		FirebaseClient client = GetAuthenticatedClient();
		await client
			.Child(path)
			.Child(key)
			.PutAsync(data);
	}

	// Delete data
	public async Task DeleteDataAsync(string path, string key)
	{
		FirebaseClient client = GetAuthenticatedClient();
		await client
			.Child(path)
			.Child(key)
			.DeleteAsync();
	}
}
