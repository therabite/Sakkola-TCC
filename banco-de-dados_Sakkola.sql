create database sakkolaMarket;
use sakkolaMarket;

create table tbUser(
id_user int primary key auto_increment,
nome varchar(50) not null,
data_nasc date not null,
telefone int not null,
CPF int not null,
RG int not null,
email varchar(50) not null,
senha varchar(8) not null
);

create table tbFuncionario(
id_employed int primary key auto_increment,
is_admin boolean default false,
foreign key (id_employed) references tbUser(id_user) on delete cascade
);

create table tbClient(
id_client int primary key auto_increment,
foreign key (id_client) references tbUser(id_user)
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

create table tbAddress(
id_address int primary key auto_increment,
CEP int not null,
Logradouro varchar(100) not null,
Numero int not null,
Bairro varchar(50) not null,
Estado varchar(2) not null
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
foreign key (id_produto) references tbCategoria(id_categoria)
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


alter table tbClient add constraint fk__endereco_client foreign key (id_client) references tbAddress(id_address);
alter table tbAddress add constraint fk_entrega_endereco foreign key (id_address) references tbEntrega(id_entrega);
alter table tbentrega add constraint fk_pedido_entrega foreign key (id_entrega) references tbPedido(id_pedido);

alter table tbUser add column confirmacaoSenha varchar(8) not null;