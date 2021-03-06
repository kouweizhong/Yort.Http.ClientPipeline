﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yort.Http.ClientPipeline
{
	/// <summary>
	/// Automatically (gzip) compresses request contents sets the content encoding header to gzip.
	/// </summary>
	public sealed class CompressedRequestHandler : System.Net.Http.DelegatingHandler
	{
		#region Fields

		private IRequestCondition _RequestCondition;

		#endregion

		#region Constructors

		/// <summary>
		/// Partial constructor.
		/// </summary>
		/// <param name="innerHandler">The next handler in the pipeline to pass requests through.</param>
		public CompressedRequestHandler(System.Net.Http.HttpMessageHandler innerHandler) : this(innerHandler, null)
		{
		}	

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="innerHandler">The next handler in the pipeline to pass requests through.</param>
		/// <param name="requestCondition">A <see cref="IRequestCondition"/> that controls whether any individual request is compressed or not. If null, all requests are compressed.</param>
		public CompressedRequestHandler(System.Net.Http.HttpMessageHandler innerHandler, IRequestCondition requestCondition) : base(innerHandler)
		{
			_RequestCondition = requestCondition;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Compresses the content if suitable, sets the content encoding header and passes on the request to the next handler in the pipeline.
		/// </summary>
		/// <param name="request">The <see cref="System.Net.Http.HttpRequestMessage"/> to send.</param>
		/// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the request.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task{T}"/> whose result is the <see cref="System.Net.Http.HttpResponseMessage"/> from the server.</returns>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (ShouldCompress(request))
				request.Content = await GetCompressedContent(request.Content).ConfigureAwait(false);
			
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		#endregion

		#region Private Methods

		private static async Task<HttpContent> GetCompressedContent(HttpContent originalContent)
		{
			var ms = new System.IO.MemoryStream();
			try
			{
				await CompressOriginalContentStream(originalContent, ms).ConfigureAwait(false);

				ms.Seek(0, System.IO.SeekOrigin.Begin);

				var compressedContent = new StreamContent(ms);
				originalContent.CopyHeadersTo(compressedContent);
				compressedContent.Headers.ContentEncoding.Clear();
				compressedContent.Headers.ContentEncoding.Add("gzip");
				compressedContent.Headers.ContentLength = ms.Length;

				originalContent.Dispose();
				
				return compressedContent;
			}
			catch
			{
				ms?.Dispose();
				throw;
			}
		}

		private static async Task CompressOriginalContentStream(HttpContent originalContent, System.IO.MemoryStream ms)
		{
			using (var compressingStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
			{
				using (var originalContentStream = await originalContent.ReadAsStreamAsync().ConfigureAwait(false))
				{
					originalContentStream.CopyTo(compressingStream);
				}
				compressingStream.Flush();
			}
		}

		private bool ShouldCompress(HttpRequestMessage request)
		{
			return request.Content != null 
				&& !(request.Content?.Headers?.ContentEncoding?.Contains("gzip") ?? false)
				&& !(request.Content?.Headers?.ContentEncoding?.Contains("deflate") ?? false)
				&& (_RequestCondition?.ShouldProcess(request) ?? true);
		}

		#endregion

	}
}