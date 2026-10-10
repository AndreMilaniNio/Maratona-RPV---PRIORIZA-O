using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farol.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    acao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    entidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: true),
                    valores_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    valores_novos = table.Column<string>(type: "jsonb", nullable: true),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    endereco_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "classes_cliente",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    essencial = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_classes_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "criterios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    agregacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    multipla_escolha = table.Column<bool>(type: "boolean", nullable: false),
                    regra_aplicacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    alterado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alterado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_criterios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feriados",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feriados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "municipios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    uf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    codigo_ibge = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    prefixo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    raio_km = table.Column<double>(type: "double precision", nullable: true),
                    cep_unico = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_municipios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prioridades",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    critica = table.Column<bool>(type: "boolean", nullable: false),
                    prazo_triagem_min = table.Column<int>(type: "integer", nullable: false),
                    prazo_despacho_min = table.Column<int>(type: "integer", nullable: false),
                    prazo_inicio_min = table.Column<int>(type: "integer", nullable: false),
                    prazo_restabelecimento_min = table.Column<int>(type: "integer", nullable: true),
                    prazo_conclusao_min = table.Column<int>(type: "integer", nullable: false),
                    unidade_prazo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    calendario = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    considera_feriados = table.Column<bool>(type: "boolean", nullable: false),
                    tratamento_critico = table.Column<string>(type: "text", nullable: true),
                    escalonamento = table.Column<string>(type: "text", nullable: true),
                    vigencia_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vigencia_fim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    demonstrativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prioridades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qualificacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualificacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recursos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recursos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_ocorrencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo_manutencao_sugerido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipos_ocorrencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "opcoes_criterio",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    criterio_id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    rotulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    representa_desconhecido = table.Column<bool>(type: "boolean", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opcoes_criterio", x => x.id);
                    table.ForeignKey(
                        name: "fk_opcoes_criterio_criterios_criterio_id",
                        column: x => x.criterio_id,
                        principalTable: "criterios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ceps",
                columns: table => new
                {
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    logradouro = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ceps", x => x.cep);
                    table.ForeignKey(
                        name: "fk_ceps_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "criterio_municipios",
                columns: table => new
                {
                    criterio_id = table.Column<int>(type: "integer", nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_criterio_municipios", x => new { x.criterio_id, x.municipio_id });
                    table.ForeignKey(
                        name: "fk_criterio_municipios_criterios_criterio_id",
                        column: x => x.criterio_id,
                        principalTable: "criterios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_criterio_municipios_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    municipio_base_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    capacidade = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    localizacao_atualizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    origem_localizacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    demonstrativa = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipes", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipes_municipios_municipio_base_id",
                        column: x => x.municipio_base_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "localidades",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_localidades", x => x.id);
                    table.ForeignKey(
                        name: "fk_localidades_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "numeracao_sequencias",
                columns: table => new
                {
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ultimo = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_numeracao_sequencias", x => new { x.municipio_id, x.ano, x.tipo });
                    table.ForeignKey(
                        name: "fk_numeracao_sequencias_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subestacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    local = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subestacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_subestacoes_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "versoes_pontuacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    municipio_id = table.Column<int>(type: "integer", nullable: true),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    demonstrativa = table.Column<bool>(type: "boolean", nullable: false),
                    vigencia_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    aplicacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    autor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    autor_nome = table.Column<string>(type: "text", nullable: true),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    alterada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    publicada_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    publicada_por_nome = table.Column<string>(type: "text", nullable: true),
                    publicada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    aprovada_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovada_por_nome = table.Column<string>(type: "text", nullable: true),
                    aprovada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reclassificacao_aplicada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    restaurada_de_id = table.Column<int>(type: "integer", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_versoes_pontuacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_versoes_pontuacao_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfil_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_perfil_claims_perfis_role_id",
                        column: x => x.role_id,
                        principalTable: "perfis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regras_precedencia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    condicoes = table.Column<string>(type: "jsonb", nullable: false),
                    nivel_precedencia = table.Column<int>(type: "integer", nullable: false),
                    prioridade_minima_id = table.Column<int>(type: "integer", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    demonstrativa = table.Column<bool>(type: "boolean", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    aprovada_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    alterada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regras_precedencia", x => x.id);
                    table.ForeignKey(
                        name: "fk_regras_precedencia_prioridades_prioridade_minima_id",
                        column: x => x.prioridade_minima_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_ocorrencia_qualificacoes",
                columns: table => new
                {
                    tipo_ocorrencia_id = table.Column<int>(type: "integer", nullable: false),
                    qualificacao_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_ocorrencia_qualificacoes", x => new { x.tipo_ocorrencia_id, x.qualificacao_id });
                    table.ForeignKey(
                        name: "fk_tipo_ocorrencia_qualificacoes_qualificacoes_qualificacao_id",
                        column: x => x.qualificacao_id,
                        principalTable: "qualificacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_ocorrencia_qualificacoes_tipos_ocorrencia_tipo_ocorren~",
                        column: x => x.tipo_ocorrencia_id,
                        principalTable: "tipos_ocorrencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipe_municipios",
                columns: table => new
                {
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipe_municipios", x => new { x.equipe_id, x.municipio_id });
                    table.ForeignKey(
                        name: "fk_equipe_municipios_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipe_municipios_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipe_qualificacoes",
                columns: table => new
                {
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    qualificacao_id = table.Column<int>(type: "integer", nullable: false),
                    valida_ate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipe_qualificacoes", x => new { x.equipe_id, x.qualificacao_id });
                    table.ForeignKey(
                        name: "fk_equipe_qualificacoes_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipe_qualificacoes_qualificacoes_qualificacao_id",
                        column: x => x.qualificacao_id,
                        principalTable: "qualificacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipe_recursos",
                columns: table => new
                {
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    recurso_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipe_recursos", x => new { x.equipe_id, x.recurso_id });
                    table.ForeignKey(
                        name: "fk_equipe_recursos_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipe_recursos_recursos_recurso_id",
                        column: x => x.recurso_id,
                        principalTable: "recursos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "integrantes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    matricula = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    funcao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integrantes", x => x.id);
                    table.ForeignKey(
                        name: "fk_integrantes_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "localizacoes_equipe",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    origem = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    registrada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrada_por_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_localizacoes_equipe", x => x.id);
                    table.ForeignKey(
                        name: "fk_localizacoes_equipe_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    municipio_preferido_id = table.Column<int>(type: "integer", nullable: true),
                    equipe_id = table.Column<int>(type: "integer", nullable: true),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuarios_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_usuarios_municipios_municipio_preferido_id",
                        column: x => x.municipio_preferido_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "conjuntos_eletricos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subestacao_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conjuntos_eletricos", x => x.id);
                    table.ForeignKey(
                        name: "fk_conjuntos_eletricos_subestacoes_subestacao_id",
                        column: x => x.subestacao_id,
                        principalTable: "subestacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "faixas_prioridade",
                columns: table => new
                {
                    versao_pontuacao_id = table.Column<int>(type: "integer", nullable: false),
                    prioridade_id = table.Column<int>(type: "integer", nullable: false),
                    minimo = table.Column<int>(type: "integer", nullable: false),
                    maximo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faixas_prioridade", x => new { x.versao_pontuacao_id, x.prioridade_id });
                    table.ForeignKey(
                        name: "fk_faixas_prioridade_prioridades_prioridade_id",
                        column: x => x.prioridade_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_faixas_prioridade_versoes_pontuacao_versao_pontuacao_id",
                        column: x => x.versao_pontuacao_id,
                        principalTable: "versoes_pontuacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pontos_opcao",
                columns: table => new
                {
                    versao_pontuacao_id = table.Column<int>(type: "integer", nullable: false),
                    opcao_id = table.Column<int>(type: "integer", nullable: false),
                    pontos = table.Column<int>(type: "integer", nullable: true),
                    requer_confirmacao = table.Column<bool>(type: "boolean", nullable: false),
                    origem = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    alterado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alterado_por_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    alterado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pontos_opcao", x => new { x.versao_pontuacao_id, x.opcao_id });
                    table.ForeignKey(
                        name: "fk_pontos_opcao_opcoes_criterio_opcao_id",
                        column: x => x.opcao_id,
                        principalTable: "opcoes_criterio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pontos_opcao_versoes_pontuacao_versao_pontuacao_id",
                        column: x => x.versao_pontuacao_id,
                        principalTable: "versoes_pontuacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "versao_criterios",
                columns: table => new
                {
                    versao_pontuacao_id = table.Column<int>(type: "integer", nullable: false),
                    criterio_id = table.Column<int>(type: "integer", nullable: false),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_versao_criterios", x => new { x.versao_pontuacao_id, x.criterio_id });
                    table.ForeignKey(
                        name: "fk_versao_criterios_criterios_criterio_id",
                        column: x => x.criterio_id,
                        principalTable: "criterios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_versao_criterios_versoes_pontuacao_versao_pontuacao_id",
                        column: x => x.versao_pontuacao_id,
                        principalTable: "versoes_pontuacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_claims_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_usuario_logins_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_municipios",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_municipios", x => new { x.usuario_id, x.municipio_id });
                    table.ForeignKey(
                        name: "fk_usuario_municipios_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_municipios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_perfis",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_perfis", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_usuario_perfis_perfis_role_id",
                        column: x => x.role_id,
                        principalTable: "perfis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_perfis_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_usuario_tokens_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transformadores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_completo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_localidade = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    numero_local = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    localidade_id = table.Column<int>(type: "integer", nullable: true),
                    subestacao_id = table.Column<int>(type: "integer", nullable: true),
                    conjunto_id = table.Column<int>(type: "integer", nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transformadores", x => x.id);
                    table.ForeignKey(
                        name: "fk_transformadores_conjuntos_eletricos_conjunto_id",
                        column: x => x.conjunto_id,
                        principalTable: "conjuntos_eletricos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transformadores_localidades_localidade_id",
                        column: x => x.localidade_id,
                        principalTable: "localidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transformadores_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transformadores_subestacoes_subestacao_id",
                        column: x => x.subestacao_id,
                        principalTable: "subestacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unidades_consumidoras",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cliente_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    classe_cliente_id = table.Column<int>(type: "integer", nullable: false),
                    situacao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    logradouro = table.Column<string>(type: "text", nullable: true),
                    numero_imovel = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    transformador_id = table.Column<int>(type: "integer", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    medidor = table.Column<string>(type: "text", nullable: true),
                    fases = table.Column<string>(type: "text", nullable: true),
                    demonstrativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unidades_consumidoras", x => x.id);
                    table.ForeignKey(
                        name: "fk_unidades_consumidoras_classes_cliente_classe_cliente_id",
                        column: x => x.classe_cliente_id,
                        principalTable: "classes_cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_unidades_consumidoras_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_unidades_consumidoras_transformadores_transformador_id",
                        column: x => x.transformador_id,
                        principalTable: "transformadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "anexos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_conteudo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanho = table.Column<long>(type: "bigint", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    enviado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enviado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anexos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comentarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comentarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "despachos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipe_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    designado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    designado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    aceito_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    iniciado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    encerrado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_encerramento = table.Column<string>(type: "text", nullable: true),
                    equipe_latitude = table.Column<double>(type: "double precision", nullable: true),
                    equipe_longitude = table.Column<double>(type: "double precision", nullable: true),
                    equipe_localizacao_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    distancia_km = table.Column<double>(type: "double precision", nullable: true),
                    tempo_estimado_min = table.Column<double>(type: "double precision", nullable: true),
                    origem_estimativa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    apoio_intermunicipal = table.Column<bool>(type: "boolean", nullable: false),
                    excecao = table.Column<bool>(type: "boolean", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    observacao_conclusao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_despachos", x => x.id);
                    table.ForeignKey(
                        name: "fk_despachos_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_despachos_usuarios_designado_por_id",
                        column: x => x.designado_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historico_os",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status_anterior = table.Column<string>(type: "text", nullable: true),
                    status_novo = table.Column<string>(type: "text", nullable: true),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_os", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_manutencao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    tipo_ocorrencia_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    aberta_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    atualizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    encerrada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero_endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bairro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    endereco_completo = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    ponto_referencia = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    observacoes_localizacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    origem_coordenada = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    precisao_metros = table.Column<double>(type: "double precision", nullable: true),
                    coordenada_registrada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    localizacao_pendente = table.Column<bool>(type: "boolean", nullable: false),
                    subestacao_id = table.Column<int>(type: "integer", nullable: true),
                    conjunto_id = table.Column<int>(type: "integer", nullable: true),
                    transformador_numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transformador_localidade = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    transformador_local = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    transformador_id = table.Column<int>(type: "integer", nullable: true),
                    origem_rede = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    trecho = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    trecho_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    equipamento_descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    identificador_equipamento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    chave_eletrica = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pessoas_afetadas = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantidade_pessoas = table.Column<int>(type: "integer", nullable: true),
                    ucs_afetadas = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantidade_ucs = table.Column<int>(type: "integer", nullable: true),
                    servico_essencial = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    situacao_cliente = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    condicao_fornecimento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    redundancia = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fonte_reserva = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    equipe_especializada = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    equipamento_afetado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    abrangencia = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nivel_rede = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantidade_equipamentos = table.Column<int>(type: "integer", nullable: true),
                    duracao_estimada_min = table.Column<int>(type: "integer", nullable: true),
                    data_limite = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    recursos_especiais = table.Column<bool>(type: "boolean", nullable: false),
                    versao_pontuacao_id = table.Column<int>(type: "integer", nullable: false),
                    resultado_atual_id = table.Column<long>(type: "bigint", nullable: true),
                    prioridade_calculada_id = table.Column<int>(type: "integer", nullable: true),
                    prioridade_id = table.Column<int>(type: "integer", nullable: true),
                    pontuacao = table.Column<int>(type: "integer", nullable: false),
                    nivel_precedencia = table.Column<int>(type: "integer", nullable: false),
                    prioridade_manual_id = table.Column<int>(type: "integer", nullable: true),
                    justificativa_manual = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    prazo_triagem = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prazo_despacho = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prazo_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prazo_restabelecimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prazo_conclusao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ordens_servico", x => x.id);
                    table.ForeignKey(
                        name: "fk_ordens_servico_conjuntos_eletricos_conjunto_id",
                        column: x => x.conjunto_id,
                        principalTable: "conjuntos_eletricos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_prioridades_prioridade_calculada_id",
                        column: x => x.prioridade_calculada_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_prioridades_prioridade_id",
                        column: x => x.prioridade_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_prioridades_prioridade_manual_id",
                        column: x => x.prioridade_manual_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_subestacoes_subestacao_id",
                        column: x => x.subestacao_id,
                        principalTable: "subestacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_tipos_ocorrencia_tipo_ocorrencia_id",
                        column: x => x.tipo_ocorrencia_id,
                        principalTable: "tipos_ocorrencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ordens_servico_transformadores_transformador_id",
                        column: x => x.transformador_id,
                        principalTable: "transformadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_ordens_servico_versoes_pontuacao_versao_pontuacao_id",
                        column: x => x.versao_pontuacao_id,
                        principalTable: "versoes_pontuacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "os_classes_cliente",
                columns: table => new
                {
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    classe_cliente_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_os_classes_cliente", x => new { x.ordem_servico_id, x.classe_cliente_id });
                    table.ForeignKey(
                        name: "fk_os_classes_cliente_classes_cliente_classe_cliente_id",
                        column: x => x.classe_cliente_id,
                        principalTable: "classes_cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_os_classes_cliente_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_condicoes_seguranca",
                columns: table => new
                {
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_os_condicoes_seguranca", x => new { x.ordem_servico_id, x.codigo });
                    table.ForeignKey(
                        name: "fk_os_condicoes_seguranca_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_recursos",
                columns: table => new
                {
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_os_recursos", x => new { x.ordem_servico_id, x.recurso_id });
                    table.ForeignKey(
                        name: "fk_os_recursos_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_os_recursos_recursos_recurso_id",
                        column: x => x.recurso_id,
                        principalTable: "recursos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "os_respostas_criterio",
                columns: table => new
                {
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criterio_id = table.Column<int>(type: "integer", nullable: false),
                    opcao_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_os_respostas_criterio", x => new { x.ordem_servico_id, x.criterio_id, x.opcao_id });
                    table.ForeignKey(
                        name: "fk_os_respostas_criterio_criterios_criterio_id",
                        column: x => x.criterio_id,
                        principalTable: "criterios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_os_respostas_criterio_opcoes_criterio_opcao_id",
                        column: x => x.opcao_id,
                        principalTable: "opcoes_criterio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_os_respostas_criterio_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resultados_prioridade",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_pontuacao_id = table.Column<int>(type: "integer", nullable: false),
                    calculado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    pontuacao = table.Column<int>(type: "integer", nullable: false),
                    nivel_precedencia = table.Column<int>(type: "integer", nullable: false),
                    prioridade_calculada_id = table.Column<int>(type: "integer", nullable: false),
                    prioridade_id = table.Column<int>(type: "integer", nullable: false),
                    regras_aplicadas = table.Column<string>(type: "jsonb", nullable: false),
                    motivo_principal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resultados_prioridade", x => x.id);
                    table.ForeignKey(
                        name: "fk_resultados_prioridade_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resultados_prioridade_prioridades_prioridade_calculada_id",
                        column: x => x.prioridade_calculada_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resultados_prioridade_prioridades_prioridade_id",
                        column: x => x.prioridade_id,
                        principalTable: "prioridades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resultados_prioridade_versoes_pontuacao_versao_pontuacao_id",
                        column: x => x.versao_pontuacao_id,
                        principalTable: "versoes_pontuacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    chave_idempotencia = table.Column<Guid>(type: "uuid", nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    canal = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    origem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    protocolo_externo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    registrada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrada_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    uc_nao_informada = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_uc_nao_informada = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    dados_informados = table.Column<string>(type: "jsonb", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitacoes_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacoes_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacoes_usuarios_registrada_por_id",
                        column: x => x.registrada_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resultados_prioridade_itens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resultado_prioridade_id = table.Column<long>(type: "bigint", nullable: false),
                    criterio_codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    criterio_nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    opcoes_codigos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    opcoes_rotulos = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    pontos = table.Column<int>(type: "integer", nullable: false),
                    sem_pontos_definidos = table.Column<bool>(type: "boolean", nullable: false),
                    requer_confirmacao = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resultados_prioridade_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_resultados_prioridade_itens_resultados_prioridade_resultado~",
                        column: x => x.resultado_prioridade_id,
                        principalTable: "resultados_prioridade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacao_ucs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unidade_consumidora_id = table.Column<int>(type: "integer", nullable: true),
                    validada_no_cadastro = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitacao_ucs", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitacao_ucs_solicitacoes_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "solicitacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_solicitacao_ucs_unidades_consumidoras_unidade_consumidora_id",
                        column: x => x.unidade_consumidora_id,
                        principalTable: "unidades_consumidoras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "vinculos_solicitacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vinculado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vinculado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vinculos_solicitacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_vinculos_solicitacao_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vinculos_solicitacao_ordens_servico_ordem_servico_origem_id",
                        column: x => x.ordem_servico_origem_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vinculos_solicitacao_solicitacoes_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "solicitacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anexos_ordem_servico_id",
                table: "anexos",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_entidade_entidade_id",
                table: "auditoria",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_ocorrido_em",
                table: "auditoria",
                column: "ocorrido_em");

            migrationBuilder.CreateIndex(
                name: "ix_ceps_municipio_id",
                table: "ceps",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_classes_cliente_codigo",
                table: "classes_cliente",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comentarios_ordem_servico_id",
                table: "comentarios",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_conjuntos_eletricos_subestacao_id_numero",
                table: "conjuntos_eletricos",
                columns: new[] { "subestacao_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_criterio_municipios_municipio_id",
                table: "criterio_municipios",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_criterios_codigo",
                table: "criterios",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_despachos_designado_por_id",
                table: "despachos",
                column: "designado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_despachos_equipe_id_ativo",
                table: "despachos",
                columns: new[] { "equipe_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "ux_despachos_os_ativo",
                table: "despachos",
                column: "ordem_servico_id",
                unique: true,
                filter: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_equipe_municipios_municipio_id",
                table: "equipe_municipios",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipe_qualificacoes_qualificacao_id",
                table: "equipe_qualificacoes",
                column: "qualificacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipe_recursos_recurso_id",
                table: "equipe_recursos",
                column: "recurso_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipes_codigo",
                table: "equipes",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equipes_municipio_base_id",
                table: "equipes",
                column: "municipio_base_id");

            migrationBuilder.CreateIndex(
                name: "ix_faixas_prioridade_prioridade_id",
                table: "faixas_prioridade",
                column: "prioridade_id");

            migrationBuilder.CreateIndex(
                name: "ix_feriados_data_municipio_id",
                table: "feriados",
                columns: new[] { "data", "municipio_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historico_os_ordem_servico_id_ocorrido_em",
                table: "historico_os",
                columns: new[] { "ordem_servico_id", "ocorrido_em" });

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_equipe_id",
                table: "integrantes",
                column: "equipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_localidades_codigo",
                table: "localidades",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_localidades_municipio_id",
                table: "localidades",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_localizacoes_equipe_equipe_id_registrada_em",
                table: "localizacoes_equipe",
                columns: new[] { "equipe_id", "registrada_em" });

            migrationBuilder.CreateIndex(
                name: "ix_municipios_prefixo",
                table: "municipios",
                column: "prefixo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_opcoes_criterio_criterio_id_codigo",
                table: "opcoes_criterio",
                columns: new[] { "criterio_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_aberta_em",
                table: "ordens_servico",
                column: "aberta_em");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_conjunto_id",
                table: "ordens_servico",
                column: "conjunto_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_municipio_id_status",
                table: "ordens_servico",
                columns: new[] { "municipio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_numero",
                table: "ordens_servico",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_prioridade_calculada_id",
                table: "ordens_servico",
                column: "prioridade_calculada_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_prioridade_id",
                table: "ordens_servico",
                column: "prioridade_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_prioridade_manual_id",
                table: "ordens_servico",
                column: "prioridade_manual_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_resultado_atual_id",
                table: "ordens_servico",
                column: "resultado_atual_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_solicitacao_id",
                table: "ordens_servico",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_subestacao_id",
                table: "ordens_servico",
                column: "subestacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_tipo_ocorrencia_id",
                table: "ordens_servico",
                column: "tipo_ocorrencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_transformador_id",
                table: "ordens_servico",
                column: "transformador_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_transformador_numero",
                table: "ordens_servico",
                column: "transformador_numero");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_versao_pontuacao_id",
                table: "ordens_servico",
                column: "versao_pontuacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_os_classes_cliente_classe_cliente_id",
                table: "os_classes_cliente",
                column: "classe_cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_os_recursos_recurso_id",
                table: "os_recursos",
                column: "recurso_id");

            migrationBuilder.CreateIndex(
                name: "ix_os_respostas_criterio_criterio_id",
                table: "os_respostas_criterio",
                column: "criterio_id");

            migrationBuilder.CreateIndex(
                name: "ix_os_respostas_criterio_opcao_id",
                table: "os_respostas_criterio",
                column: "opcao_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfil_claims_role_id",
                table: "perfil_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "perfis",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pontos_opcao_opcao_id",
                table: "pontos_opcao",
                column: "opcao_id");

            migrationBuilder.CreateIndex(
                name: "ix_prioridades_codigo",
                table: "prioridades",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_qualificacoes_codigo",
                table: "qualificacoes",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recursos_codigo",
                table: "recursos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regras_precedencia_codigo",
                table: "regras_precedencia",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regras_precedencia_prioridade_minima_id",
                table: "regras_precedencia",
                column: "prioridade_minima_id");

            migrationBuilder.CreateIndex(
                name: "ix_resultados_prioridade_ordem_servico_id_calculado_em",
                table: "resultados_prioridade",
                columns: new[] { "ordem_servico_id", "calculado_em" });

            migrationBuilder.CreateIndex(
                name: "ix_resultados_prioridade_prioridade_calculada_id",
                table: "resultados_prioridade",
                column: "prioridade_calculada_id");

            migrationBuilder.CreateIndex(
                name: "ix_resultados_prioridade_prioridade_id",
                table: "resultados_prioridade",
                column: "prioridade_id");

            migrationBuilder.CreateIndex(
                name: "ix_resultados_prioridade_versao_pontuacao_id",
                table: "resultados_prioridade",
                column: "versao_pontuacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_resultados_prioridade_itens_resultado_prioridade_id",
                table: "resultados_prioridade_itens",
                column: "resultado_prioridade_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_ucs_numero",
                table: "solicitacao_ucs",
                column: "numero");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_ucs_solicitacao_id",
                table: "solicitacao_ucs",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_ucs_unidade_consumidora_id",
                table: "solicitacao_ucs",
                column: "unidade_consumidora_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_chave_idempotencia",
                table: "solicitacoes",
                column: "chave_idempotencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_municipio_id_registrada_em",
                table: "solicitacoes",
                columns: new[] { "municipio_id", "registrada_em" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_numero",
                table: "solicitacoes",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_ordem_servico_id",
                table: "solicitacoes",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_registrada_por_id",
                table: "solicitacoes",
                column: "registrada_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_subestacoes_municipio_id_codigo",
                table: "subestacoes",
                columns: new[] { "municipio_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_ocorrencia_qualificacoes_qualificacao_id",
                table: "tipo_ocorrencia_qualificacoes",
                column: "qualificacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_tipos_ocorrencia_codigo",
                table: "tipos_ocorrencia",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_codigo_localidade_numero_local",
                table: "transformadores",
                columns: new[] { "codigo_localidade", "numero_local" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_conjunto_id",
                table: "transformadores",
                column: "conjunto_id");

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_localidade_id",
                table: "transformadores",
                column: "localidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_municipio_id",
                table: "transformadores",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_numero_completo",
                table: "transformadores",
                column: "numero_completo");

            migrationBuilder.CreateIndex(
                name: "ix_transformadores_subestacao_id",
                table: "transformadores",
                column: "subestacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidades_consumidoras_classe_cliente_id",
                table: "unidades_consumidoras",
                column: "classe_cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidades_consumidoras_municipio_id",
                table: "unidades_consumidoras",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidades_consumidoras_numero",
                table: "unidades_consumidoras",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidades_consumidoras_transformador_id",
                table: "unidades_consumidoras",
                column: "transformador_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_claims_user_id",
                table: "usuario_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_logins_user_id",
                table: "usuario_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_municipios_municipio_id",
                table: "usuario_municipios",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_perfis_role_id",
                table: "usuario_perfis",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "usuarios",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_equipe_id",
                table: "usuarios",
                column: "equipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_municipio_preferido_id",
                table: "usuarios",
                column: "municipio_preferido_id");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "usuarios",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_versao_criterios_criterio_id",
                table: "versao_criterios",
                column: "criterio_id");

            migrationBuilder.CreateIndex(
                name: "ix_versoes_pontuacao_municipio_id_numero",
                table: "versoes_pontuacao",
                columns: new[] { "municipio_id", "numero" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ux_versoes_rascunho_escopo",
                table: "versoes_pontuacao",
                column: "municipio_id",
                unique: true,
                filter: "status IN ('Rascunho', 'AguardandoAprovacao')")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_vinculos_solicitacao_ordem_servico_id",
                table: "vinculos_solicitacao",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_vinculos_solicitacao_ordem_servico_origem_id",
                table: "vinculos_solicitacao",
                column: "ordem_servico_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix_vinculos_solicitacao_solicitacao_id",
                table: "vinculos_solicitacao",
                column: "solicitacao_id");

            migrationBuilder.AddForeignKey(
                name: "fk_anexos_ordens_servico_ordem_servico_id",
                table: "anexos",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_comentarios_ordens_servico_ordem_servico_id",
                table: "comentarios",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_despachos_ordens_servico_ordem_servico_id",
                table: "despachos",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_historico_os_ordens_servico_ordem_servico_id",
                table: "historico_os",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordens_servico_resultados_prioridade_resultado_atual_id",
                table: "ordens_servico",
                column: "resultado_atual_id",
                principalTable: "resultados_prioridade",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_resultados_prioridade_ordens_servico_ordem_servico_id",
                table: "resultados_prioridade");

            migrationBuilder.DropTable(
                name: "anexos");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "ceps");

            migrationBuilder.DropTable(
                name: "comentarios");

            migrationBuilder.DropTable(
                name: "criterio_municipios");

            migrationBuilder.DropTable(
                name: "despachos");

            migrationBuilder.DropTable(
                name: "equipe_municipios");

            migrationBuilder.DropTable(
                name: "equipe_qualificacoes");

            migrationBuilder.DropTable(
                name: "equipe_recursos");

            migrationBuilder.DropTable(
                name: "faixas_prioridade");

            migrationBuilder.DropTable(
                name: "feriados");

            migrationBuilder.DropTable(
                name: "historico_os");

            migrationBuilder.DropTable(
                name: "integrantes");

            migrationBuilder.DropTable(
                name: "localizacoes_equipe");

            migrationBuilder.DropTable(
                name: "numeracao_sequencias");

            migrationBuilder.DropTable(
                name: "os_classes_cliente");

            migrationBuilder.DropTable(
                name: "os_condicoes_seguranca");

            migrationBuilder.DropTable(
                name: "os_recursos");

            migrationBuilder.DropTable(
                name: "os_respostas_criterio");

            migrationBuilder.DropTable(
                name: "perfil_claims");

            migrationBuilder.DropTable(
                name: "pontos_opcao");

            migrationBuilder.DropTable(
                name: "regras_precedencia");

            migrationBuilder.DropTable(
                name: "resultados_prioridade_itens");

            migrationBuilder.DropTable(
                name: "solicitacao_ucs");

            migrationBuilder.DropTable(
                name: "tipo_ocorrencia_qualificacoes");

            migrationBuilder.DropTable(
                name: "usuario_claims");

            migrationBuilder.DropTable(
                name: "usuario_logins");

            migrationBuilder.DropTable(
                name: "usuario_municipios");

            migrationBuilder.DropTable(
                name: "usuario_perfis");

            migrationBuilder.DropTable(
                name: "usuario_tokens");

            migrationBuilder.DropTable(
                name: "versao_criterios");

            migrationBuilder.DropTable(
                name: "vinculos_solicitacao");

            migrationBuilder.DropTable(
                name: "recursos");

            migrationBuilder.DropTable(
                name: "opcoes_criterio");

            migrationBuilder.DropTable(
                name: "unidades_consumidoras");

            migrationBuilder.DropTable(
                name: "qualificacoes");

            migrationBuilder.DropTable(
                name: "perfis");

            migrationBuilder.DropTable(
                name: "solicitacoes");

            migrationBuilder.DropTable(
                name: "criterios");

            migrationBuilder.DropTable(
                name: "classes_cliente");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "equipes");

            migrationBuilder.DropTable(
                name: "ordens_servico");

            migrationBuilder.DropTable(
                name: "resultados_prioridade");

            migrationBuilder.DropTable(
                name: "tipos_ocorrencia");

            migrationBuilder.DropTable(
                name: "transformadores");

            migrationBuilder.DropTable(
                name: "prioridades");

            migrationBuilder.DropTable(
                name: "versoes_pontuacao");

            migrationBuilder.DropTable(
                name: "conjuntos_eletricos");

            migrationBuilder.DropTable(
                name: "localidades");

            migrationBuilder.DropTable(
                name: "subestacoes");

            migrationBuilder.DropTable(
                name: "municipios");
        }
    }
}
