# ProductReviewApp

## Assumptions:
* A set of products is provided by the service.
* Reviews can be written to existing products, but no new product can be added by users.


## Planned improvements before going live:
* User handling (with HTTPS)
* Use a logging framework (e.g.: Nlog, Log4Net) instead of logging to the console
* client side and server side validation
* performance improvement ideas:
  - see if storing the review text as part of an indexed column (e.g.: row key) has any benefit
* +1 extra
