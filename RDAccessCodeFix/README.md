# Race Driver Access Code Fix

This plugin unlocks plaintext cheat codes in TOCA Race Driver 3, so cheat codes can be used without an access code.
Additionally, a registry query used by the game to compute the access code has been fixed for 64-bit systems and/or
Windows 10, so the access code will not show as `????` anymore.

Refer to [RD3Tools](../RD3Tools/#cheat-table) for a full list of cheats.

## Research and Notes

On Windows, the game attempts to query the `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion->ProductId` value to generate an access code (4 digits).
This is invalid for two reasons:
1. This registry key is not present on Windows 10 anymore, since it has always been a mirrored key, presumably for legacy reasons.
   The correct registry key to use here was always `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion->ProductId`.
2. `ProductId` is one of the keys that are not mirrored on 64-bit systems to a 32-bit registry key, so the game was unable to query it as-is.

This plugin corrects both issues, querying the key from a correct place and explicitly specifying to query from a native registry view.

***

Requires [Ultimate ASI Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases/latest/download/Ultimate-ASI-Loader.zip).
