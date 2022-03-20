const uri = 'api/reviews';
let todos = [];

function getItems() {
    fetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'product' : 'products';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById('products');
    tBody.innerHTML = '';

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {

        let reviewButton = button.cloneNode(false);
        reviewButton.innerText = 'Write a review';
        //reviewButton.setAttribute('onclick', `displayEditForm(${item.id})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(item);
        td1.appendChild(textNode);

        let td2 = tr.insertCell(1);
        td2.appendChild(reviewButton);
    });

    todos = data;
}