# 🛒 Sakkola — Supermercado

> Trabalho de Conclusão de Curso (TCC) apresentado no curso Técnico em Desenvolvimento de Sistemas da **Etec Professor Basilides de Godoy**.

---

## 📌 Sobre o Projeto

O **Sakkola** é uma plataforma de e-commerce focada em facilitar as compras de supermercado online, oferecendo uma experiência prática para os clientes e um painel de gestão eficiente para a equipe do estabelecimento.

O sistema conta com fluxos dinâmicos e controle de permissões por níveis de acesso:
- **Cliente:** Navegação por catálogo, pesquisa e filtragem de produtos por categoria, gerenciamento interativo do carrinho de compras e simulação de formas de pagamento (Cartão e PIX).
- **Colaborador / Administrador:** Painel de controle restrito (com acesso via e-mail corporativo `@admin.com`) para gestão de pedidos em tempo real, atualização de status de entrega, cadastro de novos produtos com upload de imagens e gerenciamento completo do sistema.
  
---

## 📂 Estrutura do Projeto

O projeto está organizado em pastas estruturadas no padrão ASP.NET Core MVC, facilitando a manutenção, escalabilidade e separação de responsabilidades:

```text
Sakkola/
│
├── Controllers/                   # Controladores responsáveis pelo fluxo de requisições web
├── Data/                          # Configuração do contexto do banco de dados (AppDbContext)
├── Libraries/                     # Filtros de autorização personalizados e utilitários
├── Models/                        # Classes de domínio e ViewModels de transporte de dados
├── Repository/                    # Camada de persistência e contratos de acesso ao banco
├── Sessao/                        # Gerenciamento de dados de sessão e carrinho de compras
├── Views/                         # Páginas, componentes e interfaces visuais (Razor)
├── wwwroot/                       # Arquivos estáticos (CSS customizado, JS, Plugins, Imagens e Ícones)
├── .gitignore                     # Arquivos ignorados pelo controle de versão Git
├── appsettings.json               # Configurações de conexão com o banco e parâmetros da aplicação
├── Program.cs                     # Inicialização, injeção de dependências, middleware de sessão e rotas
└── README.md                      # Documentação oficial do projeto
```

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem**: [C#]

* **Framework**: [ASP.NET Core MVC (.NET 10 / EF Core)]

* **Banco de Dados**: [MySQL]

* **Front-end**: [HTML5, CSS3, JavaScript, Bootstrap 5, jQuery e Bootstrap Icons]

* **Controle de Versão**: [GitBash e GitHub]
  
---

## 🚀 Funcionalidades Principais

* [x] Arquitetura em Camadas e Áreas (MVC)

* [x] Sistema de Autenticação e Sessão para Clientes e Colaboradores

* [x] Painel Administrativo de Gestão de Pedidos com alteração de status em tempo real

* [x] Cadastro, Edição e Exclusão de Produtos com upload de imagens dedicado

* [x] Carrinho de compras interativo com cálculo dinâmico e integração com meios de pagamento (PIX com QR Code dinâmico e Cartão com parcelamento)

* [x] Restrição de rotas e filtros de autorização para administradores

---

## 👥 Integrantes do Grupo

* **[Ana Júlia Marques do Prado]** — *Gerente de Projeto* — [GitHub](https://github.com/Anajuxgit23)
* **[Débora Lopes de Souza]** — *Dev FullStack* — [GitHub](https://github.com/therabite)
* **[Guilherme Lineker Freitas Lima]** — *Dev Front-End* — [GitHub](https://github.com/Lineker280)
* **[Leonardo de Oliveira Alves Bezerra]** — *Dev Back-End* — [GitHub](https://github.com/LeonardoOAB)
* **[Thiago Ferreira Silva]** — *Analista de dados* — [GitHub](https://github.com/ThiagoFerreiraSil)
---

## 🏫 Instituição

**Etec Professor Basilides de Godoy**  
Curso Técnico em Desenvolvimento de Sistemas  
São Paulo — SP

*© 2026 — Sakkola / Todos os direitos reservados.*
