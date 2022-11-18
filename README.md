# Raid Split Discord Bot
This bot was built to manage the raid splits for an Everquest guild (Altered Minds) on the Aradune server. While most features are custom to everquest and the guild's processes you may be able to customize them to meet your needs. The bot leverages Discord.Net for all discord interaction. Check out their Repo [Here](https://github.com/discord-net/Discord.Net).

## Setup
You will need to register your instance of the bot with discord and should be provided an authentication token to put into the app settings below.  Follow [these](https://discordpy.readthedocs.io/en/stable/discord.html) instructions if you need help setting up your bot. When creating your invite link you need to make sure the following permissions are checked.
![image](https://user-images.githubusercontent.com/118477422/202589449-483464fe-8c3a-455d-b6a8-9885078a61ec.png)


### appsettings.json
The bot uses an appsettings.json file for customizations. Someday I may move these settings to be per discord guild, but for now its a single configuration.  
```
{
  "Settings":{
    "Token":__YourTokenHere__,
    "BackupPath":__A Path the Bot can use to backup your data files leave blank if you don't want to backup the data__
  }
}
```
