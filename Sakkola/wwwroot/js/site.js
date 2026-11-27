document.addEventListener("DOMContentLoaded", function() {
    inicializarCarrossel();
});

function inicializarCarrossel() {
    const bannerEl = document.getElementById('bannerSakkola');
    if (bannerEl && typeof bootstrap !== 'undefined') {
        const carousel = new bootstrap.Carousel(bannerEl, {
            interval: 5000,
            ride: 'carousel'
        });
        carousel.cycle();
    }
}

async function enviarPostAjax(url) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        return await response.json();
    } catch (error) {
        console.error('Erro na requisição:', error);
        return null;
    }
}

window.alterarQtd = function(delta) {
    const input = document.getElementById('quantidadeInput');
    if (!input) return;

    const valorAtual = parseInt(input.value, 10) || 1;
    input.value = Math.max(1, valorAtual + delta);
};

window.adicionarAoCarrinho = async function(idProduto, quantidadeCustomizada) {
    let quantidade = 1;

    if (quantidadeCustomizada !== undefined && quantidadeCustomizada > 0) {
        quantidade = quantidadeCustomizada;
    } else {
        const qtdInput = document.getElementById('quantidadeInput');
        if (qtdInput) {
            quantidade = Math.max(1, parseInt(qtdInput.value, 10) || 1);
        }
    }

    const data = await enviarPostAjax(`/Carrinho/Adicionar?idProduto=${idProduto}&quantidade=${quantidade}`);
    if (!data) return;

    if (data.requiresLogin) {
        alert(data.message);
        window.location.href = data.redirect;
    } else if (data.success) {
        const toastEl = document.getElementById('toastCarrinho');
        if (toastEl) {
            new bootstrap.Toast(toastEl).show();
        }

        const badgeCarrinho = document.getElementById('cart-badge-count');
        if (badgeCarrinho && data.totalItens !== undefined) {
            badgeCarrinho.innerText = data.totalItens;
        }
    } else {
        alert("Não foi possível adicionar o produto ao carrinho.");
    }
};

window.alterarQtdItem = async function(idItem, delta) {
    const input = document.getElementById(`qtdItem_${idItem}`);
    if (!input) return;

    const valorAtual = parseInt(input.value, 10) || 1;
    const novoValor = Math.max(1, valorAtual + delta);
    input.value = novoValor;

    const data = await enviarPostAjax(`/Carrinho/AtualizarQuantidade?idItem=${idItem}&quantidade=${novoValor}`);
    if (data && data.success) {
        location.reload();
    } else {
        alert("Não foi possível atualizar a quantidade.");
    }
};