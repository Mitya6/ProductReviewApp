function getProducts() {
    fetch('api/products')
        .then(response => response.json())
        .then(products => displayProducts(products))
        .catch(error => console.error('Unable to get products.', error));
}

function getReviews(product) {
    fetch('api/products/' + product)
        .then(response => response.json())
        .then(reviews => displayReviews(reviews))
        .catch(error => console.error('Unable to get reviews.', error));
}

function displayReviews(reviews) {
    document.getElementById('productList').style.display = 'none';
    document.getElementById('reviewList').style.display = 'block';
    document.getElementById('pageHeader').innerHTML = 'Please write a review.';

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