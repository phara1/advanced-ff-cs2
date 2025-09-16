# Advanced Friendly Fire 
- Enable damage inflictors for friendly fire
- Revamp of [CS2 - Damage Management](https://github.com/hoan111/CS2-DamageManagement)

- If you found any bugs, please report it so I can fix it
- If you have any ideas for this project, I'm more than happy to hear your request.

![image](https://github.com/user-attachments/assets/fbd0632d-283a-4204-a763-89bb927e4624)
> [!WARNING]
> As with the latest update, [Advanced Friendly Fire v1.1], which introduces the punishment features, has only been minimally tested (primarily with bots), and the code is a total mess, but will be fixed soon. Please report any bugs you encounter in the [issues](https://github.com/phara1/advanced-ff-cs2/issues) section

# Requirements
> [CounterStrikeSharp](https://docs.cssharp.dev/) installed.

> mp_friendlyfire 1

# Features
- Manage Friendly Fire Damage Inflictors

- Track players who inflict friendly fire damage.
- Warning System Based on Team Damage

- Punishments are issued based on the amount of team damage inflicted.
- System allows up to 3 warnings.
- Each Warning Has 3 Configurable Settings

- Damage HP Metric
- - Define the amount of damage required to trigger a warning.
- Warning Chat Message
- - Send a custom message to the player when a warning is issued.
- Warning Punishment
- - Apply a server command (e.g., css_slay) to the player.
- - - Use {Player} as a placeholder in the command for the player name.
- Persistent Damage Tracking
- - Player inflicted team damage is stored even if they disconnect and reconnect to the server, making them unable to avoid warnings.

# Installation
- Download the [latest release](https://github.com/phara1/advanced-ff-cs2/releases)
- Paste the ```advanced_friendlyfire``` folder inside your plugins folder

# Config
```json
{
  "Enable/Disable Advanced Friendly Fire": true,
  "Enable/Disable Punishments": true,
  "Damage Inflictors": [
    "inferno",
    "hegrenade_projectile",
    "flashbang_projectile",
    "smokegrenade_projectile",
    "decoy_projectile",
    "planted_c4"
  ],
  "Warning #1 Required Team Damage (HP Metrics)": 100, 
  "Warning #1 Chat message": "Avoid friendly fire, or you will be punished! Friendly fire warning [1/3]",
  "Warning #1 Punishment": "css_slay {Player}",
  "Warning #2 Required Team Damage (HP Metrics)": 200,
  "Warning #2 Chat message": "You have been kicked for dealing excessive damage to your teammates!",
  "Warning #2 Punishment": "css_kick {Player} \"Friendly fire warning [2/3]\"",
  "Warning #3 Required Team Damage (HP Metrics)": 300,
  "Warning #3 Chat message": "You have been banned for dealing excessive damage to your teammates!",
  "Warning #3 Punishment": "css_ban {Player} 30 \"Friendly fire warning [3/3]\"",
  "ConfigVersion": 1
}
```
- A list of Damage Inflictors can be found here: [Entity List](https://cs2.poggu.me/dumped-data/entity-list/)

# Credits
- Idea taken from the original plugin author - https://github.com/hoan111
