using UnityEngine;
using System.Collections.Generic;
using System;
using DataParsing;

namespace DataParsing // Add the namespace
{
    public static class JsonReader
    {

        public static JsonData<T>[] ParseJson<T>(TextAsset json) where T : JsonData<T>, new()
        {
            return DeserializeJsonArray<T>(json.text);
        }

        private static T[] DeserializeJsonArray<T>(string json) where T : JsonData<T>, new() // Ensures we inherit from JsonData<T> and have a default constructor
        {
            string formattedJson = "{\"dataArray\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(formattedJson);
            return wrapper.dataArray;
        }

        [Serializable] // TODO: remove wrapper
        private class Wrapper<T>
        {
            public T[] dataArray;
        }
    }
}
