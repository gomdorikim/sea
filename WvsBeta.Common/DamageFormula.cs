using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common
{
    public class DamageFormula
    {
        public double GeneralDamage(double Primary, double Secondary, double TotalWat, byte Type, int Mastery, double Secondary2, double TotalWat2, int CharLevel, int MobDefense, int MobLevel)
        {
            //todo!
            return ((BaseMinDamage(Primary, Secondary, TotalWat, 1, Mastery, Secondary2, TotalWat2) * ModifierA(CharLevel, MobLevel)) - (MobDefense * .5));
        }

        public double CritDamage(double Primary, double Secondary, double TotalWat, byte Type, int Mastery, double Secondary2, double TotalWat2, int CharLevel, int MobDefense, int MobLevel)
        {
            return (GeneralDamage(Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, CharLevel, MobDefense, MobLevel) * .20) + (GeneralDamage(Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, CharLevel, MobDefense, MobLevel));
        }

        public double GeneralDamageWithSkill(double Luk, double WeaponAttack, double Primary, double Secondary, double TotalWat, byte Type, int Mastery, double Secondary2, double TotalWat2, int skillID, int CharLevel, int MobDefense, int MobLevel, short Job)
        {
            if (Job == 3) //i mean wat
            {
                return GeneralDamage(Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, CharLevel, MobDefense, MobLevel) * 3 * WeaponAttack / 150;
            }
            else
            {
                return GeneralDamage(Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, CharLevel, MobDefense, MobLevel) + Luk * (2.8) * WeaponAttack / 100;
            }
        }

        public double GeneralDamageWithSkillWithCrit(double Luk, double WeaponAttack, double Primary, double Secondary, double TotalWat, byte Type, int Mastery, double Secondary2, double TotalWat2, int SkillID, int CharLevel, int MobDefense, int MobLevel, short Job)
        {
            return (GeneralDamageWithSkill(Luk, WeaponAttack, Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, SkillID, CharLevel, MobDefense, MobLevel, Job) * .20) + GeneralDamageWithSkill(Luk, WeaponAttack, Primary, Secondary, TotalWat, Type, Mastery, Secondary2, TotalWat2, SkillID, CharLevel, MobDefense, MobLevel, Job);
        }

        public double PrimarySecondary(double Primary, double Secondary, bool isStar, bool isBow, bool isCrossbow)
        {
            if (isStar) return ((Primary * 3.6) + Secondary);
            if (isBow) return ((Primary * 3.4) + Secondary);
            if (isCrossbow) return ((Primary * 3.6) + Secondary);
            return 0.0;
        }

        public double Primary(double Primary, byte Type)
        {
            switch (Type)
            {
                case 0: return (Primary * 3.6); //star
                case 1: return (Primary * 3.4); //bow
                case 2: return (Primary * 3.6); //crossbow
            }
            return 0.0;
        }

        public double Secondary(double pSecondary, byte Type, double Secondary2 = 0)
        {
            switch (Type)
            {
                case 0: return (pSecondary + Secondary2);
                case 1: return (pSecondary);
                case 2: return (pSecondary);
            }
            return 0.0;
        }

        public double BaseMaxDamage(double Prime, double Sec, double WeapAttack, bool isStar, bool isBow, bool isCrossbow)
        {
            return (PrimarySecondary(Prime, Sec, isStar, isBow, isCrossbow) * WeapAttack / 100);
        }

        public double BaseMinDamage(double Prime, double Sec, double WeapAttack, byte Type, int Mastery, double Secondary2, double WeaponAttack)
        {
            return (Primary(Prime, Type) + Mastery * 0.9 + Secondary(Sec, Type, Secondary2)) * WeaponAttack / 100;
        }

        public double ModifierA(int CharLevel, int MobLevel)
        {
            return LevelDisadvantage(CharLevel, MobLevel);
        }

        public double LevelDisadvantage(int CharLevel, int MobLevel)
        {
            if (MobLevel > CharLevel)
            {
                return 1 - 0.01 * (MobLevel - CharLevel);
            }
            else
            {
                return 1 - 0.01 * (0);
            }
        }

        public double ElementModifier()
        {
            //leave this for now... doesn't make a huge difference
            return 1.0;
        }
    }
}
