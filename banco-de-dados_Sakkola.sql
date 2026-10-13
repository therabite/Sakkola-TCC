create database sakkolaMarket;
use sakkolaMarket;

create table tbUser(
id_user int primary key auto_increment,
nome varchar(50) not null,
data_nasc date not null,
telefone int not null,
CPF int not null,
email varchar(50) not null,
senha varchar(8) not null
);

create table tbAddress(
id_address int primary key auto_increment,
CEP int,
Logradouro varchar(100),
Numero int,
Bairro varchar(50),
Estado varchar(2)
);

create table tbClient(
id_client int primary key auto_increment,
foreign key (id_client) references tbUser(id_user)
);

create table tbFuncionario(
id_employed int primary key auto_increment,
is_admin boolean default false,
foreign key (id_employed) references tbUser(id_user) on delete cascade
);

create table tbPedido(
id_pedido int primary key auto_increment,
data_pedido date not null,
status_pedido varchar(8) not null,
valor_total decimal(6,2) not null,
foreign key (id_pedido) references tbClient(id_client),
foreign key (id_pedido) references tbUser(id_user)
);

create table tbPagamento(
id_pagamento int primary key auto_increment,
valor decimal(6,2) not null,
forma_pagamento varchar(50) not null,
foreign key (id_pagamento) references tbPedido(id_pedido)
);

create table tbCarrinho(
id_carrinho int primary key auto_increment,
valor_total decimal(6,2) not null,
foreign key (id_carrinho) references tbClient(id_client), 
foreign key (id_carrinho) references tbUser(id_user) 
);

create table tbEntrega(
id_entrega int primary key auto_increment,
status_entrega varchar(8) not null
);

create table tbProduto(
id_produto int primary key auto_increment,
nome varchar(50) not null,
descricao varchar(75) not null,
preco decimal(6,2) not null,
estoque int not null,
id_categoria INT NOT NULL,
FOREIGN KEY (id_categoria) REFERENCES tbCategoria(id_categoria)
);

create table tbItemPedido(
fk_pedido_id int not null,
fk_produto_id int not null,
quantidade int not null,
preco_unidade decimal(6,2) not null,
primary key (fk_pedido_id, fk_produto_id),
foreign key (fk_pedido_id) references tbPedido(id_pedido) on delete cascade,
foreign key (fk_produto_id) references tbProduto(id_produto)
);

create table tbCategoria(
id_categoria int primary key auto_increment,
nome varchar(50) not null
);

ALTER TABLE tbClient ADD CONSTRAINT fk_client_user FOREIGN KEY (id_client) REFERENCES tbUser(id_user);

ALTER TABLE tbEntrega ADD COLUMN id_address INT, ADD COLUMN id_pedido INT;
ALTER TABLE tbEntrega ADD CONSTRAINT fk_entrega_address FOREIGN KEY (id_address) REFERENCES tbAddress(id_address);

ALTER TABLE tbEntrega ADD CONSTRAINT fk_entrega_pedido FOREIGN KEY (id_pedido) REFERENCES tbPedido(id_pedido);

ALTER TABLE tbUser MODIFY telefone VARCHAR(15);
ALTER TABLE tbUser MODIFY CPF VARCHAR(14);

ALTER TABLE tbUser ADD CONSTRAINT unique_email UNIQUE (email);

select * from tbUser;
select * from tbAddress;
select * from tbcategoria;

INSERT IGNORE INTO tbCategoria (nome) VALUES 
('Açougue'),
('Hortifrúti'),
('Padaria'),
('Higiene'),
('Limpeza'),
('Frios e Laticínios'),
('Mercearia');

DELIMITER //
CREATE PROCEDURE sp_InserirProduto (
    IN p_nome_produto VARCHAR(50),
    IN p_descricao VARCHAR(75),
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

SELECT p.id_produto, p.nome, c.nome AS Categoria
FROM tbProduto p
INNER JOIN tbCategoria c ON p.id_categoria = c.id_categoria
WHERE c.nome LIKE '%Frios%';

-- =============================================
-- 1. AÇOUGUE
-- =============================================
CALL sp_InserirProduto('Picanha Bovina', 'Picanha Bovina - Peça embalada a vácuo ~1kg', 79.90, 25, 'Açougue');
CALL sp_InserirProduto('Contrafilé Fatiado', 'Contrafilé Fatiado - Bifes frescos 500g', 32.50, 30, 'Açougue');
CALL sp_InserirProduto('Alcatra Bovina', 'Alcatra Bovina - Peça ou fatiada ~1kg', 49.90, 20, 'Açougue');
CALL sp_InserirProduto('Acém Moído', 'Acém Moído - Carne moída de primeira 500g', 18.90, 40, 'Açougue');
CALL sp_InserirProduto('Costela Bovina', 'Costela Bovina - Ideal para churrasco ~1.5kg', 28.90, 15, 'Açougue');
CALL sp_InserirProduto('Filet Mignon', 'Filet Mignon - Peça inteira ~1.2kg', 89.90, 12, 'Açougue');
CALL sp_InserirProduto('Peito de Frango', 'Peito de Frango - Filé sem osso 1kg', 21.90, 50, 'Açougue');
CALL sp_InserirProduto('Coxa e Sobrecoxa de Frango', 'Coxa e Sobrecoxa de Frango - Bandeja 1kg', 14.90, 45, 'Açougue');
CALL sp_InserirProduto('Bisteca Suína', 'Bisteca Suína - Bandeja fatiada 800g', 19.90, 35, 'Açougue');
CALL sp_InserirProduto('Linguiça Toscana', 'Linguiça Toscana - Para churrasco 1kg', 22.90, 40, 'Açougue');

-- =============================================
-- 2. HORTIFRÚTI
-- =============================================
CALL sp_InserirProduto('Banana Prata', 'Banana Prata - Penca ~1kg', 6.99, 50, 'Hortifrúti');
CALL sp_InserirProduto('Maçã Fuji', 'Maçã Fuji - Bandeja ou kg', 11.90, 60, 'Hortifrúti');
CALL sp_InserirProduto('Tomate Italiano', 'Tomate Italiano - Kg', 9.80, 45, 'Hortifrúti');
CALL sp_InserirProduto('Cebola Nacional', 'Cebola Nacional - Saco 1kg', 7.50, 55, 'Hortifrúti');
CALL sp_InserirProduto('Batata Monalisa', 'Batata Monalisa - Kg', 8.90, 50, 'Hortifrúti');
CALL sp_InserirProduto('Alface Crespa', 'Alface Crespa - Pé fresquinho', 3.50, 30, 'Hortifrúti');
CALL sp_InserirProduto('Cenoura', 'Cenoura - Bandeja 500g ou kg', 6.90, 40, 'Hortifrúti');
CALL sp_InserirProduto('Laranja Pera', 'Laranja Pera - Saco 2kg', 8.50, 50, 'Hortifrúti');
CALL sp_InserirProduto('Mamão Papaia', 'Mamão Papaia - Unidade', 6.50, 35, 'Hortifrúti');
CALL sp_InserirProduto('Alho Roxo', 'Alho Roxo - Pacote 200g', 11.99, 45, 'Hortifrúti');

-- =============================================
-- 3. PADARIA
-- =============================================
CALL sp_InserirProduto('Pão de Queijo', 'Pão de Queijo - Bandeja com 10 unidades', 12.90, 30, 'Padaria');
CALL sp_InserirProduto('Pão de Forma Tradicional', 'Pão de Forma Tradicional - Pacote 450g', 7.50, 45, 'Padaria');
CALL sp_InserirProduto('Croissant de Presunto e Queijo', 'Croissant de Presunto e Queijo - Unidade', 8.90, 20, 'Padaria');
CALL sp_InserirProduto('Bolo de Cenoura com Cobertura', 'Bolo de Cenoura com Cobertura - Unidade ~400g', 18.50, 15, 'Padaria');
CALL sp_InserirProduto('Broa de Milho', 'Broa de Milho - Unidade', 4.50, 25, 'Padaria');
CALL sp_InserirProduto('Baguete Recheada', 'Baguete Recheada - Unidade', 14.90, 18, 'Padaria');
CALL sp_InserirProduto('Sonho de Creme', 'Sonho de Creme - Unidade', 5.50, 30, 'Padaria');
CALL sp_InserirProduto('Torrada Tradicional', 'Torrada Tradicional - Embalagem 140g', 6.90, 40, 'Padaria');
CALL sp_InserirProduto('Bisnaguinha Integral', 'Bisnaguinha Integral - Pacote 300g', 8.20, 35, 'Padaria');

-- =============================================
-- 4. HIGIENE
-- =============================================
CALL sp_InserirProduto('Shampoo Hidratante', 'Shampoo Hidratante - Frasco 350ml', 16.90, 50, 'Higiene');
CALL sp_InserirProduto('Condicionador Hidratante', 'Condicionador Hidratante - Frasco 350ml', 18.90, 45, 'Higiene');
CALL sp_InserirProduto('Creme Dental Proteção Cáries', 'Creme Dental Proteção Cáries - Tubo 90g', 4.90, 80, 'Higiene');
CALL sp_InserirProduto('Escova de Dentes Macia', 'Escova de Dentes Macia - Unidade', 9.90, 60, 'Higiene');
CALL sp_InserirProduto('Desodorante Aerosol', 'Desodorante Aerosol - Frasco 150ml', 15.90, 70, 'Higiene');
CALL sp_InserirProduto('Papel Higiênico Folha Dupla', 'Papel Higiênico Folha Dupla - Pacote com 12 rolos', 22.90, 40, 'Higiene');
CALL sp_InserirProduto('Fio Dental', 'Fio Dental - Rolo 50m', 8.50, 50, 'Higiene');
CALL sp_InserirProduto('Protetor Solar FPS 50', 'Protetor Solar FPS 50 - Frasco 200ml', 49.90, 25, 'Higiene');
CALL sp_InserirProduto('Creme para Pentear', 'Creme para Pentear - Frasco 300ml', 14.90, 35, 'Higiene');

-- =============================================
-- 5. LIMPEZA
-- =============================================
CALL sp_InserirProduto('Sabão em Pó Multiação', 'Sabão em Pó Multiação - Caixa 1kg', 14.90, 60, 'Limpeza');
CALL sp_InserirProduto('Amaciante de Roupas Concentrado', 'Amaciante de Roupas Concentrado - Frasco 1L', 17.50, 40, 'Limpeza');
CALL sp_InserirProduto('Desinfetante Pinho', 'Desinfetante Pinho - Frasco 1L', 8.90, 50, 'Limpeza');
CALL sp_InserirProduto('Limpador Multiuso', 'Limpador Multiuso - Frasco 500ml', 5.90, 75, 'Limpeza');
CALL sp_InserirProduto('Água Sanitária', 'Água Sanitária - Garrafa 2L', 7.90, 65, 'Limpeza');
CALL sp_InserirProduto('Esponja Multiuso', 'Esponja Multiuso - Pacote com 4 unidades', 4.90, 100, 'Limpeza');
CALL sp_InserirProduto('Lustra-Móveis', 'Lustra-Móveis - Frasco 200ml', 11.90, 30, 'Limpeza');
CALL sp_InserirProduto('Saco de Lixo 50 Litros', 'Saco de Lixo 50 Litros - Rolo com 10 unidades', 13.90, 45, 'Limpeza');
CALL sp_InserirProduto('Lisoform / Sanificante', 'Lisoform / Sanificante - Frasco 500ml', 15.90, 35, 'Limpeza');

-- =============================================
-- 6. FRIOS E LATICÍNIOS
-- =============================================
CALL sp_InserirProduto('Presunto Cozido Fatiado', 'Presunto Cozido Fatiado - Bandeja 200g', 9.90, 50, 'Frios e Laticínios');
CALL sp_InserirProduto('Requeijão Cremoso', 'Requeijão Cremoso - Copo 200g', 8.50, 60, 'Frios e Laticínios');
CALL sp_InserirProduto('Manteiga com Sal', 'Manteiga com Sal - Pote 200g', 11.90, 40, 'Frios e Laticínios');
CALL sp_InserirProduto('Iogurte Natural Integral', 'Iogurte Natural Integral - Pote 170g', 3.90, 70, 'Frios e Laticínios');
CALL sp_InserirProduto('Leite Integral UHT', 'Leite Integral UHT - Caixa 1L', 5.20, 120, 'Frios e Laticínios');
CALL sp_InserirProduto('Queijo Prato Fatiado', 'Queijo Prato Fatiado - Bandeja 200g', 12.90, 45, 'Frios e Laticínios');
CALL sp_InserirProduto('Peito de Peru Defumado', 'Peito de Peru Defumado - Bandeja 150g', 13.50, 35, 'Frios e Laticínios');
CALL sp_InserirProduto('Creme de Leite', 'Creme de Leite - Caixinha 200g', 3.80, 90, 'Frios e Laticínios');
CALL sp_InserirProduto('Cream Cheese Tradicional', 'Cream Cheese Tradicional - Pote 150g', 10.90, 30, 'Frios e Laticínios');

-- =============================================
-- 7. MERCEARIA
-- =============================================
CALL sp_InserirProduto('Feijão Carioca', 'Feijão Carioca - Pacote 1kg', 8.90, 80, 'Mercearia');
CALL sp_InserirProduto('Óleo de Soja Refinado', 'Óleo de Soja Refinado - Garrafa 900ml', 6.90, 100, 'Mercearia');
CALL sp_InserirProduto('Açúcar Refinado', 'Açúcar Refinado - Pacote 1kg', 4.80, 90, 'Mercearia');
CALL sp_InserirProduto('Café Torrado e Moído', 'Café Torrado e Moído - Almofada/Vácuo 500g', 18.90, 65, 'Mercearia');
CALL sp_InserirProduto('Macarrão Spaghetini', 'Macarrão Spaghetini - Pacote 500g', 4.50, 85, 'Mercearia');
CALL sp_InserirProduto('Molho de Tomate Tradicional', 'Molho de Tomate Tradicional - Sachê 300g', 2.90, 110, 'Mercearia');
CALL sp_InserirProduto('Farinha de Trigo Tradicional', 'Farinha de Trigo Tradicional - Pacote 1kg', 5.50, 70, 'Mercearia');
CALL sp_InserirProduto('Sal Refinado', 'Sal Refinado - Pacote 1kg', 2.50, 100, 'Mercearia');
CALL sp_InserirProduto('Biscoito Recheado de Chocolate', 'Biscoito Recheado de Chocolate - Pacote 130g', 3.50, 95, 'Mercearia');