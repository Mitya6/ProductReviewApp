let displayedProductName = '';

function getProducts() {
    fetch('api/products')
        .then(response => response.json())
        .then(products => displayProducts(products))
        .catch(error => console.error('Unable to get products.', error));
}

function getReviews(product) {
    fetch('api/products/' + product)
        .then(response => response.json())
        .then(reviews => displayReviews(product, reviews))
        .catch(error => console.error('Unable to get reviews.', error));
}

function displayReviews(product, reviews) {
    document.getElementById('productList').style.display = 'none';
    document.getElementById('reviewList').style.display = 'block';
    document.getElementById('pageHeader').innerHTML = 'Please write a review. (Max 500 characters)';

    displayedProductName = product;

    const tBody = document.getElementById('reviews');
    tBody.innerHTML = '';

    reviews.forEach(review => {
        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let date = document.createTextNode(review.timestamp);
        td1.appendChild(date);


        let td2 = tr.insertCell(1);
        let reviewText = document.createTextNode(review.reviewText);
        td2.appendChild(reviewText);
    });
}

function displayCount(itemCount) {
    const name = (itemCount === 1) ? 'product' : 'products';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function displayProducts(products) {
    document.getElementById('productList').style.display = 'block';
    document.getElementById('reviewList').style.display = 'none';
    document.getElementById('pageHeader').innerHTML = 'Please select a product.';

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
            getReviews(displayedProductName);
        })
        .catch(error => console.error('Unable to add new review.', error));
}