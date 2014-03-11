using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using WvsBeta.Common.Sessions;
using WvsBeta.Common;

namespace WvsBeta.Game
{
    public enum MobControlStatus
    {
        ControlNormal,
        ControlNone
    }

    public class Mob : MovableLife
    {
        public Life SpawnData { get; set; }
        public int MobID { get; set; }
        public int MapID { get; set; }
        public int SpawnID { get; set; }

        public int EXP { get { return Data.EXP; } }
        public int HP { get; set; }
        public int MaxHP { get { return Data.MaxHP; } }
        public int MP { get; set; }
        public int MaxMP { get { return Data.MaxMP; } }

        public Mob Owner { get; set; }
        public Character Controller { get; set; }
        public short OriginalFoothold { get; set; }
        public Dictionary<int, ulong> Damages { get; set; }
        public MobControlStatus ControlStatus { get; set; }
        public float AllowedSpeed { get; set; }
        public DateTime lastMove { get; set; }
        public bool DoesRespawn { get; set; }
        public bool IsDead { get; set; }
        public Pos OriginalPosition { get; set; }
        public Dictionary<byte, DateTime> LastSkillUse { get; set; }
        public Dictionary<int, Mob> SpawnedMobs { get; set; }
        public bool IsBoss { get { return Data.Boss; } }
        public byte MpEats { get; set; }

        public MobData Data { get { return DataProvider.Mobs[MobID]; } }

        public MobStatus Status { get; private set; }

        private int DeadsInFiveMinutes { get; set; }

        internal Mob(int id, int mapid, int mobid, Pos position, int spawnid, byte direction, short foothold, MobControlStatus controlStatus, bool pDoesRespawn = true) :
            base(foothold, position, 2)
        {
            Damages = new Dictionary<int, ulong>();
            OriginalFoothold = foothold;
            MobID = mobid;
            MapID = mapid;
            SpawnID = id;
            ControlStatus = controlStatus;
            DoesRespawn = pDoesRespawn;
            OriginalPosition = position;
            DeadsInFiveMinutes = 0;
            Init();
        }

        internal Mob(int id, int mapid, int mobid, Pos position, short foothold, MobControlStatus controlStatus, bool pDoesRespawn = true) :
            base(foothold, position, 2)
        {
            Damages = new Dictionary<int, ulong>();
            OriginalFoothold = foothold;
            MobID = mobid;
            MapID = mapid;
            SpawnID = id;
            ControlStatus = controlStatus;
            DoesRespawn = pDoesRespawn;
            OriginalPosition = position;
            DeadsInFiveMinutes = 0;
            Init();
        }

        public void Cleanup()
        {
            OriginalPosition = null;
            Position = null;
            Owner = null;
            Controller = null;
            Status = null;
            LastSkillUse = null;
            SpawnData = null;
            Damages = null;
        }

        public void SetSpawnData(Life sd) { SpawnData = sd; }

        public void InitAndSpawn()
        {
            Init();
            MpEats = 0;

            MobPacket.SendMobSpawn(null, this, 0, null, true, false);

            DataProvider.Maps[MapID].UpdateMobControl(this, true, null);
        }

        DateTime lastCheck = DateTime.Now;
        DateTime lastPoisonDMG = DateTime.Now;
        DateTime lastStatusUpdate = DateTime.Now;
        DateTime lastHeal = DateTime.Now;
        public void UpdateDeads(DateTime pNow)
        {
            if (IsDead) return;
            if (DeadsInFiveMinutes > 0 && (pNow - lastCheck).TotalMinutes >= 1) DeadsInFiveMinutes--;
            lastCheck = pNow;


            if ((pNow - lastStatusUpdate).TotalSeconds >= 3)
            {
                lastStatusUpdate = pNow;
                Status.Update(pNow);
            }
            if (Status.Poison_N == 1 && (pNow - lastPoisonDMG).TotalSeconds >= 1)
            {
                lastPoisonDMG = pNow;
                GiveDamage(null, Status.Poison_DMG, true);
            }
            if ((pNow - lastHeal).TotalSeconds >= 8)
            {
                if (Data.HPRecoverAmount > 0 && HP < MaxHP)
                {
                    HP += Data.HPRecoverAmount;
                    if (HP > MaxHP) HP = MaxHP;

                    if (IsBoss && Data.HPTagBgColor > 0) // There's no way to hide this :|
                    {
                        MapPacket.SendBossHPBar(MapID, IsDead ? -1 : HP, MaxHP, Data.HPTagBgColor, Data.HPTagColor);
                    }
                }

                if (Data.MPRecoverAmount > 0 && MP < MaxMP)
                {
                    MP += Data.MPRecoverAmount;
                    if (MP > MaxMP) MP = MaxMP;
                }
                lastHeal = pNow;
            }
        }


        public void Init()
        {
            IsDead = false;

            if (LastSkillUse != null)
                LastSkillUse.Clear();
            else
                LastSkillUse = new Dictionary<byte, DateTime>();

            if (SpawnedMobs != null)
                SpawnedMobs.Clear();
            else
                SpawnedMobs = new Dictionary<int, Mob>();

            HP = MaxHP;
            MP = MaxMP;
            Owner = null;
            Controller = null;
            AllowedSpeed = (100 + Data.Speed) / 100.0f;
            lastMove = MasterThread.CurrentDate;
            if (Status == null)
            {
                Status = new MobStatus(this);
            }
            Status.Clear();
        }

        public bool GiveDamage(Character fucker, int amount, bool pWasPoison = false)
        {
            if (fucker != null)
            {
                if (amount > 99999 && !fucker.Admin)
                {
                    // DAMAGE HAX
                    fucker.SetAP(0);
                    fucker.SetSP(0);
                    fucker.SetStr(0);
                    fucker.SetDex(0);
                    fucker.SetInt(0);
                    fucker.SetHP(1);
                    fucker.SetMP(0);
                    MessagePacket.SendMegaphoneMessage(string.Format("{0} : I was just p0wned by the Anti-Hack system. Damage hax!", fucker.Name));
                    Program.LogFile.WriteLine("{0} has used a damage hack! {1} damage done D:", fucker.Name, amount);
                    return false;
                }

                if (HP == 0) return false;
                if (!Damages.ContainsKey(fucker.ID))
                    Damages.Add(fucker.ID, 0);
                Damages[fucker.ID] += (ulong)amount;

                if (HP < amount)
                    HP = 0;
                else
                    HP -= amount;

                if (IsBoss && Data.HPTagBgColor > 0) // There's no way to hide this :|
                {
                    MapPacket.SendBossHPBar(MapID, IsDead ? -1 : HP, MaxHP, Data.HPTagBgColor, Data.HPTagColor);
                }
            }
            else
            {
                int minVal = pWasPoison ? 1 : 0;

                HP -= amount;

                if (HP < 0) HP = minVal;

                if (pWasPoison)
                {
                    if (HP == minVal)
                    {
                        Status.Poison_DMG = Status.Poison_N = Status.Poison_R = 0;
                        Status.Poison_T = 0;
                        MobPacket.SendMobStatsTempReset(this, (uint)MobStatus.MobStatValue.Poison);
                    }
                    else
                    {
                        MobPacket.SendMobDamageOrHeal(MapID, SpawnID, amount, false);
                    }
                }
            }
            return true;
        }

        public bool CheckDead(Pos killPos = null)
        {
            if (!(HP == 0 && !IsDead)) return false;
            IsDead = true;
            if (killPos != null)
                Position = killPos;

            SetControl(null, false, null);

            MobPacket.SendMobDeath(this, 1);


            Character maxDmgChar = null;
            ulong maxDmgAmount = 0;
            MobData md = DataProvider.Mobs[MobID];
            DeadsInFiveMinutes++;

            foreach (KeyValuePair<int, ulong> dmg in Damages)
            {
                if (maxDmgAmount < dmg.Value && Server.Instance.CharacterList.ContainsKey(dmg.Key))
                {
                    Character chr = Server.Instance.CharacterList[dmg.Key];
                    if (chr.Map == MapID)
                    {
                        maxDmgAmount = dmg.Value;
                        maxDmgChar = chr;
                    }
                }
            }

            if (maxDmgChar != null)
            {
                if (maxDmgChar.Donator || Map.isPremium(maxDmgChar.Map))
                {
                    maxDmgChar.AddEXP((double)EXP * maxDmgChar.Inventory.GetExtraEXPRate() * Server.mobExpRate * Map.MAP_PREMIUM_EXP); //Premium Maps, also known as Internet Cafe maps
                }
                else
                {
                    maxDmgChar.AddEXP((uint)EXP * maxDmgChar.Inventory.GetExtraEXPRate() * Server.mobExpRate);
                }
                DropPacket.HandleDrops(maxDmgChar, MapID, Constants.getDropName(MobID, true), SpawnID, Position, false, false, false);
            }


            foreach (int mobid in md.Revive)
            {
                DataProvider.Maps[MapID].spawnMobNoRespawn(mobid, Position, Foothold);
            }


            Damages.Clear();

            if (DoesRespawn)
            {
                Position = OriginalPosition;
                Foothold = OriginalFoothold;

                int respawnTime = SpawnData.RespawnTime;
                if (respawnTime == 0)
                {
                    respawnTime = Server.Instance.Randomizer.ValueBetween(0, (DeadsInFiveMinutes + 1) * 5) + 4;
                }

                double derp = respawnTime;
                derp *= 1000;
                derp /= DataProvider.Maps[MapID].MobRate;

                derp = (double)Server.Instance.Randomizer.ValueBetween((int)derp, (int)(derp * 2));


                MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction("Mob Remover", (date) =>
                {
                    InitAndSpawn();
                }, (ulong)derp, 0));

            }
            else
            {
                Cleanup();
                DataProvider.Maps[MapID].RemoveMob(this);
            }
            return true;
        }

        public void SetControl(Character control, bool spawn, Character display)
        {
            Controller = control;
            if (HP == 0) return;
            if (control != null)
            {
                MobPacket.SendMobRequestControl(control, this, spawn, null);
            }
            else if (ControlStatus == MobControlStatus.ControlNone)
            {
                MobPacket.SendMobRequestControl(control, this, spawn, display);
            }
        }

        public void endControl()
        {
            if (Controller != null && Controller.Map == MapID)
            {
                MobPacket.SendMobRequestEndControl(Controller, this);
            }
        }

        public void setControlStatus(MobControlStatus mcs)
        {
            MobPacket.SendMobRequestEndControl(null, this);
            MobPacket.SendMobSpawn(null, this, 0, null, false, false);
            ControlStatus = mcs;
            DataProvider.Maps[MapID].UpdateMobControl(this, false, null);
        }
    }

    public class MobStatus
    {
        public MobStatus(Mob pMob)
        {
            Mob = pMob;
        }
        public Mob Mob { get; private set; }

        public int PhysicalDamage_N { get; set; }
        public int PhysicalDamage_R { get; set; }
        public long PhysicalDamage_T { get; set; }
        public int PhysicalDefense_N { get; set; }
        public int PhysicalDefense_R { get; set; }
        public long PhysicalDefense_T { get; set; }
        public int MagicDamage_N { get; set; }
        public int MagicDamage_R { get; set; }
        public long MagicDamage_T { get; set; }
        public int MagicDefense_N { get; set; }
        public int MagicDefense_R { get; set; }
        public long MagicDefense_T { get; set; }
        public int Accurrency_N { get; set; }
        public int Accurrency_R { get; set; }
        public long Accurrency_T { get; set; }
        public int Evasion_N { get; set; }
        public int Evasion_R { get; set; }
        public long Evasion_T { get; set; }
        public int Speed_N { get; set; }
        public int Speed_R { get; set; }
        public long Speed_T { get; set; }
        public int Stun_N { get; set; }
        public int Stun_R { get; set; }
        public long Stun_T { get; set; }
        public int Freeze_N { get; set; }
        public int Freeze_R { get; set; }
        public long Freeze_T { get; set; }
        public int Poison_N { get; set; }
        public int Poison_R { get; set; }
        public long Poison_T { get; set; }
        public int Poison_DMG { get; set; }
        public int Seal_N { get; set; }
        public int Seal_R { get; set; }
        public long Seal_T { get; set; }
        public int Darkness_N { get; set; }
        public int Darkness_R { get; set; }
        public long Darkness_T { get; set; }
        public int PowerUp_N { get; set; }
        public int PowerUp_R { get; set; }
        public long PowerUp_T { get; set; }
        public int MagicUp_N { get; set; }
        public int MagicUp_R { get; set; }
        public long MagicUp_T { get; set; }
        public int PowerGuardUp_N { get; set; }
        public int PowerGuardUp_R { get; set; }
        public long PowerGuardUp_T { get; set; }
        public int MagicGuardUp_N { get; set; }
        public int MagicGuardUp_R { get; set; }
        public long MagicGuardUp_T { get; set; }
        public int Doom_N { get; set; }
        public int Doom_R { get; set; }
        public long Doom_T { get; set; }
        public int Web_N { get; set; }
        public int Web_R { get; set; }
        public long Web_T { get; set; }
        public int PhysicalImmune_N { get; set; }
        public int PhysicalImmune_R { get; set; }
        public long PhysicalImmune_T { get; set; }
        public int MagicImmune_N { get; set; }
        public int MagicImmune_R { get; set; }
        public long MagicImmune_T { get; set; }
        public int HardSkin_N { get; set; }
        public int HardSkin_R { get; set; }
        public long HardSkin_T { get; set; }
        public int Ambush_N { get; set; }
        public int Ambush_R { get; set; }
        public long Ambush_T { get; set; }
        public int Venom_N { get; set; }
        public int Venom_R { get; set; }
        public long Venom_T { get; set; }
        public int Blind_N { get; set; }
        public int Blind_R { get; set; }
        public long Blind_T { get; set; }
        public int SealSkill_N { get; set; }
        public int SealSkill_R { get; set; }
        public long SealSkill_T { get; set; }


        public enum MobStatValue : uint
        {
            PhysicalDamage = 0x1,
            PhysicalDefense = 0x2,
            MagicDamage = 0x4,
            MagicDefense = 0x8,
            Accurrency = 0x10,
            Evasion = 0x20,
            Speed = 0x40,
            Stun = 0x80,
            Freeze = 0x100,
            Poison = 0x200,
            Seal = 0x400,
            Darkness = 0x800,
            PowerUp = 0x1000,
            MagicUp = 0x2000,
            PowerGuardUp = 0x4000,
            MagicGuardUp = 0x8000,
            Doom = 0x10000,
            Web = 0x20000,
            PhysicalImmune = 0x40000,
            MagicImmune = 0x80000,
            HardSkin = 0x200000,
            Ambush = 0x400000,
            Venom = 0x1000000,
            Blind = 0x2000000,
            SealSkill = 0x4000000,
        }



        public void Encode(Packet pPacket, uint pSpecificFlag = 0xFFFFFFFF)
        {
            long currentTime = Tools.GetTimeAsMilliseconds(MasterThread.CurrentDate);
            int tmpBuffPos = pPacket.Position;
            uint endFlag = 0;
            pPacket.WriteUInt(endFlag);

            if (PhysicalDamage_T > 0 && (pSpecificFlag & (uint)MobStatValue.PhysicalDamage) == (uint)MobStatValue.PhysicalDamage)
            {
                pPacket.WriteShort((short)PhysicalDamage_N);
                pPacket.WriteInt(PhysicalDamage_R);
                pPacket.WriteShort((short)(PhysicalDamage_T - currentTime));
                endFlag |= (uint)MobStatValue.PhysicalDamage;
            }
            if (PhysicalDefense_T > 0 && (pSpecificFlag & (uint)MobStatValue.PhysicalDefense) == (uint)MobStatValue.PhysicalDefense)
            {
                pPacket.WriteShort((short)PhysicalDefense_N);
                pPacket.WriteInt(PhysicalDefense_R);
                pPacket.WriteShort((short)(PhysicalDefense_T - currentTime));
                endFlag |= (uint)MobStatValue.PhysicalDefense;
            }
            if (MagicDamage_T > 0 && (pSpecificFlag & (uint)MobStatValue.MagicDamage) == (uint)MobStatValue.MagicDamage)
            {
                pPacket.WriteShort((short)MagicDamage_N);
                pPacket.WriteInt(MagicDamage_R);
                pPacket.WriteShort((short)(MagicDamage_T - currentTime));
                endFlag |= (uint)MobStatValue.MagicDamage;
            }
            if (MagicDefense_T > 0 && (pSpecificFlag & (uint)MobStatValue.MagicDefense) == (uint)MobStatValue.MagicDefense)
            {
                pPacket.WriteShort((short)MagicDefense_N);
                pPacket.WriteInt(MagicDefense_R);
                pPacket.WriteShort((short)(MagicDefense_T - currentTime));
                endFlag |= (uint)MobStatValue.MagicDefense;
            }
            if (Accurrency_T > 0 && (pSpecificFlag & (uint)MobStatValue.Accurrency) == (uint)MobStatValue.Accurrency)
            {
                pPacket.WriteShort((short)Accurrency_N);
                pPacket.WriteInt(Accurrency_R);
                pPacket.WriteShort((short)(Accurrency_T - currentTime));
                endFlag |= (uint)MobStatValue.Accurrency;
            }
            if (Evasion_T > 0 && (pSpecificFlag & (uint)MobStatValue.Evasion) == (uint)MobStatValue.Evasion)
            {
                pPacket.WriteShort((short)Evasion_N);
                pPacket.WriteInt(Evasion_R);
                pPacket.WriteShort((short)(Evasion_T - currentTime));
                endFlag |= (uint)MobStatValue.Evasion;
            }
            if (Speed_T > 0 && (pSpecificFlag & (uint)MobStatValue.Speed) == (uint)MobStatValue.Speed)
            {
                pPacket.WriteShort((short)Speed_N);
                pPacket.WriteInt(Speed_R);
                pPacket.WriteShort((short)(Speed_T - currentTime));
                endFlag |= (uint)MobStatValue.Speed;
            }
            if (Stun_T > 0 && (pSpecificFlag & (uint)MobStatValue.Stun) == (uint)MobStatValue.Stun)
            {
                pPacket.WriteShort((short)Stun_N);
                pPacket.WriteInt(Stun_R);
                pPacket.WriteShort((short)(Stun_T - currentTime));
                endFlag |= (uint)MobStatValue.Stun;
            }
            if (Freeze_T > 0 && (pSpecificFlag & (uint)MobStatValue.Freeze) == (uint)MobStatValue.Freeze)
            {
                pPacket.WriteShort((short)Freeze_N);
                pPacket.WriteInt(Freeze_R);
                pPacket.WriteShort((short)(Freeze_T - currentTime));
                endFlag |= (uint)MobStatValue.Freeze;
            }
            if (Poison_T > 0 && (pSpecificFlag & (uint)MobStatValue.Poison) == (uint)MobStatValue.Poison)
            {
                pPacket.WriteShort((short)Poison_N);
                pPacket.WriteInt(Poison_R);
                pPacket.WriteShort((short)(Poison_T - currentTime));
                endFlag |= (uint)MobStatValue.Poison;
            }
            if (Seal_T > 0 && (pSpecificFlag & (uint)MobStatValue.Seal) == (uint)MobStatValue.Seal)
            {
                pPacket.WriteShort((short)Seal_N);
                pPacket.WriteInt(Seal_R);
                pPacket.WriteShort((short)(Seal_T - currentTime));
                endFlag |= (uint)MobStatValue.Seal;
            }
            if (Darkness_T > 0 && (pSpecificFlag & (uint)MobStatValue.Darkness) == (uint)MobStatValue.Darkness)
            {
                pPacket.WriteShort((short)Darkness_N);
                pPacket.WriteInt(Darkness_R);
                pPacket.WriteShort((short)(Darkness_T - currentTime));
                endFlag |= (uint)MobStatValue.Darkness;
            }
            if (PowerUp_T > 0 && (pSpecificFlag & (uint)MobStatValue.PowerUp) == (uint)MobStatValue.PowerUp)
            {
                pPacket.WriteShort((short)PowerUp_N);
                pPacket.WriteInt(PowerUp_R);
                pPacket.WriteShort((short)(PowerUp_T - currentTime));
                endFlag |= (uint)MobStatValue.PowerUp;
            }
            if (MagicUp_T > 0 && (pSpecificFlag & (uint)MobStatValue.MagicUp) == (uint)MobStatValue.MagicUp)
            {
                pPacket.WriteShort((short)MagicUp_N);
                pPacket.WriteInt(MagicUp_R);
                pPacket.WriteShort((short)(MagicUp_T - currentTime));
                endFlag |= (uint)MobStatValue.MagicUp;
            }
            if (PowerGuardUp_T > 0 && (pSpecificFlag & (uint)MobStatValue.PowerGuardUp) == (uint)MobStatValue.PowerGuardUp)
            {
                pPacket.WriteShort((short)PowerGuardUp_N);
                pPacket.WriteInt(PowerGuardUp_R);
                pPacket.WriteShort((short)(PowerGuardUp_T - currentTime));
                endFlag |= (uint)MobStatValue.PowerGuardUp;
            }
            if (MagicGuardUp_T > 0 && (pSpecificFlag & (uint)MobStatValue.MagicGuardUp) == (uint)MobStatValue.MagicGuardUp)
            {
                pPacket.WriteShort((short)MagicGuardUp_N);
                pPacket.WriteInt(MagicGuardUp_R);
                pPacket.WriteShort((short)(MagicGuardUp_T - currentTime));
                endFlag |= (uint)MobStatValue.MagicGuardUp;
            }
            if (PhysicalImmune_T > 0 && (pSpecificFlag & (uint)MobStatValue.PhysicalImmune) == (uint)MobStatValue.PhysicalImmune)
            {
                pPacket.WriteShort((short)PhysicalImmune_N);
                pPacket.WriteInt(PhysicalImmune_R);
                pPacket.WriteShort((short)(PhysicalImmune_T - currentTime));
                endFlag |= (uint)MobStatValue.PhysicalImmune;
            }
            if (MagicImmune_T > 0 && (pSpecificFlag & (uint)MobStatValue.MagicImmune) == (uint)MobStatValue.MagicImmune)
            {
                pPacket.WriteShort((short)MagicImmune_N);
                pPacket.WriteInt(MagicImmune_R);
                pPacket.WriteShort((short)(MagicImmune_T - currentTime));
                endFlag |= (uint)MobStatValue.MagicImmune;
            }
            if (Doom_T > 0 && (pSpecificFlag & (uint)MobStatValue.Doom) == (uint)MobStatValue.Doom)
            {
                pPacket.WriteShort((short)Doom_N);
                pPacket.WriteInt(Doom_R);
                pPacket.WriteShort((short)(Doom_T - currentTime));
                endFlag |= (uint)MobStatValue.Doom;
            }
            if (Web_T > 0 && (pSpecificFlag & (uint)MobStatValue.Web) == (uint)MobStatValue.Web)
            {
                pPacket.WriteShort((short)Web_N);
                pPacket.WriteInt(Web_R);
                pPacket.WriteShort((short)(Web_T - currentTime));
                endFlag |= (uint)MobStatValue.Web;
            }
            if (HardSkin_T > 0 && (pSpecificFlag & (uint)MobStatValue.HardSkin) == (uint)MobStatValue.HardSkin)
            {
                pPacket.WriteShort((short)HardSkin_N);
                pPacket.WriteInt(HardSkin_R);
                pPacket.WriteShort((short)(HardSkin_T - currentTime));
                endFlag |= (uint)MobStatValue.HardSkin;
            }
            if (Ambush_T > 0 && (pSpecificFlag & (uint)MobStatValue.Ambush) == (uint)MobStatValue.Ambush)
            {
                pPacket.WriteShort((short)Ambush_N);
                pPacket.WriteInt(Ambush_R);
                pPacket.WriteShort((short)(Ambush_T - currentTime));
                endFlag |= (uint)MobStatValue.Ambush;
            }
            if (Venom_T > 0 && (pSpecificFlag & (uint)MobStatValue.Venom) == (uint)MobStatValue.Venom)
            {
                pPacket.WriteShort((short)Venom_N);
                pPacket.WriteInt(Venom_R);
                pPacket.WriteShort((short)(Venom_T - currentTime));
                endFlag |= (uint)MobStatValue.Venom;
            }
            if (Blind_T > 0 && (pSpecificFlag & (uint)MobStatValue.Blind) == (uint)MobStatValue.Blind)
            {
                pPacket.WriteShort((short)Blind_N);
                pPacket.WriteInt(Blind_R);
                pPacket.WriteShort((short)(Blind_T - currentTime));
                endFlag |= (uint)MobStatValue.Blind;
            }
            if (SealSkill_T > 0 && (pSpecificFlag & (uint)MobStatValue.SealSkill) == (uint)MobStatValue.SealSkill)
            {
                pPacket.WriteShort((short)SealSkill_N);
                pPacket.WriteInt(SealSkill_R);
                pPacket.WriteShort((short)(SealSkill_T - currentTime));
                endFlag |= (uint)MobStatValue.SealSkill;
            }

            int tmpBuffPos2 = pPacket.Position;
            pPacket.Position = tmpBuffPos;
            pPacket.WriteUInt(endFlag);
            pPacket.Position = tmpBuffPos2;
        }

        public void Clear()
        {
            PhysicalDamage_N = 0;
            PhysicalDamage_R = 0;
            PhysicalDamage_T = 0;
            PhysicalDefense_N = 0;
            PhysicalDefense_R = 0;
            PhysicalDefense_T = 0;
            MagicDamage_N = 0;
            MagicDamage_R = 0;
            MagicDamage_T = 0;
            MagicDefense_N = 0;
            MagicDefense_R = 0;
            MagicDefense_T = 0;
            Accurrency_N = 0;
            Accurrency_R = 0;
            Accurrency_T = 0;
            Evasion_N = 0;
            Evasion_R = 0;
            Evasion_T = 0;
            Speed_N = 0;
            Speed_R = 0;
            Speed_T = 0;
            Stun_N = 0;
            Stun_R = 0;
            Stun_T = 0;
            Freeze_N = 0;
            Freeze_R = 0;
            Freeze_T = 0;
            Poison_N = 0;
            Poison_R = 0;
            Poison_T = 0;
            Seal_N = 0;
            Seal_R = 0;
            Seal_T = 0;
            Darkness_N = 0;
            Darkness_R = 0;
            Darkness_T = 0;
            PowerUp_N = 0;
            PowerUp_R = 0;
            PowerUp_T = 0;
            MagicUp_N = 0;
            MagicUp_R = 0;
            MagicUp_T = 0;
            PowerGuardUp_N = 0;
            PowerGuardUp_R = 0;
            PowerGuardUp_T = 0;
            MagicGuardUp_N = 0;
            MagicGuardUp_R = 0;
            MagicGuardUp_T = 0;
            Doom_N = 0;
            Doom_R = 0;
            Doom_T = 0;
            Web_N = 0;
            Web_R = 0;
            Web_T = 0;
            PhysicalImmune_N = 0; // TEST MODE
            PhysicalImmune_R = 0;
            PhysicalImmune_T = 0;
            MagicImmune_N = 0;
            MagicImmune_R = 0;
            MagicImmune_T = 0;
            HardSkin_N = 0;
            HardSkin_R = 0;
            HardSkin_T = 0;
            Ambush_N = 0;
            Ambush_R = 0;
            Ambush_T = 0;
            Venom_N = 0;
            Venom_R = 0;
            Venom_T = 0;
            Blind_N = 0;
            Blind_R = 0;
            Blind_T = 0;
            SealSkill_N = 0;
            SealSkill_R = 0;
            SealSkill_T = 0;
        }

        public void Update(DateTime pNow)
        {
            long currentTime = Tools.GetTimeAsMilliseconds(pNow);
            uint endFlag = 0;
            if (PhysicalDamage_N > 0 && currentTime - PhysicalDamage_T > 0)
            {
                PhysicalDamage_N = 0;
                PhysicalDamage_R = 0;
                PhysicalDamage_T = 0;
                endFlag |= (uint)MobStatValue.PhysicalDamage;
            }
            if (PhysicalDefense_N > 0 && currentTime - PhysicalDefense_T > 0)
            {
                PhysicalDefense_N = 0;
                PhysicalDefense_R = 0;
                PhysicalDefense_T = 0;
                endFlag |= (uint)MobStatValue.PhysicalDefense;
            }
            if (MagicDamage_N > 0 && currentTime - MagicDamage_T > 0)
            {
                MagicDamage_N = 0;
                MagicDamage_R = 0;
                MagicDamage_T = 0;
                endFlag |= (uint)MobStatValue.MagicDamage;
            }
            if (MagicDefense_N > 0 && currentTime - MagicDefense_T > 0)
            {
                MagicDefense_N = 0;
                MagicDefense_R = 0;
                MagicDefense_T = 0;
                endFlag |= (uint)MobStatValue.MagicDefense;
            }
            if (Accurrency_N > 0 && currentTime - Accurrency_T > 0)
            {
                Accurrency_N = 0;
                Accurrency_R = 0;
                Accurrency_T = 0;
                endFlag |= (uint)MobStatValue.Accurrency;
            }
            if (Evasion_N > 0 && currentTime - Evasion_T > 0)
            {
                Evasion_N = 0;
                Evasion_R = 0;
                Evasion_T = 0;
                endFlag |= (uint)MobStatValue.Evasion;
            }
            if (Speed_N > 0 && currentTime - Speed_T > 0)
            {
                Speed_N = 0;
                Speed_R = 0;
                Speed_T = 0;
                endFlag |= (uint)MobStatValue.Speed;
            }
            if (Stun_N > 0 && currentTime - Stun_T > 0)
            {
                Stun_N = 0;
                Stun_R = 0;
                Stun_T = 0;
                endFlag |= (uint)MobStatValue.Stun;
            }
            if (Freeze_N > 0 && currentTime - Freeze_T > 0)
            {
                Freeze_N = 0;
                Freeze_R = 0;
                Freeze_T = 0;
                endFlag |= (uint)MobStatValue.Freeze;
            }
            if (Poison_N > 0 && currentTime - Poison_T > 0)
            {
                Poison_N = 0;
                Poison_R = 0;
                Poison_T = 0;
                endFlag |= (uint)MobStatValue.Poison;
            }
            if (Seal_N > 0 && currentTime - Seal_T > 0)
            {
                Seal_N = 0;
                Seal_R = 0;
                Seal_T = 0;
                endFlag |= (uint)MobStatValue.Seal;
            }
            if (Darkness_N > 0 && currentTime - Darkness_T > 0)
            {
                Darkness_N = 0;
                Darkness_R = 0;
                Darkness_T = 0;
                endFlag |= (uint)MobStatValue.Darkness;
            }
            if (PowerUp_N > 0 && currentTime - PowerUp_T > 0)
            {
                PowerUp_N = 0;
                PowerUp_R = 0;
                PowerUp_T = 0;
                endFlag |= (uint)MobStatValue.PowerUp;
            }
            if (MagicUp_N > 0 && currentTime - MagicUp_T > 0)
            {
                MagicUp_N = 0;
                MagicUp_R = 0;
                MagicUp_T = 0;
                endFlag |= (uint)MobStatValue.MagicUp;
            }
            if (PowerGuardUp_N > 0 && currentTime - PowerGuardUp_T > 0)
            {
                PowerGuardUp_N = 0;
                PowerGuardUp_R = 0;
                PowerGuardUp_T = 0;
                endFlag |= (uint)MobStatValue.PowerGuardUp;
            }
            if (MagicGuardUp_N > 0 && currentTime - MagicGuardUp_T > 0)
            {
                MagicGuardUp_N = 0;
                MagicGuardUp_R = 0;
                MagicGuardUp_T = 0;
                endFlag |= (uint)MobStatValue.MagicGuardUp;
            }
            if (Doom_N > 0 && currentTime - Doom_T > 0)
            {
                Doom_N = 0;
                Doom_R = 0;
                Doom_T = 0;
                endFlag |= (uint)MobStatValue.Doom;
            }
            if (Web_N > 0 && currentTime - Web_T > 0)
            {
                Web_N = 0;
                Web_R = 0;
                Web_T = 0;
                endFlag |= (uint)MobStatValue.Web;
            }
            if (PhysicalImmune_N > 0 && currentTime - PhysicalImmune_T > 0)
            {
                PhysicalImmune_N = 0;
                PhysicalImmune_R = 0;
                PhysicalImmune_T = 0;
                endFlag |= (uint)MobStatValue.PhysicalImmune;
            }
            if (MagicImmune_N > 0 && currentTime - MagicImmune_T > 0)
            {
                MagicImmune_N = 0;
                MagicImmune_R = 0;
                MagicImmune_T = 0;
                endFlag |= (uint)MobStatValue.MagicImmune;
            }
            if (HardSkin_N > 0 && currentTime - HardSkin_T > 0)
            {
                HardSkin_N = 0;
                HardSkin_R = 0;
                HardSkin_T = 0;
                endFlag |= (uint)MobStatValue.HardSkin;
            }
            if (Ambush_N > 0 && currentTime - Ambush_T > 0)
            {
                Ambush_N = 0;
                Ambush_R = 0;
                Ambush_T = 0;
                endFlag |= (uint)MobStatValue.Ambush;
            }
            if (Venom_N > 0 && currentTime - Venom_T > 0)
            {
                Venom_N = 0;
                Venom_R = 0;
                Venom_T = 0;
                endFlag |= (uint)MobStatValue.Venom;
            }
            if (Blind_N > 0 && currentTime - Blind_T > 0)
            {
                Blind_N = 0;
                Blind_R = 0;
                Blind_T = 0;
                endFlag |= (uint)MobStatValue.Blind;
            }
            if (SealSkill_N > 0 && currentTime - SealSkill_T > 0)
            {
                SealSkill_N = 0;
                SealSkill_R = 0;
                SealSkill_T = 0;
                endFlag |= (uint)MobStatValue.SealSkill;
            }

            if (endFlag > 0)
            {
                MobPacket.SendMobStatsTempReset(Mob, endFlag);
            }
        }
    }
}