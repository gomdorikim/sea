using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using WvsBeta.Common;
using WvsBeta.Common.Sessions;

namespace WvsBeta.Game
{
    public class CharacterPets
    {
        public Character mCharacter { get; set; }
        public List<Item> mPets { get; set; }
        public short mSpawned { get; set; }

        public CharacterPets(Character character)
        {
            mCharacter = character;
            mPets = new List<Item>();
            mSpawned = 0;
        }

        public void Save()
        {
            foreach (Item pet in mPets)
            {
                Server.Instance.CharacterDatabase.RunQuery("DELETE FROM pets WHERE id = " + pet.CashId.ToString());
                Server.Instance.CharacterDatabase.RunQuery("INSERT INTO pets VALUES (" + pet.CashId.ToString() + ", " + (pet.Pet.Spawned ? "1" : "-1") + ", '" + MySqlHelper.EscapeString(pet.Pet.Name) + "', " + pet.Pet.Level.ToString() + ", " + pet.Pet.Closeness.ToString() + ", " + pet.Pet.Fullness.ToString() + ", " + pet.Pet.Expiration.ToString() + ");");
            }
        }

        public Pet LoadPet(Item item)
        {
            Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM pets WHERE id = " + item.CashId.ToString());

            MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
            if (!data.HasRows)
            {
                return null;
            }
            else
            {
                data.Read(); // Only one pet lol
                Pet pet = new Pet(item);
                pet.Name = data.GetString("name");
                pet.Level = data.GetByte("level");
                pet.Closeness = data.GetInt16("closeness");
                pet.Fullness = data.GetByte("fullness");
                pet.Expiration = data.GetInt64("expiration");
                if (data.GetInt16("index") == 1)
                {
                    mSpawned = item.InventorySlot;
                    pet.Spawned = true;
                }
                else
                {
                    pet.Spawned = false;
                }
                mPets.Add(item);
                return pet;
            }
        }

        public void SpawnPet(Character victim = null)
        {
            if (mSpawned != 0 && mCharacter.Inventory.GetItem(5, mSpawned) != null)
            {
                PetsPacket.SendSpawnPet(mCharacter, mCharacter.Inventory.GetItem(5, mSpawned).Pet, victim);
            }
        }

        public void ChangePetname(string name)
        {
            if (mSpawned != 0 && mCharacter.Inventory.GetItem(5, mSpawned) != null)
            {
                mCharacter.Inventory.GetItem(5, mSpawned).Pet.Name = name;
                PetsPacket.SendPetNamechange(mCharacter, name);
            }
        }

        public void AddCloseness(short amount)
        {
            if (mSpawned != 0 && mCharacter.Inventory.GetItem(5, mSpawned) != null)
            {
                Pet pet = mCharacter.Inventory.GetItem(5, mSpawned).Pet;
                if (pet.Closeness + amount > Constants.MaxCloseness)
                    pet.Closeness = Constants.MaxCloseness;
                else
                    pet.Closeness += amount;
                while (pet.Closeness >= Constants.PetExp[pet.Level - 1] && pet.Level < Constants.PetLevels)
                {
                    pet.Level++;
                    PetsPacket.SendPetLevelup(mCharacter);
                }
            }
        }

        public Pet GetEquippedPet()
        {
            if (mSpawned != 0 && mCharacter.Inventory.GetItem(5, mSpawned) != null)
            {
                return mCharacter.Inventory.GetItem(5, mSpawned).Pet;
            }
            return null;
        }
    }
}