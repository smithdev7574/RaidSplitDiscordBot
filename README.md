# Raid Split Discord Bot
This bot was built to manage the raid splits for an Everquest guild (Altered Minds) on the Aradune server. While most features are custom to everquest and the guild's processes you may be able to customize them to meet your needs. The bot leverages Discord.Net for all discord interaction. Check out their Repo [Here](https://github.com/discord-net/Discord.Net).

## Setup
You will need to register your instance of the bot with discord and should be provided an authentication token to put into the app settings below.  Follow [these](https://discordpy.readthedocs.io/en/stable/discord.html) instructions if you need help setting up your bot.

Make sure Message Content is selected or the bot can't read any messages.
![image](https://user-images.githubusercontent.com/118477422/202597644-ab31d5b5-81b7-4ebe-987e-74a6ab76526e.png)


When creating your invite link you need to make sure the following permissions are checked.
![image](https://user-images.githubusercontent.com/118477422/202589449-483464fe-8c3a-455d-b6a8-9885078a61ec.png)


### appsettings.json
The bot uses an appsettings.json file for customizations. Someday I may move these settings to be per discord guild, but for now its a single configuration. 
* Settings - A section for generic variables the app needs to function
* RaidTypes - Our guild has multiple raids, a main raids and an alt raids. This configuration lets you set the types of raids and character ranks you want to register for each raid event.
```
{
  "Settings":{
    "Token":__YourTokenHere__,
    "BackupPath":__A Path the Bot can use to backup your data files leave blank if you don't want to backup the data__,
    "MessagePrefix": __The characters to start when sending a message to the bot__",
    "RegistrationChannel": "__Name of the channel peple will use to register__",
    "SpamChannel": "__Name of the channel where all the spam can go___",
    "AdminChannel": "__Name of the channel where you want to perform admin commands__",
    "SplitChannel":  "__Name of the channel where splits are announced__"
  }
   "RaidTypes": [
    {
      "Name": "Main",
      "CharacterTypes": [
        {
          "Name": "Main",
          "EmojiCode": "😺"
        },
        {
          "Name": "PrimaryAlt",
          "EmojiCode": "💩"
        }
      ]
    },
    {
      "Name": "Alt",
      "CharacterTypes": [
        {
          "Name": "AltMain",
          "EmojiCode": "🦖"
        },
        {
          "Name": "AltPrimary",
          "EmojiCode": "🍄"
        }
      ]
    },
    {
      "Name": "Free",
      "CharacterTypes": [
        {
          "Name": "Main",
          "EmojiCode": "😺"
        },
        {
          "Name": "PrimaryAlt",
          "EmojiCode": "💩"
        },
        {
          "Name": "AltMain",
          "EmojiCode": "🦖"
        },
        {
          "Name": "AltPrimary",
          "EmojiCode": "🍄"
        },
        {
          "Name": "Alt",
          "EmojiCode": "🌪"
        }
      ]
    }
  ]
}
```
