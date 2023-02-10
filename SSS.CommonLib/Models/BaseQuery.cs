using SSS.CommonLib.Enums;
using System;

namespace SSS.CommonLib.Models
{
    public sealed class BaseQuery
    {
        public BaseQuery()
        {
        }

        public BaseQuery(string searchText, string searchFields)
        {
            SearchText = searchText;
            SearchFields = searchFields;
        }

        public int PageIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;

        public string SearchText { get; }

        public string SearchFields { get; }


        public DateTimeOffset? StartDate { get; set; }


        public DateTimeOffset? EndDate { get; set; }

        public string OrderBy { get; set; } = "CreatedAt";

        public OrderingDirection Direction { get; set; } = OrderingDirection.Desc;

        public string[] GetFilteredFields() => SearchFields.Split(",");

        public int GetSkip() => PageIndex * PageSize;

        public int GetTake() => PageSize;
    }
}