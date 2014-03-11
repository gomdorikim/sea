using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public enum BuffValueTypes : uint
    {
        // Byte 1
        WeaponAttack = 0x00000001,
        WeaponDefense = 0x00000002,
        MagicAttack = 0x00000004,
        MagicDefense = 0x00000008,

        Accurancy = 0x00000010,
        Avoidability = 0x00000020,
        Hands = 0x00000040, // Yes, this has a modifier too.
        Speed = 0x00000080,

        // Byte 2
        Jump = 0x00000100,
        MagicGuard = 0x00000200,
        DarkSight = 0x00000400,
        Booster = 0x00000800,

        PowerGuard = 0x00001000,
        MaxHP = 0x00002000,
        MaxMP = 0x00004000,
        Invincible = 0x00008000,

        // Byte 3
        SoulArrow = 0x00010000,
        Stun = 0x00020000, // Mob Skill: Stun and Dragon Roar
        Poison = 0x00040000, // Mob Skill: Poison
        Seal = 0x00080000, // Mob Skill: Seal

        Darkness = 0x00100000, // Mob Skill: Darkness
        ComboAttack = 0x00200000,
        Charges = 0x00400000,
        DragonBlood = 0x00800000,

        // Byte 4
        HolySymbol = 0x01000000,
        MesoUP = 0x02000000,
        ShadowPartner = 0x04000000,
        PickPocketMesoUP = 0x08000000,

        MesoGuard = 0x10000000,
        Thaw = 0x20000000,
        Weakness = 0x40000000, // Mob Skill: Weakness
        Curse = 0x80000000  // Mob Skill: Curse
    }

    public class BuffDataProvider
    {
        public static Dictionary<int, List<BuffValueTypes>> mSkillBuffValues { get; set; }

        public static void LoadBuffs()
        {
            mSkillBuffValues = new Dictionary<int, List<BuffValueTypes>>();

            AddSkillBuff((int)Constants.Fighter.Skills.AxeBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Fighter.Skills.SwordBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Page.Skills.BwBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Page.Skills.SwordBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.FPMage.Skills.SpellBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.ILMage.Skills.SpellBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Hunter.Skills.BowBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Crossbowman.Skills.CrossbowBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Assassin.Skills.ClawBooster, BuffValueTypes.Booster);
            AddSkillBuff((int)Constants.Bandit.Skills.DaggerBooster, BuffValueTypes.Booster);

            AddSkillBuff((int)Constants.Magician.Skills.MagicGuard, BuffValueTypes.MagicGuard);

            AddSkillBuff((int)Constants.Magician.Skills.MagicArmor, BuffValueTypes.WeaponDefense);
            AddSkillBuff((int)Constants.Swordsman.Skills.IronBody, BuffValueTypes.WeaponDefense);

            //AddSkillBuff((int)Constants.Archer.Skills.Focus, BuffValueTypes.Accurancy);
            AddSkillBuff((int)Constants.Archer.Skills.Focus, BuffValueTypes.Avoidability);



            AddSkillBuff((int)Constants.Fighter.Skills.Rage, BuffValueTypes.WeaponAttack, BuffValueTypes.WeaponDefense);

            AddSkillBuff((int)Constants.Fighter.Skills.PowerGuard, BuffValueTypes.PowerGuard);

            AddSkillBuff((int)Constants.Spearman.Skills.IronWill, BuffValueTypes.WeaponDefense, BuffValueTypes.MagicDefense);

            AddSkillBuff((int)Constants.Spearman.Skills.HyperBody, BuffValueTypes.MaxHP);
            //AddSkillBuff((int)Constants.Spearman.Skills.HyperBody, BuffValueTypes.MaxMP);

            AddSkillBuff((int)Constants.FPWizard.Skills.Meditation, BuffValueTypes.MagicAttack);
            AddSkillBuff((int)Constants.ILWizard.Skills.Meditation, BuffValueTypes.MagicAttack);

            AddSkillBuff((int)Constants.Cleric.Skills.Invincible, BuffValueTypes.Invincible);

            AddSkillBuff((int)Constants.Cleric.Skills.Bless, BuffValueTypes.WeaponAttack, BuffValueTypes.WeaponDefense, BuffValueTypes.MagicAttack, BuffValueTypes.MagicDefense, BuffValueTypes.Accurancy, BuffValueTypes.Avoidability);
            AddSkillBuff((int)Constants.Gm.Skills.Bless, BuffValueTypes.WeaponAttack, BuffValueTypes.WeaponDefense, BuffValueTypes.MagicAttack, BuffValueTypes.MagicDefense, BuffValueTypes.Accurancy, BuffValueTypes.Avoidability);

            AddSkillBuff((int)Constants.ChiefBandit.Skills.MesoGuard, BuffValueTypes.MesoGuard);

            AddSkillBuff((int)Constants.Priest.Skills.HolySymbol, BuffValueTypes.HolySymbol);

            AddSkillBuff((int)Constants.ChiefBandit.Skills.Pickpocket, BuffValueTypes.PickPocketMesoUP);
            AddSkillBuff((int)Constants.Hermit.Skills.MesoUp, BuffValueTypes.PickPocketMesoUP);

            AddSkillBuff((int)Constants.DragonKnight.Skills.DragonRoar, BuffValueTypes.Stun);

            AddSkillBuff((int)Constants.WhiteKnight.Skills.BwFireCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);
            AddSkillBuff((int)Constants.WhiteKnight.Skills.BwIceCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);
            AddSkillBuff((int)Constants.WhiteKnight.Skills.BwLitCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);
            AddSkillBuff((int)Constants.WhiteKnight.Skills.SwordFireCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);
            AddSkillBuff((int)Constants.WhiteKnight.Skills.SwordIceCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);
            AddSkillBuff((int)Constants.WhiteKnight.Skills.SwordLitCharge, BuffValueTypes.MagicAttack, BuffValueTypes.Charges);

            AddSkillBuff((int)Constants.Assassin.Skills.Haste, BuffValueTypes.Speed, BuffValueTypes.Jump);
            AddSkillBuff((int)Constants.Bandit.Skills.Haste, BuffValueTypes.Speed, BuffValueTypes.Jump);
            AddSkillBuff((int)Constants.Gm.Skills.Haste, BuffValueTypes.Speed, BuffValueTypes.Jump);

            AddSkillBuff((int)Constants.Rogue.Skills.DarkSight, BuffValueTypes.Speed, BuffValueTypes.DarkSight);
            AddSkillBuff((int)Constants.Gm.Skills.Hide, BuffValueTypes.Invincible);

            AddSkillBuff((int)Constants.Hunter.Skills.SoulArrow, BuffValueTypes.SoulArrow);
            AddSkillBuff((int)Constants.Crossbowman.Skills.SoulArrow, BuffValueTypes.SoulArrow);

            AddSkillBuff((int)Constants.Hermit.Skills.ShadowPartner, BuffValueTypes.ShadowPartner);

            AddSkillBuff((int)Constants.Crusader.Skills.ComboAttack, BuffValueTypes.ComboAttack);

            AddSkillBuff((int)Constants.DragonKnight.Skills.DragonBlood, BuffValueTypes.WeaponAttack, BuffValueTypes.DragonBlood);


            // Todo: Add mob buffs

        }

        private static void AddSkillBuff(int pSkillID, params BuffValueTypes[] pBuffVals)
        {
            mSkillBuffValues.Add(pSkillID, new List<BuffValueTypes>(pBuffVals));
        }
    }
}
