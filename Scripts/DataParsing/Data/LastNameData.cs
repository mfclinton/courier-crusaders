using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataParsing
{
    [System.Serializable]
    public class LastNameData : JsonData<LastNameData>
    {
        public string last_name;
    }
}
