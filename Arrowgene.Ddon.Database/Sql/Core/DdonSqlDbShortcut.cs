using System.Data.Common;
using Arrowgene.Ddon.Shared.Entity.Structure;

namespace Arrowgene.Ddon.Database.Sql.Core
{
    public abstract partial class DdonSqlDb<TCon, TCom> : SqlDb<TCon, TCom>
        where TCon : DbConnection
        where TCom : DbCommand
    {
        private static readonly string[] ShortcutFields = new string[]
        {
            "character_id", "page_no", "button_no", "shortcut_id", "u32_data", "f32_data", "exex_type"
        };

        private readonly string SqlInsertShortcut = $"INSERT INTO `ddon_shortcut` ({BuildQueryField(ShortcutFields)}) VALUES ({BuildQueryInsert(ShortcutFields)});";
        private readonly string SqlReplaceShortcut = $"INSERT OR REPLACE INTO `ddon_shortcut` ({BuildQueryField(ShortcutFields)}) VALUES ({BuildQueryInsert(ShortcutFields)});";
        private static readonly string SqlUpdateShortcut = $"UPDATE `ddon_shortcut` SET {BuildQueryUpdate(ShortcutFields)} WHERE `character_id`=@old_character_id AND `page_no`=@old_page_no AND `button_no`=@old_button_no";
        private static readonly string SqlSelectShortcuts = $"SELECT {BuildQueryField(ShortcutFields)} FROM `ddon_shortcut` WHERE `character_id`=@character_id;";
        private const string SqlDeleteShortcut = "DELETE FROM `ddon_shortcut` WHERE `character_id`=@character_id AND `page_no`=@page_no AND `button_no`=@button_no";

        public bool InsertShortcut(uint characterId, CDataShortCut shortcut)
        {
            return ExecuteNonQuery(SqlInsertShortcut, command =>
            {
                AddParameter(command, characterId, shortcut);
            }) == 1;
        }
        
        public bool ReplaceShortcut(uint characterId, CDataShortCut shortcut)
        {
            ExecuteNonQuery(SqlReplaceShortcut, command =>
            {
                AddParameter(command, characterId, shortcut);
            });
            return true;
        }

        public bool UpdateShortcut(uint characterId, uint oldPageNo, uint oldButtonNo, CDataShortCut updatedShortcut)
        {
            return ExecuteNonQuery(SqlDeleteShortcut, command =>
            {
                AddParameter(command, characterId, updatedShortcut);
                AddParameter(command, "@old_character_id", characterId);
                AddParameter(command, "@old_page_no", oldPageNo);
                AddParameter(command, "@old_button_no", oldButtonNo);
            }) == 1;
        }

        public bool DeleteShortcut(uint characterId, uint pageNo, uint buttonNo)
        {
            return ExecuteNonQuery(SqlDeleteShortcut, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@old_page_no", pageNo);
                AddParameter(command, "@old_button_no", buttonNo);
            }) == 1;
        }

        private CDataShortCut ReadShortCut(DbDataReader reader)
        {
            CDataShortCut shortcut = new CDataShortCut();
            shortcut.PageNo = GetUInt32(reader, "page_no");
            shortcut.ButtonNo = GetUInt32(reader, "button_no");
            shortcut.ShortcutId = GetUInt32(reader, "shortcut_id");
            shortcut.U32Data = GetUInt32(reader, "u32_data");
            shortcut.F32Data = GetUInt32(reader, "f32_data");
            shortcut.ExexType = GetByte(reader, "exex_type");
            return shortcut;
        }

        private void AddParameter(TCom command, uint characterId, CDataShortCut shortcut)
        {
            AddParameter(command, "character_id", characterId);
            AddParameter(command, "page_no", shortcut.PageNo);
            AddParameter(command, "button_no", shortcut.ButtonNo);
            AddParameter(command, "shortcut_id", shortcut.ShortcutId);
            AddParameter(command, "u32_data", shortcut.U32Data);
            AddParameter(command, "f32_data", shortcut.F32Data);
            AddParameter(command, "exex_type", shortcut.ExexType);
        }
    }
}