﻿using Kermalis.EndianBinaryIO;
using Kermalis.PokemonBattleEngine.Data;
using Kermalis.PokemonGameEngine.Util;
using System;
using System.Collections.Generic;

namespace Kermalis.PokemonGameEngine.Overworld
{
    internal sealed class EncounterTable
    {
        public sealed class Encounter
        {
            public byte Chance;
            public byte MinLevel;
            public byte MaxLevel;
            public PBESpecies Species;
            public PBEForm Form;

            public Encounter(EndianBinaryReader r)
            {
                Chance = r.ReadByte();
                MinLevel = r.ReadByte();
                MaxLevel = r.ReadByte();
                Species = r.ReadEnum<PBESpecies>();
                Form = r.ReadEnum<PBEForm>();
            }
        }

        public byte ChanceOfPhenomenon;
        public readonly Encounter[] Encounters;

        private EncounterTable(string name)
        {
            using (var r = new EndianBinaryReader(Utils.GetResourceStream(_encounterTablePath + name + _encounterTableExtension)))
            {
                ChanceOfPhenomenon = r.ReadByte();
                byte count = r.ReadByte();
                Encounters = new Encounter[count];
                for (int i = 0; i < count; i++)
                {
                    Encounters[i] = new Encounter(r);
                }
            }
        }

        private const string _encounterTableExtension = ".pgeenctbl";
        private const string _encounterTablePath = "Encounter.";
        private static readonly IdList _ids = new IdList(_encounterTablePath + "EncounterTableIds.txt");
        private static readonly Dictionary<int, WeakReference<EncounterTable>> _loadedEncounterTables = new Dictionary<int, WeakReference<EncounterTable>>();
        public static EncounterTable LoadOrGet(int id)
        {
            string name = _ids[id];
            if (name is null)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            EncounterTable w;
            if (!_loadedEncounterTables.ContainsKey(id))
            {
                w = new EncounterTable(name);
                _loadedEncounterTables.Add(id, new WeakReference<EncounterTable>(w));
                return w;
            }
            if (_loadedEncounterTables[id].TryGetTarget(out w))
            {
                return w;
            }
            w = new EncounterTable(name);
            _loadedEncounterTables[id].SetTarget(w);
            return w;
        }

        public ushort GetCombinedChance()
        {
            ushort sum = 0;
            for (int i = 0; i < Encounters.Length; i++)
            {
                sum += Encounters[i].Chance;
            }
            return sum;
        }
    }

    internal sealed class EncounterGroups
    {
        public sealed class EncounterGroup
        {
            public EncounterType Type;
            public EncounterTable Table;

            public EncounterGroup(EndianBinaryReader r)
            {
                Type = r.ReadEnum<EncounterType>();
                Table = EncounterTable.LoadOrGet(r.ReadInt32());
            }
        }

        public readonly EncounterGroup[] Groups;

        public EncounterGroups(EndianBinaryReader r)
        {
            byte count = r.ReadByte();
            Groups = new EncounterGroup[count];
            for (int i = 0; i < count; i++)
            {
                Groups[i] = new EncounterGroup(r);
            }
        }

        public EncounterTable GetEncounterTable(EncounterType t)
        {
            foreach (EncounterGroup grp in Groups)
            {
                if (grp.Type == t)
                {
                    return grp.Table;
                }
            }
            return null;
        }
    }
}