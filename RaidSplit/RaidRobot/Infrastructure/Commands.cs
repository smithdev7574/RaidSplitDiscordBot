using Discord.Commands;
using RaidRobot.Logic;
using RaidRobot.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Infrastructure
{
    public partial class Commands :ModuleBase<SocketCommandContext>
    {
        private readonly IRaidSplitConfiguration config;
        private readonly IPermissionChecker permissionChecker;
        private readonly IEventCreator eventCreator;

        public Commands(
            IRaidSplitConfiguration config,
            IPermissionChecker permissionChecker, 
            IEventCreator eventCreator)
        {
            this.config = config;
            this.permissionChecker = permissionChecker;
            this.eventCreator = eventCreator;
        }

        [Command("Help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var sb = new StringBuilder();

            sb.AppendLine("__**User Commands**__");
            sb.AppendLine($"**{config.Settings.MessagePrefix} ClassIS** followed by your character's name (in game) then your character's class. Use this command when the Bot can't figure out a character's class.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} MyAltMainIs** followed by your character's name (in game). Use this command when the Bot can't figure out your *Alt Main* character.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} MyAltPrimaryIs** followed by your character's name (in game). Use this command when the Bot can't figure out your *Alt Primary* character.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} MyMainIs** followed by your character's name (in game). Use this command when the Bot can't figure out your *Main* character.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} MyPrimaryAltIs** followed by your character's name (in game). Use this command when the Bot can't figure out your *Primary Alt* character.");
            sb.AppendLine(string.Empty);

            sb.AppendLine("__**Admin Commands**__");

            sb.AppendLine($"**{config.Settings.MessagePrefix} HelpRaidAdmin** for Admin Commands to manage raid events.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} HelpRosterAdmin** for Admin Commands to manage the roster.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} HelpPreSplitAdmin** for Admin Commands to manage the pre splits.");

            await ReplyAsync(sb.ToString());
        }

        [Command("HelpRaidAdmin")]
        public async Task HelpRaidAdmin()
        {
            var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Help Raid Admin");
            if(!permissionResult.HasPremission)
            {
                await ReplyAsync(permissionResult.Message);
                return;
            }

            var sb = new StringBuilder();

            sb.AppendLine("__**Admin Raid Commands**__");
            sb.AppendLine($"**{config.Settings.MessagePrefix} CancelEvent** followed by the Event Name remove an existing event from the bot.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} CheckRegistration** followed by the event name to see who has signed up for an event and their status with the bot.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} CreateEvent** followed by the event name and Raid Type (Main, Alt, Free) to create a raid event that users can sign up to attend.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} FinalizeEvent** followed by the event name when you are ready to commit to the splits for the event.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} MoveTo** followed by the event name the character name you want to move and then the split number (You can use this to add new people too).");
            sb.AppendLine($"**{config.Settings.MessagePrefix} NeedsItem** followed by the event name then the item name (no spaces) followed by the character names (spaces between) that need the item. The bot will try to avoid putting these characters together in a split.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} OpenEvent** followed by the event name if you need to unlock the event and work on the splits.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} PreviewEvent** followed by the event name and number of splits to have the bot create preview's of the splits.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} Remove** followed by the event name and the character Name to Remove Them.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} SimulateRegistration** followed by the event name to have the bot randomly perform the split behavior **Testing Only**.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} SplitASplit** followed by the event name, the split number and how many splits you want to make to create splits from an existing split.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} Swap** followed by the event name then the two character names you want to swap between splits.");

            sb.AppendLine(string.Empty);

            await ReplyAsync(sb.ToString());
        }

        [Command("HelpPreSplitAdmin")]
        public async Task HelpPreSplitAdmin()
        {
            var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Help PreSplit Admin");
            if (!permissionResult.HasPremission)
            {
                await ReplyAsync(permissionResult.Message);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("__**Admin Pre Split Commands**__");
            sb.AppendLine($"**{config.Settings.MessagePrefix} CreatePreSplit** followed by a name, leader, looter and inviter to Create a Pre Split.");
            sb.AppendLine($"*EX: {config.Settings.MessagePrefix} CreatePreSplit PreSplit1 Lilmeech Pota Ivoridawn*");
            sb.AppendLine($"**{config.Settings.MessagePrefix} SetPreSplitRole** followed by the name of the pre split, character name then role (Leader, Looter, Inviter) to set the role.");
            sb.AppendLine($"*Ex: {config.Settings.MessagePrefix} SetPreSplitRole PreSplit1 Pota Looter*");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemovePreSplit** followed by the name of the pre split to remove it.");
            sb.AppendLine($"*Ex: {config.Settings.MessagePrefix} RemovePreSplit PreSplit1*");
            sb.AppendLine($"**{config.Settings.MessagePrefix} AddPreSplitCharacters** followed by the name of the pre split then character names you wish to add (space delimited).");
            sb.AppendLine($"*Ex: {config.Settings.MessagePrefix} AddPreSplitCharacters PreSplit1 Jinoy Kinetic Kolevii*");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemovePreSplitCharacters** followed by the name of the pre split then character names you wish to remove (space delimited).");
            sb.AppendLine($"*Ex: {config.Settings.MessagePrefix} RemovePreSplitCharacters PreSplit1 Jinoy Kinetic Kolevi*");
            sb.AppendLine($"**{config.Settings.MessagePrefix} ViewPreSplits** to see the currnet pre splits.");

            await ReplyAsync(sb.ToString());

        }

        [Command("HelpRosterAdmin")]
        public async Task HelpUserAdmin()
        {

            var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Help Roster Admin");
            if (!permissionResult.HasPremission)
            {
                await ReplyAsync(permissionResult.Message);
                return;
            }

            var sb = new StringBuilder();

            sb.AppendLine("__**Admin User Management Commands**__");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemoveAnchors**  followed by the character names (spaces between) to remove their anchor status.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemoveBuddies** followed by the character names (spaces between) to remove from a buddy group.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemoveInviters**  followed by the character names (spaces between) to remove their ability to be a raid inviter.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemoveLeaders**  followed by the character names (spaces between) to remove their ability to be a raid leader.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} RemoveLooters**  followed by the character names (spaces between) to remove their ability to be a master looter.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} ManualMap**  followed by the character name, the character type then the discord user name to manually map a user to a character.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} AddAnchors** followed by the character names (spaces between) to set their anchor status.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} SetBuddies** followed by the character names (spaces between) to add as a buddy group.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} AddInviters** followed by the character names (spaces between) to grant them ability to be a raid inviter.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} AddLeaders** followed by the character names (spaces between) to grant them ability to be a raid leader.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} AddLooters** followed by the character names (spaces between) to grant them ability to be a master looter.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} Unmap** followed by a character name to remove the mapping between the character and discord user.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} ViewBuddies** to see the buddies.");
            sb.AppendLine($"**{config.Settings.MessagePrefix} UpdateRoster** to tell the bot you are about to post the guild dump from EQ and it will start looking for your post.");
            sb.AppendLine(string.Empty);

            await ReplyAsync(sb.ToString());
        }

    }
}
