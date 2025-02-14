﻿using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects
{
  /// <summary>
  /// Should not be changed, once a GameObject of a certain Entry has been added to the World
  /// </summary>
  [DependingProducer(GameObjectType.Mailbox, typeof(GOMailboxEntry))]
  [DependingProducer(GameObjectType.GuardPost, typeof(GOGuardPostEntry))]
  [DependingProducer(GameObjectType.DuelFlag, typeof(GODuelFlagEntry))]
  [DataHolder("Type")]
  [DependingProducer(GameObjectType.Door, typeof(GODoorEntry))]
  [DependingProducer(GameObjectType.Button, typeof(GOButtonEntry))]
  [DependingProducer(GameObjectType.QuestGiver, typeof(GOQuestGiverEntry))]
  [DependingProducer(GameObjectType.Custom, typeof(GOCustomEntry))]
  [DependingProducer(GameObjectType.Binder, typeof(GOBinderEntry))]
  [DependingProducer(GameObjectType.Generic, typeof(GOGenericEntry))]
  [DependingProducer(GameObjectType.Trap, typeof(GOTrapEntry))]
  [DependingProducer(GameObjectType.Chair, typeof(GOChairEntry))]
  [DependingProducer(GameObjectType.SpellFocus, typeof(GOSpellFocusEntry))]
  [DependingProducer(GameObjectType.Text, typeof(GOTextEntry))]
  [DependingProducer(GameObjectType.Goober, typeof(GOGooberEntry))]
  [DependingProducer(GameObjectType.Transport, typeof(GOTransportEntry))]
  [DependingProducer(GameObjectType.AreaDamage, typeof(GOAreaDamageEntry))]
  [DependingProducer(GameObjectType.Camera, typeof(GOCameraEntry))]
  [DependingProducer(GameObjectType.MapObject, typeof(GOMapObjectEntry))]
  [DependingProducer(GameObjectType.MOTransport, typeof(GOMOTransportEntry))]
  [DependingProducer(GameObjectType.FishingNode, typeof(GOFishingNodeEntry))]
  [DependingProducer(GameObjectType.SummoningRitual, typeof(GOSummoningRitualEntry))]
  [DependingProducer(GameObjectType.Chest, typeof(GOChestEntry))]
  [DependingProducer(GameObjectType.DONOTUSE, typeof(GOAuctionHouseEntry))]
  [DependingProducer(GameObjectType.SpellCaster, typeof(GOSpellCasterEntry))]
  [DependingProducer(GameObjectType.MeetingStone, typeof(GOMeetingStoneEntry))]
  [DependingProducer(GameObjectType.FlagStand, typeof(GOFlagStandEntry))]
  [DependingProducer(GameObjectType.FishingHole, typeof(GOFishingHoleEntry))]
  [DependingProducer(GameObjectType.FlagDrop, typeof(GOFlagDropEntry))]
  [DependingProducer(GameObjectType.MiniGame, typeof(GOMiniGameEntry))]
  [DependingProducer(GameObjectType.LotteryKiosk, typeof(GOLotteryKioskEntry))]
  [DependingProducer(GameObjectType.CapturePoint, typeof(GOCapturePointEntry))]
  [DependingProducer(GameObjectType.AuraGenerator, typeof(GOAuraGeneratorEntry))]
  [DependingProducer(GameObjectType.DungeonDifficulty, typeof(GODungeonDifficultyEntry))]
  [DependingProducer(GameObjectType.BarberChair, typeof(GOBarberChairEntry))]
  [DependingProducer(GameObjectType.DestructibleBuilding, typeof(GODestructibleBuildingEntry))]
  [DependingProducer(GameObjectType.GuildBank, typeof(GOGuildBankEntry))]
  [DependingProducer(GameObjectType.TrapDoor, typeof(GOTrapDoorEntry))]
  public abstract class GOEntry : ObjectTemplate, IDataHolder
  {
    [Persistent(24)]public int[] Fields = new int[24];

    /// <summary>All Templates that use this GOEntry</summary>
    [NotPersistent]public readonly List<GOSpawnEntry> SpawnEntries = new List<GOSpawnEntry>();

    [NotPersistent]public readonly List<QuestTemplate> RequiredQuests = new List<QuestTemplate>(3);
    public uint DisplayId;
    public FactionTemplateId FactionId;
    public GameObjectFlags Flags;
    public GameObjectType Type;
    [NotPersistent]public GOEntryId GOId;
    [Persistent(8)]public string[] Names;

    /// <summary>Whether this GO vanishes after using</summary>
    [NotPersistent]public bool IsConsumable;

    /// <summary>
    /// Whether one may be mounted when using GOs of this Entry.
    /// </summary>
    [NotPersistent]public bool AllowMounted;

    /// <summary>Whether one needs LoS to use this GO</summary>
    [NotPersistent]public bool LosOk;

    /// <summary>
    /// Whether this GO's loot can be taken by the whole Group.
    /// </summary>
    [NotPersistent]public bool UseGroupLoot;

    /// <summary>
    /// The Id of a GOTrapEntry that is associated with this chest.
    /// </summary>
    [NotPersistent]public uint LinkedTrapId;

    /// <summary>The Trap that is linked to this Object (if any)</summary>
    [NotPersistent]public GOTrapEntry LinkedTrap;

    [NotPersistent]public uint SummonSlotId;

    /// <summary>The lock of this GO (if any)</summary>
    [NotPersistent]public LockEntry Lock;

    [NotPersistent]public Func<GameObject> GOCreator;
    [NotPersistent]public Func<GameObjectHandler> HandlerCreator;

    /// <summary>
    /// Is called whenever an instance of this entry is used by the given user
    /// </summary>
    public event GOUseHandler Used;

    /// <summary>
    /// Is called whenever a new GameObject of this entry is added to the world
    /// (usually only happens once, when the GO is created)
    /// </summary>
    public event Action<GameObject> Activated;

    [NotPersistent]
    public string DefaultName
    {
      get { return Names.LocalizeWithDefaultLocale(); }
      set
      {
        if(Names == null)
          Names = new string[8];
        Names[(int) RealmServerConfiguration.DefaultLocale] = value;
      }
    }

    public GOSpawnEntry FirstSpawnEntry
    {
      get
      {
        if(SpawnEntries.Count <= 0)
          return null;
        return SpawnEntries[0];
      }
    }

    public override IWorldLocation[] GetInWorldTemplates()
    {
      return SpawnEntries.ToArray();
    }

    [NotPersistent]
    public Faction Faction { get; set; }

    public virtual bool IsTransport
    {
      get { return false; }
    }

    public override List<Asda2LootItemEntry> GetLootEntries()
    {
      if(this is IGOLootableEntry)
        return Asda2LootMgr.GetEntries(Asda2LootEntryType.Npc, Id);
      return null;
    }

    /// <summary>Some GOs have a QuestId</summary>
    public virtual uint QuestId
    {
      get { return 0; }
    }

    /// <summary>Some GOs have a PageId</summary>
    public virtual uint PageId
    {
      get { return 0; }
    }

    public virtual uint GossipId
    {
      get { return 0; }
    }

    /// <summary>
    /// Whether only users of the same Party as the owner
    /// may use this
    /// </summary>
    public virtual bool IsPartyOnly
    {
      get { return false; }
    }

    protected internal virtual void InitEntry()
    {
    }

    /// <summary>
    /// Is called when the given new GameObject has been created.
    /// </summary>
    /// <param name="go"></param>
    protected internal virtual void InitGO(GameObject go)
    {
    }

    public override string ToString()
    {
      return DefaultName + " (ID: " + (int) Id + ", " + Id + ")";
    }

    public virtual void FinalizeDataHolder()
    {
      if(Id != 0U)
        GOId = (GOEntryId) Id;
      else
        Id = (uint) GOId;
      if(FactionId != FactionTemplateId.None)
        Faction = FactionMgr.Get(FactionId);
      InitEntry();
      if(GossipId != 0U && DefaultGossip == null)
      {
        if(GossipMgr.GetEntry(GossipId) == null)
        {
          ContentMgr.OnInvalidDBData("GOEntry {0} has missing GossipId: {1}", (object) this,
            (object) GossipId);
          DefaultGossip = new GossipMenu();
        }
        else
          DefaultGossip = new GossipMenu(GossipId);
      }
      else if(QuestHolderInfo != null)
        DefaultGossip = new GossipMenu();

      if(HandlerCreator == null)
        HandlerCreator = GOMgr.Handlers[(int) Type];
      if(GOCreator == null)
        GOCreator = !IsTransport
          ? () => new GameObject()
          : (Func<GameObject>) (() => (GameObject) new Transport());
      if(Fields == null)
        return;
      GOMgr.Entries[Id] = this;
    }

    public GameObject Spawn(IWorldLocation location)
    {
      return Spawn(location, location as Unit);
    }

    public GameObject Spawn(IWorldLocation where, Unit owner)
    {
      GameObject gameObject = GameObject.Create(this, where, FirstSpawnEntry, null);
      gameObject.Owner = owner;
      return gameObject;
    }

    /// <summary>
    /// Spawns and returns a new GameObject from this template into the given map
    /// </summary>
    /// <param name="owner">Can be null, if the GO is not owned by anyone</param>
    /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
    public GameObject Spawn(MapId map, Vector3 pos)
    {
      return Spawn(map, pos, null);
    }

    /// <summary>
    /// Spawns and returns a new GameObject from this template into the given map
    /// </summary>
    /// <param name="owner">Can be null, if the GO is not owned by anyone</param>
    /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
    public GameObject Spawn(MapId map, Vector3 pos, Unit owner)
    {
      return Spawn(World.GetNonInstancedMap(map), pos, owner);
    }

    /// <summary>
    /// Spawns and returns a new GameObject from this template into the given map
    /// </summary>
    /// <param name="owner">Can be null, if the GO is not owned by anyone</param>
    /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
    public GameObject Spawn(Map map, Vector3 pos, Unit owner = null)
    {
      if(map == null)
        throw new ArgumentNullException(nameof(map));
      return Spawn(new WorldLocation(map, pos, 1U), owner);
    }

    /// <summary>
    /// Returns the GOTemplate of this entry that is closest to the given location
    /// </summary>
    public GOSpawnEntry GetClosestTemplate(IWorldLocation pos)
    {
      return SpawnEntries.GetClosestEntry(pos);
    }

    /// <summary>
    /// Returns the GOTemplate of this entry that is closest to the given location
    /// </summary>
    public GOSpawnEntry GetClosestTemplate(MapId rgn, Vector3 pos)
    {
      return SpawnEntries.GetClosestEntry(new WorldLocationStruct(rgn, pos, 1U));
    }

    /// <summary>
    /// Returns the GOTemplate of this entry that is closest to the given location
    /// </summary>
    public GOSpawnEntry GetClosestTemplate(Map rgn, Vector3 pos)
    {
      return SpawnEntries.GetClosestEntry(new WorldLocationStruct(rgn, pos, 1U));
    }

    internal bool NotifyUsed(GameObject go, Character user)
    {
      GOUseHandler used = Used;
      if(used != null)
        return used(go, user);
      return true;
    }

    internal void NotifyActivated(GameObject go)
    {
      Action<GameObject> activated = Activated;
      if(activated == null)
        return;
      activated(go);
    }

    public static IEnumerable<GOEntry> GetAllDataHolders()
    {
      return GOMgr.Entries.Values;
    }

    public delegate bool GOUseHandler(GameObject go, Character user);
  }
}