﻿using System;
using System.Collections.Generic;
using Windows.Web.Http;
using System.Text;

namespace Yort.Http.ClientPipeline
{
	/// <summary>
	/// Extensions for <see cref="Windows.Web.Http.HttpStatusCode"/>.
	/// </summary>
	public static class WebHttpStatusCodeExtensions
	{
		/// <summary>
		/// Returns true if <paramref name="statusCode"/> represents any kind of redirect or moved response.
		/// </summary>
		/// <param name="statusCode">The status code to check.</param>
		/// <remarks>
		/// <para>Returns true if the <see cref="IsPermanentRedirect(HttpStatusCode)"/> returns true, or if the status is one of;</para>
		/// <list type="bullet">
		/// <item><see cref="System.Net.HttpStatusCode.Moved"/></item>
		/// <item><see cref="System.Net.HttpStatusCode.TemporaryRedirect"/></item>
		/// </list>
		/// </remarks>
		/// <returns>True if the status code represents a redirect or moved response.</returns>
		public static bool IsRedirect(this HttpStatusCode statusCode)
		{
			return IsPermanentRedirect(statusCode)
				|| statusCode == HttpStatusCode.MovedPermanently
				|| statusCode == HttpStatusCode.TemporaryRedirect;
		}

		/// <summary>
		/// Returns true if <paramref name="statusCode"/> represents any kind of permanent redirect or moved response.
		/// </summary>
		/// <param name="statusCode">The status code to check.</param>
		/// <remarks>
		/// <para>Returns true if the status is one of;</para>
		/// <list type="bullet">
		/// <item><see cref="System.Net.HttpStatusCode.MovedPermanently"/></item>
		/// <item><see cref="System.Net.HttpStatusCode.Redirect"/></item>
		/// </list>
		/// </remarks>
		/// <returns>True if the status code represents a permanent redirect or moved response.</returns>
		public static bool IsPermanentRedirect(this HttpStatusCode statusCode)
		{
			return statusCode == HttpStatusCode.MovedPermanently || statusCode == HttpStatusCode.PermanentRedirect;
		}
	}
}