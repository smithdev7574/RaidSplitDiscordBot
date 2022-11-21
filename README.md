# Raid Split Discord Bot
This bot was built rather quickly to manage the raid splits for an Everquest guild (Altered Minds) on the Aradune server. I recently refactored some of the code trying to add as many settings as possible and make the code a bit more readable.  This is the version after the first refactor. I could have kept going with the refactor, but it should be good enough to use at this point. You may see some unique behavior based on our Guild's processes, but most should be configurable now.

It was built using .Net 6 in Visual Studio Community 2022 using .Net 6. The bot also leverages Discord.Net for all discord interaction. Check out their Repo [Here](https://github.com/discord-net/Discord.Net). 

## Setup
Register your instance of the bot with discord and an authentication token should be provided the app settings below. (Some Day I will move this to a bit more secure location in the app).  Here is a good [guide](https://discordpy.readthedocs.io/en/stable/discord.html) on setting up your bot.

Make sure Message Content is selected or the bot can't read any messages. I was having problems with discord.Net and the message's content being blank so I turned on all contents as I didn't want to mess w/ it.
![image](https://user-images.githubusercontent.com/118477422/202597644-ab31d5b5-81b7-4ebe-987e-74a6ab76526e.png)

When creating an invite link make sure the following permissions are checked.
![image](https://user-images.githubusercontent.com/118477422/202589449-483464fe-8c3a-455d-b6a8-9885078a61ec.png)


### appsettings.json
The bot uses an appsettings.json file for customizations. Someday I may move these settings to be per discord guild, but for now its a single configuration. 
* Settings - A section for generic variables the app needs to function
* RaidTypes - Our guild has multiple raids, a main raids and an alt raids. This configuration lets you set the types of raids and character ranks you want to register for each raid event.
* The emoji for a character type drives the reactions users will use to register.
* Character weight helps the split logic when boxes are involved so one box doesn't equal one main
* Classes - standard everquest classes, the bot tries to split classes evenly.
* An emoji code for classes is optional, our server uses custom emohis for EQ classes. 
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
  },
  "RaidTypes": [
    {
      "Name": "Main",
      "CharacterTypes": [
        {
          "Name": "Main",
          "EmojiCode": "😺",
          "CharacterWeight": 1.0
        },
        {
          "Name": "PrimaryAlt",
          "EmojiCode": "💩",
          "IsBox": true,
          "CharacterWeight": 0.75
        }
      ]
    },
    {
      "Name": "Alt",
      "CharacterTypes": [
        {
          "Name": "AltMain",
          "EmojiCode": "🦖",
          "CharacterWeight": 1.0
        },
        {
          "Name": "AltPrimary",
          "EmojiCode": "🍄",
          "IsBox": true,
          "CharacterWeight": 0.75
        }
      ]
    },
    {
      "Name": "Free",
      "CharacterTypes": [
        {
          "Name": "Main",
          "EmojiCode": "😺",
          "CharacterWeight": 1.0
        },
        {
          "Name": "PrimaryAlt",
          "EmojiCode": "💩",
          "IsBox": true,
          "CharacterWeight": 0.75
        },
        {
          "Name": "AltMain",
          "EmojiCode": "🦖",
          "CharacterWeight": 1.0
        },
        {
          "Name": "AltPrimary",
          "EmojiCode": "🍄",
          "IsBox": true,
          "CharacterWeight": 0.75
        },
        {
          "Name": "Alt",
          "EmojiCode": "🌪",
          "IsBox": true,
          "CharacterWeight": 0.75
        }
      ]
    }
  ],
  "Classes": [
    {
      "Name": "Bard",
      "EmojiCode": "<:bard:556197385554886677>",
      "ShortName": "BRD",
      "IsMelee": true
    },
    {
      "Name": "Beastlord",
      "EmojiCode": "<:beastlord:556197853337223174>",
      "ShortName": "BST",
      "IsMelee": true
    },
    {
      "Name": "Berserker",
      "EmojiCode": "<:gnome:556200261530550286>",
      "ShortName": "BER",
      "IsMelee": true
    },
    {
      "Name": "Cleric",
      "EmojiCode": "<:cleric:556197385760538634>",
      "ShortName": "CLR"
    },
    {
      "Name": "Druid",
      "EmojiCode": "<:druid:556197853043884042>",
      "ShortName": "DRU"
    },
    {
      "Name": "Enchanter",
      "EmojiCode": "<:enchanter:556197386008133645>",
      "ShortName": "ENC",
      "IsCaster": true
    },
    {
      "Name": "Magician",
      "EmojiCode": "<:magician:556197385819258900>",
      "ShortName": "MAG",
      "IsCaster": true
    },
    {
      "Name": "Monk",
      "EmojiCode": "<:monk:556195589965086753>",
      "ShortName": "MNK",
      "IsMelee": true
    },
    {
      "Name": "Necromancer",
      "EmojiCode": "<:necromancer:556195589961023489>",
      "ShortName": "NEC",
      "IsCaster": true
    },
    {
      "Name": "Paladin",
      "EmojiCode": "<:paladin:556197385991094282>",
      "ShortName": "PAL",
      "IsMelee": true
    },
    {
      "Name": "Ranger",
      "EmojiCode": "<:ranger:556197385798287380>",
      "ShortName": "RNG",
      "IsMelee": true
    },
    {
      "Name": "Rogue",
      "EmojiCode": "<:rogue:556195589554044950>",
      "ShortName": "ROG",
      "IsMelee": true
    },
    {
      "Name": "Shadow Knight",
      "EmojiCode": "<:shadowknight:556197385924116528>",
      "ShortName": "SHD",
      "IsMelee": true
    },
    {
      "Name": "Shaman",
      "EmojiCode": "<:shaman:556195589721948161>",
      "ShortName": "SHM"
    },
    {
      "Name": "Warrior",
      "EmojiCode": "<:warrior:556197385643098118>",
      "ShortName": "WAR",
      "IsMelee": true
    },
    {
      "Name": "Wizard",
      "EmojiCode": "<:wizard:556197385752281093>",
      "ShortName": "WIZ",
      "IsCaster": true
    }
  ]
}
```
