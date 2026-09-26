using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataParsing
{
    [System.Serializable]
    public class FirstNameData : JsonData<FirstNameData>
    {
        public string gender;
        public string first_name;
    }
}
