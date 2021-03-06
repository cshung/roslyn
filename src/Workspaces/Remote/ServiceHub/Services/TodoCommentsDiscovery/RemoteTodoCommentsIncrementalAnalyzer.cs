﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.TodoComments;

namespace Microsoft.CodeAnalysis.Remote
{
    internal sealed class RemoteTodoCommentsIncrementalAnalyzer : AbstractTodoCommentsIncrementalAnalyzer
    {
        /// <summary>
        /// Channel back to VS to inform it of the designer attributes we discover.
        /// </summary>
        private readonly RemoteCallback<ITodoCommentsListener> _callback;

        public RemoteTodoCommentsIncrementalAnalyzer(RemoteCallback<ITodoCommentsListener> callback)
            => _callback = callback;

        protected override async ValueTask ReportTodoCommentDataAsync(DocumentId documentId, ImmutableArray<TodoCommentData> data, CancellationToken cancellationToken)
        {
            // cancel whenever the analyzer runner cancels or the client disconnects and the request is canceled:
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _callback.ClientDisconnectedSource.Token);

            await _callback.InvokeAsync(
                (callback, cancellationToken) => callback.ReportTodoCommentDataAsync(documentId, data, cancellationToken),
                linkedSource.Token).ConfigureAwait(false);
        }
    }
}
