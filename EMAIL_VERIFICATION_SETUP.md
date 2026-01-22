# Email Verification Setup for Cloudflare Pages

## Overview
This project uses Cloudflare Workers (Functions) to handle email verification via Postmark. The client-side Blazor app calls serverless functions that securely send emails.

## Cloudflare Setup

### 1. Create KV Namespace
In your Cloudflare dashboard:
1. Go to **Workers & Pages** → **KV**
2. Click **Create namespace**
3. Name it: `EMAIL_VERIFICATION`
4. Copy the namespace ID

### 2. Configure Environment Variables
In your Cloudflare Pages project settings:
1. Go to **Settings** → **Environment variables**
2. Add the following variables:

**Production:**
- `POSTMARK_SERVER_TOKEN` = Your Postmark Server API Token
- `POSTMARK_FROM_EMAIL` = Your verified sender email (e.g., `noreply@yourdomain.com`)

**Preview (optional, for testing):**
- Same variables as production or use Postmark's sandbox token

### 3. Bind KV Namespace
In your Cloudflare Pages project:
1. Go to **Settings** → **Functions**
2. Under **KV namespace bindings**, click **Add binding**
3. Variable name: `EMAIL_VERIFICATION`
4. KV namespace: Select the namespace you created
5. Save

## Postmark Setup

### 1. Get Your API Token
1. Sign in to [Postmark](https://postmarkapp.com)
2. Go to **Servers** → Select your server
3. Go to **API Tokens** tab
4. Copy your **Server API token**

### 2. Add Sender Signature
1. In Postmark, go to **Sender Signatures**
2. Click **Add Domain** or **Add Email Address**
3. Verify your email/domain
4. Use this verified email as `POSTMARK_FROM_EMAIL`

## Testing Locally

To test locally with Cloudflare Wrangler:

```bash
# Install Wrangler CLI
npm install -g wrangler

# Create wrangler.toml in your project root
cat > wrangler.toml << EOF
name = "mymfm"
compatibility_date = "2026-01-21"

[[kv_namespaces]]
binding = "EMAIL_VERIFICATION"
id = "your-kv-namespace-id"

[vars]
POSTMARK_FROM_EMAIL = "noreply@yourdomain.com"

[env.production]
vars = { }
EOF

# Add secrets (don't commit these!)
wrangler secret put POSTMARK_SERVER_TOKEN

# Test the functions
wrangler pages dev bin/Release/net10.0/publish/wwwroot
```

## API Endpoints

After deployment, your functions will be available at:

- `POST /send-verification-code` - Sends verification email
  ```json
  { "email": "user@example.com" }
  ```

- `POST /verify-email-code` - Verifies the code
  ```json
  { "email": "user@example.com", "code": "123456" }
  ```

## How It Works

1. **User enters email** → Clicks "Send Verification Code"
2. **Client calls** `/send-verification-code` Cloudflare Function
3. **Function generates** 6-digit code and stores in KV with 10-min expiration
4. **Function calls** Postmark API to send email
5. **User receives** email with code
6. **User enters** code in the form
7. **Client calls** `/verify-email-code` to validate
8. **Function checks** KV storage and validates code
9. **User proceeds** with registration after successful verification

## Security Notes

- ✅ API keys never exposed to client
- ✅ Codes expire after 10 minutes
- ✅ Maximum 3 verification attempts
- ✅ Cloudflare edge computing for fast delivery
- ✅ All traffic goes through Cloudflare's secure network

## Troubleshooting

### "Failed to send email"
- Check Postmark Server Token is correct
- Verify sender email is verified in Postmark
- Check Cloudflare Function logs

### "Code expired or not found"
- Ensure KV namespace is properly bound
- Check the binding name is exactly `EMAIL_VERIFICATION`
- Verify namespace ID matches in bindings

### CORS Issues
- Functions automatically handle CORS
- Make sure you're calling `/send-verification-code` not the full URL
- The HttpClient in Blazor should use relative paths

### Local Testing Issues
- Use `wrangler pages dev` instead of `dotnet run`
- Ensure environment variables are set in wrangler.toml
- Check KV namespace binding is configured
