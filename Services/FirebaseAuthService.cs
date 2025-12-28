using Firebase.Auth;
using Firebase.Auth.Providers;

namespace mymfm.Services;

public class FirebaseAuthService
{
	private readonly FirebaseAuthClient _authClient;
	private UserCredential? _currentUser;

	public FirebaseAuthService(FirebaseConfig config)
	{
		var authConfig = new FirebaseAuthConfig
		{
			ApiKey = config.ApiKey,
			AuthDomain = config.AuthDomain,
			Providers = new FirebaseAuthProvider[]
			{
				new EmailProvider(),
				new GoogleProvider()
			}
		};
		_authClient = new FirebaseAuthClient(authConfig);
	}

	// Sign in anonymously for form submissions
	public async Task SignInAnonymouslyAsync()
	{
		_currentUser = await _authClient.SignInAnonymouslyAsync();
	}

	// Sign in with email/password for admin access
	public async Task<bool> SignInWithEmailAsync(string email, string password)
	{
		try
		{
			_currentUser = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
			return true;
		}
		catch
		{
			return false;
		}
	}

	// Sign out
	public void SignOut()
	{
		_currentUser = null;
	}

	// Check if user is authenticated
	public bool IsAuthenticated => _currentUser != null;

	// Get current auth token
	public string? GetCurrentAuth()
	{
		return _currentUser?.User?.Credential?.IdToken;
	}

	// Get current user
	public User? GetCurrentUser()
	{
		return _currentUser?.User;
	}

	// Check if current user is anonymous
	public bool IsAnonymous => _currentUser?.User?.Info?.IsAnonymous ?? false;
}
