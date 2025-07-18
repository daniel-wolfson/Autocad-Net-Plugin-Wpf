using ID.Infrastructure.Core;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Data.Common;

namespace ID.Infrastructure.Migrations
{
    public abstract class MigrationEnsured : Migration
    {
        private MigrationBuilderEnsured _migrationBuilder;
        private DbConnection _dbConnenction;

        /// <summary> Setup migrations </summary>
        protected sealed override void Up(MigrationBuilder migrationBuilder)
        {
            _migrationBuilder = new MigrationBuilderEnsured(migrationBuilder);
            this.OnSetUp(_migrationBuilder);
        }

        /// <summary> Remove migrations, that built by setup </summary>
        protected sealed override void Down(MigrationBuilder migrationBuilder)
        {
            this.Down(_migrationBuilder);
        }

        /// <summary> On setup migrations </summary>
        private void OnSetUp(MigrationBuilderEnsured migrationBuilder)
        {
            using var initServiceScope = GeneralContext.CreateServiceScope();
            var initServiceProvider = initServiceScope.ServiceProvider;
            this.Init(migrationBuilder, initServiceProvider);
            this.Up(migrationBuilder);
            this.Down(migrationBuilder);
        }

        /// <summary> method "Init", called before "Up" </summary>
        protected virtual void Init(MigrationBuilderEnsured migrationBuilder, IServiceProvider serviceProvider) { }

        /// <summary> method "Up" calling migration </summary>
        protected abstract void Up(MigrationBuilderEnsured migrationBuilder);

        /// <summary> method "Down", removing data after "Up" and "Seed"</summary>
        protected virtual void Down(MigrationBuilderEnsured migrationBuilder) { }

        /// <summary> method "Exists", testing existing data by the straight direction to db</summary>
        protected bool Exists(string tableSchema, string tableName, string columnName = null, object columnValue = null)
        {
            var exists = _migrationBuilder.Exists(tableSchema, tableName, columnName, columnValue);
            return exists;
        }
    }
}
