using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace GodlessFantasyServer.Data
{
    public class UniverseData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int MaxPlayers { get; set; }
        public int CurrentOnline { get; set; }
    }

    public class CharacterData
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int UniverseId { get; set; }
        public string Name { get; set; } = "";
        public string Gender { get; set; } = "Nam";
        public string ClassName { get; set; } = "Kị Sĩ";
        public string Element { get; set; } = "Hỏa";
        public int Strength { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Vitality { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int Level { get; set; } = 1;
        public int CurrentHp { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public int CurrentMana { get; set; } = 80;
        public int MaxMana { get; set; } = 80;
        public int CurrentExp { get; set; } = 0;
        public int MaxExp { get; set; } = 100;
        public float PosX { get; set; } = 0f;
        public float PosY { get; set; } = 0f;
        public string CreatedAt { get; set; } = "";
    }

    public class PendingCharacterRoll
    {
        public int AccountId { get; set; }
        public int UniverseId { get; set; }
        public string ClassName { get; set; } = "Kị Sĩ";
        public string Element { get; set; } = "Hỏa";
        public int Strength { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Vitality { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int RerollsLeft { get; set; } = 1;
    }

    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        public static DatabaseManager Instance => _instance ??= new DatabaseManager();

        private readonly string _connectionString;

        public DatabaseManager(string dbPath = "godless_fantasy.db")
        {
            _connectionString = $"Data Source={dbPath};Cache=Shared";
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Bật chế độ WAL (Write-Ahead Logging) chuẩn công nghiệp:
            // Cho phép nhiều tiến trình đọc cùng lúc mà không bị khóa khi ghi, triệt tiêu lỗi Database Locked!
            using (var walCmd = connection.CreateCommand())
            {
                walCmd.CommandText = @"
                    PRAGMA journal_mode = WAL;
                    PRAGMA synchronous = NORMAL;
                    PRAGMA temp_store = MEMORY;
                    PRAGMA cache_size = -64000;
                ";
                await walCmd.ExecuteNonQueryAsync();
            }

            // Tạo các bảng dữ liệu với ràng buộc chuẩn
            using (var tableCmd = connection.CreateCommand())
            {
                tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS accounts (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                        password_hash TEXT NOT NULL,
                        salt TEXT NOT NULL,
                        created_at TEXT NOT NULL,
                        last_login TEXT
                    );

                    CREATE TABLE IF NOT EXISTS universes (
                        id INTEGER PRIMARY KEY,
                        name TEXT NOT NULL,
                        description TEXT NOT NULL,
                        max_players INTEGER NOT NULL DEFAULT 500
                    );

                    CREATE TABLE IF NOT EXISTS characters (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        account_id INTEGER NOT NULL,
                        universe_id INTEGER NOT NULL,
                        name TEXT NOT NULL COLLATE NOCASE,
                        gender TEXT NOT NULL,
                        class_name TEXT NOT NULL,
                        element TEXT NOT NULL,
                        strength INTEGER NOT NULL,
                        intelligence INTEGER NOT NULL,
                        vitality INTEGER NOT NULL,
                        agility INTEGER NOT NULL,
                        level INTEGER NOT NULL DEFAULT 1,
                        current_hp INTEGER NOT NULL DEFAULT 100,
                        max_hp INTEGER NOT NULL DEFAULT 100,
                        current_mana INTEGER NOT NULL DEFAULT 80,
                        max_mana INTEGER NOT NULL DEFAULT 80,
                        current_exp INTEGER NOT NULL DEFAULT 0,
                        max_exp INTEGER NOT NULL DEFAULT 100,
                        pos_x REAL NOT NULL DEFAULT 0.0,
                        pos_y REAL NOT NULL DEFAULT 0.0,
                        created_at TEXT NOT NULL,
                        FOREIGN KEY(account_id) REFERENCES accounts(id),
                        FOREIGN KEY(universe_id) REFERENCES universes(id),
                        UNIQUE(universe_id, name)
                    );

                    CREATE INDEX IF NOT EXISTS idx_char_acc_uni ON characters(account_id, universe_id);

                    CREATE TABLE IF NOT EXISTS pending_character_rolls (
                        account_id INTEGER NOT NULL,
                        universe_id INTEGER NOT NULL,
                        class_name TEXT NOT NULL,
                        element TEXT NOT NULL,
                        strength INTEGER NOT NULL,
                        intelligence INTEGER NOT NULL,
                        vitality INTEGER NOT NULL,
                        agility INTEGER NOT NULL,
                        rerolls_left INTEGER NOT NULL DEFAULT 1,
                        PRIMARY KEY (account_id, universe_id)
                    );
                ";
                await tableCmd.ExecuteNonQueryAsync();
            }

            // Thêm các Vũ Trụ mặc định (Universe Shards) nếu chưa có
            await SeedUniversesAsync(connection);
            Console.WriteLine("[DB] SQLite database initialized successfully in WAL mode!");
        }

        private async Task SeedUniversesAsync(SqliteConnection connection)
        {
            var defaultUniverses = new[]
            {
                (1, "Vũ Trụ 1 - Sơ Khai (Alpha Realm)", "Vũ trụ cội nguồn, nơi các anh hùng đầu tiên khai hoang lập nghiệp."),
                (2, "Vũ Trụ 2 - Hỗn Độn (Chaos Realm)", "Vùng đất giao tranh khốc liệt, dã thú hung hãn và cơ duyên bất tận."),
                (3, "Vũ Trụ 3 - Thần Giới (Celestial Realm)", "Vũ trụ mới khai mở với năng lượng ma pháp dồi dào.")
            };

            foreach (var (id, name, desc) in defaultUniverses)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO universes (id, name, description, max_players) 
                    VALUES (@id, @name, @desc, 500);
                ";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@desc", desc);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        #region Account Security & Authentication (100% Parameterized & Salting)

        public static (string hash, string salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);

            using var sha256 = SHA256.Create();
            byte[] saltedPwd = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = sha256.ComputeHash(saltedPwd);
            string hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using var sha256 = SHA256.Create();
            byte[] saltedPwd = Encoding.UTF8.GetBytes(password + storedSalt);
            byte[] hashBytes = sha256.ComputeHash(saltedPwd);
            string computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == storedHash;
        }

        public async Task<(bool success, string message, int accountId)> RegisterAccountAsync(string username, string password)
        {
            username = username.Trim();
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
                return (false, "Tên tài khoản phải từ 3 đến 20 ký tự!", 0);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
                return (false, "Mật khẩu phải từ 4 ký tự trở lên!", 0);

            var (hash, salt) = HashPassword(password);
            string now = DateTime.UtcNow.ToString("o");

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO accounts (username, password_hash, salt, created_at, last_login)
                VALUES (@u, @p, @s, @now, @now);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@now", now);

            try
            {
                var result = await cmd.ExecuteScalarAsync();
                int accountId = Convert.ToInt32(result);
                return (true, "Đăng ký tài khoản thành công!", accountId);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint failed
            {
                return (false, "Tên tài khoản đã tồn tại, vui lòng chọn tên khác!", 0);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi đăng ký: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message, int accountId, string username)> AuthenticateAccountAsync(string username, string password)
        {
            username = username.Trim();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT id, username, password_hash, salt FROM accounts WHERE username = @u LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return (false, "Tài khoản không tồn tại!", 0, "");
            }

            int accId = reader.GetInt32(0);
            string accName = reader.GetString(1);
            string storedHash = reader.GetString(2);
            string storedSalt = reader.GetString(3);

            if (!VerifyPassword(password, storedHash, storedSalt))
            {
                return (false, "Mật khẩu không chính xác!", 0, "");
            }

            // Cập nhật last_login
            reader.Close();
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE accounts SET last_login = @now WHERE id = @id;";
            updateCmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
            updateCmd.Parameters.AddWithValue("@id", accId);
            await updateCmd.ExecuteNonQueryAsync();

            return (true, "Đăng nhập thành công!", accId, accName);
        }

        #endregion

        #region Universes & Character Management

        public async Task<List<UniverseData>> GetUniversesAsync()
        {
            var list = new List<UniverseData>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, name, description, max_players FROM universes ORDER BY id ASC;";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new UniverseData
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    MaxPlayers = reader.GetInt32(3),
                    CurrentOnline = 0
                });
            }
            return list;
        }

        public async Task<CharacterData?> GetCharacterByAccountAndUniverseAsync(int accountId, int universeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT id, account_id, universe_id, name, gender, class_name, element,
                       strength, intelligence, vitality, agility, level, current_hp, max_hp,
                       current_mana, max_mana, current_exp, max_exp, pos_x, pos_y, created_at
                FROM characters
                WHERE account_id = @accId AND universe_id = @uniId
                LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("@accId", accountId);
            cmd.Parameters.AddWithValue("@uniId", universeId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new CharacterData
            {
                Id = reader.GetInt32(0),
                AccountId = reader.GetInt32(1),
                UniverseId = reader.GetInt32(2),
                Name = reader.GetString(3),
                Gender = reader.GetString(4),
                ClassName = reader.GetString(5),
                Element = reader.GetString(6),
                Strength = reader.GetInt32(7),
                Intelligence = reader.GetInt32(8),
                Vitality = reader.GetInt32(9),
                Agility = reader.GetInt32(10),
                Level = reader.GetInt32(11),
                CurrentHp = reader.GetInt32(12),
                MaxHp = reader.GetInt32(13),
                CurrentMana = reader.GetInt32(14),
                MaxMana = reader.GetInt32(15),
                CurrentExp = reader.GetInt32(16),
                MaxExp = reader.GetInt32(17),
                PosX = reader.GetFloat(18),
                PosY = reader.GetFloat(19),
                CreatedAt = reader.GetString(20)
            };
        }

        public async Task<(bool success, string message, CharacterData? character)> CreateCharacterAsync(
            int accountId, int universeId, string name, string gender, string className,
            string element, int str, int intStat, int vit, int agi)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 25)
                return (false, "Tên nhân vật phải từ 2 đến 25 ký tự!", null);

            // Kiểm tra tên đã từng tồn tại trên TOÀN BỘ MÁY CHỦ (Global Unique Name)
            if (await IsCharacterNameExistsGloballyAsync(name))
            {
                return (false, "Tên nhân vật đã từng tồn tại trên máy chủ! Vui lòng chọn một tên mới.", null);
            }

            // Kiểm tra xem tài khoản đã có nhân vật trong vũ trụ này chưa
            var existing = await GetCharacterByAccountAndUniverseAsync(accountId, universeId);
            if (existing != null)
            {
                return (false, "Bạn đã có nhân vật trong vũ trụ này rồi!", null);
            }

            int baseHp = 80 + vit * 12;
            int baseMana = 50 + intStat * 8;
            string now = DateTime.UtcNow.ToString("o");

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO characters (
                    account_id, universe_id, name, gender, class_name, element,
                    strength, intelligence, vitality, agility, level, current_hp, max_hp,
                    current_mana, max_mana, current_exp, max_exp, pos_x, pos_y, created_at
                ) VALUES (
                    @accId, @uniId, @name, @gender, @class, @elem,
                    @str, @int, @vit, @agi, 1, @hp, @hp,
                    @mana, @mana, 0, 100, 0.0, 0.0, @now
                );
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@accId", accountId);
            cmd.Parameters.AddWithValue("@uniId", universeId);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@gender", gender);
            cmd.Parameters.AddWithValue("@class", className);
            cmd.Parameters.AddWithValue("@elem", element);
            cmd.Parameters.AddWithValue("@str", str);
            cmd.Parameters.AddWithValue("@int", intStat);
            cmd.Parameters.AddWithValue("@vit", vit);
            cmd.Parameters.AddWithValue("@agi", agi);
            cmd.Parameters.AddWithValue("@hp", baseHp);
            cmd.Parameters.AddWithValue("@mana", baseMana);
            cmd.Parameters.AddWithValue("@now", now);

            try
            {
                var result = await cmd.ExecuteScalarAsync();
                int charId = Convert.ToInt32(result);

                var newChar = new CharacterData
                {
                    Id = charId,
                    AccountId = accountId,
                    UniverseId = universeId,
                    Name = name,
                    Gender = gender,
                    ClassName = className,
                    Element = element,
                    Strength = str,
                    Intelligence = intStat,
                    Vitality = vit,
                    Agility = agi,
                    Level = 1,
                    CurrentHp = baseHp,
                    MaxHp = baseHp,
                    CurrentMana = baseMana,
                    MaxMana = baseMana,
                    CurrentExp = 0,
                    MaxExp = 100,
                    PosX = 0f,
                    PosY = 0f,
                    CreatedAt = now
                };
                return (true, "Khởi tạo nhân vật thành công!", newChar);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return (false, "Tên nhân vật này đã có người sử dụng trong vũ trụ này!", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi tạo nhân vật: {ex.Message}", null);
            }
        }

        public async Task SaveCharacterPositionAsync(int charId, float x, float y)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE characters SET pos_x = @x, pos_y = @y WHERE id = @id;";
            cmd.Parameters.AddWithValue("@x", x);
            cmd.Parameters.AddWithValue("@y", y);
            cmd.Parameters.AddWithValue("@id", charId);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Kiểm tra tên nhân vật đã từng tồn tại trên toàn bộ Server (không phân biệt vũ trụ)
        /// </summary>
        public async Task<bool> IsCharacterNameExistsGloballyAsync(string name)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM characters WHERE name = @name COLLATE NOCASE LIMIT 1;";
            cmd.Parameters.AddWithValue("@name", name.Trim());

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        /// <summary>
        /// Lấy tổng số lượng nhân vật / cư dân đã tạo trong một Vũ Trụ cụ thể
        /// </summary>
        public async Task<int> GetCharacterCountInUniverseAsync(int universeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE universe_id = @uniId;";
            cmd.Parameters.AddWithValue("@uniId", universeId);

            var count = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(count ?? 0);
        }

        #region Pending Character Roll Methods

        public async Task<PendingCharacterRoll?> GetPendingRollAsync(int accountId, int universeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT account_id, universe_id, class_name, element, strength, intelligence, vitality, agility, rerolls_left
                FROM pending_character_rolls
                WHERE account_id = @accId AND universe_id = @uniId
                LIMIT 1;
            ";
            cmd.Parameters.AddWithValue("@accId", accountId);
            cmd.Parameters.AddWithValue("@uniId", universeId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PendingCharacterRoll
                {
                    AccountId = reader.GetInt32(0),
                    UniverseId = reader.GetInt32(1),
                    ClassName = reader.GetString(2),
                    Element = reader.GetString(3),
                    Strength = reader.GetInt32(4),
                    Intelligence = reader.GetInt32(5),
                    Vitality = reader.GetInt32(6),
                    Agility = reader.GetInt32(7),
                    RerollsLeft = reader.GetInt32(8)
                };
            }
            return null;
        }

        public async Task SaveOrUpdatePendingRollAsync(PendingCharacterRoll roll)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO pending_character_rolls (
                    account_id, universe_id, class_name, element, strength, intelligence, vitality, agility, rerolls_left
                ) VALUES (
                    @accId, @uniId, @class, @elem, @str, @int, @vit, @agi, @rerolls
                );
            ";
            cmd.Parameters.AddWithValue("@accId", roll.AccountId);
            cmd.Parameters.AddWithValue("@uniId", roll.UniverseId);
            cmd.Parameters.AddWithValue("@class", roll.ClassName);
            cmd.Parameters.AddWithValue("@elem", roll.Element);
            cmd.Parameters.AddWithValue("@str", roll.Strength);
            cmd.Parameters.AddWithValue("@int", roll.Intelligence);
            cmd.Parameters.AddWithValue("@vit", roll.Vitality);
            cmd.Parameters.AddWithValue("@agi", roll.Agility);
            cmd.Parameters.AddWithValue("@rerolls", roll.RerollsLeft);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeletePendingRollAsync(int accountId, int universeId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM pending_character_rolls WHERE account_id = @accId AND universe_id = @uniId;";
            cmd.Parameters.AddWithValue("@accId", accountId);
            cmd.Parameters.AddWithValue("@uniId", universeId);

            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #endregion
    }
}
