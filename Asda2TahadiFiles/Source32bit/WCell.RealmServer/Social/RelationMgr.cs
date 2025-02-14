﻿using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Relations;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Res;
using WCell.Util.NLog;

namespace WCell.RealmServer.Interaction
{
  public sealed class RelationMgr : Manager<RelationMgr>
  {
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<uint, HashSet<IBaseRelation>>[] m_activeRelations;
    private readonly Dictionary<uint, HashSet<IBaseRelation>>[] m_passiveRelations;
    private readonly ReaderWriterLockSlim m_lock;

    private RelationMgr()
    {
      m_activeRelations = new Dictionary<uint, HashSet<IBaseRelation>>[6];
      m_passiveRelations = new Dictionary<uint, HashSet<IBaseRelation>>[6];
      for(int index = 1; index < 6; ++index)
      {
        m_activeRelations[index] = new Dictionary<uint, HashSet<IBaseRelation>>();
        m_passiveRelations[index] = new Dictionary<uint, HashSet<IBaseRelation>>();
      }

      m_lock = new ReaderWriterLockSlim();
    }

    private void Initialize()
    {
      BaseRelation[] all;
      try
      {
        all = PersistedRelation.GetAll();
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        all = PersistedRelation.GetAll();
      }

      m_lock.EnterWriteLock();
      try
      {
        foreach(PersistedRelation persistedRelation in all)
        {
          HashSet<IBaseRelation> baseRelationSet;
          if(!m_activeRelations[(int) persistedRelation.Type]
            .TryGetValue(persistedRelation.CharacterId, out baseRelationSet))
            m_activeRelations[(int) persistedRelation.Type].Add(persistedRelation.CharacterId,
              baseRelationSet = new HashSet<IBaseRelation>());
          baseRelationSet.Add(persistedRelation);
          if(!m_passiveRelations[(int) persistedRelation.Type]
            .TryGetValue(persistedRelation.RelatedCharacterId, out baseRelationSet))
            m_passiveRelations[(int) persistedRelation.Type].Add(persistedRelation.RelatedCharacterId,
              baseRelationSet = new HashSet<IBaseRelation>());
          baseRelationSet.Add(persistedRelation);
        }
      }
      finally
      {
        m_lock.ExitWriteLock();
      }
    }

    /// <summary>
    /// The lock to synchronize with, when iterating over this Manager's Relations etc.
    /// </summary>
    public ReaderWriterLockSlim Lock
    {
      get { return m_lock; }
    }

    private void LoadRelations(uint charId)
    {
      m_lock.EnterWriteLock();
      try
      {
        foreach(PersistedRelation persistedRelation in PersistedRelation.GetByCharacterId(charId))
        {
          HashSet<IBaseRelation> baseRelationSet;
          if(!m_activeRelations[(int) persistedRelation.Type]
            .TryGetValue(persistedRelation.CharacterId, out baseRelationSet))
            m_activeRelations[(int) persistedRelation.Type].Add(persistedRelation.CharacterId,
              baseRelationSet = new HashSet<IBaseRelation>());
          baseRelationSet.Add(persistedRelation);
          if(!m_passiveRelations[(int) persistedRelation.Type]
            .TryGetValue(persistedRelation.CharacterId, out baseRelationSet))
            m_passiveRelations[(int) persistedRelation.Type].Add(persistedRelation.RelatedCharacterId,
              baseRelationSet = new HashSet<IBaseRelation>());
          baseRelationSet.Add(persistedRelation);
        }
      }
      finally
      {
        m_lock.ExitWriteLock();
      }
    }

    public static bool IsIgnoring(uint chr, uint ignoredCharId)
    {
      return Instance.GetRelation(chr, ignoredCharId, CharacterRelationType.Ignored) !=
             null;
    }

    /// <summary>Only needed if Character gets removed?</summary>
    public void RemoveRelations(uint lowId)
    {
      m_lock.EnterWriteLock();
      try
      {
        for(int index = 1; index < 6; ++index)
        {
          HashSet<IBaseRelation> baseRelationSet;
          if(m_activeRelations[index].TryGetValue(lowId, out baseRelationSet))
          {
            foreach(IBaseRelation baseRelation in baseRelationSet)
            {
              if(baseRelation is PersistedRelation)
                ((PersistedRelation) baseRelation).Delete();
            }
          }

          if(m_passiveRelations[index].TryGetValue(lowId, out baseRelationSet))
          {
            foreach(IBaseRelation baseRelation in baseRelationSet)
            {
              if(baseRelation is PersistedRelation)
                ((PersistedRelation) baseRelation).Delete();
            }
          }

          m_activeRelations[index].Remove(lowId);
          m_passiveRelations[index].Remove(lowId);
        }
      }
      finally
      {
        m_lock.ExitWriteLock();
      }
    }

    /// <summary>
    /// Retrieves all Relations that the given Character started with others
    /// </summary>
    /// <param name="charLowId">The <see cref="T:WCell.Core.EntityId" /> of the character wich relations are requested</param>
    /// <param name="relationType">The type of the relation</param>
    /// <returns>The list of the related characters relations.</returns>
    public HashSet<IBaseRelation> GetRelations(uint charLowId, CharacterRelationType relationType)
    {
      if(charLowId == 0U || relationType == CharacterRelationType.Invalid)
        return BaseRelation.EmptyRelationSet;
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(m_activeRelations[(int) relationType].TryGetValue(charLowId, out baseRelationSet))
          return baseRelationSet;
      }
      finally
      {
        m_lock.ExitReadLock();
      }

      return BaseRelation.EmptyRelationSet;
    }

    /// <summary>
    /// Retrieves all Relations of the given type that others have started with the given Character
    /// </summary>
    /// <param name="charLowId">The <see cref="T:WCell.Core.EntityId" /> of the character wich relations are requested</param>
    /// <param name="relationType">The type of the relation</param>
    public HashSet<IBaseRelation> GetPassiveRelations(uint charLowId, CharacterRelationType relationType)
    {
      if(charLowId == 0U || relationType == CharacterRelationType.Invalid)
        return BaseRelation.EmptyRelationSet;
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(m_passiveRelations[(int) relationType].TryGetValue(charLowId, out baseRelationSet))
          return baseRelationSet;
      }
      finally
      {
        m_lock.ExitReadLock();
      }

      return BaseRelation.EmptyRelationSet;
    }

    /// <summary>
    /// Returns whether the Character with the given low Entityid has any passive relations of the given type
    /// </summary>
    /// <param name="charLowId"></param>
    /// <param name="relationType"></param>
    /// <returns></returns>
    public bool HasPassiveRelations(uint charLowId, CharacterRelationType relationType)
    {
      if(charLowId == 0U || relationType == CharacterRelationType.Invalid)
        return false;
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(m_passiveRelations[(int) relationType].TryGetValue(charLowId, out baseRelationSet))
          return baseRelationSet.Count > 0;
        return false;
      }
      finally
      {
        m_lock.ExitReadLock();
      }
    }

    /// <summary>
    /// Retrieves a current relationship between the given characters and the type
    /// </summary>
    /// <param name="charId">The first character EntityId</param>
    /// <param name="relatedCharId">The related character low EntityId</param>
    /// <param name="relationType">The relationship type</param>
    /// <returns>A <see cref="T:WCell.RealmServer.Interaction.BaseRelation" /> object representing the relation;
    /// null if the relation wasnt found.</returns>
    public BaseRelation GetRelation(uint charId, uint relatedCharId, CharacterRelationType relationType)
    {
      if(charId == 0U || relatedCharId == 0U || relationType == CharacterRelationType.Invalid)
        return null;
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(m_activeRelations[(int) relationType].TryGetValue(charId, out baseRelationSet))
        {
          foreach(BaseRelation baseRelation in baseRelationSet)
          {
            if((int) baseRelation.RelatedCharacterId == (int) relatedCharId)
              return baseRelation;
          }
        }
      }
      finally
      {
        m_lock.ExitReadLock();
      }

      return null;
    }

    /// <summary>Sets a relation note text</summary>
    /// <param name="charId">The first character EntityId</param>
    /// <param name="relatedCharId">The related character low EntityId</param>
    /// <param name="note">The note to be assigned to the relation</param>
    /// <param name="relationType">The relationship type</param>
    public void SetRelationNote(uint charId, uint relatedCharId, string note, CharacterRelationType relationType)
    {
      if(charId == 0U || relatedCharId == 0U ||
         (string.IsNullOrEmpty(note) || relationType == CharacterRelationType.Invalid))
        return;
      BaseRelation relation = GetRelation(charId, relatedCharId, relationType);
      if(relation == null)
        return;
      relation.Note = note;
      if(!(relation is PersistedRelation))
        return;
      ((PersistedRelation) relation).SaveToDB();
    }

    /// <summary>
    /// Returns whether the given char has the given relationType with the given relatedChar
    /// </summary>
    /// <param name="charId">The first character EntityId</param>
    /// <param name="relatedCharId">The related character EntityId</param>
    /// <param name="relationType">The relationship type</param>
    /// <returns>True if the relation exist. False otherwise</returns>
    public bool HasRelation(uint charId, uint relatedCharId, CharacterRelationType relationType)
    {
      return GetRelation(charId, relatedCharId, relationType) != null;
    }

    /// <summary>Adds a character relation</summary>
    /// <param name="character">The first character in the relation</param>
    /// <param name="relatedCharName">The related character name</param>
    /// <param name="note">A note describing the relation. Used for Friend only relation types</param>
    /// <param name="relationType">The relation type</param>
    internal void AddRelation(Character character, string relatedCharName, string note,
      CharacterRelationType relationType)
    {
      Character character1 = World.GetCharacter(relatedCharName, false);
      CharacterRecord relatedCharInfo =
        character1 == null ? CharacterRecord.GetRecordByName(relatedCharName) : character1.Record;
      if(relatedCharInfo != null)
      {
        BaseRelation relation =
          CreateRelation(character.EntityId.Low, relatedCharInfo.EntityLowId, relationType);
        relation.Note = note;
        RelationResult relResult;
        if(!relation.Validate(character.Record, relatedCharInfo, out relResult))
          _log.Debug(WCell_RealmServer.CharacterRelationValidationFailed, (object) character.Name,
            (object) character.EntityId, (object) relatedCharName, (object) relatedCharInfo.EntityLowId,
            (object) relationType, (object) relResult);
        else
          AddRelation(relation);
        if(relResult == RelationResult.FRIEND_ADDED_ONLINE)
          SendFriendOnline(character, character1, note, true);
        else
          SendFriendStatus(character, relatedCharInfo.EntityLowId, note, relResult);
      }
      else
        SendFriendStatus(character, 0U, note, RelationResult.FRIEND_NOT_FOUND);
    }

    /// <summary>Adds a character relation</summary>
    /// <param name="relation">The relation to be added</param>
    public void AddRelation(BaseRelation relation)
    {
      try
      {
        if(relation is PersistedRelation)
          (relation as PersistedRelation).SaveToDB();
        m_lock.EnterWriteLock();
        try
        {
          HashSet<IBaseRelation> baseRelationSet1;
          if(!m_activeRelations[(int) relation.Type]
            .TryGetValue(relation.CharacterId, out baseRelationSet1))
            m_activeRelations[(int) relation.Type].Add(relation.CharacterId,
              baseRelationSet1 = new HashSet<IBaseRelation>());
          HashSet<IBaseRelation> baseRelationSet2;
          if(!m_passiveRelations[(int) relation.Type]
            .TryGetValue(relation.RelatedCharacterId, out baseRelationSet2))
            m_passiveRelations[(int) relation.Type].Add(relation.RelatedCharacterId,
              baseRelationSet2 = new HashSet<IBaseRelation>());
          baseRelationSet1.Add(relation);
          baseRelationSet2.Add(relation);
        }
        finally
        {
          m_lock.ExitWriteLock();
        }

        _log.Debug(WCell_RealmServer.CharacterRelationAdded, (object) string.Empty,
          (object) relation.CharacterId, (object) string.Empty, (object) relation.RelatedCharacterId,
          (object) relation.Type, (object) 0);
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex,
          string.Format(WCell_RealmServer.CharacterRelationAddedFailed, (object) string.Empty,
            (object) relation.CharacterId, (object) string.Empty, (object) relation.RelatedCharacterId,
            (object) relation.Type, (object) 0));
      }
    }

    /// <summary>Removes a character relation</summary>
    /// <param name="relCharId">The related character low <see cref="T:WCell.Core.EntityId" /></param>
    /// <param name="relationType">The relation type</param>
    internal void RemoveRelation(uint charId, uint relCharId, CharacterRelationType relationType)
    {
      RelationResult relResult;
      if(RemoveRelation(GetRelation(charId, relCharId, relationType)))
      {
        relResult = GetDeleteRelationResult(relationType);
      }
      else
      {
        relResult = RelationResult.FRIEND_DB_ERROR;
        _log.Debug(WCell_RealmServer.CharacterRelationRemoveFailed, (object) charId,
          (object) relCharId, (object) relationType, (object) relResult);
      }

      Character character = World.GetCharacter(charId);
      if(character == null)
        return;
      SendFriendStatus(character, relCharId, string.Empty, relResult);
    }

    /// <summary>
    /// Removes all Relations that the given Character has of the given type.
    /// </summary>
    /// <returns>Whether there were any Relations of the given type</returns>
    public bool RemoveRelations(uint charLowId, CharacterRelationType type)
    {
      m_lock.EnterWriteLock();
      try
      {
        return m_activeRelations[(int) type].Remove(charLowId) &&
               m_passiveRelations[(int) type].Remove(charLowId);
      }
      finally
      {
        m_lock.ExitWriteLock();
      }
    }

    /// <summary>Removes a character relation</summary>
    public bool RemoveRelation(IBaseRelation relation)
    {
      bool flag = false;
      m_lock.EnterWriteLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(m_activeRelations[(int) relation.Type].TryGetValue(relation.CharacterId, out baseRelationSet))
          baseRelationSet.Remove(relation);
        if(m_passiveRelations[(int) relation.Type]
          .TryGetValue(relation.RelatedCharacterId, out baseRelationSet))
        {
          baseRelationSet.Remove(relation);
          flag = true;
        }
      }
      finally
      {
        m_lock.ExitWriteLock();
      }

      if(relation is PersistedRelation)
        ((PersistedRelation) relation).Delete();
      _log.Debug(WCell_RealmServer.CharacterRelationRemoved, (object) relation.CharacterId,
        (object) relation.RelatedCharacterId, (object) relation.Type, (object) 0);
      return flag;
    }

    /// <summary>
    /// Saves all relations of the Character with the given low EntityId
    /// </summary>
    public void SaveRelations(uint lowUid)
    {
      m_lock.EnterReadLock();
      try
      {
        for(int index = 1; index < m_activeRelations.Length && index != 4; ++index)
        {
          HashSet<IBaseRelation> baseRelationSet;
          if(m_activeRelations[index].TryGetValue(lowUid, out baseRelationSet))
          {
            foreach(PersistedRelation persistedRelation in baseRelationSet)
              persistedRelation.SaveToDB();
          }
        }
      }
      finally
      {
        m_lock.ExitReadLock();
      }
    }

    public static BaseRelation CreateRelation(uint charId, uint relatedCharId, CharacterRelationType relationType)
    {
      switch(relationType)
      {
        case CharacterRelationType.Friend:
          return new FriendRelation(charId, relatedCharId);
        case CharacterRelationType.Ignored:
          return new IgnoredRelation(charId, relatedCharId);
        case CharacterRelationType.Muted:
          return new MutedRelation(charId, relatedCharId);
        case CharacterRelationType.GroupInvite:
          return new GroupInviteRelation(charId, relatedCharId);
        case CharacterRelationType.GuildInvite:
          return new GuildInviteRelation(charId, relatedCharId);
        default:
          return null;
      }
    }

    public static BaseRelation CreateRelation(CharacterRelationRecord relationRecord)
    {
      if(relationRecord == null)
        return null;
      return CreateRelation(relationRecord.CharacterId, relationRecord.RelatedCharacterId,
        relationRecord.RelationType);
    }

    public static void SendFriendStatus(Character target, uint friendId, string note, RelationResult relResult)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_FRIEND_STATUS))
      {
        packet.WriteByte((byte) relResult);
        switch(relResult)
        {
          case RelationResult.FRIEND_DB_ERROR:
          case RelationResult.FRIEND_NOT_FOUND:
          case RelationResult.FRIEND_REMOVED:
          case RelationResult.FRIEND_ALREADY:
          case RelationResult.FRIEND_SELF:
          case RelationResult.FRIEND_ENEMY:
          case RelationResult.IGNORE_SELF:
          case RelationResult.IGNORE_NOT_FOUND:
          case RelationResult.IGNORE_ALREADY:
          case RelationResult.IGNORE_ADDED:
          case RelationResult.IGNORE_REMOVED:
          case RelationResult.MUTED_SELF:
          case RelationResult.MUTED_NOT_FOUND:
          case RelationResult.MUTED_ALREADY:
          case RelationResult.MUTED_ADDED:
          case RelationResult.MUTED_REMOVED:
            packet.Write(EntityId.GetPlayerId(friendId));
            break;
          case RelationResult.FRIEND_OFFLINE:
            packet.Write(EntityId.GetPlayerId(friendId));
            packet.WriteByte((byte) 0);
            break;
          case RelationResult.FRIEND_ADDED_OFFLINE:
            packet.Write(EntityId.GetPlayerId(friendId));
            packet.WriteCString(note);
            break;
        }

        target.Client.Send(packet, false);
      }
    }

    public static void SendFriendOnline(Character target, Character friend, string note, bool justAdded)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_FRIEND_STATUS))
      {
        packet.WriteByte(justAdded ? (byte) 6 : (byte) 2);
        packet.Write(friend.EntityId);
        if(justAdded)
          packet.WriteCString(note ?? string.Empty);
        packet.WriteByte((byte) friend.Status);
        packet.Write((int) friend.Zone.Id);
        packet.WriteUInt(friend.Level);
        packet.WriteUInt((byte) friend.Class);
        target.Client.Send(packet, false);
      }
    }

    /// <summary>Notifies all friends etc</summary>
    /// <param name="character">The character logging in</param>
    internal void OnCharacterLogin(Character character)
    {
      SendRelationList(character,
        RelationTypeFlag.Friend | RelationTypeFlag.Ignore | RelationTypeFlag.Muted);
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(!m_passiveRelations[1].TryGetValue(character.EntityId.Low, out baseRelationSet))
          return;
        foreach(IBaseRelation baseRelation in baseRelationSet)
        {
          Character character1 = World.GetCharacter(baseRelation.CharacterId);
          if(character1 != null)
            SendFriendOnline(character1, character, baseRelation.Note, false);
        }
      }
      finally
      {
        m_lock.ExitReadLock();
      }
    }

    /// <summary>Notifies all friends etc</summary>
    /// <param name="character">The character logging off</param>
    internal void OnCharacterLogout(Character character)
    {
      SaveRelations(character.EntityId.Low);
      NotifyFriendRelations(character.EntityId.Low, RelationResult.FRIEND_OFFLINE);
    }

    private void NotifyFriendRelations(uint friendLowId, RelationResult relResult)
    {
      m_lock.EnterReadLock();
      try
      {
        HashSet<IBaseRelation> baseRelationSet;
        if(!m_passiveRelations[1].TryGetValue(friendLowId, out baseRelationSet))
          return;
        foreach(IBaseRelation baseRelation in baseRelationSet)
        {
          Character character = World.GetCharacter(baseRelation.CharacterId);
          if(character != null)
            SendFriendStatus(character, friendLowId, baseRelation.Note, relResult);
        }
      }
      finally
      {
        m_lock.ExitReadLock();
      }
    }

    [Initialization(InitializationPass.Fifth, "Start relation manager")]
    public static void StartRelationMgr()
    {
      Instance.Initialize();
    }

    /// <summary>
    /// Sends the specified character relation lists to the specified character
    /// </summary>
    /// <param name="character">The character to send the list</param>
    /// <param name="flag">Flag indicating which lists should be sent to the character</param>
    internal void SendRelationList(Character character, RelationTypeFlag flag)
    {
      Dictionary<uint, RelationListEntry> flatRelations =
        GetFlatRelations(character.EntityId.Low, flag);
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CONTACT_LIST))
      {
        packet.WriteUInt((uint) flag);
        packet.WriteUInt((uint) flatRelations.Count);
        foreach(RelationListEntry relationListEntry in flatRelations.Values)
        {
          packet.Write(EntityId.GetPlayerId(relationListEntry.RelatedCharacterId));
          packet.WriteUInt((uint) relationListEntry.Flag);
          packet.WriteCString(relationListEntry.Note);
          if(relationListEntry.Flag.HasFlag(RelationTypeFlag.Friend))
          {
            Character character1 = World.GetCharacter(relationListEntry.RelatedCharacterId);
            if(character1 != null)
            {
              packet.WriteByte((byte) character1.Status);
              packet.Write(character1.Zone != null ? (int) character1.Zone.Id : 0);
              packet.Write(character1.Level);
              packet.Write((int) character1.Class);
            }
            else
              packet.WriteByte((byte) 0);
          }
        }

        character.Client.Send(packet, false);
      }
    }

    private Dictionary<uint, RelationListEntry> GetFlatRelations(uint characterId,
      RelationTypeFlag flags)
    {
      Dictionary<uint, RelationListEntry> source =
        new Dictionary<uint, RelationListEntry>();
      HashSet<IBaseRelation> relations1 = GetRelations(characterId, CharacterRelationType.Friend);
      if(relations1 != null)
      {
        foreach(IBaseRelation relation in relations1)
          source[relation.RelatedCharacterId] =
            new RelationListEntry(relation, RelationTypeFlag.Friend);
      }

      HashSet<IBaseRelation> relations2 = GetRelations(characterId, CharacterRelationType.Ignored);
      if(relations2 != null)
      {
        foreach(IBaseRelation relation in relations2)
        {
          if(source.ContainsKey(relation.RelatedCharacterId))
            source[relation.RelatedCharacterId].Flag |= RelationTypeFlag.Ignore;
          else
            source[relation.RelatedCharacterId] =
              new RelationListEntry(relation, RelationTypeFlag.Ignore);
        }
      }

      HashSet<IBaseRelation> relations3 = GetRelations(characterId, CharacterRelationType.Muted);
      if(relations3 != null)
      {
        foreach(IBaseRelation relation in relations3)
        {
          if(source.ContainsKey(relation.RelatedCharacterId))
            source[relation.RelatedCharacterId].Flag |= RelationTypeFlag.Muted;
          else
            source[relation.RelatedCharacterId] =
              new RelationListEntry(relation, RelationTypeFlag.Muted);
        }
      }

      foreach(uint key in source
        .Where(
          entry =>
            !entry.Value.Flag.HasAnyFlag(flags))
        .Select(
          entry => entry.Key))
        source.Remove(key);
      return source;
    }

    private static RelationResult GetDeleteRelationResult(CharacterRelationType relationType)
    {
      switch(relationType)
      {
        case CharacterRelationType.Friend:
          return RelationResult.FRIEND_REMOVED;
        case CharacterRelationType.Ignored:
          return RelationResult.IGNORE_REMOVED;
        case CharacterRelationType.Muted:
          return RelationResult.MUTED_REMOVED;
        default:
          throw new ArgumentOutOfRangeException(nameof(relationType));
      }
    }

    private sealed class RelationListEntry
    {
      private string _note = string.Empty;

      public uint RelatedCharacterId { get; private set; }

      public RelationTypeFlag Flag { get; set; }

      public string Note
      {
        get { return _note; }
        private set { _note = string.IsNullOrEmpty(value) ? string.Empty : value; }
      }

      public RelationListEntry(IBaseRelation relation, RelationTypeFlag flag)
      {
        RelatedCharacterId = relation.RelatedCharacterId;
        Flag = flag;
        Note = relation.Note;
      }
    }
  }
}