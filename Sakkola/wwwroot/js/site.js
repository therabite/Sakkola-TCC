document.addEventListener("DOMContentLoaded", function () {
    var myElement = document.getElementById('bannerSakkola');
    if (typeof bootstrap !== 'undefined') {
        var myCarousel = new bootstrap.Carousel(myElement, {
            interval: 5000,
            ride: 'carousel'
        });
        myCarousel.cycle();
    } else {
        console.error("A biblioteca do Bootstrap não foi encontrada.");
    }
});

//seletor de quantidade
window.alterarQtd = function(delta) {
    let input = document.getElementById('quantidadeInput');
    
    if (input) {
        let valorAtual = parseInt(input.value) || 1;
        let novoValor = valorAtual + delta;
        
        if (novoValor < 1) {
            novoValor = 1;
        }
        
        input.value = novoValor;
    }
};

// Função para adicionar ao carrinho
window.adicionarAoCarrinho = function(idProduto) {

    let qtdInput = document.getElementById('quantidadeInput');
    let quantidade = 1;
    if (qtdInput) {
        quantidade = parseInt(qtdInput.value, 10);
        if (isNaN(quantidade) || quantidade < 1) {
            quantidade = 1;
        }
    }

    fetch(`/Carrinho/Adicionar?idProduto=${idProduto}&quantidade=${quantidade}`, {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.requiresLogin) {
            alert(data.message);
            window.location.href = data.redirect;
        } else if (data.success) {
            let toastEl = document.getElementById('toastCarrinho');
            if (toastEl) {
                let toast = new bootstrap.Toast(toastEl);
                toast.show();
            }

            let badgeCarrinho = document.getElementById('cart-badge-count');
            if (badgeCarrinho && data.totalItens !== undefined) {
                badgeCarrinho.innerText = data.totalItens;
            }
        } else {
            alert("Não foi possível adicionar o produto ao carrinho.");
        }
    })
    .catch(error => console.error('Erro na requisição:', error));
};

// Função para alterar visualmente o valor no seletor
window.alterarQtd = function(delta) {
    let input = document.getElementById('quantidadeInput');
    if (input) {
        let valorAtual = parseInt(input.value, 10) || 1;
        let novoValor = valorAtual + delta;
        
        if (novoValor < 1) {
            novoValor = 1;
        }
        
        input.value = novoValor;
    }
};

window.alterarQtdItem = function(idItem, delta) {
    let input = document.getElementById('qtdItem_' + idItem);
    if (!input) return;

    let valorAtual = parseInt(input.value, 10) || 1;
    let novoValor = valorAtual + delta;

    if (novoValor < 1) {
        novoValor = 1;
    }

    input.value = novoValor;
    fetch(`/Carrinho/AtualizarQuantidade?idItem=${idItem}&quantidade=${novoValor}`, {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            location.reload();
        } else {
            alert("Não foi possível atualizar a quantidade.");
        }
    })
    .catch(error => console.error('Erro na requisição:', error));
};
