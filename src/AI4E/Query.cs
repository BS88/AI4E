/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        Query.cs 
 * Types:           AI4E.Integration.Query'1
 *                  AI4E.Integration.ByIdQuery'2
 *                  AI4E.Integration.ByIdQuery'1
 *                  AI4E.Integration.ByParentQuery'3
 *                  AI4E.Integration.ByParentQuery'2
 *                  AI4E.Integration.IQueryHandler'1
 *                  AI4E.Integration.IByIdQueryHandler'2
 *                  AI4E.Integration.IByIdQueryHandler'1
 *                  AI4E.Integration.IByParentQueryHandler'3
 *                  AI4E.Integration.IByParentQueryHandler'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   17.07.2017 
 * Status:          Ready
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://gitlab.com/EnterpriseApplicationEquipment/AI4E)
 * Copyright (c) 2017 Andreas Trütschel.
 * 
 * AI4E is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * AI4E is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a query of the specified result without any conditions.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public class Query<TResult> { }

    /// <summary>
    /// Represents a query of the specified result that is identified by id.
    /// </summary>
    /// <typeparam name="TId">The type of id.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public class ByIdQuery<TId, TResult>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ByIdQuery{TId, TResult}"/>.
        /// </summary>
        public ByIdQuery() { } // TODO: Make the query immutable?

        /// <summary>
        /// Creates a new instance of the <see cref="ByIdQuery{TId, TResult}"/> with the specified id.
        /// </summary>
        /// <param name="id">The id that identifies the result.</param>
        public ByIdQuery(TId id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the id that identifies the result.
        /// </summary>
        public TId Id { get; set; } // TODO: Make the query immutable?
    }

    /// <summary>
    /// Represents a query of the specified result that is identified by id.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public class ByIdQuery<TResult>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ByIdQuery{TResult}"/>.
        /// </summary>
        public ByIdQuery() { } // TODO: Make the query immutable?

        /// <summary>
        /// Creates a new instance of the <see cref="ByIdQuery{TResult}"/> with the specified id.
        /// </summary>
        /// <param name="id">The id that identifies the result.</param>
        public ByIdQuery(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the id that identifies the result.
        /// </summary>
        public Guid Id { get; set; } // TODO: Make the query immutable?
    }

    /// <summary>
    /// Represents a query of the specified result that is identified by its parent.
    /// </summary>
    /// <typeparam name="TId">The type of id.</typeparam>
    /// <typeparam name="TParent">The type of parent.</typeparam>
    /// <typeparam name="TResult">The type of parent.</typeparam>
    public class ByParentQuery<TId, TParent, TResult>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ByParentQuery{TId, TParent, TResult}"/>.
        /// </summary>
        public ByParentQuery() { } // TODO: Make the query immutable?

        /// <summary>
        /// Creates a new instance of the <see cref="ByParentQuery{TId, TParent, TResult}"/> with the specified parent-id.
        /// </summary>
        /// <param name="parentId">The id that identifies the results parent.</param>
        public ByParentQuery(TId parentId)
        {
            ParentId = parentId;
        }

        /// <summary>
        /// Gets or sets the id that identifies the results parent.
        /// </summary>
        public TId ParentId { get; set; } // TODO: Make the query immutable?
    }

    /// <summary>
    /// Represents a query of the specified result that is identified by its parent.
    /// </summary>
    /// <typeparam name="TParent">The type of parent.</typeparam>
    /// <typeparam name="TResult">The type of parent.</typeparam>
    public class ByParentQuery<TParent, TResult>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ByParentQuery{TParent, TResult}"/>.
        /// </summary>
        public ByParentQuery() { } // TODO: Make the query immutable?

        /// <summary>
        /// Creates a new instance of the <see cref="ByParentQuery{TParent, TResult}"/> with the specified parent-id.
        /// </summary>
        /// <param name="parentId">The id that identifies the results parent.</param>
        public ByParentQuery(Guid parentId)
        {
            ParentId = parentId;
        }

        /// <summary>
        /// Gets or sets the id that identifies the results parent.
        /// </summary>
        public Guid ParentId { get; set; } // TODO: Make the query immutable?
    }

    /// <summary>
    /// Represents a query handler that handles queries of type <see cref="ByIdQuery{TId, TResult}"/>.
    /// </summary>
    /// <typeparam name="TId">The type of id.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    [Obsolete]
    public interface IByIdQueryHandler<TId, TResult> : IQueryHandler<ByIdQuery<TId, TResult>>
        where TId : struct, IEquatable<TId>
    { }

    /// <summary>
    /// Represents a query handler that handles queries of type <see cref="ByIdQuery{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    [Obsolete]
    public interface IByIdQueryHandler<TResult> : IQueryHandler<ByIdQuery<TResult>> { }

    /// <summary>
    /// Represents a query handler that handles queries of type <see cref="IByParentQueryHandler{TId, TParent, TResult}"/>.
    /// </summary>
    /// <typeparam name="TId">The type of id.</typeparam>
    /// <typeparam name="TParent">The type of parent.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    [Obsolete]
    public interface IByParentQueryHandler<TId, TParent, TResult> : IQueryHandler<ByParentQuery<TId, TParent, TResult>>
        where TId : struct, IEquatable<TId>
    { }

    /// <summary>
    /// Represents a query handler that handlers queries of type <see cref="IByParentQueryHandler{TParent, TResult}"/>.
    /// </summary>
    /// <typeparam name="TParent">The type of parent.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    [Obsolete]
    public interface IByParentQueryHandler<TParent, TResult> : IQueryHandler<ByParentQuery<TParent, TResult>> { }
}
