# Deployment notes

## Email (Brevo SMTP)

The app sends transactional email from two processes:

- **BusinessIdea** (API) - sends the 2FA / email-verification code at registration, directly (critical path, must not wait on the Worker).
- **BusinessIdea.Worker** - sends outbox notifications (idea won, etc.).

Both use the same MailKit `SmtpEmailSender` and the same `Email` config section, so **both must be configured in production**.

### Config layout

| Setting | Where it lives | Value |
| --- | --- | --- |
| Host / Port / UseSsl / FromAddress / FromName | `appsettings.json` (both projects) | Brevo defaults, committed |
| Host / Port / UseSsl | `appsettings.Development.json` (both projects) | Points back at local smtp4dev for dev |
| Username / Password | **Environment variables only** - never committed | Brevo SMTP login + SMTP key |

`appsettings.json` now targets Brevo:

```json
"Email": {
  "Host": "smtp-relay.brevo.com",
  "Port": 587,
  "UseSsl": true,
  "FromAddress": "noreply@idea-arena.local",
  "FromName": "Idea Arena"
}
```

Local dev still uses smtp4dev because `appsettings.Development.json` overrides Host/Port/UseSsl. No dev workflow changes.

### One-time Brevo setup

1. Create a free Brevo account (300 emails/day free tier).
2. **Verify a sender.** Either verify a single sender address, or (recommended) add and verify a **domain** you own so you can send from `noreply@yourdomain.com`. Domain verification means adding Brevo's **SPF** and **DKIM** records to your DNS. Without this, mail goes to spam.
3. Update `FromAddress` in both `appsettings.json` files to the sender/domain you verified. The current `noreply@idea-arena.local` is a placeholder and will bounce.
4. In Brevo: **SMTP & API -> SMTP**, copy your **SMTP login** and generate an **SMTP key** (this is the password - not your account password).

### Production secrets (environment variables)

ASP.NET maps `__` (double underscore) to config nesting. Set these on both the API host and the Worker host:

```
Email__Username=<your Brevo SMTP login>
Email__Password=<your Brevo SMTP key>
```

On Azure App Service these go in Application Settings; in Docker use `-e` / `environment:`; on a bare VPS export them in the service unit file.

### Also update for hosting

- **`BusinessIdea.Worker/appsettings.json` -> `App:PublicBaseUrl`** currently points at `http://localhost:4200`. Links inside notification emails use it, so set it to the real frontend URL or emailed links break.

### Verify it works

Send a real email through the deployed app (register a test account). In Gmail, open the message, "Show original", and confirm **SPF=pass** and **DKIM=pass**. If they fail, mail will silently land in spam even though the app reports success.

## Code caveat

`SmtpEmailSender` supports STARTTLS (`UseSsl=true`) or plaintext only - not implicit SSL on port 465. Brevo uses STARTTLS on port 587, so no code change is needed. Only a provider that forces port 465 would require adding a `SslOnConnect` branch.
