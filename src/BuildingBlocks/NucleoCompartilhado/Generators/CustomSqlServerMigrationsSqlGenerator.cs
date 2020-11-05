using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace NucleoCompartilhado.Generators
{
    public sealed class CustomSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        internal const string DatabaseCollationName = "SQL_Latin1_General_CP1_CI_AI";

        public CustomSqlServerMigrationsSqlGenerator(
              MigrationsSqlGeneratorDependencies dependencies,
              IMigrationsAnnotationProvider migrationsAnnotations)
             : base(dependencies, migrationsAnnotations) { }

        protected override void Generate(SqlServerCreateDatabaseOperation operation,
             IModel model,
             MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            builder
                 .Append("ALTER DATABASE ")
                 .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                 .Append(" COLLATE ")
                 .Append(DatabaseCollationName)
                 .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                 .EndCommand(suppressTransaction: true);
        }
    }
}
