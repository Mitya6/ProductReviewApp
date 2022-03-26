using Microsoft.Azure.Cosmos.Table;

namespace ProductReviewService.Services
{
    public class ChunkedResult<T>
    {
        public T[] Results { get; set; }

        public int ChunkSize { get; set; }

        public TableContinuationToken ContinuationToken { get; set; }
    }
}
