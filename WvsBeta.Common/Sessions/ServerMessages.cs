using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common.Sessions {
	public enum ServerMessages : byte {
		CLIENT_CONNECT_TO_SERVER_LOGIN = 0x05,
		LOGIN_CHARACTER_REMOVE_RESULT = 0x08,
		CLIENT_CONNECT_TO_SERVER = 0x09,

		INVENTORY_CHANGE_SLOT = 0x12,
		INVENTORY_CHANGE_INVENTORY_SLOTS = 0x13,
		
		STATS_CHANGE = 0x14,

		SKILLS_GIVE_BUFF = 0x15,
		SKILLS_GIVE_DEBUFF = 0x16,
		SKILLS_ADD_POINT = 0x17,

		Fame = 0x19,

		Notice = 0x1A,
		TeleportRock = 0x1C,

		PlayerInformation = 0x1F,
		Message = 0x23,
		EnterMap = 0x26,

		IncorrectChannelNumber = 0x2B, //0x2B
        CashshopUnavailable = 0x64,
		SlashCmdAnswer = 0x2E,
        Party_Operation = 0x20,
        Buddy_Operation = 0x21,

		RemotePlayerSpawn = 0x3C,
		RemotePlayerDespawn = 0x3D,
		RemotePlayerChat = 0x3F,

		SummonDespawn = 0x4B,
		SummonMove = 0x4B,
		SummonAttack = 0x4D,
		SummonDamage = 0x4E,

		RemotePlayerMove = 0x52,
		RemotePlayerMeleeAttack = 0x53,
		RemotePlayerRangedAttack = 0x54,
		RemotePlayerMagicAttack = 0x55,
		RemotePlayerGetDamage = 0x58,
		RemotePlayerEmote = 0x59,
		RemotePlayerChangeEquips = 0x5A,
		RemotePlayerAnimation = 0x5B,
		RemotePlayerSkillBuff = 0x5C,
		RemotePlayerSkillDebuff = 0x5D,

		RemotePlayerSitOnChair = 0x61,
		RemotePlayerThirdPartyAnimation = 0x62,

		MesoSackResult = 0x65,

		MobSpawn = 0x6A,
		MobRespawn = 0x6B,
		MobControlRequest = 0x6C,
		MobMovement = 0x6E,
		MobControlResponse = 0x6F,
		MobChangeHealth = 0x75,

		NpcSpawn = 0x7B,
		NpcControlRequest = 0x7D,

		NpcAnimate = 0x7F,

		DropSpawn = 0x83,
		DropModify = 0x84,
        Reactor_Hit = 0x94,
        Reactor_Spawn = 0x96,
        Reactor_Destroy = 0x97,

		SnowBall_State = 0x9A,
		SnowBall_Hit = 0x9B,

		Coconut_Hit = 0x9C,
		Coconut_Score = 0x9D,

		NpcScriptChat = 0xA0,

		NpcShopShow = 0xA3,
		NpcShopResult = 0xA4,

		StorageShow = 0xA7,
		StorageResult = 0xA8
	}
}
