# VEGA

A multi-purpose Discord bot built with .NET 9 and [NetCord](https://netcord.dev/), backed by PostgreSQL.

Message triggers, scheduled Reddit feeds, anonymous polls, emote extraction — most of it driven
through buttons and forms rather than long command lines. Answers follow each user's Discord
locale (English / French).

## Commands

Every command checks that the caller holds the Discord permission matching what the command
actually does. Missing permissions produce an ephemeral error, never a silent no-op.

### Slash commands

| Command | What it does | Requires |
|---|---|---|
| `/trigger list` · `add` · `delete` | Manage regex triggers: when a message matches the pattern, the bot replies with the configured response. Scoped to the server. | `ManageMessages` |
| `/feed list` · `add` · `delete` | Manage Reddit feeds: a subreddit is polled on an interval and new posts are relayed to a channel. | `ManageMessages` |
| `/poll` | Open a form to start an anonymous poll. Votes are cast with buttons and stay hidden until the poll closes. | — |
| `/clear` | Delete between 1 and 50 recent messages in the current channel. | `ManageMessages` |
| `/showprofile` | Show a user's avatar and banner in high resolution. | — |
| `/diceroll` | Roll 1–100 dice of 2–100 faces. | — |
| `/up` | Uptime and runtime information about the bot. | — |

`list`, `add` and `delete` all open the same widget: a paginated list with buttons, so a
trigger or a feed can be created, inspected and removed without retyping anything.

### Context-menu commands

| Command | Type | What it does | Requires |
|---|---|---|---|
| `DownloadEmotes` | Message | Extract every custom emote from the message as high-res PNG/GIF and return them in a single zip. A button then offers to add them to the server. | `AttachFiles` — adding to the server also needs `ManageGuildExpressions` |
| `ID` | User | Return the target's Discord ID, ephemerally. | — |

### Backoffice commands

Registered **only** on the guild set as `backofficeGuildId`, so they never appear anywhere else.

| Command | What it does | Requires |
|---|---|---|
| `/feedconfig` | Edit the global feed settings — fetch size, refresh interval, history size, sort mode, feed cap per guild. | Super admin |
| `/clearcache guild` · `current` · `info` | Inspect and flush the in-memory guild-settings cache. | — |
| `/clearcommands` | Unregister every command from Discord. Development aid. | — |

## Setup

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL
- A Discord application with a bot user

### Discord application

Enable the **Message Content** privileged intent in the Discord developer portal — without it
triggers never fire, since the bot cannot read message bodies.

Invite the bot with at least: `Send Messages`, `Read Message History`, `Manage Messages`,
`Attach Files`, `Manage Expressions`.

### Configuration

Copy `VEGA/appsettings-example.json` to `VEGA/appsettings.json` and fill it in:

| Key | Purpose |
|---|---|
| `botToken` | The bot token from the developer portal. |
| `postgres.connexionString` | `Host=…;Database=…;Username=…;Password=…` |
| `backofficeGuildId` | Guild the backoffice commands are registered on. `null` disables them. |
| `superAdminUserIds` | User IDs allowed to run super-admin commands such as `/feedconfig`. |

`appsettings.json` is gitignored and is never shipped by a deploy.

### Database

The `database/` folder is split in two:

- **`database/createdb.sql`** — one-shot **manual** bootstrap, run as the `postgres` superuser:
  creates the role, the database and the `public` schema privileges. **Not** played by the
  deploy runner. Replace the `<DB_NAME>` / `<DB_USER>` / `<DB_PASSWORD>` placeholders first.

  ```bash
  sudo -u postgres psql -f database/createdb.sql
  ```

- **`database/migrations/`** — idempotent schema migrations (`001_core.sql` … `003_polls.sql`),
  applied in lexicographic order. Each one uses `CREATE … IF NOT EXISTS`, so replaying them on
  every deploy is safe.

  ```bash
  bash scripts/migrate.sh
  ```

  `migrate.sh` reads `postgres.connexionString` straight from `appsettings.json`.

### Run

```bash
dotnet run --project VEGA
```

## Deployment

Copy `deploy.example.ps1` to `deploy.ps1` — the copy is gitignored, since once filled in it
describes where the bot lives rather than what it is. Fill in the SSH host, the remote path and
the systemd service name at the top of the file.

The script publishes a self-contained `linux-arm64` binary, ships it over SSH, applies the
migrations, then restarts the service:

```powershell
.\deploy.ps1                 # publish + deploy + migrate + restart
.\deploy.ps1 -SkipMigrate    # skip the migrations
.\deploy.ps1 -SkipBuild      # deploy an already-published binary
```

The remote `appsettings.json` and `logs/` are preserved — a deploy never overwrites them.
