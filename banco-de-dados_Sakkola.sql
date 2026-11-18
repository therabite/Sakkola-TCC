CREATE DATABASE sakkolaMarket;
USE sakkolaMarket;

-- 1. TABELAS BASE (SEM DEPENDÊNCIAS EXTERNAS)

CREATE TABLE tbCategoria (
    id_categoria INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL
);

CREATE TABLE tbUser (
    id_user INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    data_nasc DATE NOT NULL,
    telefone VARCHAR(20) NOT NULL,
    CPF VARCHAR(14) NOT NULL,
    email VARCHAR(50) NOT NULL UNIQUE,
    senha VARCHAR(8) NOT NULL
);

CREATE TABLE tbAddress (
    id_address INT PRIMARY KEY AUTO_INCREMENT,
    CEP VARCHAR(10),
    Logradouro VARCHAR(100),
    Numero VARCHAR(20),
    Bairro VARCHAR(50),
    Cidade VARCHAR(50),
    Estado VARCHAR(2)
);

-- 2. TABELAS COM RELACIONAMENTOS (HERANÇA E PRODUTOS)

CREATE TABLE tbClient (
    id_client INT PRIMARY KEY,
    CONSTRAINT fk_client_user FOREIGN KEY (id_client) REFERENCES tbUser(id_user) ON DELETE CASCADE
);
CREATE TABLE tbFuncionario (
    id_employed INT PRIMARY KEY,
    is_admin BOOLEAN DEFAULT FALSE,
    CONSTRAINT fk_func_user FOREIGN KEY (id_employed) REFERENCES tbUser(id_user) ON DELETE CASCADE
);
CREATE TABLE tbProduto (
    id_produto INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    preco DECIMAL(6,2) NOT NULL,
    estoque INT NOT NULL,
    id_categoria INT NOT NULL,
    CONSTRAINT fk_produto_categoria FOREIGN KEY (id_categoria) REFERENCES tbCategoria(id_categoria)
);

-- 3. TABELAS DE PEDIDOS, PAGAMENTOS E ENTREGAS

CREATE TABLE tbPedido (
    id_pedido INT PRIMARY KEY AUTO_INCREMENT,
    id_user INT NOT NULL,
    data_pedido DATETIME NOT NULL,
    status VARCHAR(100) NOT NULL,
    valor_total DECIMAL(6,2) NOT NULL,
    forma_pagamento VARCHAR(50) NOT NULL,
    CONSTRAINT fk_pedido_user FOREIGN KEY (id_user) REFERENCES tbUser(id_user)
);
CREATE TABLE tbPagamento (
    id_pagamento INT PRIMARY KEY AUTO_INCREMENT,
    id_pedido INT NOT NULL,
    valor DECIMAL(6,2) NOT NULL,
    forma_pagamento VARCHAR(50) NOT NULL,
    CONSTRAINT fk_pagamento_pedido FOREIGN KEY (id_pedido) REFERENCES tbPedido(id_pedido) ON DELETE CASCADE
);
CREATE TABLE tbEntrega (
    id_entrega INT PRIMARY KEY AUTO_INCREMENT,
    status_entrega VARCHAR(8) NOT NULL,
    id_address INT,
    id_pedido INT,
    CONSTRAINT fk_entrega_address FOREIGN KEY (id_address) REFERENCES tbAddress(id_address),
    CONSTRAINT fk_entrega_pedido FOREIGN KEY (id_pedido) REFERENCES tbPedido(id_pedido) ON DELETE CASCADE
);

-- 4. TABELAS DE CARRINHO E ITENS

CREATE TABLE tbCarrinho (
    id_carrinho INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT NOT NULL,
    CONSTRAINT fk_carrinho_usuario FOREIGN KEY (id_usuario) REFERENCES tbUser(id_user) ON DELETE CASCADE
);
CREATE TABLE tbCarrinhoItem (
    id_item INT PRIMARY KEY AUTO_INCREMENT,
    id_carrinho INT NOT NULL,
    id_produto INT NOT NULL,
    quantidade INT NOT NULL,
    CONSTRAINT fk_item_carrinho FOREIGN KEY (id_carrinho) REFERENCES tbCarrinho(id_carrinho) ON DELETE CASCADE,
    CONSTRAINT fk_item_produto FOREIGN KEY (id_produto) REFERENCES tbProduto(id_produto) ON DELETE CASCADE
);
CREATE TABLE tbItemPedido (
    fk_pedido_id INT NOT NULL,
    fk_produto_id INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unidade DECIMAL(6,2) NOT NULL,
    PRIMARY KEY (fk_pedido_id, fk_produto_id),
    CONSTRAINT fk_itempedido_pedido FOREIGN KEY (fk_pedido_id) REFERENCES tbPedido(id_pedido) ON DELETE CASCADE,
    CONSTRAINT fk_itempedido_produto FOREIGN KEY (fk_produto_id) REFERENCES tbProduto(id_produto)
);

-- 5. STORED PROCEDURE PARA INSERÇÃO DE PRODUTOS

DELIMITER //
CREATE PROCEDURE sp_InserirProduto (
    IN p_nome_produto VARCHAR(50),
    IN p_descricao VARCHAR(100),
    IN p_preco DECIMAL(6,2),
    IN p_estoque INT,
    IN p_nome_categoria VARCHAR(50)
)
BEGIN
    DECLARE v_id_categoria INT;
	
    -- 1. Verifica se a categoria já existe e pega o ID
    SELECT id_categoria INTO v_id_categoria 
    FROM tbCategoria 
    WHERE LOWER(nome) = LOWER(p_nome_categoria) 
    LIMIT 1;

    -- Se a categoria não existir, cria uma nova
    IF v_id_categoria IS NULL THEN
        INSERT INTO tbCategoria (nome) VALUES (p_nome_categoria);
        SET v_id_categoria = LAST_INSERT_ID();
    END IF;

    -- Insere o produto associado à categoria encontrada/criada
    INSERT INTO tbProduto (nome, descricao, preco, estoque, id_categoria) 
    VALUES (p_nome_produto, p_descricao, p_preco, p_estoque, v_id_categoria);
END //
DELIMITER ;

-- 6. CARGA DE PRODUTOS UTILIZANDO A PROCEDURE

-- Açougue
CALL sp_InserirProduto('Picanha Bovina', 'Picanha Bovina - Peça embalada a vácuo 1kg', 69.90, 20, 'Açougue');
CALL sp_InserirProduto('Contrafilé Fatiado', 'Contrafilé Fatiado - Bifes frescos com 500g cada', 69.90, 65, 'Açougue');
CALL sp_InserirProduto('Alcatra Bovina', 'Alcatra Bovina - Peça ou fatiada ~1kg', 54.60, 40, 'Açougue');
CALL sp_InserirProduto('Acém Moído', 'Acém Moído - Carne moída de primeira 500g', 31.20, 50, 'Açougue');
CALL sp_InserirProduto('Costela Bovina', 'Costela Bovina - Ideal para churrasco ~1.5kg', 32.90, 20, 'Açougue');
CALL sp_InserirProduto('Filé Mignon', 'Filé Mignon - Peça inteira ~1.2kg', 89.90, 15, 'Açougue');
CALL sp_InserirProduto('Peito de Frango', 'Peito de Frango - Filé sem osso 1kg', 19.50, 50, 'Açougue');
CALL sp_InserirProduto('Coxa e Sobretoxa de Frango', 'Coxa e Sobretoxa de Frango - Bandeja 1kg', 18.90, 50, 'Açougue');
CALL sp_InserirProduto('Bisteca Suína', 'Bisteca Suína - Bandeja fatiada 800g', 19.98, 57, 'Açougue');
CALL sp_InserirProduto('Linguiça Toscada', 'Linguiça Toscada - Para churrasco 1kg', 14.90, 53, 'Açougue');
CALL sp_InserirProduto('Maminha Bovina', 'Maminha Bovina - Peça embalada a vácuo ~1.2kg', 59.90, 30, 'Açougue');
CALL sp_InserirProduto('Coração de Frango', 'Coração de Frango - Bandeja temperada 500g', 16.50, 40, 'Açougue');

-- Hortifrúti
CALL sp_InserirProduto('Banana Prata', 'Banana Prata - Penca de banana prata fresca', 7.99, 50, 'Hortifrúti');
CALL sp_InserirProduto('Maçã Fuji', 'Maçã Fuji - Bandeja com 1kg', 23.90, 65, 'Hortifrúti');
CALL sp_InserirProduto('Tomate Italiano', 'Tomate Italiano - Bandeja com 400g', 15.39, 40, 'Hortifrúti');
CALL sp_InserirProduto('Cebola Nacional', 'Cebola Nacional - Saco com 2kg', 13.67, 50, 'Hortifrúti');
CALL sp_InserirProduto('Batata Monalisa', 'Batata Monalisa - Saco com 1kg', 6.99, 20, 'Hortifrúti');
CALL sp_InserirProduto('Alface Crespa', 'Alface Crespa verde - Pé fresquinho', 5.99, 15, 'Hortifrúti');
CALL sp_InserirProduto('Cenoura', 'Cenoura Ralada - Bandeja com 500g', 17.98, 50, 'Hortifrúti');
CALL sp_InserirProduto('Laranja Pera', 'Laranja Pera - Saco com 2kg', 17.99, 50, 'Hortifrúti');
CALL sp_InserirProduto('Mamão Papaia', 'Mamão Papaia - Unidade', 15.05, 57, 'Hortifrúti');
CALL sp_InserirProduto('Alho Roxo', 'Alho Roxo - Pacote de 200g', 11.99, 53, 'Hortifrúti');
CALL sp_InserirProduto('Morango Fresco', 'Morango Fresco - Bandeja com 250g', 8.90, 35, 'Hortifrúti');
CALL sp_InserirProduto('Abacate Mantiqueira', 'Abacate Mantiqueira - Unidade selecionada ~400g', 6.50, 25, 'Hortifrúti');

-- Padaria
CALL sp_InserirProduto('Pão Francês', 'Pão Francês - Pão francês quentinho por kg', 14.50, 100, 'Padaria');
CALL sp_InserirProduto('Pão de Queijo', 'Pão de Queijo - Bandeja com 10 unidades', 12.90, 30, 'Padaria');
CALL sp_InserirProduto('Pão de Forma Tradicional', 'Pão de Forma Tradicional - Pacote 450g', 7.50, 45, 'Padaria');
CALL sp_InserirProduto('Croissant de Presunto e Queijo', 'Croissant de Presunto e Queijo - Unidade', 8.90, 20, 'Padaria');
CALL sp_InserirProduto('Bolo de Cenoura com Cobertura', 'Bolo de Cenoura com Cobertura - Unidade ~400g', 18.50, 15, 'Padaria');
CALL sp_InserirProduto('Broa de Milho', 'Broa de Milho - Unidade', 4.50, 25, 'Padaria');
CALL sp_InserirProduto('Baguete Recheada', 'Baguete Recheada - Unidade', 14.90, 18, 'Padaria');
CALL sp_InserirProduto('Sonho de Creme', 'Sonho de Creme - Unidade', 5.50, 30, 'Padaria');
CALL sp_InserirProduto('Torrada Tradicional', 'Torrada Tradicional - Embalagem 140g', 6.90, 40, 'Padaria');
CALL sp_InserirProduto('Bisnaguinha Integral', 'Bisnaguinha Integral - Pacote 300g', 8.20, 35, 'Padaria');
CALL sp_InserirProduto('Pão Integral com Grãos', 'Pão Integral - Pacote fatiado 400g', 11.50, 25, 'Padaria');
CALL sp_InserirProduto('Rosca Doce Caseira', 'Rosca Doce - Unidade recheada ~350g', 12.90, 15, 'Padaria');

-- Higiene
CALL sp_InserirProduto('Sabonete em Barra', 'Sabonete em Barra - Sabonete suave 85g', 2.50, 200, 'Higiene');
CALL sp_InserirProduto('Shampoo Hidratante', 'Shampoo Hidratante - Frasco 350ml', 16.90, 50, 'Higiene');
CALL sp_InserirProduto('Condicionador Hidratante', 'Condicionador Hidratante - Frasco 350ml', 18.90, 45, 'Higiene');
CALL sp_InserirProduto('Creme Dental Proteção Cáries', 'Creme Dental Proteção Cáries - Tubo 90g', 4.90, 80, 'Higiene');
CALL sp_InserirProduto('Escova de Dentes Macia', 'Escova de Dentes Macia - Unidade', 9.90, 60, 'Higiene');
CALL sp_InserirProduto('Desodorante Aerosol', 'Desodorante Aerosol - Frasco 150ml', 15.90, 70, 'Higiene');
CALL sp_InserirProduto('Papel Higiênico Folha Dupla', 'Papel Higiênico Folha Dupla - Pacote com 12 rolos', 22.90, 40, 'Higiene');
CALL sp_InserirProduto('Fio Dental', 'Fio Dental - Rolo 50m', 8.50, 50, 'Higiene');
CALL sp_InserirProduto('Protetor Solar FPS 50', 'Protetor Solar FPS 50 - Frasco 200ml', 49.90, 25, 'Higiene');
CALL sp_InserirProduto('Creme para Pentear', 'Creme para Pentear - Frasco 300ml', 14.90, 35, 'Higiene');
CALL sp_InserirProduto('Antisséptico Bucal', 'Antisséptico Bucal sem Álcool - Frasco 500ml', 21.90, 25, 'Higiene');
CALL sp_InserirProduto('Haste Flexível de Algodão', 'Hastes Flexíveis - Caixa com 150 unidades', 7.90, 50, 'Higiene');

-- Limpeza
CALL sp_InserirProduto('Detergente Neutro', 'Detergente Neutro - Detergente líquido 500ml', 2.29, 150, 'Limpeza');
CALL sp_InserirProduto('Sabão em Pó Multiação', 'Sabão em Pó Multiação - Caixa 1kg', 14.90, 60, 'Limpeza');
CALL sp_InserirProduto('Amaciante de Roupas Concentrado', 'Amaciante de Roupas Concentrado - Frasco 1L', 17.50, 40, 'Limpeza');
CALL sp_InserirProduto('Desinfetante Pinho', 'Desinfetante Pinho - Frasco 1L', 8.90, 50, 'Limpeza');
CALL sp_InserirProduto('Limpador Multiuso', 'Limpador Multiuso - Frasco 500ml', 5.90, 75, 'Limpeza');
CALL sp_InserirProduto('Água Sanitária', 'Água Sanitária - Garrafa 2L', 7.90, 65, 'Limpeza');
CALL sp_InserirProduto('Esponja Multiuso', 'Esponja Multiuso - Pacote com 4 unidades', 4.90, 100, 'Limpeza');
CALL sp_InserirProduto('Lustra-Móveis', 'Lustra-Móveis - Frasco 200ml', 11.90, 30, 'Limpeza');
CALL sp_InserirProduto('Saco de Lixo 50 Litros', 'Saco de Lixo 50 Litros - Rolo com 10 unidades', 13.90, 45, 'Limpeza');
CALL sp_InserirProduto('Lisoform / Sanificante', 'Lisoform / Sanificante - Frasco 500ml', 15.90, 35, 'Limpeza');
CALL sp_InserirProduto('Esponja de Aço', 'Esponja de Aço Multiuso - Pacote com 8 unidades', 4.50, 43, 'Limpeza');
CALL sp_InserirProduto('Limpa Vidros Spray', 'Limpa Vidros - Frasco com gatilho 500ml', 9.90, 40, 'Limpeza');

-- Frios e Laticínios
CALL sp_InserirProduto('Queijo Mussarela', 'Queijo Mussarela - Mussarela fatiada 200g', 9.90, 80, 'Frios e Laticínios');
CALL sp_InserirProduto('Presunto Cozido Fatiado', 'Presunto Cozido Fatiado - Bandeja 200g', 9.90, 50, 'Frios e Laticínios');
CALL sp_InserirProduto('Requeijão Cremoso', 'Requeijão Cremoso - Copo 200g', 8.50, 60, 'Frios e Laticínios');
CALL sp_InserirProduto('Manteiga com Sal', 'Manteiga com Sal - Pote 200g', 11.90, 40, 'Frios e Laticínios');
CALL sp_InserirProduto('Iogurte Natural Integral', 'Iogurte Natural Integral - Pote 170g', 3.90, 70, 'Frios e Laticínios');
CALL sp_InserirProduto('Leite Integral UHT', 'Leite Integral UHT - Caixa 1L', 5.20, 120, 'Frios e Laticínios');
CALL sp_InserirProduto('Queijo Prato Fatiado', 'Queijo Prato Fatiado - Bandeja 200g', 12.90, 45, 'Frios e Laticínios');
CALL sp_InserirProduto('Peito de Peru Defumado', 'Peito de Peru Defumado - Bandeja 150g', 13.50, 35, 'Frios e Laticínios');
CALL sp_InserirProduto('Creme de Leite', 'Creme de Leite - Caixinha 200g', 3.80, 90, 'Frios e Laticínios');
CALL sp_InserirProduto('Cream Cheese Tradicional', 'Cream Cheese Tradicional - Pote 150g', 10.90, 30, 'Frios e Laticínios');
CALL sp_InserirProduto('Queijo Coalho', 'Queijo Coalho para Churrasco - Pacote 400g', 18.90, 25, 'Frios e Laticínios');
CALL sp_InserirProduto('Apresuntado Fatiado', 'Apresuntado Fatiado - Bandeja com 200g', 7.90, 40, 'Frios e Laticínios');

-- Mercearia
CALL sp_InserirProduto('Arroz Branco Tipo 1', 'Arroz Branco - Arroz tipo 1 pacote 5kg', 28.90, 60, 'Mercearia');
CALL sp_InserirProduto('Feijão Carioca', 'Feijão Carioca - Pacote 1kg', 8.90, 80, 'Mercearia');
CALL sp_InserirProduto('Óleo de Soja Refinado', 'Óleo de Soja Refinado - Garrafa 900ml', 6.90, 100, 'Mercearia');
CALL sp_InserirProduto('Açúcar Refinado', 'Açúcar Refinado - Pacote 1kg', 4.80, 90, 'Mercearia');
CALL sp_InserirProduto('Café Torrado e Moído', 'Café Torrado e Moído - Almofada/Vácuo 500g', 18.90, 65, 'Mercearia');
CALL sp_InserirProduto('Macarrão Spaghetini', 'Macarrão Spaghetini - Pacote 500g', 4.50, 85, 'Mercearia');
CALL sp_InserirProduto('Molho de Tomate Tradicional', 'Molho de Tomate Tradicional - Sachê 300g', 2.90, 110, 'Mercearia');
CALL sp_InserirProduto('Farinha de Trigo Tradicional', 'Farinha de Trigo Tradicional - Pacote 1kg', 5.50, 70, 'Mercearia');
CALL sp_InserirProduto('Sal Refinado', 'Sal Refinado - Pacote 1kg', 2.50, 100, 'Mercearia');
CALL sp_InserirProduto('Biscoito Recheado de Chocolate', 'Biscoito Recheado de Chocolate - Pacote 130g', 3.50, 95, 'Mercearia');
CALL sp_InserirProduto('Aveia em Flocos', 'Aveia em Flocos Finos - Pacote 250g', 4.20, 50, 'Mercearia');
CALL sp_InserirProduto('Extrato de Tomate', 'Extrato de Tomate Concentrado - Lata 340g', 6.50, 70, 'Mercearia');

-- Promoção
CALL sp_InserirProduto('Panetone com Frutas Cristalizadas', 'Panetone tradicional com frutas cristalizadas - 500g', 19.99, 40, 'Promoção');
CALL sp_InserirProduto('Chocotone', 'Panetone com gotas de chocolate - 500g', 22.99, 35, 'Promoção');
CALL sp_InserirProduto('Peru Temperado', 'Peru temperado congelado para ceia de Natal - 3kg', 89.90, 20, 'Promoção');
CALL sp_InserirProduto('Chester Natalino', 'Ave especial temperada e congelada - 3kg', 99.90, 15, 'Promoção');
CALL sp_InserirProduto('Tender Suíno', 'Tender suíno defumado para ceia de Natal - 1kg', 49.90, 25, 'Promoção');
CALL sp_InserirProduto('Pernil Temperado', 'Pernil suíno temperado congelado - 2kg', 59.90, 20, 'Promoção');
CALL sp_InserirProduto('Frutas Cristalizadas', 'Mix de frutas cristalizadas - 100g', 8.99, 50, 'Promoção');
CALL sp_InserirProduto('Nozes sem Casca', 'Nozes selecionadas para ceia de Natal - 150g', 16.99, 30, 'Promoção');
CALL sp_InserirProduto('Castanha-do-Pará', 'Castanhas selecionadas e sem casca - 150g', 14.99, 35, 'Promoção');
CALL sp_InserirProduto('Uva Passa', 'Uva passa preta sem sementes - 200g', 7.99, 60, 'Promoção');
CALL sp_InserirProduto('Ameixa Seca', 'Ameixa seca sem caroço - 200g', 9.99, 45, 'Promoção');
CALL sp_InserirProduto('Pêssego em Calda', 'Pêssegos em calda para sobremesas natalinas - 450g', 12.99, 40, 'Promoção');
CALL sp_InserirProduto('Leite Condensado', 'Leite condensado integral para sobremesas - 395g', 6.49, 80, 'Promoção');
CALL sp_InserirProduto('Creme de Leite', 'Creme de leite tradicional - 200g', 3.49, 100, 'Promoção');
CALL sp_InserirProduto('Panetone Trufado', 'Panetone Recheado trufado - 750g', 34.99, 25, 'Promoção');
CALL sp_InserirProduto('Caixa de Bombons Natalina', 'Caixa de bombons sortidos para presentear - 250g', 17.99, 50, 'Promoção');
CALL sp_InserirProduto('Biscoito Natalino', 'Biscoitos amanteigados decorados com motivos natalinos - 300g', 14.99, 30, 'Promoção');
<<<<<<< HEAD
CALL sp_InserirProduto('Espumante Sem Álcool', 'Bebida gaseificada saborizada para comemorações - 750ml', 18.99, 40, 'Promoção');
=======
CALL sp_InserirProduto('Espumante Sem Álcool', 'Bebida gaseificada saborizada para comemorações - 750ml', 18.99, 40, 'Promoção');

SELECT p.id_produto, p.nome AS Produto, c.nome AS CategoriaOficial 
FROM tbProduto p
INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria;
>>>>>>> 4664ec0d5f3be8898c1d94dfecce1fbd22c761b3
