function getProducts() {
    fetch('api/products')
        .then(response => response.json())
        .then(products => _displayProducts(products))
        .catch(error => console.error('Unable to get products.', error));
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'product' : 'products';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayProducts(products) {
    const tBody = document.getElementById('products');
    tBody.innerHTML = '';

    _displayCount(products.length);

    const button = document.createElement('button');

    products.forEach(product => {

        let reviewButton = button.cloneNode(false);
        reviewButton.innerText = 'Write a review';
        //reviewButton.setAttribute('onclick', `displayEditForm(${product.id})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(product);
        td1.appendChild(textNode);

        let td2 = tr.insertCell(1);
        td2.appendChild(reviewButton);
    });
}