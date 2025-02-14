/*************************************************************************
 *
 *   file		: Guild.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate$
 *   last author	: $LastChangedBy$
 *   revision		: $Rev$
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Synchronization;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Guilds
{
    public partial class Guild : INamed, IEnumerable<GuildMember>, IChatTarget
    {
        #region Fields
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        internal SimpleLockWrapper syncRoot;
        private GuildMember m_leader;
        private ImmutableList<GuildRank> m_ranks;

        public readonly IDictionary<uint, GuildMember> Members = new Dictionary<uint, GuildMember>();
        private List<HistoryRecord> _history;
        private GuildSkill[] _skills;

        #endregion

        #region Properties
        /// <summary>
        /// Id of this guild
        /// </summary>
        /// <remarks>UpdateField's GuildId is equal to it</remarks>
        public uint Id
        {
            get { return (uint)_id; }
        }
        /// <summary>
        /// Guild's name
        /// </summary>
        /// <remarks>length is limited with MAX_GUILDNAME_LENGTH</remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.GuildNameChanged); }
        }

        /// <summary>
        /// Guild's message of the day
        /// </summary>
        /// <remarks>length is limited with MAX_GUILDMOTD_LENGTH</remarks>
        public string MOTD
        {
            get { return _MOTD; }
            set
            {
                if (value != null && value.Length > GuildMgr.MaxGuildMotdLength)
                    return;

                _MOTD = value;

                //GuildHandler.SendGuildRosterToGuildMembers(this);
                //GuildHandler.SendEventToGuild(this, GuildEvents.MOTD);
                this.UpdateLater();
            }
        }

        /// <summary>
        /// Guild's information
        /// </summary>
        /// <remarks>length is limited with MAX_GUILDINFO_LENGTH</remarks>
        public string Info
        {
            get { return _info; }
            set
            {
                if (value != null && value.Length > GuildMgr.MaxGuildInfoLength)
                    return;

                _info = value;

                GuildHandler.SendGuildRosterToGuildMembers(this);
                this.UpdateLater();
            }
        }

        /// <summary>
        /// Date and time of guild creation
        /// </summary>
        public DateTime Created
        {
            get { return _created; }
        }

        /// <summary>
        /// Guild leader's GuildMember
        /// Setting it does not send event to the guild. Use Guild.SendEvent
        /// </summary>
        public GuildMember Leader
        {
            get { return m_leader; }
            set
            {
                if (value == null || value.Guild != this)
                    return;

                m_leader = value;
                _leaderLowId = (int)value.Id;

                this.UpdateLater();
            }
        }

        /// <summary>
        /// Guild's tabard
        /// </summary>
        public GuildTabard Tabard
        {
            get { return _tabard; }
            set
            {
                _tabard = value;
                this.UpdateLater();
            }
        }

        /// <summary>
        /// Number of guild's members
        /// </summary>
        public int MemberCount
        {
            get { return Members.Count; }
        }

        public GuildEventLog EventLog
        {
            get;
            private set;
        }

        public GuildBank Bank
        {
            get;
            private set;
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor is implicitely called when Guild is loaded from DB
        /// </summary>
        public Guild()
            : this(false)
        {

        }

        protected Guild(bool isNew)
        {
            syncRoot = new SimpleLockWrapper();
            EventLog = new GuildEventLog(this, isNew);
            Bank = new GuildBank(this, isNew);
        }

        /// <summary>
        /// Creates a new GuildRecord row in the database with the given information.
        /// </summary>
        /// <param name="leader">leader's character record</param>
        /// <param name="name">the name of the new character</param>
        /// <returns>the <seealso cref="Guild"/> object</returns>
        public Guild(CharacterRecord leader, string name)
            : this(true)
        {
            _created = DateTime.Now;
            _id = _idGenerator.Next();
            _leaderLowId = (int)leader.EntityLowId;
            _name = name;
            _tabard = new GuildTabard();
            _MOTD = "Default MOTD";
            _info = "Default info";
            _level = 1;
            m_ranks = GuildMgr.CreateDefaultRanks(this);
            foreach (var guildRank in m_ranks)
            {
                guildRank.CreateLater();
            }
            //m_leader = new GuildMember(leader, this, HighestRank);

            Register();

            m_leader = AddMember(leader);
            var chr = World.GetCharacter(leader.Name, true);
            Leader.Character = chr;

            //Set the leader as guild master rank
            m_leader.RankId = 0;
            _clanCrest = new byte[40];
            this.CreateLater();						// save to DB, asynchronously

        }
        #endregion

        #region Init
        /// <summary>
        /// Initialize the Guild after loading from DB
        /// </summary>
        internal void InitAfterLoad()
        {
            var ranks = GuildRank.FindAll(this);

            if (ranks.Length == 0)
            {
                log.Warn(string.Format("Guild {0} did not have ranks - Recreating default Ranks.", this));
                m_ranks = GuildMgr.CreateDefaultRanks(this);
            }
            else
            {
                m_ranks = new ImmutableList<GuildRank>(ranks.OrderBy(rank => rank.RankIndex));
            }

            foreach (var rank in m_ranks)
            {
                rank.InitRank();
            }

            var members = GuildMember.FindAll(Id);
            foreach (var gm in members)
            {
                gm.Init(this, World.GetCharacter((uint)gm.CharacterLowId));
                Members.Add(gm.Id, gm);
            }

            var skills = GuildSkill.FindAll(this);
            foreach (var guildSkill in skills)
            {
                guildSkill.InitAfterLoad(this);
                if (Skills[(int)guildSkill.Id] == null)
                    Skills[(int)guildSkill.Id] = guildSkill;
                else
                {
                    guildSkill.DeleteLater();
                }
            }
            m_leader = this[LeaderLowId];
            if (m_leader == null)
            {
                OnNoLeaderFound();
            }
            if (m_leader != null)
            {
                Register();
            }
        }

        /// <summary>
        /// Initializes guild after its creation or restoration from DB
        /// </summary>
        internal void Register()
        {
            GuildMgr.Instance.RegisterGuild(this);
        }
        #endregion

        #region GuildMembers
        public GuildMember AddMember(Character chr)
        {
            var member = AddMember(chr.Record);
            if (member != null)
            {
                member.Character = chr;
            }
            return member;
        }

        /// <summary>
        /// Adds a new guild member
        /// Calls GuildMgr.OnJoinGuild
        /// </summary>
        /// <param name="chr">character to add</param>
        /// <param name="update">if true, sends event to the guild</param>
        /// <returns>GuildMember of new member</returns>
        public GuildMember AddMember(CharacterRecord chr)
        {
            GuildMember newMember;

            /*using (syncRoot.Enter())
            {*/
            lock (this)
            {
                if (Members.TryGetValue(chr.EntityLowId, out newMember))
                {
                    return newMember;
                }
                newMember = new GuildMember(chr, this, m_ranks.Last());
                Members.Add(newMember.Id, newMember);
                newMember.Create();
            }
            /*}*/

            GuildMgr.Instance.RegisterGuildMember(newMember);

            EventLog.AddJoinEvent(newMember.Id);
            GuildHandler.SendEventToGuild(this, GuildEvents.JOINED, newMember);
            var character = World.GetCharacterByAccId((uint)chr.AccountId);
            foreach (var activeSkill in ActiveSkills)
            {
                activeSkill.ApplyToCharacter(character);
            }
            character.GuildMember = newMember;
            Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.Joined, newMember);
            Asda2GuildHandler.SendGuildMembersInfoResponse(character.Client, this);
            AddHistoryMessage(Asda2GuildHistoryType.Joined, 0, chr.Name, DateTime.Now.ToLongTimeString());
            return newMember;
        }

        public IEnumerable<GuildSkill> ActiveSkills
        {
            get { return Skills.Where(s => s != null && s.IsActivated); }
        }

        public bool RemoveMember(uint memberId)
        {
            var member = this[memberId];
            if (member != null)
            {
                return RemoveMember(member, true);
            }
            return false;
        }

        public bool RemoveMember(string name, bool kicked = false)
        {
            var member = this[name];
            if (member != null)
            {
                return RemoveMember(member, true, kicked);
            }
            return false;
        }

        /// <summary>
        /// Removes GuildMember from the guild
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if true, sends event to the guild</param>
        public bool RemoveMember(GuildMember member)
        {
            return RemoveMember(member, true);
        }

        /// <summary>
        /// Removes GuildMember from the guild
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if false, changes to the guild will not be promoted anymore (used when the Guild is being disbanded)</param>
        public bool RemoveMember(GuildMember member, bool update, bool kicked = false)
        {
            AddHistoryMessage(kicked ? Asda2GuildHistoryType.Kicked : Asda2GuildHistoryType.Left, 0, member.Name, DateTime.Now.ToLongTimeString());
            if (member.Character != null)
            {
                foreach (var activeSkill in ActiveSkills)
                {
                    activeSkill.RemoveFromCharacter(member.Character);
                }
            }
            if (update)
            {
                EventLog.AddLeaveEvent(member.Id);
                Asda2GuildHandler.SendGuildNotificationResponse(this, kicked ? GuildNotificationType.Kicked : GuildNotificationType.Left, member);
                //GuildHandler.SendEventToGuild(this, GuildEvents.LEFT, member);
            }
            OnRemoveMember(member);

            if (update && member == m_leader)
            {
                Disband();
                return true;
            }

            /*using (syncRoot.Enter())
            {*/
            lock (this)
            {
                if (!Members.Remove(member.Id))
                {
                    return false;
                }
            }
            //}



            RealmServer.IOQueue.AddMessage(() =>
            {
                member.Delete();
                if (update)
                {
                    Update();
                }
            });
            return true;
        }

        /// <summary>
        /// Called before the given member is removed to clean up everything related to the given member
        /// </summary>
        protected void OnRemoveMember(GuildMember member)
        {
            GuildMgr.Instance.UnregisterGuildMember(member);

            var chr = member.Character;
            if (chr != null)
            {
                chr.GuildMember = null;
            }
        }

        private void OnNoLeaderFound()
        {
            // leader was deleted
            var highestRank = int.MaxValue;
            GuildMember highestMember = null;
            foreach (var member in Members.Values)
            {
                if (member.RankId < highestRank)
                {
                    highestRank = member.RankId;
                    highestMember = member;
                }
            }

            if (highestMember == null)
            {
                Disband();
            }
        }
        #endregion

        #region Ranks
        public GuildRank HighestRank
        {
            get { return m_ranks[0]; }
        }

        public GuildRank LowestRank
        {
            get
            {
                var ranks = Ranks;
                return ranks[ranks.Length - 1];
            }
        }

        /// <summary>
        /// Guild ranks as an array
        /// </summary>
        public GuildRank[] Ranks
        {
            get { return m_ranks == null ? null : m_ranks.ToArray(); }
        }

        /// <summary>
        /// Adds rank to the tail of ranks list
        /// </summary>
        /// <param name="name">name of new rank</param>
        /// <param name="privileges">privileges of new rank</param>
        /// <param name="update">if true, sends event to the guild</param>
        /// <returns>new rank</returns>
        public GuildRank AddRank(string name, GuildPrivileges privileges, bool update)
        {
            GuildRank rank;
            //using (syncRoot.Enter())
            lock (this)
            {
                foreach (var gRank in m_ranks)
                {
                    if (gRank.Name == name)
                        return null;
                }

                if (m_ranks.Count >= GuildMgr.MAX_GUILD_RANKS)
                    return null;

                rank = new GuildRank(this, name, privileges, m_ranks.Count);
                m_ranks.Add(rank);
            }


            rank.SaveLater();
            if (update)
            {
                GuildHandler.SendGuildQueryToGuildMembers(this);
                GuildHandler.SendGuildRosterToGuildMembers(this);
            }

            return rank;
        }

        /// <summary>
        /// Deletes last rank from ranks list
        /// </summary>
        /// <param name="update">if true, sends event to the guild</param>
        public void RemoveRank(bool update)
        {
            try
            {
                if (m_ranks.Count <= GuildMgr.MIN_GUILD_RANKS)
                    return;

                var lastRankId = m_ranks.Count - 1;

                foreach (var gm in Members.Values)
                {
                    if (gm.RankId == lastRankId) // promote
                        gm.RankId = lastRankId - 1;
                }

                m_ranks.RemoveAt(lastRankId);

                RealmServer.IOQueue.AddMessage(() => m_ranks[lastRankId].Delete());
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e, string.Format("Could not delete rank from guild {0}",
                    this));
            }

            if (update)
            {
                GuildHandler.SendGuildQueryToGuildMembers(this);
                GuildHandler.SendGuildRosterToGuildMembers(this);
            }
        }

        /// <summary>
        /// Changes priviliges and name of a rank
        /// </summary>
        /// <param name="rankId">Id of rank to modify</param>
        /// <param name="newName">new name of rank</param>
        /// <param name="newPrivileges">new priviliges of rank</param>
        /// <param name="update">if true, sends event to the guild</param>
        public void ChangeRank(int rankId, string newName, GuildPrivileges newPrivileges, bool update)
        {
            foreach (var gr in m_ranks)
            {
                if ((gr.Name == newName) && (gr.RankIndex != rankId))
                    return;
            }

            try
            {
                if (m_ranks.Count <= rankId)
                    return;

                m_ranks[rankId].Name = newName;
                m_ranks[rankId].Privileges = newPrivileges;
                m_ranks[rankId].SaveLater();
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e, string.Format("Could not modify rank in guild {0}", this));
            }

            if (update)
            {
                GuildHandler.SendGuildQueryToGuildMembers(this);
                GuildHandler.SendGuildRosterToGuildMembers(this);
            }
        }

        /// <summary>
        /// Promotes GuildMember. It's impossible to promote to guild leader
        /// </summary>
        /// <param name="member">member to promote</param>
        /// <returns>true, if success</returns>
        public bool Promote(GuildMember member)
        {
            if (member.Rank.RankIndex <= 1)
                return false;

            member.RankId--;
            member.UpdateLater();

            return true;
        }

        /// <summary>
        /// Demotes GuildMember
        /// </summary>
        /// <param name="member">member to promote</param>
        /// <returns>true, if success</returns>
        public bool Demote(GuildMember member)
        {
            if (member.Rank.RankIndex >= m_ranks.Count - 1)
                return false;

            member.RankId++;
            member.UpdateLater();

            return true;
        }

        /// <summary>
        /// Requests member by his low id
        /// </summary>
        /// <param name="lowMemberId">low id of member's character</param>
        /// <returns>requested member or null</returns>
        public GuildMember this[uint lowMemberId]
        {
            get
            {
                foreach (var member in Members.Values)
                {
                    if (member.Id == lowMemberId)
                        return member;
                }

                return null;
            }
        }

        /// <summary>
        /// Requests member by his name
        /// </summary>
        /// <param name="name">name of member's character (not case-sensitive)</param>
        /// <returns>requested member</returns>
        public GuildMember this[string name]
        {
            get
            {
                name = name.ToLower();

                foreach (var member in Members.Values)
                {
                    if (member.Name.ToLower() == name)
                        return member;
                }

                return null;
            }
        }

        /// <summary>
        /// Disbands the guild
        /// </summary>
        /// <param name="update">if true, sends event to the guild</param>
        public void Disband()
        {
            //using (syncRoot.Enter())
            lock (this)
            {
                GuildHandler.SendEventToGuild(this, GuildEvents.DISBANDED);

                var members = Members.Values.ToArray();

                foreach (var member in members)
                {
                    RemoveMember(member, false);
                }

                GuildMgr.Instance.UnregisterGuild(this);
                RealmServer.IOQueue.AddMessage(() => Delete());
            }
        }

        /// <summary>
        /// Changes leader of the guild
        /// </summary>
        /// <param name="newLeader">GuildMember of new leader</param>
        /// <param name="update">if true, sends event to the guild</param>
        public void ChangeLeader(GuildMember newLeader)
        {
            if (newLeader.Guild != this)
                return;

            GuildMember currentLeader;
            //using (syncRoot.Enter())
            lock (this)
            {
                currentLeader = Leader;
                if (currentLeader != null)
                {
                    currentLeader.RankId = 1;
                }
                newLeader.RankId = 0;
                Leader = newLeader;
            }

            RealmServer.IOQueue.AddMessage(new Message(() =>
            {
                if (currentLeader != null)
                {
                    currentLeader.Update();
                }
                newLeader.Update();
                Update();
            }));

            if (currentLeader != null)
            {
                GuildHandler.SendEventToGuild(this, GuildEvents.LEADER_CHANGED, newLeader, currentLeader);
            }
        }

        public void TrySetTabard(GuildMember member, NPC vendor, GuildTabard tabard)
        {
            var chr = member.Character;
            if (chr == null) return;

            if (!vendor.IsTabardVendor || !vendor.CheckVendorInteraction(chr))
            {
                //"That's not an emblem vendor!"
                GuildHandler.SendTabardResult(chr, GuildTabardResult.InvalidVendor);
                return;
            }

            if (!member.IsLeader)
            {
                //"Only guild leaders can create emblems."
                GuildHandler.SendTabardResult(chr, GuildTabardResult.NotGuildMaster);
                return;
            }

            if (chr.Money < GuildMgr.GuildTabardCost)
            {
                //"You can't afford to do that."
                GuildHandler.SendTabardResult(chr, GuildTabardResult.NotEnoughMoney);
                return;
            }

            chr.SubtractMoney(GuildMgr.GuildTabardCost);
            Tabard = tabard;

            //"Guild Emblem saved."
            GuildHandler.SendTabardResult(chr, GuildTabardResult.Success);
            GuildHandler.SendGuildQueryResponse(chr, this);
        }

        #endregion

        #region Checks
        /// <summary>
        /// Check whether the given inviter may invite the given target
        /// Sends result to the inviter
        /// </summary>
        /// <param name="inviter">inviter's character, can be null. If null, sending result is suppressed</param>
        /// <param name="target">invitee's character, can be null</param>
        /// <param name="targetName">invitee character's name</param>
        /// <returs>result of invite</returs>
        public static GuildResult CheckInvite(Character inviter, Character target, string targetName = null)
        {
            GuildResult err = GuildResult.SUCCESS;

            var inviterMember = inviter.GuildMember;

            if (inviterMember == null)
            {
                err = GuildResult.PLAYER_NOT_IN_GUILD;
            }
            else if (target == null)
            {
                // target is offline or doesn't exist
                err = GuildResult.PLAYER_NOT_FOUND;
            }
            else if (inviter == target)
            {
                err = GuildResult.PERMISSIONS;
            }
            else if (target.GuildMember != null)
            {
                err = GuildResult.ALREADY_IN_GUILD;
            }
            else if (target.IsInvitedToGuild)
            {
                err = GuildResult.ALREADY_INVITED_TO_GUILD;
            }
            //else if (inviter.Asda2FactionId != target.Asda2FactionId)
            //{
            //    //Check if the inviter and invitee are from the same faction
            //    err = GuildResult.NOT_ALLIED;
            //}
            else
            {
                if (!inviter.Role.IsStaff)
                {
                    if (!inviterMember.HasRight(GuildPrivileges.InviteMembers))
                    {
                        err = GuildResult.PERMISSIONS;
                    }
                    else if (target.IsIgnoring(inviter))
                    {
                        err = GuildResult.PLAYER_IGNORING_YOU;
                    }
                    else
                    {
                        return GuildResult.SUCCESS;
                    }
                }
                else
                {
                    return GuildResult.SUCCESS;
                }
            }
            inviter.SendSystemMessage(string.Format("Unable to invite {1} cause : {0}.", err, targetName));
            //GuildHandler.SendResult(inviter.Client, GuildCommandId.INVITE, targetName, err);
            return err;
        }

        /// <summary>
        /// Checks whether the given target exists in requester's guild and whether the given requestMember has needed privs
        /// Sends result of action to the requester
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetChar">target's character, can be null</param>
        /// <param name="targetName">target character's name</param>
        /// <param name="commandId">executed command. Used for sending result</param>
        /// <param name="reqPrivs">priviliges required for executing this action</param>
        /// <param name="canAffectSelf">can this action be executed on self?</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckAction(Character reqChar, Character targetChar, string targetName, GuildCommandId commandId,
                                              GuildPrivileges reqPrivs, bool canAffectSelf)
        {
            GuildResult err;

            var requester = reqChar.GuildMember;

            if (requester == null)
            {
                err = GuildResult.PLAYER_NOT_IN_GUILD;
                targetName = string.Empty;
            }
            else if (targetChar == null)
            {
                err = GuildResult.PLAYER_NOT_FOUND;
            }
            else if (!canAffectSelf && (reqChar == targetChar))
            {
                err = GuildResult.PERMISSIONS;
            }
            else if (reqChar.Guild != targetChar.Guild)
            {
                // Member is offline or doesn't exist
                err = GuildResult.PLAYER_NOT_IN_GUILD;
            }
            else if (!requester.HasRight(reqPrivs))
            {
                err = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }
            else
            {
                return GuildResult.SUCCESS;
            }

            GuildHandler.SendResult(reqChar.Client, commandId, targetName, err);
            return err;
        }

        /// <summary>
        /// Checks whether the given target exists in requester's guild and whether the given requestMember has needed privs
        /// Sends result of action to the requester
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetName">target character's name</param>
        /// <param name="targetGM">target's GuildMember entry is returned through this</param>
        /// <param name="commandId">executed command. Used for sending result</param>
        /// <param name="reqPrivs">priviliges required for executing this action</param>
        /// <param name="canAffectSelf">can this action be executed on self?</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckAction(Character reqChar, string targetName, out GuildMember targetGM,
            GuildCommandId commandId, GuildPrivileges reqPrivs, bool canAffectSelf)
        {
            targetGM = null;
            GuildResult err;

            var requester = reqChar.GuildMember;

            if (requester == null)
            {
                err = GuildResult.PLAYER_NOT_IN_GUILD;
                targetName = string.Empty;
            }
            else if ((targetGM = requester.Guild[targetName]) == null)
            {
                err = GuildResult.PLAYER_NOT_FOUND;
            }
            else if (!canAffectSelf && (requester == targetGM))
            {
                err = GuildResult.PERMISSIONS;
            }
            else if (!requester.HasRight(reqPrivs))
            {
                err = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }
            else
            {
                return GuildResult.SUCCESS;
            }

            GuildHandler.SendResult(reqChar.Client, commandId, targetName, err);
            return err;
        }

        /// <summary>
        /// Checks if given character has necessary priviliges
        /// CheckInGuild call is done automatically
        /// </summary>
        /// <param name="character">character to check. May be null</param>
        /// <param name="commandId">executed command (used for sending result)</param>
        /// <param name="reqPrivs">required privileges</param>
        /// <returns>The Character's guild if the character has required privileges within the guild, otherwise null</returns>
        public static Guild CheckPrivs(Character character, GuildCommandId commandId, GuildPrivileges reqPrivs)
        {
            var member = character.GuildMember;
            if (member == null)
            {
                GuildHandler.SendResult(character, commandId, GuildResult.PLAYER_NOT_IN_GUILD);
                return null;
            }
            else
            {
                if (!member.HasRight(reqPrivs))
                {
                    var requester = member.Character;
                    if (requester != null)
                    {
                        GuildHandler.SendResult(requester, commandId, GuildResult.PERMISSIONS);
                    }
                    return null;
                }
            }
            return member.Guild;
        }

        //public static bool CheckMemberInGuild(GuildMember requester, GuildMember target)
        //{
        //    if (requester == null)
        //        return false;

        //    if (requester.Guild != target.Guild)
        //    {
        //        Guild.SendResult(requester.Character.Client, GuildCommand.CREATE, target.Name,
        //            GuildResult.PLAYER_NOT_IN_GUILD);
        //        return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Checks whether a guild member may make another guild member the guild leader
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetChar">target's character, can be null</param>
        /// <param name="targetName">target character's name</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckIsLeader(Character reqChar, Character targetChar, GuildCommandId cmd, string targetName)
        {
            GuildResult err;

            GuildMember requester = reqChar.GuildMember, target;

            if (requester == null)
            {
                err = GuildResult.PLAYER_NOT_IN_GUILD;
            }
            else if (targetChar == null)
            {
                err = GuildResult.PLAYER_NOT_FOUND;
            }
            else if ((target = targetChar.GuildMember) == null || target.Guild != requester.Guild)
            {
                err = GuildResult.PLAYER_NOT_IN_GUILD;
            }
            else if (requester == target)
            {
                err = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }
            else if (!requester.IsLeader)
            {
                err = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }
            else
            {
                return GuildResult.SUCCESS;
            }

            GuildHandler.SendResult(reqChar, cmd, targetName, err);
            return err;
        }
        #endregion

        #region IEnumerable<GuildMember> Members

        public IEnumerator<GuildMember> GetEnumerator()
        {
            return GetMembers().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetMembers().GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Send a packet to every guild member
        /// </summary>
        /// <param name="packet">the packet to send</param>
        public void Broadcast(RealmPacketOut packet)
        {
            Broadcast(packet, null);
        }

        /// <summary>
        /// Send a packet to every guild member except for the one specified.
        /// </summary>
        /// <param name="packet">the packet to send</param>
        /// <param name="ignoredCharacter">the <see cref="Character" /> that won't receive the packet</param>
        public void Broadcast(RealmPacketOut packet, Character ignoredCharacter)
        {
            foreach (var member in GetMembers())
            {
                var character = member.Character;

                if ((character != null) && (character != ignoredCharacter))
                {
                    character.Client.Send(packet, addEnd: false);
                }
            }
        }

        public void ForeachMember(Action<GuildMember> callback)
        {
            //using (syncRoot.Enter())
            lock (this)
            {
                foreach (var member in Members.Values)
                {
                    callback(member);
                }
            }
        }

        #region IChatTarget

        /// <summary>
        /// The EntityId (only set for Character)
        /// </summary>
        public EntityId EntityId
        {
            get { return EntityId.Zero; }
        }

        //TODO update onlinemembersCount
        public byte OnlineMembersCount { get; set; }

        public List<HistoryRecord> History
        {
            get { return _history ?? (_history = new List<HistoryRecord>(12)); }
        }

        public GuildSkill[] Skills
        {
            get { return _skills ?? (_skills = new GuildSkill[10]); }
        }

        public void AddHistoryMessage(Asda2GuildHistoryType type, int value, string trigerName, string time)
        {
            if (History.Count == 12)
                History.RemoveAt(11);
            History.Insert(0, new HistoryRecord((byte)type, value, trigerName, time));
        }
        public void SendSystemMsg(string msg)
        {
            foreach (var chr in GetCharacters())
            {
                chr.SendSystemMessage(msg);
            }
        }

        public void SendMessage(string message)
        {
            // TODO: No chatter argument
            //throw new NotImplementedException();
            LogManager.GetCurrentClassLogger().Warn("Tried to send message to guild {0} but Guild.SendMessage(string) is not implemented yet: {1}", Name, message);
            //ChatMgr.SendGuildMessage(sender, this, message);
        }

        /// <summary>
        /// Say something to this target
        /// </summary>		
        public void SendMessage(IChatter sender, string message)
        {
            ChatMgr.SendGuildMessage(sender, this, message);
        }

        /// <summary>
        /// All members. 
        /// Looping over this enumeration is synchronized.
        /// </summary>
        public IEnumerable<GuildMember> GetMembers()
        {
            //using (syncRoot.Enter())
            lock (this)
            {
                foreach (var member in Members.Values)
                {
                    yield return member;
                }
            }
        }

        /// <summary>
        /// All online characters.
        /// Looping over this enumeration is synchronized.
        /// </summary>
        public IEnumerable<Character> GetCharacters()
        {
            foreach (var member in GetMembers())
            {
                var chr = member.Character;
                if (chr != null)
                {
                    yield return chr;
                }
            }
        }

        /// <summary>
        /// Sends the given packet to all online characters
        /// </summary>
        public void Send(RealmPacketOut packet, bool addEnd = false, Locale locale = Locale.Any)
        {
            foreach (var chr in GetCharacters())
            {
                if (locale == Locale.Any || locale == chr.Client.Locale)
                    chr.Client.Send(packet, addEnd: addEnd);
            }
        }

        /// <summary>
        /// All online chat listeners
        /// </summary>
        public IEnumerable<Character> GetCharacters(GuildPrivileges requiredPrivs)
        {
            foreach (var member in GetMembers())
            {
                var chr = member.Character;
                if (chr != null && member.HasRight(requiredPrivs))
                {
                    yield return chr;
                }
            }
        }

        /// <summary>
        /// Sends the given packet to all online chat listeners
        /// </summary>
        public void SendToChatListeners(RealmPacketOut packet)
        {
            /*foreach (var chr in GetCharacters(GuildPrivileges.GCHATLISTEN))
            {
                chr.Send(packet, false);
            }*/
        }

        /// <summary>
        /// Sends the given packet to all online officers
        /// </summary>
        public void SendToOfficers(RealmPacketOut packet)
        {
            /*foreach (var chr in GetCharacters(GuildPrivileges.OFFCHATLISTEN))
            {
                chr.Client.Send(packet, false);
            }*/
        }

        #endregion

        public override string ToString()
        {
            return Name + string.Format(" (Id: {0}, Members:{1}) - {2}", Id, MemberCount, Info);
        }

        public LearnGuildSkillResult TryLearnSkill(GuildSkillId skillId, out GuildSkill skill)
        {

            skill = Skills[(int)skillId];
            if (skill != null)
            {
                if (skill.IsMaxLevel)
                    return LearnGuildSkillResult.ThisIsTheMaxLevelOfSkill;
                //todo get real learn cost levelupSkill
                if (!SubstractGuildPoints(skill.NextLearnCost))
                    return LearnGuildSkillResult.IncifitientPoints;
                Skills[(int)skillId].Level++;
            }
            else
            {
                if (!SubstractGuildPoints(GuildSkillTemplate.Templates[(int)skillId].LearnCosts[1]))
                    return LearnGuildSkillResult.IncifitientPoints;

                RealmServer.IOQueue.AddMessage(() =>
                {
                   var s = new GuildSkill(this, skillId);
                    Skills[(int)skillId] = s;
                    s.CreateLater();
                });
            }
            Asda2GuildHandler.SendGuildSkillStatusChangedResponse(skill, ClanSkillStatus.Learned);

            return LearnGuildSkillResult.Ok;
        }

        public bool SubstractGuildPoints(int points)
        {
            if (Points < points)
                return false;
            Points -= (uint)points;
            AddHistoryMessage(Asda2GuildHistoryType.UsedPoints, points, points.ToString(), DateTime.Now.ToLongTimeString());
            //Asda2GuildHandler.SendUpdateGuildInfoResponse(this);
            return true;
        }
        public bool AddGuildPoints(int points)
        {
            Points += (uint)points;

            Asda2GuildHandler.SendUpdateGuildInfoResponse(this);
            return true;
        }

        private Dictionary<GuildMember, GuildMember> _acceptedMembers = new Dictionary<GuildMember, GuildMember>();
        private GuildMember _newLeader;
        private DateTime _impeachmentStartTime;
        public Asda2GuildHandler.CreateImpeachmentResult CreateImpeachment(GuildMember guildMember)
        {
            if (DateTime.Now - _impeachmentStartTime < TimeSpan.FromMinutes(4))
                return Asda2GuildHandler.CreateImpeachmentResult.AlreadyInProgress;
            _impeachmentStartTime = DateTime.Now;
            _acceptedMembers.Clear();
            _newLeader = guildMember;
            Asda2GuildHandler.SendImpeachmentAnswerResponse(this, _newLeader.Name);
            guildMember.Character.Map.CallDelayed(3 * 60 * 1000, ImpeachmentCallback);
            return Asda2GuildHandler.CreateImpeachmentResult.Success;
        }
        public void AddImpeachmentVote(GuildMember member)
        {
            if (_acceptedMembers.ContainsKey(member) || member == _newLeader || member.IsLeader)
                return;
            _acceptedMembers.Add(member, member);
        }
        void ImpeachmentCallback()
        {
            var prcVotes = (float)_acceptedMembers.Count / (MemberCount - 2) * 100;
            SendSystemMsg(string.Format("{0}% members accepted new leader.", prcVotes));
            if (prcVotes > 70)
            {
                //ok
                Leader.Asda2RankId = 3;
                Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.RankChanged, Leader);
                Leader = _newLeader;
                Leader.Asda2RankId = 4;
                Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.ApointedAsNewGuildLeader, Leader);
                Asda2GuildHandler.SendImpeachmentResultResponse(this, Asda2GuildHandler.ImpeachmentResult.Success);
                Asda2GuildHandler.SendUpdateGuildInfoResponse(this);
                AddHistoryMessage(Asda2GuildHistoryType.ApointedAsGuildLeaderThorowVote, 0, Leader.Name, DateTime.Now.ToLongTimeString());
            }
            else
            {
                //fail
                Asda2GuildHandler.SendImpeachmentResultResponse(this, Asda2GuildHandler.ImpeachmentResult.Failed);
            }
            _acceptedMembers.Clear();
            _newLeader = null;
            _impeachmentStartTime = DateTime.MinValue;
        }

        public bool LevelUp()
        {
            if (!SubstractGuildPoints(CharacterFormulas.GuildLevelUpCost[Level]))
                return false;
            Level++;
            AddHistoryMessage(Asda2GuildHistoryType.GuildLevelNowIs, Level, "system", DateTime.Now.ToLongTimeString());
            Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.GuildLevelChanged);
            foreach (var character in GetCharacters())
            {
                GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(character);
            }
            return true;
        }
    }
    public class GuildSkillTemplate
    {
        [NotVariable]
        public static GuildSkillTemplate[] Templates = new GuildSkillTemplate[10];
        public int[] ActivationCosts { get; set; }
        public int[] LearnCosts { get; set; }
        public int[] MaitenceCosts { get; set; }
        public int[] BonusValuses { get; set; }
        public int MaxLevel { get; set; }
    }
    public enum Asda2GuildHistoryType
    {
        Joined = 1,
        Left = 2,
        Kicked = 3,
        ApointedAsGuildLeader = 4,
        ApointedAsGuildLeaderThorowVote = 5,
        DonatedPoints = 6,
        UsedPoints = 7,
        GuildLevelNowIs = 8
    }


    public class HistoryRecord
    {
        public byte Type { get; set; }
        public int Value { get; set; }
        public string TrigerName { get; set; }
        public string Time { get; set; }

        public HistoryRecord(byte msgType, int value, string trigerName, string time)
        {
            Type = msgType;
            TrigerName = trigerName;
            Time = time;
            Value = value;
        }
    }
}