using Microsoft.Azure.Cosmos.Table;

namespace ProductReviewService.Services
{
    public static class QueryHelpers
    {
        public static string StartsWithFilter(string value)
        {
            var endValue = value.Substring(0, value.Length - 1) + (char)(value[value.Length - 1] + 1);

            return TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    value),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThan,
                    endValue)); 
        }
    }
}
