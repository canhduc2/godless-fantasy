using System;
using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.World
{
    [Serializable]
    public class BlockEntry
    {
        public int x;
        public int y;
        public int id;
    }

    [Serializable]
    public class DimensionsData
    {
        public int minX = -135;
        public int maxX = 135;
        public int minY = -66;
        public int maxY = 66;
        public int width = 270;
        public int height = 132;
    }

    [Serializable]
    public class Vec2Data
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class SafeZoneData
    {
        public float centerX;
        public float centerY;
        public float radius = 14.5f;
    }

    [Serializable]
    public class BoundaryLinkData
    {
        public string targetRegionId = "";
        public string targetDisplayName = "";
        public bool enabled = false;
        public float entryOffsetX = 0f;
        public float entryOffsetY = 0f;
    }

    [Serializable]
    public class BoundaryConnectionsData
    {
        public BoundaryLinkData north = new BoundaryLinkData();
        public BoundaryLinkData south = new BoundaryLinkData();
        public BoundaryLinkData east = new BoundaryLinkData();
        public BoundaryLinkData west = new BoundaryLinkData();
    }

    [Serializable]
    public class PortalData
    {
        public string portalId = "";
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public string targetRegionId = "";
        public float targetX = 0f;
        public float targetY = 0f;
        public bool isDungeon = false;
    }

    [Serializable]
    public class MonsterCampData
    {
        public string campId = "";
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float radius = 8f;
        public string monsterType = "monster_slime_grass";
        public string displayName = "Quái Vật";
        public int level = 1;
        public bool isBoss = false;
        public int count = 5;
        public float respawnSeconds = 20f;
    }

    [Serializable]
    public class PropSpawnData
    {
        public string name = "";
        public string type = "";
        public float x = 0f;
        public float y = 0f;
        public int sort = 10;
    }

    [Serializable]
    public class RegionData
    {
        public string regionId = "region_village_0_0";
        public string displayName = "Làng Tân Thủ & Đại Lộ Cổ";
        public bool isDungeon = false;
        public DimensionsData dimensions = new DimensionsData();
        public Vec2Data defaultSpawn = new Vec2Data();
        public SafeZoneData safeZone = null;
        public BoundaryConnectionsData connections = new BoundaryConnectionsData();
        public List<PortalData> portals = new List<PortalData>();
        public List<MonsterCampData> monsterCamps = new List<MonsterCampData>();
        public List<PropSpawnData> props = new List<PropSpawnData>();
        public List<BlockEntry> blockList = new List<BlockEntry>();
    }
}
