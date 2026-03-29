# Changelog

## v2.0.0

- Refactored for Procon v2 compatibility
- Moved source to `src/` directory, converted encoding to UTF-8 LF
- Converted code style to System types (String, Int32, Boolean, etc.)
- Added `.editorconfig`, `PBSSE.csproj` for CI format checks
- Added CI and release GitHub Actions workflows

## v1.4.1.1

- Fix: Updated plugin description
- Fix: Changed Include/Exclude VIPs to Sync VIPs

## v1.4.1.0

- Fix: Added psay to ingame notification (no yell in BF4)

## v1.4.0.0

- NEW: BF4 Compatibility
- NEW: Automatic Update Check
- Fix: You can now set the minimum score for players to be checked (replaces exclude players with 0 score = idle players)

## v1.3.2.0

- NEW: Added option to automatically delete Non-ReservedSlots/ServerVIPs from the Whitelist

## v1.3.1

- FIX: Added option to exclude players with 0 score from check (idle players)

## v1.3

- UPDATE: You can now download/update PBSSE directly through your procon gui

## v1.2.5

- NEW: Option to add multiple usernames to get notified (5. Notify -> ingame username)

## v1.2.4

- FIX: Playernames containing spaces were not identified properly

## v1.2.3

- FIX: minor code fixes (everything runs much smoother now)

## v1.2.2

- NEW: Option to log all debug output to a file (PBSSELogFile.txt) in the Plugins/BF3 directory (use with caution)
- NEW: Filter to drop PBScreenshots request if requested too fast after each other (3 minutes)
- FIX: Whitelist was updating too slow under certain circumstances

## v1.2.1

- FIX: Some successfully received screenshots were not counted properly under certain circumstances

## v1.2

- NEW: Option to automatically exclude players with the same ip (same LAN) from check

## v1.1.1

- FIX: statistics (maybe check routine too) where not showing when they should show. this is now fixed

## v1.1

- NEW: Whitelist added (+Option to add ReservedSlots/ServerVIPs)
- FIX: Increased default number of requests to 4
- Minor Fixes

## v1.0

- Public release
