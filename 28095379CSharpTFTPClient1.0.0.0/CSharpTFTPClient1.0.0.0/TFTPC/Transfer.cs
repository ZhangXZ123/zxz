using System;
using System.Collections.Generic;
using System.Text;

namespace TFTPC
{
    public class Transfer
    {
        public enum Type
        {
            Get = 0,
            Put = 1
        }

        public struct Options
        {
            public Type Action;
            public string LocalFilename;
            public string RemoteFilename;
            public string Host;
        }
    }
}
