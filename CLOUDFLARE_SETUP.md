# Cloudflare Pages Configuration

## Build Settings
- **Build command**: `dotnet publish -c Release`
- **Build output directory**: `bin/Release/net10.0/publish/wwwroot`
- **Root directory**: `/` (repository root)

## Environment Variables (Set in Cloudflare Dashboard)
- `DOTNET_VERSION`: `10.0.x`
- `NODE_VERSION`: `20` (for build tools if needed)

## Custom Headers (Optional)
Add to `_headers` file in wwwroot:

```
/*
  X-Frame-Options: SAMEORIGIN
  X-Content-Type-Options: nosniff
  X-XSS-Protection: 1; mode=block
  Referrer-Policy: strict-origin-when-cross-origin

/_framework/*
  Cache-Control: public, max-age=31536000, immutable

*.wasm
  Content-Type: application/wasm

*.dll
  Content-Type: application/octet-stream
```

## Redirects (Optional)
Add to `_redirects` file in wwwroot:

```
# Handle client-side routing
/*    /index.html   200
```