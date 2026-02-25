# Auth & Identity

## Identity Provider
Use Entra External ID / Azure AD B2C for user identity and tokens.

## Passwordless OTP
Use Azure Communication Services (ACS) for:
- Email OTP delivery
- WhatsApp OTP delivery

### UX flow (Email OTP)
1. User enters email.
2. System sends OTP via ACS Email.
3. User enters OTP.
4. On success: user is authenticated with External ID/B2C session and receives tokens for API calls.

### UX flow (WhatsApp OTP)
1. User enters phone number (E.164).
2. System sends OTP via ACS WhatsApp.
3. User enters OTP.
4. On success: user is authenticated with External ID/B2C session and receives tokens for API calls.

## Remembered login
- Web: persist auth session so refresh doesn't require re-login.
- Mobile: store tokens securely (platform secure storage).
- Public timeline view should not require login.

## Authorization
- Owner access: full CRUD on their timelines/items.
- Collaborators (if enabled): role-based permissions.
- Public/unlisted link: view-only by default; commenter optional; editor recommended via invites only.

## API authentication
- API validates bearer tokens from External ID/B2C.
- API may accept a ShareLink token for public viewing endpoints (no user auth required).

## Data association
- Use stable `UserId` (subject) as primary identity key.
- Email/phone may change; treat as attributes, not identity keys.