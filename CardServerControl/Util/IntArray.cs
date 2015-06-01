using System;
using System.Collections.Generic;
using LitJson;

namespace CardServerControl.Util
{
    class IntArray
    {
        public static string IntArrayToString(int[] array)
        {
            JsonData data = new JsonData();
            foreach (int a in array)
            {
                data.Add(a);
            }

            return data.ToJson();
        }

        public static int[] StringToIntArray(string json)
        {
            int[] array;
            if (json != "[]" && string.IsNullOrEmpty(json))
            {
                array = JsonMapper.ToObject<int[]>(json);
            }
            else
            {
                array = new int[0];//返回一个空数组
            }

            return array;
        }
    }
}