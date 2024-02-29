using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonObjecktOne
    {
        public JsonObjecktOne(string result) 
        { 
            Value = result;
        }

        public string Value {  get; private set; }
    }
}
