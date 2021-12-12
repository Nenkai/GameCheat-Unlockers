# GridBonusUnlocker

Edits Race Driver Grid save files to unlock bonus codes.

## Research and Notes

The process is mostly the same as Race Driver 3. Generator cannot be made due to the use of an RSA Private Key.

The registry key has been changed to `HKCU\Software\Codemasters\UUID`, to which a `GRiD` value can be found that generates the access code.