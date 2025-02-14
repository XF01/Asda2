﻿using Castle.ActiveRecord;
using WCell.Constants.Skills;

namespace WCell.RealmServer.Database
{
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class SkillRecord : ActiveRecordBase<SkillRecord>
  {
    [Field("SkillId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _skillId;

    [Field("CurrentValue", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private short _value;

    [Field("MaxVal", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private short _max;

    [PrimaryKey(PrimaryKeyType.Increment, "EntityLowId")]
    public long Guid { get; set; }

    [Property(NotNull = true)]
    public long OwnerId { get; set; }

    public SkillId SkillId
    {
      get { return (SkillId) _skillId; }
      set { _skillId = (int) value; }
    }

    public ushort CurrentValue
    {
      get { return (ushort) _value; }
      set { _value = (short) value; }
    }

    public ushort MaxValue
    {
      get { return (ushort) _max; }
      set { _max = (short) value; }
    }

    public static SkillRecord[] GetAllSkillsFor(long charRecordId)
    {
      return FindAllByProperty("OwnerId", charRecordId);
    }
  }
}