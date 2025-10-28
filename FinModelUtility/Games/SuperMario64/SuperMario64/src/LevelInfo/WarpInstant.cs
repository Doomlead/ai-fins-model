﻿using System.ComponentModel;

namespace sm64.LevelInfo {
  public sealed class WarpInstant {
    private const ushort NUM_OF_CATERGORIES = 2;

    private byte triggerID;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Trigger ID")]
    public byte TriggerID {
      get { return this.triggerID; }
      set { this.triggerID = value; }
    }

    private byte areaID;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("To Area")]
    public byte AreaID {
      get { return this.areaID; }
      set { this.areaID = value; }
    }

    private short teleX;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Teleport X")]
    public short TeleX {
      get { return this.teleX; }
      set { this.teleX = value; }
    }

    private short teleY;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Teleport Y")]
    public short TeleY {
      get { return this.teleY; }
      set { this.teleY = value; }
    }

    private short teleZ;

    [CustomSortedCategory("Instant Warp", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("Teleport Z")]
    public short TeleZ {
      get { return this.teleZ; }
      set { this.teleZ = value; }
    }

    [CustomSortedCategory("Info", 2, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [Description("Location inside the ROM file")]
    [DisplayName("Address")]
    [ReadOnly(true)]
    public string Address { get; set; }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
    }


    private string getWarpName() {
      return " [to Area " + this.AreaID + "]";
    }

    public override string ToString() {
      //isPaintingWarp
      string warpName = "Instant Warp 0x";

      warpName += this.TriggerID.ToString("X2") + this.getWarpName();

      return warpName;
    }
  }
}