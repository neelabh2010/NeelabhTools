using System.Collections.Generic;

namespace NeelabhMVCTools.Utilities.DropDowns
{
    public class DropDownItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}
