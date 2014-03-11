using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace WvsBeta.Shop {
	public class CharacterPets {
		public Character mCharacter { get; set; }
		public List<Item> mPets { get; set; }
		public short mSpawned { get; set; }

		public CharacterPets(Character character) {
			mCharacter = character;
			mPets = new List<Item>();
			mSpawned = 0;
		}

		public void Save() {
			foreach (Item pet in mPets) {
				Server.Instance.CharacterDatabase.RunQuery("DELETE FROM pets WHERE id = " + pet.CashId.ToString());
				Server.Instance.CharacterDatabase.RunQuery("INSERT INTO pets VALUES (" + pet.CashId.ToString() + ", " + ( pet.Pet.Spawned ? "1" : "-1") + ", '" + MySqlHelper.EscapeString(pet.Pet.Name) + "', " + pet.Pet.Level.ToString() + ", " + pet.Pet.Closeness.ToString() + ", " + pet.Pet.Fullness.ToString() + ", " + pet.Pet.Expiration.ToString() + ");");
			}
		}

		public Pet LoadPet(Item item) {
			Server.Instance.CharacterDatabase.RunQuery("SELECT * FROM pets WHERE id = " + item.CashId.ToString());

			MySqlDataReader data = Server.Instance.CharacterDatabase.Reader;
			if (!data.HasRows) {
				return null;
			}
			else {
				data.Read();
				Pet pet = new Pet();
				pet.Name = data.GetString("name");
				pet.Level = data.GetByte("level");
				pet.Closeness = data.GetInt16("closeness");
				pet.Fullness = data.GetByte("fullness");
				pet.Expiration = data.GetInt64("expiration");
				pet.Item = item;
				item.Pet = pet;
				if (data.GetInt16("index") == 1) {
					mSpawned = item.InventorySlot;
					pet.Spawned = true;
				}
				else {
					pet.Spawned = false;
				}
				mPets.Add(item);
				return pet;
			}
		}
		
		public Pet GetEquippedPet() {
			if (mSpawned != 0 && mCharacter.mInventory.GetItem(5, mSpawned) != null) {
				return mCharacter.mInventory.GetItem(5, mSpawned).Pet;
			}
			return null;
		}

		public long GetEquippedPetCashid() {
			Pet pet = GetEquippedPet();
			if (pet == null) return 0;
			else return pet.Item.CashId;
		}
	}
}
