// Cloudflare Pages Function to verify email code

// CORS headers
const corsHeaders = {
	'Access-Control-Allow-Origin': '*',
	'Access-Control-Allow-Methods': 'POST, OPTIONS',
	'Access-Control-Allow-Headers': 'Content-Type',
	'Content-Type': 'application/json'
};

// Handle OPTIONS request for CORS preflight
export async function onRequestOptions() {
	return new Response(null, {
		status: 204,
		headers: corsHeaders
	});
}

export async function onRequestPost(context) {
	try {
		const { request, env } = context;
		const { email, code } = await request.json();

		// Validate input
		if (!email || !code) {
			return new Response(JSON.stringify({ error: 'Email and code are required' }), {
				status: 400,
				headers: corsHeaders
			});
		}

		// Get stored code from KV
		const storedDataJson = await env.EMAIL_VERIFICATION.get(`verification:${email}`);

		if (!storedDataJson) {
			return new Response(JSON.stringify({
				valid: false,
				error: 'Code expired or not found'
			}), {
				status: 200,
				headers: corsHeaders
			});
		}

		const storedData = JSON.parse(storedDataJson);

		// Check if expired
		if (Date.now() > storedData.expiresAt) {
			await env.EMAIL_VERIFICATION.delete(`verification:${email}`);
			return new Response(JSON.stringify({
				valid: false,
				error: 'Code has expired'
			}), {
				status: 200,
				headers: corsHeaders
			});
		}

		// Check attempts
		storedData.attempts++;
		if (storedData.attempts > 3) {
			await env.EMAIL_VERIFICATION.delete(`verification:${email}`);
			return new Response(JSON.stringify({
				valid: false,
				error: 'Too many attempts. Please request a new code.'
			}), {
				status: 200,
				headers: corsHeaders
			});
		}

		// Update attempts
		await env.EMAIL_VERIFICATION.put(
			`verification:${email}`,
			JSON.stringify(storedData),
			{ expirationTtl: Math.floor((storedData.expiresAt - Date.now()) / 1000) }
		);

		// Verify code
		if (storedData.code === code) {
			// Delete code after successful verification
			await env.EMAIL_VERIFICATION.delete(`verification:${email}`);
			return new Response(JSON.stringify({ valid: true }), {
				status: 200,
				headers: corsHeaders
			});
		}

		return new Response(JSON.stringify({
			valid: false,
			error: 'Invalid code'
		}), {
			status: 200,
			headers: corsHeaders
		});

	} catch (error) {
		console.error('Error:', error);
		return new Response(JSON.stringify({ error: error.message }), {
			status: 500,
			headers: corsHeaders
		});
	}
}
