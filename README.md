# PBSSE - PunkBuster ScreenShot Enforcer

A Procon plugin that monitors PunkBuster screenshot requests and enforces compliance. Players who block PunkBuster screenshots can be automatically kicked, temp-banned, or permanently banned.

## Features

- **Screenshot monitoring** — Tracks PunkBuster screenshot requests and successful responses per player
- **Configurable enforcement** — Choose between kick, temp-ban, or permanent ban for non-compliant players
- **Whitelist** — Exempt specific players from checks
- **VIP/Reserved Slots sync** — Automatically whitelist players in the server's Reserved Slots list
- **Duplicate IP detection** — Skip enforcement for players sharing the same IP (LAN players)
- **Score threshold** — Exclude idle/low-score players from checks
- **In-game admin notification** — Yell/say alerts to specified admin players when violations are detected
- **File logging** — Optional debug output to a log file
- **Configurable ban method** — Ban by name, EA GUID, or PB GUID

## Supported Games

- Battlefield 3 (BF3)
- Battlefield 4 (BF4)

## Installation

1. Download `PBSSE.cs` from the [Releases](../../releases) page
2. Place it in your Procon `Plugins/BF4/` (or `Plugins/BF3/`) directory
3. Restart Procon or reload plugins
4. Enable the plugin in the Procon plugin panel

## PunkBuster Configuration

You must enable automatic PunkBuster screenshots in your `pbsv.cfg`:

```
pb_sv_AutoSs 1           // Enable auto screenshots
pb_sv_AutoSsFrom 600     // Min seconds between screenshot requests
pb_sv_AutoSsTo 1200      // Max seconds between screenshot requests
```

## Settings

### 1. Check
- **Max requests before action** — Number of screenshot requests before enforcement (default: 4, range: 3-10)
- **Exclude same-IP players** — Skip players sharing an IP address (default: Yes)
- **Minimum score threshold** — Skip players below this score (default: 500)
- **Ban method** — By name, EA GUID, or PB GUID
- **Whitelist** — Player names exempt from checks
- **Sync Reserved Slots** — Auto-sync whitelist with server VIP list (default: Yes)

### 2. Kick
- **Enable kick** — Kick players with no successful screenshots (default: No)
- **Kick message** — Message shown to kicked player (supports `%maxreqs%` placeholder)

### 3. Temp Ban
- **Enable temp ban** — Temp-ban instead of kick (default: Yes)
- **Temp ban message** — Message shown to temp-banned player
- **Temp ban duration** — Length in minutes (default: 15)

### 4. Permanent Ban
- **Enable ban** — Permanently ban instead of kick/temp-ban (default: No)
- **Ban message** — Message shown to banned player

### 5. Notify
- **Enable in-game notification** — Alert admin players via yell/say (default: Yes)
- **Admin usernames** — Pipe-separated list of players to notify
- **Display time** — Notification display duration in seconds (default: 30)

### 6. Debug
- **Debug level (0-5)** — Controls console output verbosity
  - 0: No messages
  - 1: Only kicks/bans
  - 2: Statistics and resets
  - 3: Individual requests/receives
  - 4: List add/remove operations
  - 5: Development/testing
- **Log to file** — Write debug output to a file (default: No)

## Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for code style checks only)

### Code Style Checks

```bash
dotnet restore
dotnet format whitespace --verify-no-changes
dotnet format style --verify-no-changes --severity warn --exclude-diagnostics IDE1007
```

## Author

**onegrizzlybeer**

- Forum: https://forum.myrcon.com/member.php?13930-grizzlybeer
- Plugin thread: https://forum.myrcon.com/showthread.php?5202-PunkBuster-ScreenShot-Enforcer-1-3-2-0

## License

This project is licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for details.
