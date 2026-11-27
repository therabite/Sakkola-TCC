document.addEventListener("DOMContentLoaded", function () {
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

window.alterarQtd = function (delta) {
    const input = document.getElementById('quantidadeInput');
    if (!input) return;

    const valorAtual = parseInt(input.value, 10) || 1;
    input.value = Math.max(1, valorAtual + delta);
};

function mostrarNotificacao(mensagem, tipo, duracaoMs, callbackAoFechar) {
    const toastEl = document.getElementById('toastCarrinho');
    const toastBody = document.getElementById('toastCarrinhoMensagem');

    toastEl.classList.remove('bg-success', 'bg-danger');

    if (tipo === 'sucesso') {
        toastEl.classList.add('bg-success');
        toastBody.innerHTML = `<i class="bi bi-check-circle-fill fs-5"></i> <span>${mensagem}</span>`;
    } else {
        toastEl.classList.add('bg-danger');
        toastBody.innerHTML = `<i class="bi bi-exclamation-circle-fill fs-5"></i> <span>${mensagem}</span>`;
    }
    $(toastEl).off('hidden.bs.toast');

    if (typeof callbackAoFechar === 'function') {
        $(toastEl).one('hidden.bs.toast', function () {
            callbackAoFechar();
        });
    }
    const toastInstance = bootstrap.Toast.getOrCreateInstance(toastEl, {
        autohide: true,
        delay: duracaoMs
    });
    toastInstance.show();
}

function adicionarAoCarrinho(idProduto, quantidade = 1) {
    if (!quantidade) {
        const inputQtd = document.getElementById('quantidadeInput');
        quantidade = inputQtd ? parseInt(inputQtd.value) || 1 : 1;
    }

    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Carrinho/Adicionar',
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            idProduto: idProduto,
            quantidade: quantidade
        },
        success: function (response) {
            if (response.requiresLogin) {
                mostrarNotificacao(response.message, 'erro', 2500, function () {
                    window.location.href = response.redirect;
                });
            } else if (response.success) {
                mostrarNotificacao("Produto adicionado ao carrinho!", 'sucesso', 3000);
                
                if (response.totalItens !== undefined) {
                    $('#contador-carrinho').text(response.totalItens);
                }
            } else {
                mostrarNotificacao("Não foi possível adicionar o produto.", 'erro', 3000);
            }
        },
        error: function (xhr, status, error) {
            console.error("Erro AJAX:", status, error, xhr.responseText);
            mostrarNotificacao("Erro de comunicação com o servidor.", 'erro', 3000);
        }
    });
}

window.alterarQtdItem = async function (idItem, delta) {
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