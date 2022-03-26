# ProductReviewApp

## Assumptions:
* A set of products is provided by the service.
* Reviews can be written to existing products, but no new product can be added by users.


## Planned improvements before going live:
* user handling (with HTTPS)
* client side and server side validation
* performance improvement ideas:
  - see if storing the review text as part of an indexed column (e.g.: row key) has any benefit
  - implement paging of data using ContinuationTokens (started working on it on ContinuationTokens branch)
* +1 extra
