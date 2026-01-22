// Cloudflare Pages Function to send verification email
export async function onRequestPost(context) {
	try {
		const { request, env } = context;
		const { email } = await request.json();

		// Validate email
		if (!email || !email.includes('@')) {
			return new Response(JSON.stringify({ error: 'Invalid email address' }), {
				status: 400,
				headers: { 'Content-Type': 'application/json' }
			});
		}

		// Generate 6-digit code
		const code = Math.floor(100000 + Math.random() * 900000).toString();
		const expiresAt = Date.now() + (10 * 60 * 1000); // 10 minutes

		// Store code in KV (Cloudflare Key-Value storage)
		await env.EMAIL_VERIFICATION.put(
			`verification:${email}`,
			JSON.stringify({ code, expiresAt, attempts: 0 }),
			{ expirationTtl: 600 } // Auto-expire after 10 minutes
		);

		// Send email via Postmark
		const postmarkResponse = await fetch('https://api.postmarkapp.com/email', {
			method: 'POST',
			headers: {
				'Accept': 'application/json',
				'Content-Type': 'application/json',
				'X-Postmark-Server-Token': env.POSTMARK_SERVER_TOKEN
			},
			body: JSON.stringify({
				From: env.POSTMARK_FROM_EMAIL,
				To: email,
				Subject: 'Verify Your Email - MFM Registration',
				HtmlBody: `
          <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
              <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; text-align: center;'>Email Verification</h1>
              </div>
              <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                <p style='font-size: 16px; color: #333;'>Thank you for registering for the MFM Americas & Caribbean Youth Convention!</p>
                <p style='font-size: 16px; color: #333;'>Your verification code is:</p>
                <div style='background: white; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                  <span style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 5px;'>${code}</span>
                </div>
                <p style='font-size: 14px; color: #666;'>This code will expire in 10 minutes.</p>
                <p style='font-size: 14px; color: #666;'>If you didn't request this code, please ignore this email.</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                <p style='font-size: 12px; color: #999; text-align: center;'>Mountain of Fire and Miracles Ministries</p>
              </div>
            </body>
          </html>
        `,
				TextBody: `Email Verification\n\nThank you for registering for the MFM Americas & Caribbean Youth Convention!\n\nYour verification code is: ${code}\n\nThis code will expire in 10 minutes.\n\nIf you didn't request this code, please ignore this email.\n\n---\nMountain of Fire and Miracles Ministries`,
				MessageStream: 'outbound'
			})
		});

		if (!postmarkResponse.ok) {
			const errorData = await postmarkResponse.text();
			console.error('Postmark error:', errorData);
			return new Response(JSON.stringify({ error: 'Failed to send email' }), {
				status: 500,
				headers: { 'Content-Type': 'application/json' }
			});
		}

		return new Response(JSON.stringify({ success: true }), {
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
