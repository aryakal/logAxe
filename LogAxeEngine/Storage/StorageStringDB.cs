using logAxeEngine.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using logAxeEngine.Interfaces;

namespace logAxeEngine.Storage
{
    public sealed class StorageStringDB : IStorageString
    {
        public int _currentCount;
        public int Count => _currentCount;
        private StorageStringXEfficient _stringStore;        
        private Dictionary<StringGhost, int> _stringMap;
        private bool IsOptmized = false;
        private NamedLogger _logger = new NamedLogger("strDb");

        public StorageStringDB(int pagePerbook = 10, int dataPerPage = 1024 * 1024 * 50, int possibleStringSize = 1024 * 1024 * 1)
        {
            _currentCount = 0;
            _stringStore = new StorageStringXEfficient(pagePerbook, dataPerPage, possibleStringSize);
            _stringMap = new Dictionary<StringGhost, int>(_stringStore);
            _stringMap.Clear();
        }

        public void Clear()
        {            
            _currentCount = 0;
            _stringStore.Clear();
            _stringMap.Clear();
        }

        public int[] Filter(string[] includeMsgs, string[] excludeMsgs, bool caseSensitive = false, bool exactWord = false)
        {
            //TODO : search for all scenario where excluded msg is already in include message.
            var mallMsgs = new bool[Count];
            var ordinalType = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (includeMsgs.Length == 0 && excludeMsgs.Length != 0)
            {
                for (var ndx = 0; ndx < Count; ndx++)
                {
                    mallMsgs[ndx] = true;
                }
            }

            if (!exactWord)
            {
                Parallel.For(1, Count, ndx =>
                {
                    var msg = RetriveString(ndx);
                    foreach (var term in includeMsgs)
                    {
                        if (msg.IndexOf(term, ordinalType) == -1) continue;
                        mallMsgs[ndx] = true;
                        break;
                    }

                    foreach (var term in excludeMsgs)
                    {
                        if (msg.IndexOf(term, ordinalType) == -1) continue;
                        mallMsgs[ndx] = false;
                        break;
                    }
                });
            }
            else
            {
                Parallel.For(1, Count, ndx =>
                {
                    var msg = RetriveString(ndx);
                    foreach (var term in includeMsgs)
                    {
                        if (msg != term) continue;
                        mallMsgs[ndx] = true;
                        break;

                    }

                    foreach (var term in excludeMsgs)
                    {
                        if (msg != term) continue;
                        mallMsgs[ndx] = false;
                        break;
                    }
                });
            }

            var result = new List<int>();
            for (var ndx = 1; ndx < Count; ndx++)
            {
                if (mallMsgs[ndx]) result.Add(ndx);
            }

            return result.ToArray();
        }

        public Dictionary<int, int> MergeStorage(IStorageString storage)
        {
            var maps = new Dictionary<int, int>();

            if (typeof(StorageStringList) == storage.GetType())
            {
                for (int ndx = 0; ndx < storage.Count; ndx++)
                {
                    var data = storage.RetriveString(ndx);
                    int id = StoreString(data);
                    maps.Add(ndx, id);
                }
                return maps;
            }
            else
            { 
                throw new NotImplementedException();
            }
        }

        public void OptimizeData()
        {
            _stringMap.Clear();
            _stringMap = null;
            _stringMap = new Dictionary<StringGhost, int>(_stringStore);
            IsOptmized = true;
        }

        public string RetriveString(int uniqueId)
        {
            return _stringStore.RetriveString(uniqueId);
        }

        public int StoreString(string data)
        {
            if (IsOptmized)
            {
                for (int ndx = 1; ndx < _stringMap.Count; ndx++)
                {
                    var st = new StringGhost { Position = ndx, Hash = _stringStore.RetriveString(ndx).GetHashCode() };
                    _stringMap.Add(st, ndx);
                    _currentCount++;
                }
                IsOptmized = false;
            }

            var stData = new StringGhost { Position = -1, Data = data, Hash=data.GetHashCode() };
            if (!_stringMap.ContainsKey(stData))
            {
                var uniqueId = _stringStore.StoreString(data);
                stData.Position = uniqueId;
                stData.Data = null;
                _stringMap.Add(stData, uniqueId);
                _currentCount++;
                return uniqueId;
            }

            return _stringMap[stData];
        }
    }
}
