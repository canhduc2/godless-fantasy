using System;
using System.Collections.Generic;

namespace GodlessFantasyServer.Network
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

        // Server Warnings
        public const string WarningNotice = "WARNING_NOTICE";
    }

    public class PacketEnvelope
    {
        public string op { get; set; } = "";
        public string data { get; set; } = ""; // JSON serialized inner payload
    }

    // ================= DTOs =================

    public class AuthRegisterReqDto
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
    }

    public class AuthRegisterRespDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
    }

    public class AuthLoginReqDto
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
    }

    public class AuthLoginRespDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public int accountId { get; set; }
        public string username { get; set; } = "";
    }

    public class UniverseInfoDto
    {
        public int id { get; set; }
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public int onlinePlayers { get; set; }
        public int maxPlayers { get; set; }
        public int totalCharacters { get; set; }
        public bool hasCharacter { get; set; }
    }

    public class UniverseListRespDto
    {
        public List<UniverseInfoDto> universes { get; set; } = new();
    }

    public class UniverseSelectReqDto
    {
        public int universeId { get; set; }
    }

    public class UniverseSelectRespDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public int universeId { get; set; }
        public string universeName { get; set; } = "";
        public bool hasCharacter { get; set; }
        public CharacterInfoDto? character { get; set; }
    }

    public class CharacterInfoDto
    {
        public int id { get; set; }
        public string name { get; set; } = "";
        public string gender { get; set; } = "Nam";
        public string className { get; set; } = "Kị Sĩ";
        public string element { get; set; } = "Hỏa";
        public int str { get; set; }
        public int intStat { get; set; }
        public int vit { get; set; }
        public int agi { get; set; }
        public int level { get; set; }
        public int hp { get; set; }
        public int maxHp { get; set; }
        public int mana { get; set; }
        public int maxMana { get; set; }
        public int exp { get; set; }
        public int maxExp { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
    }

    public class CharRollStatsReqDto
    {
        public string className { get; set; } = "Kị Sĩ";
    }

    public class CharRollStatsRespDto
    {
        public string element { get; set; } = "Hỏa";
        public int str { get; set; }
        public int intStat { get; set; }
        public int vit { get; set; }
        public int agi { get; set; }
        public int totalPoints { get; set; }
    }

    public class CharCreateReqDto
    {
        public string name { get; set; } = "";
        public string gender { get; set; } = "Nam";
        public string className { get; set; } = "Kị Sĩ";
        public string element { get; set; } = "Hỏa";
        public int str { get; set; }
        public int intStat { get; set; }
        public int vit { get; set; }
        public int agi { get; set; }
    }

    public class CharCreateRespDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public CharacterInfoDto? character { get; set; }
    }

    public class WorldEnterReqDto
    {
    }

    public class WorldEnterRespDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public CharacterInfoDto? self { get; set; }
        public List<CharacterInfoDto> otherPlayers { get; set; } = new();
    }

    public class PlayerMoveReqDto
    {
        public float posX { get; set; }
        public float posY { get; set; }
        public float targetX { get; set; }
        public float targetY { get; set; }
        public bool isMoving { get; set; }
        public string direction { get; set; } = "South";
    }

    public class PlayerMoveBroadcastDto
    {
        public int playerId { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float targetX { get; set; }
        public float targetY { get; set; }
        public bool isMoving { get; set; }
        public string direction { get; set; } = "South";
    }

    public class ChatSendReqDto
    {
        public string channel { get; set; } = "Local"; // Local, World
        public string message { get; set; } = "";
    }

    public class ChatBroadcastDto
    {
        public int senderId { get; set; }
        public string senderName { get; set; } = "";
        public string channel { get; set; } = "Local";
        public string message { get; set; } = "";
        public string element { get; set; } = "Hỏa";
    }

    public class WarningNoticeDto
    {
        public string message { get; set; } = "";
        public int cooldownSeconds { get; set; }
    }
}
