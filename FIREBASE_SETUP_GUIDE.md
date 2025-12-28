# Firebase Authentication & Security Setup Guide

## Overview
This setup ensures:
- **Form submissions**: Only authenticated requests from your website can write data (using anonymous auth)
- **Reading data**: Only authenticated admin users can read data

## Step 1: Enable Authentication in Firebase Console

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select your project
3. Navigate to **Authentication** → **Sign-in method**
4. Enable these providers:
   - ✅ **Anonymous** (for form submissions)
   - ✅ **Email/Password** (for admin access)

## Step 2: Create Admin User

In Firebase Console → Authentication → Users:
1. Click **Add user**
2. Enter admin email and password
3. Note the **UID** of this user (you'll need it for security rules)

## Step 3: Update appsettings.json

Replace values in `wwwroot/appsettings.json`:
```json
{
  "Firebase": {
    "DatabaseUrl": "https://YOUR-PROJECT-ID.firebaseio.com/",
    "ApiKey": "YOUR-WEB-API-KEY",
    "AuthDomain": "YOUR-PROJECT-ID.firebaseapp.com"
  }
}
```

Find these values:
- **Project Settings** → **General** → Web API Key
- Database URL is in **Realtime Database** section

## Step 4: Set Firebase Security Rules

In Firebase Console → **Realtime Database** → **Rules**, paste this:

```json
{
  "rules": {
    "form-responses": {
      ".read": "auth != null && auth.token.firebase.sign_in_provider != 'anonymous'",
      ".write": "auth != null",
      ".indexOn": ["timestamp"]
    }
  }
}
```

**What this does:**
- `.read`: Only authenticated **non-anonymous** users can read (admin only)
- `.write`: Any authenticated user can write (including anonymous form submissions)
- Replace `"form-responses"` with your actual data path

## Step 5: Usage in Your App

### For Form Submissions (Anonymous Auth)
```csharp
@inject FirebaseAuthService Auth
@inject FirebaseService Firebase

private async Task SubmitForm()
{
    // Sign in anonymously before submitting
    await Auth.SignInAnonymouslyAsync();
    
    // Now submit the form data
    var responseId = await Firebase.SaveDataAsync("form-responses", formData);
}
```

### For Admin/Reading Data (Email Auth)
```csharp
@inject FirebaseAuthService Auth
@inject FirebaseService Firebase

private async Task AdminLogin()
{
    bool success = await Auth.SignInWithEmailAsync("admin@example.com", "password");
    if (success)
    {
        // Now can read data
        var responses = await Firebase.GetAllDataAsync<FormResponse>("form-responses");
    }
}
```

## Security Best Practices

1. **Admin UID Restriction** (Recommended):
   Replace the read rule with this to restrict to specific admin UID:
   ```json
   ".read": "auth.uid === 'YOUR-ADMIN-UID-HERE'"
   ```

2. **Validate Write Data**:
   Add validation to ensure only expected fields are written:
   ```json
   ".write": "auth != null && newData.hasChildren(['name', 'email', 'timestamp'])"
   ```

3. **Keep API Key in Environment Variables**:
   For production, move sensitive config to environment variables

4. **Add Timestamp**:
   Always include a timestamp field in your form data for auditing

## Testing

1. Test anonymous write: Submit a form → should succeed
2. Test anonymous read: Try to read data without login → should fail
3. Test admin login: Sign in with admin credentials → should be able to read data

## Troubleshooting

- **"Permission denied"**: Check that auth is enabled and rules are published
- **"Invalid API key"**: Verify API key in appsettings.json
- **Can't write data**: Make sure `SignInAnonymouslyAsync()` is called before saving
