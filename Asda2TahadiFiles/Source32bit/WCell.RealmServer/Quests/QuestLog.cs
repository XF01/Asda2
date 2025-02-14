﻿using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Quests
{
  /// <summary>TODO: Change dictionary to array</summary>
  public class QuestLog
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private int m_activeQuestCount = 1;
    internal List<Quest> m_RequireGOsQuests = new List<Quest>();
    internal List<Quest> m_NPCInteractionQuests = new List<Quest>();
    internal List<Quest> m_RequireItemsQuests = new List<Quest>();
    internal List<Quest> m_RequireSpellCastsQuests = new List<Quest>();
    public const int INVALID_SLOT = -1;
    public const int MaxQuestCount = 25;
    public const int MaxDailyQuestCount = 25;
    public uint[] DailyQuests;
    private readonly Character m_Owner;
    private Quest[] m_ActiveQuests;
    internal HashSet<uint> m_FinishedQuests;
    private Quest m_timedQuest;
    private Quest m_escortQuest;
    private List<QuestTemplate> m_DailyQuestsToday;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:WCell.RealmServer.Quests.QuestLog" /> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    public QuestLog(Character owner)
    {
      m_timedQuest = null;
      m_escortQuest = null;
      m_Owner = owner;
      m_ActiveQuests = new Quest[25];
      m_DailyQuestsToday = new List<QuestTemplate>();
      m_FinishedQuests = new HashSet<uint>();
    }

    /// <summary>
    /// Gets the timed quest.
    /// Every Character is only allowed to solve one timed Quest at a time (makes sense doesn't it?)
    /// </summary>
    public Quest TimedQuestSlot
    {
      get { return m_timedQuest; }
    }

    /// <summary>Gets the escort quest.</summary>
    /// <value>The escort quest.</value>
    public Quest EscortQuestSlot
    {
      get { return m_escortQuest; }
    }

    /// <summary>Gets the current quests.</summary>
    /// <value>The current quests.</value>
    public Quest[] ActiveQuests
    {
      get { return m_ActiveQuests; }
    }

    public int ActiveQuestCount
    {
      get { return m_activeQuestCount; }
    }

    /// <summary>Gets the current daily count.</summary>
    /// <value>The current daily count.</value>
    public uint CurrentDailyCount
    {
      get { return (uint) m_DailyQuestsToday.Count; }
    }

    /// <summary>Gets the finished quests.</summary>
    /// <value>The finished quests.</value>
    public HashSet<uint> FinishedQuests
    {
      get { return m_FinishedQuests; }
    }

    /// <summary>Gets the owner.</summary>
    /// <value>The owner.</value>
    public Character Owner
    {
      get { return m_Owner; }
    }

    /// <summary>
    /// Determines whether the amount of active Quests is less than the maximum amount of Quests.
    /// </summary>
    public bool HasFreeSpace
    {
      get { return m_activeQuestCount < 25; }
    }

    public bool CanAcceptDailyQuest
    {
      get { return m_DailyQuestsToday.Count < 25; }
    }

    public List<Quest> RequireGOQuests
    {
      get { return m_RequireGOsQuests; }
    }

    public List<Quest> RequireNPCInteractionQuests
    {
      get { return m_NPCInteractionQuests; }
    }

    public List<Quest> RequireItemQuests
    {
      get { return m_RequireItemsQuests; }
    }

    public Quest TryAddQuest(QuestTemplate template, IQuestHolder questGiver)
    {
      int freeSlot = m_Owner.QuestLog.FindFreeSlot();
      if(freeSlot == -1)
      {
        QuestHandler.SendQuestLogFull(m_Owner);
      }
      else
      {
        QuestInvalidReason reason = template.CheckBasicRequirements(m_Owner);
        if(reason != QuestInvalidReason.Ok)
          QuestHandler.SendQuestInvalid(m_Owner, reason);
        else if(m_Owner.QuestLog.GetActiveQuest(template.Id) != null)
          QuestHandler.SendQuestInvalid(m_Owner, QuestInvalidReason.AlreadyHave);
        else if(!template.Repeatable && m_Owner.QuestLog.FinishedQuests.Contains(template.Id))
          QuestHandler.SendQuestInvalid(m_Owner, QuestInvalidReason.AlreadyCompleted);
        else if(!questGiver.CanGiveQuestTo(m_Owner))
        {
          QuestHandler.SendQuestInvalid(m_Owner, QuestInvalidReason.Tired);
        }
        else
        {
          Quest quest = m_Owner.QuestLog.AddQuest(template, freeSlot);
          if(quest.Template.Flags.HasFlag(QuestFlags.Escort))
            AutoComplete(quest, m_Owner);
          return quest;
        }
      }

      return null;
    }

    private static void AutoComplete(Quest quest, Character chr)
    {
      quest.CompleteStatus = QuestCompleteStatus.Completed;
      QuestHandler.SendComplete(quest.Template, chr);
      QuestHandler.SendQuestGiverOfferReward(chr, quest.Template, chr);
    }

    /// <summary>
    /// Adds the given new Quest.
    /// Returns null if no free slot was available or inital Items could not be handed out.
    /// </summary>
    /// <param name="qt">The qt.</param>
    /// <returns>false if it failed to add the quest</returns>
    public Quest AddQuest(QuestTemplate qt)
    {
      int freeSlot = FindFreeSlot();
      if(freeSlot != -1)
        return AddQuest(qt, freeSlot);
      return null;
    }

    /// <summary>
    /// Adds the given Quest as new active Quest
    /// Returns null if inital Items could not be handed out.
    /// </summary>
    /// <param name="qt"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    public Quest AddQuest(QuestTemplate qt, int slot)
    {
      Quest quest = new Quest(this, qt, slot);
      AddQuest(quest);
      return quest;
    }

    public Quest AddQuest(Quest quest)
    {
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Whether any Quest was cancelled</returns>
    public bool Cancel(uint id)
    {
      Quest activeQuest = GetActiveQuest(id);
      if(activeQuest == null)
        return false;
      activeQuest.Cancel(true);
      return true;
    }

    /// <summary>
    /// Removes the given quest.
    /// Internal - Use <see cref="M:WCell.RealmServer.Quests.Quest.Cancel(System.Boolean)" /> instead.
    /// </summary>
    internal bool RemoveQuest(uint questId)
    {
      for(int index = 0; index < m_ActiveQuests.Length; ++index)
      {
        if(m_ActiveQuests[index] != null && (int) m_ActiveQuests[index].Template.Id == (int) questId)
        {
          RemoveQuest(m_ActiveQuests[index]);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Removes the given quest.
    /// Internal - Use <see cref="M:WCell.RealmServer.Quests.Quest.Cancel(System.Boolean)" /> instead.
    /// </summary>
    internal void RemoveQuest(Quest quest)
    {
    }

    /// <summary>
    /// Adds the given Quest (if not already existing) marks it as completed
    /// and offers the reward to the user.
    /// Does nothing and returns null if all Quest slots are currently used.
    /// </summary>
    public bool HasActiveQuest(QuestTemplate templ)
    {
      return GetActiveQuest(templ.Id) != null;
    }

    public bool HasActiveQuest(uint questId)
    {
      return GetActiveQuest(questId) != null;
    }

    public bool HasFinishedQuest(uint questId)
    {
      return FinishedQuests.Contains(questId);
    }

    public bool CanFinish(uint questId)
    {
      Quest activeQuest = GetActiveQuest(questId);
      if(activeQuest != null)
        return activeQuest.CompleteStatus == QuestCompleteStatus.Completed;
      return false;
    }

    /// <summary>Gets the quest by template.</summary>
    /// <returns></returns>
    public Quest GetActiveQuest(uint questId)
    {
      foreach(Quest activeQuest in m_ActiveQuests)
      {
        if(activeQuest != null && (int) activeQuest.Template.Id == (int) questId)
          return activeQuest;
      }

      return null;
    }

    /// <summary>Gets the quest by slot.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns>Quest with given slot</returns>
    public Quest GetQuestBySlot(byte slot)
    {
      if(slot <= 25)
        return m_ActiveQuests[slot];
      return null;
    }

    /// <summary>Finds the free slot.</summary>
    /// <returns></returns>
    public int FindFreeSlot()
    {
      for(int index = 0; index < m_ActiveQuests.Length; ++index)
      {
        if(m_ActiveQuests[index] == null)
          return index;
      }

      return -1;
    }

    /// <summary>Gets the quest by id.</summary>
    /// <param name="qid">The qid.</param>
    /// <returns></returns>
    public Quest GetQuestById(uint qid)
    {
      foreach(Quest activeQuest in m_ActiveQuests)
      {
        if(activeQuest != null && (int) activeQuest.Template.Id == (int) qid)
          return activeQuest;
      }

      return null;
    }

    /// <summary>
    /// Gets the QuestGiver with the given guid from the current Map (in case of a <see cref="T:WCell.RealmServer.Entities.WorldObject" />) or
    /// Inventory (in case of an <see cref="T:WCell.RealmServer.Entities.Item">Item</see>)
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public IQuestHolder GetQuestGiver(EntityId guid)
    {
      return null;
    }

    /// <summary>
    /// Resets the daily quest count. Needs to be called in midnight (servertime) or when character logs in after the midnight
    /// </summary>
    public void ResetDailyQuests()
    {
      m_DailyQuestsToday.Clear();
    }

    /// <summary>
    /// Is called when the owner of this QuestLog did
    /// the required interaction with the given NPC (usually killing)
    /// </summary>
    /// <param name="npc"></param>
    internal void OnNPCInteraction(NPC npc)
    {
      foreach(Quest interactionQuest in m_NPCInteractionQuests)
      {
        if(interactionQuest.Template.NPCInteractions != null)
        {
          for(int index = 0; index < interactionQuest.Template.NPCInteractions.Length; ++index)
          {
            QuestInteractionTemplate npcInteraction = interactionQuest.Template.NPCInteractions[index];
            if(npcInteraction.TemplateId.Contains(npc.Entry.Id))
              UpdateInteractionCount(interactionQuest, npcInteraction, npc);
          }
        }
      }
    }

    /// <summary>
    /// Returns the first active Quest that requires the given NPC to be interacted with
    /// </summary>
    public Quest GetReqNPCQuest(NPCId npc)
    {
      for(int index1 = 0; index1 < m_NPCInteractionQuests.Count; ++index1)
      {
        Quest interactionQuest = m_NPCInteractionQuests[index1];
        for(int index2 = 0; index2 < interactionQuest.Template.NPCInteractions.Length; ++index2)
        {
          if(interactionQuest.Template.NPCInteractions[index2].TemplateId
            .Contains((uint) npc))
            return interactionQuest;
        }
      }

      return null;
    }

    /// <summary>
    /// Is called when the owner of this QuestLog used the given GameObject
    /// </summary>
    /// <param name="go"></param>
    public void OnUse(GameObject go)
    {
      foreach(Quest requireGosQuest in m_RequireGOsQuests)
      {
        if(requireGosQuest.Template.GOInteractions != null)
        {
          for(int index = 0; index < requireGosQuest.Template.GOInteractions.Length; ++index)
          {
            QuestInteractionTemplate goInteraction = requireGosQuest.Template.GOInteractions[index];
            if(goInteraction.TemplateId.Contains(go.Entry.Id))
              UpdateInteractionCount(requireGosQuest, goInteraction, go);
          }
        }
      }
    }

    private void UpdateInteractionCount(Quest quest, QuestInteractionTemplate interaction, WorldObject obj)
    {
      uint interaction1 = quest.Interactions[interaction.Index];
      if(interaction1 >= interaction.Amount)
        return;
      ++quest.Interactions[interaction.Index];
      m_Owner.SetQuestCount(quest.Slot, interaction.Index, (byte) (interaction1 + 1U));
      QuestHandler.SendUpdateInteractionCount(quest, obj, interaction, interaction1 + 1U,
        m_Owner);
      quest.UpdateStatus();
    }

    /// <summary>
    /// Whether the given GO can be used by the player to start or progress a quest
    /// </summary>
    public bool IsRequiredForAnyQuest(GameObject go)
    {
      if(go.QuestHolderInfo != null &&
         go.QuestHolderInfo.GetHighestQuestGiverStatus(Owner).CanStartOrFinish())
        return true;
      for(int index1 = 0; index1 < m_RequireGOsQuests.Count; ++index1)
      {
        Quest requireGosQuest = m_RequireGOsQuests[index1];
        for(int index2 = 0; index2 < requireGosQuest.Template.GOInteractions.Length; ++index2)
        {
          if(requireGosQuest.Template.GOInteractions[index2].TemplateId.Contains(
            go.EntryId))
            return true;
        }
      }

      return go.ContainsQuestItemsFor(Owner, LootEntryType.GameObject);
    }

    /// <summary>
    /// Is called when the owner of this QuestLog receives or looses the given amount of Items
    /// </summary>
    /// <param name="item"></param>
    internal void OnItemAmountChanged(Item item, int delta)
    {
      for(int index1 = 0; index1 < m_RequireItemsQuests.Count; ++index1)
      {
        Quest requireItemsQuest = m_RequireItemsQuests[index1];
        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableItems.Length; ++index2)
        {
          Asda2ItemStackDescription collectableItem = requireItemsQuest.Template.CollectableItems[index2];
          if(collectableItem.ItemId == item.Template.ItemId)
          {
            int collectedItem = requireItemsQuest.CollectedItems[index2];
            int num = collectedItem + delta;
            bool flag = collectedItem < collectableItem.Amount || num < collectableItem.Amount;
            requireItemsQuest.CollectedItems[index2] = num;
            if(flag)
            {
              QuestHandler.SendUpdateItems(item.Template.ItemId, delta, m_Owner);
              requireItemsQuest.UpdateStatus();
            }

            break;
          }
        }

        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableSourceItems.Length; ++index2)
        {
          Asda2ItemStackDescription collectableSourceItem =
            requireItemsQuest.Template.CollectableSourceItems[index2];
          if(collectableSourceItem.ItemId == item.Template.ItemId)
          {
            int collectedSourceItem = requireItemsQuest.CollectedSourceItems[index2];
            int num = collectedSourceItem + delta;
            bool flag = collectedSourceItem < collectableSourceItem.Amount ||
                        num < collectableSourceItem.Amount;
            requireItemsQuest.CollectedSourceItems[index2] = num;
            if(flag)
            {
              QuestHandler.SendUpdateItems(item.Template.ItemId, delta, m_Owner);
              requireItemsQuest.UpdateStatus();
            }

            break;
          }
        }
      }
    }

    /// <summary>The Quest that requires the given Item</summary>
    public Quest GetReqItemQuest(Asda2ItemId item)
    {
      for(int index1 = 0; index1 < m_RequireItemsQuests.Count; ++index1)
      {
        Quest requireItemsQuest = m_RequireItemsQuests[index1];
        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableItems.Length; ++index2)
        {
          if(requireItemsQuest.Template.CollectableItems[index2].ItemId == item)
            return requireItemsQuest;
        }

        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableSourceItems.Length; ++index2)
        {
          if(requireItemsQuest.Template.CollectableSourceItems[index2].ItemId == item)
            return requireItemsQuest;
        }
      }

      return null;
    }

    /// <summary>Whether the given Item is needed for an active Quest</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool RequiresItem(Asda2ItemId item)
    {
      for(int index1 = 0; index1 < m_RequireItemsQuests.Count; ++index1)
      {
        Quest requireItemsQuest = m_RequireItemsQuests[index1];
        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableItems.Length; ++index2)
        {
          Asda2ItemStackDescription collectableItem = requireItemsQuest.Template.CollectableItems[index2];
          if(collectableItem.ItemId == item)
            return requireItemsQuest.CollectedItems[index2] < collectableItem.Amount;
        }

        for(int index2 = 0; index2 < requireItemsQuest.Template.CollectableSourceItems.Length; ++index2)
        {
          Asda2ItemStackDescription collectableSourceItem =
            requireItemsQuest.Template.CollectableSourceItems[index2];
          if(collectableSourceItem.ItemId == item)
            return requireItemsQuest.CollectedSourceItems[index2] < collectableSourceItem.Amount;
        }
      }

      return false;
    }

    internal void OnSpellCast(SpellCast cast)
    {
      if(m_RequireSpellCastsQuests.Count <= 0)
        return;
      foreach(Quest requireSpellCastsQuest in m_RequireSpellCastsQuests)
      {
        foreach(QuestInteractionTemplate spellInteraction in requireSpellCastsQuest.Template.SpellInteractions)
        {
          QuestInteractionTemplate interaction = spellInteraction;
          if(interaction.RequiredSpellId == cast.Spell.SpellId)
          {
            switch(interaction.ObjectType)
            {
              case ObjectTypeId.Unit:
                WorldObject worldObject1 = cast.Targets.FirstOrDefault(
                  target =>
                  {
                    if(target is NPC)
                      return interaction.TemplateId.Contains(
                        target.EntryId);
                    return false;
                  });
                if(worldObject1 != null)
                {
                  UpdateInteractionCount(requireSpellCastsQuest, interaction, worldObject1);
                }

                continue;
              case ObjectTypeId.GameObject:
                WorldObject worldObject2 = cast.Targets.FirstOrDefault(
                  target =>
                  {
                    if(target is GameObject)
                      return interaction.TemplateId.Contains(
                        target.EntryId);
                    return false;
                  });
                if(worldObject2 != null)
                {
                  UpdateInteractionCount(requireSpellCastsQuest, interaction, worldObject2);
                }

                continue;
              default:
                UpdateInteractionCount(requireSpellCastsQuest, interaction, null);
                continue;
            }
          }
        }
      }
    }

    public void SaveQuests()
    {
      for(int index = 0; index < 25; ++index)
      {
        if(m_ActiveQuests[index] != null)
          m_ActiveQuests[index].Save();
      }
    }

    /// <summary>
    /// If we want this method to be public,
    /// it should update all Quests correctly (remove non-existant ones etc)
    /// </summary>
    internal void Load()
    {
      QuestRecord[] recordForCharacter;
      try
      {
        recordForCharacter = QuestRecord.GetQuestRecordForCharacter(Owner.EntityId.Low);
      }
      catch(Exception ex)
      {
        RealmDBMgr.OnDBError(ex);
        recordForCharacter = QuestRecord.GetQuestRecordForCharacter(Owner.EntityId.Low);
      }

      if(recordForCharacter == null)
        return;
      foreach(QuestRecord record in recordForCharacter)
      {
        QuestTemplate template = QuestMgr.GetTemplate(record.QuestTemplateId);
        if(template != null)
        {
          Quest quest = new Quest(this, record, template);
          AddQuest(quest);
          if(template.EventIds.Count > 0 && !template.EventIds
               .Where(WorldEventMgr.IsEventActive).Any())
            quest.Cancel(false);
        }
        else
          log.Error("Character {0} had Invalid Quest: {1} (Record: {2})", Owner,
            record.QuestTemplateId, record.QuestRecordId);
      }
    }

    /// <summary>
    /// Removes the given quest from the list of finished quests
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Whether the Character had the given Quest</returns>
    public bool RemoveFinishedQuest(uint id)
    {
      if(!m_FinishedQuests.Remove(id))
        return false;
      Owner.FindAndSendAllNearbyQuestGiverStatuses();
      return true;
    }

    public bool CanGiveQuestTo(Character chr)
    {
      return chr.IsAlliedWith(Owner);
    }
  }
}