#region

using System;
using Intellidesk.AcadNet.Data.Repositories;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;

#endregion

namespace Intellidesk.Data.Repositories.EF6.Factories
{
    /// <summary>
    ///     Interface for a class that can provide repositories by type.
    ///     The class may create the repositories dynamically if it is unable
    ///     to find one in its cache of repositories.
    /// </summary>
    /// <remarks>
    ///     Repositories created by this provider tend to require a <see cref="Intellidesk.Data.Repositories.EF6.DataContext.DataContext" />
    ///     to retrieve data.
    /// </remarks>
    public interface IRepositoryProvider
    {
        /// <summary>
        ///     Get and set the <see cref="DataContext" /> with which to initialize a repository
        ///     if one must be created.
        /// </summary>
        IDataContextAsync DataContext { get; set; }

        IUnitOfWorkAsync UnitOfWork { get; set; }

        /// <summary>
        ///     Get an <see cref="IRepository{T}" /> for entity type, T.
        /// </summary>
        /// <typeparam name="T">
        ///     Root entity type of the <see cref="IRepository{T}" />.
        /// </typeparam>
        IRepositoryAsync<T> GetRepositoryForEntityType<T>() where T : class, IObjectState;

        IXmlRepositoryAsync<T> GetXmlRepositoryForEntityType<T>() where T : class, IObjectState;

        IJsonRepositoryAsync<T> GetJsonRepositoryForEntityType<T>() where T : class, IObjectState;

        /// <summary>
        ///     Get a repository of type T.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the repository, typically a custom repository interface.
        /// </typeparam>
        /// <param name="factory">
        ///     An optional repository creation function that takes a <see cref="EF6.DataContext" />
        ///     and returns a repository of T. Used if the repository must be created.
        /// </param>
        /// <remarks>
        ///     Looks for the requested repository in its cache, returning if found.
        ///     If not found, tries to make one with the factory, fallingback to
        ///     a default factory if the factory parameter is null.
        /// </remarks>
        T GetRepository<T>(Func<IDataContextAsync, IUnitOfWorkAsync, dynamic> factory = null) where T : class;

        /// <summary>
        ///     Set the repository to return from this provider.
        /// </summary>
        /// <remarks>
        ///     Set a repository if you don't want this provider to create one.
        ///     Useful in testing and when developing without a backend
        ///     implementation of the object returned by a repository of type T.
        /// </remarks>
        void SetRepository<T>(T repository);
    }
}