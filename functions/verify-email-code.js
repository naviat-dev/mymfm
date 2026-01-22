// Cloudflare Pages Function to verify email code
export async function onRequestPost(context) {
	try {
		const { request, env } = context;
		const { email, code } = await request.json();

		// Validate input
		if (!email || !code) {
			return new Response(JSON.stringify({ error: 'Email and code are required' }), {
				status: 400,
				headers: { 'Content-Type': 'application/json' }
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
				headers: { 'Content-Type': 'application/json' }
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
				headers: { 'Content-Type': 'application/json' }
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
				headers: { 'Content-Type': 'application/json' }
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
				headers: { 'Content-Type': 'application/json' }
			});
		}

		return new Response(JSON.stringify({
			valid: false,
			error: 'Invalid code'
		}), {
			status: 200,
			headers: { 'Content-Type': 'application/json' }
		});

	} catch (error) {
		console.error('Error:', error);
		return new Response(JSON.stringify({ error: error.message }), {
			status: 500,
			headers: { 'Content-Type': 'application/json' }
		});
	}
}
