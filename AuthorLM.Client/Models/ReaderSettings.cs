using Plugin.Maui.ScreenBrightness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.Models
{
    public class ReaderSettings
    {
        public string Theme { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public double LineHeight { get; set; }
        public float Brightness { get; set; }
        public double Margin { get; set; }
    }
}
