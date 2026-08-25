// Reminder feature — placeholder, not yet wired up.
//
// No migration provisions the `reminders` table any more: the service is not
// implemented/registered, so the schema stays out of fresh installs. The full working
// implementation (timers, snooze, per-user listing) was removed from this PR to avoid
// shipping a wall of dead code — recover it and the migration from git history if/when
// reminders ship, then re-register the service in Program.cs and uncomment
// Commands/SlashCommands/Reminders.cs.
//
// TODO: implement reminders.

namespace Services;
