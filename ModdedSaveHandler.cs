using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyDeliveryAPI
{
    public class ModdedSaveSystem<T> where T : struct
    {
        private string ModID;
        public T data;
        public delegate T parseSaveDelegate(object save);
        public parseSaveDelegate ParseSave;

        private void Save()
        {
            EasyAPI.ModdedSaveHandlers[ModID] = JsonConvert.SerializeObject(data);
        }
        private T defaultParse(object input)
        {
            return (T)input;
        }
        public ModdedSaveSystem(string modID)
        {
            ModID = modID;
            EasyAPI.ModsLoaded[modID] = true;
            EasyAPI.OnSave += Save;
            EasyAPI.OnLoad += OnDataLoaded;
            ParseSave = new parseSaveDelegate(defaultParse);
        }
        public void OnDataLoaded(Dictionary<string, string> string_data)
        {
            if (string_data.ContainsKey(ModID))
            {
                Load(string_data[ModID]);
            }
            else
            {
                data = new T();
            }
        }
        public void Load(string string_data)
        {
            object parsed_data = JsonConvert.DeserializeObject<T>(string_data);
            data = ParseSave(parsed_data);
            EasyAPI.ModdedSaveHandlers[ModID] = string_data;
        }
    }//LAME but easy to use x3
}
