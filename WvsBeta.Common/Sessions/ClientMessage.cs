using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common.Sessions
{
    public enum ClientMessages : byte
    {
        LOGIN_CHECK_PASSWORD = 0x01,
        LOGIN_SELECT_CHANNEL = 0x04,
        LOGIN_SELECT_WORLD = 0x03,
        LOGIN_SELECT_CHARACTER = 0x06,

        LOGIN_WORLD_SELECT = 0x05,

        LOGIN_CHECK_NAME = 0x08,
        LOGIN_CREATE_CHARACTER = 0x09,
        LOGIN_REMOVE_CHARACTER = 0x0A,


        CLIENT_PONG = 0x0B,
        //CLIENT_CRASH_REPORT = 0x0A,
        CLIENT_HASH = 0x0E,

        Client_Hash2 = 0x10,

        FIELD_ENTER_PORTAL = 0x11,
        FIELD_CHANGE_CHANNEL = 0x12,
        FIELD_CONNECT_CASHSHOP = 0x13,
        FIELD_PLAYER_MOVEMENT = 0x14,
        FIELD_PLAYER_SIT_MAPCHAIR = 0x15,

        ATTACK_MELEE = 0x16,
        ATTACK_RANGED = 0x17,
        ATTACK_MAGIC = 0x18,

        PLAYER_RECEIVE_DAMAGE = 0x1A,
        PLAYER_CHAT = 0x2D,
        PLAYER_EMOTE = 0x1C,

        NPC_SELECT = 0x1F,
        NPC_CHAT = 0x20,
        NPC_OPEN_SHOP = 0x21,
        NPC_STORAGE = 0x22,

        INVENTORY_CHANGE_SLOT = 0x23,
        INVENTORY_USE_ITEM = 0x24,
        INVENTORY_USE_SUMMON_SACK = 0x25,
        INVENTORY_USE_CASH_ITEM = 0x27,
        INVENTORY_USE_RETURN_SCROLL = 0x28,
        INVENTORY_USE_SCROLL_ON_ITEM = 0x29,

        STATS_CHANGE = 0x2A,
        STATS_HEAL = 0x2B,

        SKILL_ADD_LEVEL = 0x2C,
        SKILL_USE = 0x2D,
        SKILL_STOP = 0x2E,

        INVENTORY_DROP_MESOS = 0x30,

        REMOTE_MODIFY_FAME = 0x31,
        REMOTE_REQUEST_INFO = 0x32,

        TELEPORTROCK_USE = 0x37,

        COMMAND_WHISPER_FIND = 0x3C,

        SUMMON_MOVE = 0x4E,
        SUMMON_ATTACK = 0x4F,
        SUMMON_DAMAGE = 0x50,

        MOB_CONTROL = 0x56,
        MOB_DISTANCE_FROM_PLAYER = 0x57,
        MOB_PICKUP_DROP = 0x58,

        NPC_ANIMATE = 0x5B,

        DROP_PICKUP = 0x5F

    }
}