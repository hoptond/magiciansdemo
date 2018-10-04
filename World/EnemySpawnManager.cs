using System;
using System.Collections.Generic;

namespace Magicians
{
	class EnemySpawnManager
    {
        //key is map filename, bool[] is enemies.
        Random rand;
        readonly SortedList<string, bool[]> DynamicEnemySpawns = new SortedList<string, bool[]>();
        //these enemies will always spawn, unless they have been defeated
        public SortedList<string, string> StaticEnemySpawns = new SortedList<string, string>();
        //if it is in the list we DON'T spawn it
        public void UpdatetaticEnemySpawn(string map, string enemy)
        {
            if (!StaticEnemySpawns.ContainsKey(map))
                StaticEnemySpawns.Add(map, "");
            StaticEnemySpawns[map] = StaticEnemySpawns[map] + "," + enemy;
        }
        public bool CanSpawnStaticEnemy(string map, string enemy)
        {
            if (!StaticEnemySpawns.ContainsKey(map))
                StaticEnemySpawns.Add(map, "");
            else
            {
                if (StaticEnemySpawns[map].Contains(enemy))
                    return false;
            }
            return true;
        }
        public bool HasMapEntry(string m)
        {
            if (DynamicEnemySpawns.ContainsKey(m))
            {
                return true;
            }
            return false;
        }
        public void CheckEnemySpawn(string map, List<EnemySpawn> spawns)
        {
            if (DynamicEnemySpawns.ContainsKey(map))
            {
                if (DynamicEnemySpawns[map].Length == spawns.Count)
                {
                    return;
                }
                DynamicEnemySpawns.Remove(map);
            }
            var bSpawns = new List<bool>();
            for (int i = 0; i < spawns.Count; i++)
            {
                bool spawn = false;
                if ((100 - rand.Next(0, 100) < spawns[i].Chance))
                {
                    spawn = true;
                }
                bSpawns.Add(spawn);
            }
            DynamicEnemySpawns.Add(map, bSpawns.ToArray());
        }
        public bool CanSpawnEnemy(string map, int index)
        {
            if (DynamicEnemySpawns[map][index])
                return true;
            return false;
        }
        public void FlagSpawn(string map, int index)
        {
            if (index != -1)
                DynamicEnemySpawns[map][index] = false;
        }
        public void ClearSpawns()
        {
            DynamicEnemySpawns.Clear();
            StaticEnemySpawns.Clear();
        }
        public void ClearMapSpawns(string Map)
        {
            if (DynamicEnemySpawns.ContainsKey(Map))
                DynamicEnemySpawns.Remove(Map);
        }
        public EnemySpawnManager(Random r)
        {
            rand = r;
        }
        public List<string> GetEnemySpawnSaveList()
        {
            var list = new List<string>();
            foreach (KeyValuePair<string, bool[]> kvp in DynamicEnemySpawns)
            {
                list.Add("m" + kvp.Key + '|' + kvp.Value.Length);
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    list.Add("b" + kvp.Value[i].ToString());
                }
            }
            return list;
        }
        public void AddSpawnKey(string m, bool[] array)
        {
            if (!DynamicEnemySpawns.ContainsKey(m))
                DynamicEnemySpawns.Add(m, array);
        }
        public void SetSpawnFlag(string map, int index, bool flag)
        {
            if (DynamicEnemySpawns.ContainsKey(map))
            {
                if (DynamicEnemySpawns[map].Length - 1 < index)
                {
                    DynamicEnemySpawns[map] = new bool[index + 1];
                }
            }
            else
            {
                DynamicEnemySpawns.Add(map, new bool[index + 1]);
            }
            DynamicEnemySpawns[map][index] = flag;
        }
    }
}
