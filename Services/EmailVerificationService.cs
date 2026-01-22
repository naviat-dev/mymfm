using System.Net.Http.Json;
using System.Text.Json;

namespace mymfm.Services;

public class EmailVerificationService
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;

	public EmailVerificationService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		// For local development, use localhost. For production, this will be your Cloudflare Pages URL
		_baseUrl = configuration["ApiBaseUrl"] ?? httpClient.BaseAddress?.ToString() ?? "";
	}

	public async Task<bool> SendVerificationCodeAsync(string email)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync(
				"/send-verification-code",
				new { email }
			);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
				return result?.Success ?? false;
			}

			var errorContent = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error sending verification code: {response.StatusCode} - {errorContent}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error sending verification email: {ex.Message}");
			return false;
		}
	}

	public async Task<(bool IsValid, string? Error)> VerifyCodeAsync(string email, string code)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync(
				"/verify-email-code",
				new { email, code }
			);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
				return (result?.Valid ?? false, result?.Error);
			}

			var errorContent = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error verifying code: {response.StatusCode} - {errorContent}");
			return (false, "Failed to verify code. Please try again.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error verifying code: {ex.Message}");
			return (false, "Network error. Please try again.");
		}
	}

	private class ApiResponse
	{
		public bool Success { get; set; }
		public string? Error { get; set; }
	}

	private class VerifyResponse
	{
		public bool Valid { get; set; }
		public string? Error { get; set; }
	}
}
