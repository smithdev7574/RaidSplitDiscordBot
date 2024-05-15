# Raid Split Discord Bot
This bot was built rather quickly to manage the raid splits for an Everquest guild (Altered Minds) on the Aradune server. I recently refactored some of the code trying to add as many settings as possible and make the code a bit more readable.  This is the version after the first refactor. I could have kept going with the refactor, but it should be good enough to use at this point. You may see some unique behavior based on our Guild's processes, but most should be configurable now.

It was built using .Net 6 in Visual Studio Community 2022. The bot also leverages Discord.Net for all discord interaction. Check out their Repo [Here](https://github.com/discord-net/Discord.Net). 

## May 2024 Update
I took a moment to upgrade the Bot to use DotNet 8 and I also updated to the latest version of Discord.Net (3.15.0). I haven't ran extensive tests with the upgraded frameworks, but there didn't seem to be any deprecated features the bot uses.

## Usage
1. Create Your Event
   - Create your event by typing !rr CreateEvent followed by a name, the raid type, and the date and time of the event.
   - The Event Name must be unique compared to the other active events.
   - Events become inactive 
2. Wait for People to Sign up
   - People will use the emoji icons to register and the bot should update the registration message with counts.
   - If the bot seems to be ignoring responses you may need to try and create a new event. The bot writes down the registration message ID for each event and uses that message ID to know which event people are signing up for. If for some reason the message was posted, but the bot couldn't write down the ID it will be unable to recover.
3. Preview your Splits
   - Have the bot create a preview of the splits in the admin channel by typing !rr PreviewEvent followed by the event name and number of splits.
   - This will give you an opportunity to review how the bot split your raid force and let you move people around by replying to messages in the admin channel.
   - If you find yourself moving people around every raid try the Anchor, PreSplit and Buddie setup features.
4. Finalize the Event
   - Once you are happy with your split composition finalize the event by typing !rr FinalizeEvent followed by the event name
   - This will post the split compositions to the raid split channel so your raid force knows which raid to join.

### Additional Features
- Buddies
  - The Buddy feature allows you to group people who like to play together. Think of spouses or RL friends. The bot will do its best to keep Buddies in the same raid.
  - Use the !rr SetBuddies command followed by as many character names as you want to create a group of buddies
  - You can remove Buddies later by using the !rr RemoveBuddies command
- Anchors
  - The Anchors feature allows you to identify your top raid members to avoid them from all being put into the same raid, by setting anchors the bot will attempt to split this group first before adding the rest of your raid force.
  - Use the !rr AddAnchors command followed by as many character names as you want to add players to your group of anchors.
  - You can remove anchors by using the !rr RemoveAnchors command
- PreSplits
   - This is really V2 of the anchor system which gives you the ability to create a static list of splits that always start with the same group of people. Depending on the amount of splits you want the bot will randomly choose that many splits from your list of pre-configured PreSplits, and apply the remaining raid force to each split.
   - Use !rr CreatePreSplit followed by a name, leader, looter and inviter (they can all be the same person if you want)
   - There are lots of other commands around PreSplits use the !rr HelpPreSplitAdmin command for more information


## Setup
Register your instance of the bot with discord and an authentication token should be provided the app settings below. (Some Day I will move this to a bit more secure location in the app).  Here is a good [guide](https://discordpy.readthedocs.io/en/stable/discord.html) on setting up your bot.

Make sure Message Content is selected or the bot can't read any messages. I was having problems with discord.Net and the message's content being blank so I turned on all contents as I didn't want to mess w/ it.
![image](https://user-images.githubusercontent.com/118477422/202597644-ab31d5b5-81b7-4ebe-987e-74a6ab76526e.png)

When creating an invite link make sure the following permissions are checked.
![image](https://user-images.githubusercontent.com/118477422/202589449-483464fe-8c3a-455d-b6a8-9885078a61ec.png)


### appsettings.json
The bot uses an appsettings.json file for customizations. Someday I may move these settings to be per discord guild, but for now its a single configuration. 
* Settings - A section for generic variables the app needs to function
    * The Two Path settings (DataFileDirectoryPath and TempFileDirectoryPath) are settings to tell the bot where to do I/O make sure the bot has access to these directories.
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
    "SplitChannel":  "__Name of the channel where splits are announced__",
    "DataFileDirectoryPath": "__DirectoryTheBotHasAccessToStoreTheDataFile__",
    "TempFileDirectoryPath": "__DirectoryTheBotHasAccessToStoreTheTempFiles__"
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
