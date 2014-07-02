using System;
using System.Collections.Generic;

namespace MagicJson.Data
{
    public class Result
    {
        public bool Valid;
        public IList<string> Messages;
        public Exception Exception;
    }
}