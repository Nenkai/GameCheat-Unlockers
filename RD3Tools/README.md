# RD3Tools

Fixes up saves checksum when editing a RD3 Save (prevents 'DAMAGED') & optionally unlock bonus codes.

## Research and Notes

On Windows, the game attempts to query the `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion->ProductId` value to generate an access code (4 digits).

However on newer Windows'es/64bit the registry key is altered to query `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion`, but this node does not exist there. Therefore the access code field will display as `????`. Adding the registry key manually is required for the game to properly generate an access code.

Generating codes for this game is not possible, as an asymmetric encryption (most likely RSA) was used in order to generate codes for players to input, with the private key unknown. A code verifier exists within the code of this tool.

The save file holds the access code twice, and the access codes in the save file MUST match the one that the game generates with the `ProductId`.

# Cheat Table

The code allows certain cheats to be inputted without any crypto/hash check. It is the case for the `NODISC` cheat.

| Id |  Name   |           Comment           | Notes |
|----|:-------:|:---------------------------:|-------|
| 0  | CHAMPS  | Unlock Championships        |N/A
| 1  | XCHAMPS | Unlock Bonus Championships  |N/A
| 2  | PALMER  | Boost For All Cars          |N/A
| 3  | JUMP    | Turbo Boost                 |N/A
| 4  | TOYCAR  | Toy Cars                    |N/A
| 5  | SLOT    | Unlock Slot Racer           |N/A
| 6  | NODENT  | Invincible Cars             |N/A
| 7  | THEEND  | Unlock Cutscenes            |N/A
| 8  | ?       | ?                           |Free slot.
| 9  | HONDA   | Unlock Honda                |N/A
| 10 | XHONDA  | Unlock Honda 2006           |N/A
| 11 | NODISC  | No Streamed Car Sound       |Can be inputed as is.
