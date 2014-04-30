using System.Collections.Generic;

namespace Piccent
{
    // http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff402557(v=vs.105).aspx
    // http://www.creepyed.com/2012/11/windows-phone-8-theme-colors-hex-rgb/

    public static class AccentManager
    {
        static Dictionary<string, string> _accentDictio = new Dictionary<string, string>()
        {
            { "#FFA4C400", "Lime"    },
            { "#FF60A917", "Green"   },
            { "#FF008A00", "Emerald" },
            { "#FF00ABA9", "Teal"    },
            { "#FF1BA1E2", "Cyan"    },
            { "#FF0050EF", "Cobalt"  },
            { "#FF6A00FF", "Indigo"  },
            { "#FFAA00FF", "Violet"  },
            { "#FFF472D0", "Pink"    },
            { "#FFD80073", "Magenta" },
            { "#FFA20025", "Crimson" },
            { "#FFE51400", "Red"     },
            { "#FFFA6800", "Orange"  },
            { "#FFF0A30A", "Amber"   },
            { "#FFE3C800", "Yellow"  },
            { "#FF825A2C", "Brown"   },
            { "#FF6D8764", "Olive"   },
            { "#FF647687", "Steel"   },
            { "#FF76608A", "Mauve"   },
            { "#FF87794E", "Taupe"   }
        };

        public static string GetName(string hex)
        {
            string value;
            return _accentDictio.TryGetValue(hex, out value) ? value : null;
        }

        public static Dictionary<string,string>.KeyCollection GetKeys()
        {
            return _accentDictio.Keys;
        }
    }
}
