using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace NeelabhMVCTools.Utilities.DropDowns
{
    public class DropDownItemList
    {
        public int DataVersion { get; set; } = 0;
        public List<DropDownItem> Items { get; set; } = new List<DropDownItem>();

        public List<SelectListItem> GetListItems(Dictionary<string, string> Filters = null)
        {
            var NewlistItems = new List<SelectListItem>();

            foreach (var item in Items)
            {
                //Filter : check whether Filter item(s) is existing in AddData or not
                if (Filters != null)
                {
                    bool IsFound = false;
                    foreach (KeyValuePair<string, string> additional in item.AdditionalData)
                    {
                        foreach (KeyValuePair<string, string> filter in Filters)
                        {
                            if (additional.Key == filter.Key && additional.Value == filter.Value)
                            {
                                NewlistItems.Add(new SelectListItem()
                                {
                                    Text = item.Text,
                                    Value = item.Value
                                });

                                IsFound = true;
                                break;
                            }
                            if (IsFound) break;
                        }
                    }
                }
                else
                {
                    NewlistItems.Add(new SelectListItem() { Text = item.Text, Value = item.Value });
                }
            }

            return NewlistItems;

        }
    }
}
