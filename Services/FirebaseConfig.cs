namespace mymfm.Services;

public class FirebaseConfig
{
	public string DatabaseUrl { get; set; } = string.Empty;
	public string ApiKey { get; set; } = string.Empty;
	public string AuthDomain { get; set; } = string.Empty;
}

public class AppSettings
{
	public FirebaseConfig Firebase { get; set; } = new();
}
