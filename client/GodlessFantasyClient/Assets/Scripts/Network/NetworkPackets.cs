using System;
using System.Collections.Generic;

namespace GodlessFantasy.Network
{
    public static class OpCodes
    {
        // Auth
        public const string AuthRegisterReq = "AUTH_REGISTER_REQ";
        public const string AuthRegisterResp = "AUTH_REGISTER_RESP";
        public const string AuthLoginReq = "AUTH_LOGIN_REQ";
        public const string AuthLoginResp = "AUTH_LOGIN_RESP";

        // Universe Selection
        public const string UniverseListReq = "UNIVERSE_LIST_REQ";
        public const string UniverseListResp = "UNIVERSE_LIST_RESP";
        public const string UniverseSelectReq = "UNIVERSE_SELECT_REQ";
        public const string UniverseSelectResp = "UNIVERSE_SELECT_RESP";

        // Character Creation & Loading
        public const string CharGetReq = "CHAR_GET_REQ";
        public const string CharGetResp = "CHAR_GET_RESP";
        public const string CharRollStatsReq = "CHAR_ROLL_STATS_REQ";
        public const string CharRollStatsResp = "CHAR_ROLL_STATS_RESP";
        public const string CharCreateReq = "CHAR_CREATE_REQ";
        public const string CharCreateResp = "CHAR_CREATE_RESP";

        // World Gameplay & Multiplayer
        public const string WorldEnterReq = "WORLD_ENTER_REQ";
        public const string WorldEnterResp = "WORLD_ENTER_RESP";
        public const string PlayerSpawnBroadcast = "PLAYER_SPAWN";
        public const string PlayerDespawnBroadcast = "PLAYER_DESPAWN";
        public const string PlayerMoveReq = "PLAYER_MOVE_REQ";
        public const string PlayerMoveBroadcast = "PLAYER_MOVE_BROADCAST";
        public const string ChatSendReq = "CHAT_SEND_REQ";
        public const string ChatBroadcast = "CHAT_BROADCAST";

        // Warnings
        public const string WarningNotice = "WARNING_NOTICE";
    }

    [Serializable]
    public class PacketEnvelope
    {
        public string op;
        public string data;
    }

    // ================= DTOs =================

    [Serializable]
    public class AuthRegisterReqDto
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class AuthRegisterRespDto
    {
        public bool success;
        public string message;
    }

    [Serializable]
    public class AuthLoginReqDto
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class AuthLoginRespDto
    {
        public bool success;
        public string message;
        public int accountId;
        public string username;
    }

    [Serializable]
    public class UniverseInfoDto
    {
        public int id;
        public string name;
        public string description;
        public int onlinePlayers;
        public int maxPlayers;
        public int totalCharacters;
        public bool hasCharacter;
    }

    [Serializable]
    public class UniverseListRespDto
    {
        public List<UniverseInfoDto> universes;
    }

    [Serializable]
    public class UniverseSelectReqDto
    {
        public int universeId;
    }

    [Serializable]
    public class UniverseSelectRespDto
    {
        public bool success;
        public string message;
        public int universeId;
        public string universeName;
        public bool hasCharacter;
        public CharacterInfoDto character;
    }

    [Serializable]
    public class CharacterInfoDto
    {
        public int id;
        public string name;
        public string gender;
        public string className;
        public string element;
        public int str;
        public int intStat;
        public int vit;
        public int agi;
        public int level;
        public int hp;
        public int maxHp;
        public int mana;
        public int maxMana;
        public int exp;
        public int maxExp;
        public float posX;
        public float posY;
    }

    [Serializable]
    public class CharRollStatsReqDto
    {
        public string className;
        public bool isReroll;
    }

    [Serializable]
    public class CharRollStatsRespDto
    {
        public string element;
        public int str;
        public int intStat;
        public int vit;
        public int agi;
        public int totalPoints;
        public int rerollsLeft;
        public bool isExistingRoll;
        public string message;
    }

    [Serializable]
    public class CharCreateReqDto
    {
        public string name;
        public string gender;
        public string className;
        public string element;
        public int str;
        public int intStat;
        public int vit;
        public int agi;
    }

    [Serializable]
    public class CharCreateRespDto
    {
        public bool success;
        public string message;
        public CharacterInfoDto character;
    }

    [Serializable]
    public class WorldEnterReqDto
    {
    }

    [Serializable]
    public class WorldEnterRespDto
    {
        public bool success;
        public string message;
        public CharacterInfoDto self;
        public List<CharacterInfoDto> otherPlayers;
    }

    [Serializable]
    public class PlayerMoveReqDto
    {
        public float posX;
        public float posY;
        public float targetX;
        public float targetY;
        public bool isMoving;
        public string direction;
    }

    [Serializable]
    public class PlayerMoveBroadcastDto
    {
        public int playerId;
        public float posX;
        public float posY;
        public float targetX;
        public float targetY;
        public bool isMoving;
        public string direction;
    }

    [Serializable]
    public class PlayerDespawnBroadcastDto
    {
        public int playerId;
    }

    [Serializable]
    public class ChatSendReqDto
    {
        public string channel;
        public string message;
    }

    [Serializable]
    public class ChatBroadcastDto
    {
        public int senderId;
        public string senderName;
        public string channel;
        public string message;
        public string element;
    }

    [Serializable]
    public class WarningNoticeDto
    {
        public string message;
        public int cooldownSeconds;
    }
}
