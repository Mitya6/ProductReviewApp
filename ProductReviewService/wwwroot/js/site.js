let displayedProductName = '';
let continuationToken = '';

function getProducts() {
    continuationToken = '';

    fetch('api/products')
        .then(response => response.json())
        .then(products => displayProducts(products))
        .catch(error => console.error('Unable to get products.', error));
}

function displayCount(itemCount) {
    const name = (itemCount === 1) ? 'product' : 'products';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function displayProducts(products) {
    document.getElementById('productList').style.display = 'block';
    document.getElementById('reviewList').style.display = 'none';
    document.getElementById('pageHeader').innerHTML = 'Please select a product.';
    document.getElementById('title').innerHTML = 'Browse products';

    displayedProductName = '';

    const tBody = document.getElementById('products');
    tBody.innerHTML = '';

    displayCount(products.length);

    const button = document.createElement('button');

    products.forEach(product => {

        let reviewButton = button.cloneNode(false);
        reviewButton.innerText = 'Write a review';
        reviewButton.setAttribute('onclick', 'getReviews( "' + product + '" )');

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(product);
        td1.appendChild(textNode);

        let td2 = tr.insertCell(1);
        td2.appendChild(reviewButton);
    });
}

function getReviews(product) {
    fetch('api/products/' + product)
        .then(response => response.json())
        .then(chunkedResult => {
            initializeReviewsPage(product);
            continuationToken = chunkedResult.continuationToken;
            appendReviews(chunkedResult.results);
        })
        .catch(error => console.error('Unable to get reviews.', error));
}

function getMoreReviews() {

    const queryString = new URLSearchParams({
        nextPartitionKey: continuationToken.nextPartitionKey,
        nextRowKey: continuationToken.nextRowKey,
        nextTableName: continuationToken.nextTableName,
        targetLocation: continuationToken.targetLocation
    });

    fetch('api/products/' + displayedProductName + '?' + queryString.toString())
        .then(response => response.json())
        .then(chunkedResult => {
            continuationToken = chunkedResult.continuationToken;
            appendReviews(chunkedResult.results);
        })
        .catch(error => console.error('Unable to get reviews.', error));
}

function initializeReviewsPage(product) {
    document.getElementById('productList').style.display = 'none';
    document.getElementById('reviewList').style.display = 'block';
    document.getElementById('loadMoreButton').style.display = 'block';
    document.getElementById('pageHeader').innerHTML = 'Please write a review for ' + product + '. (Max 500 characters)';
    document.getElementById('title').innerHTML = product;

    displayedProductName = product;

    document.getElementById('existingReviews').innerHTML = '';
}

function appendReviews(reviews) {
    reviews.forEach(review => {
        let fieldSet = document.createElement("FIELDSET");
        let legend = document.createElement("legend");
        let div = document.createElement("div");
        div.style.maxWidth = '840px';
        div.style.wordBreak = 'break-all';
        div.innerHTML = review.reviewText;
        legend.innerHTML = review.creationDateTime;
        fieldSet.appendChild(legend);
        fieldSet.appendChild(div);

        document.getElementById('existingReviews').appendChild(fieldSet);
    });

    if (!continuationToken) {

        document.getElementById('existingReviews').appendChild(document.createTextNode("End of reviews."));
        document.getElementById('loadMoreButton').style.display = 'none';
    }
}

function addReview() {
    const addReviewTextbox = document.getElementById('addReviewTextbox');

    const newReview = {
        productName: displayedProductName,
        reviewText: addReviewTextbox.value.trim()
    };

    fetch('api/products', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newReview)
    })
        .then(() => {
            addReviewTextbox.value = '';
            continuationToken = '';
            getReviews(displayedProductName);
        })
        .catch(error => console.error('Unable to add new review.', error));
}