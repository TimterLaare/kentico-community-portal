//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at https://docs.xperience.io/.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS.ContentEngine;

namespace Kentico.Community.Portal.Core.Content
{
	/// <summary>
	/// Defines a contract for content types with the <see cref="IMediaItem"/> reusable schema assigned.
	/// </summary>
	public interface IMediaItem
	{
		/// <summary>
		/// Code name of the reusable field schema.
		/// </summary>
		public const string REUSABLE_FIELD_SCHEMA_NAME = "MediaItem";


		/// <summary>
		/// MediaItemTitle.
		/// </summary>
		public string MediaItemTitle { get; set; }


		/// <summary>
		/// MediaItemShortDescription.
		/// </summary>
		public string MediaItemShortDescription { get; set; }


		/// <summary>
		/// MediaItemTaxonomy.
		/// </summary>
		public IEnumerable<TagReference> MediaItemTaxonomy { get; set; }


		/// <summary>
		/// MediaItemAssetWidth.
		/// </summary>
		public int MediaItemAssetWidth { get; set; }


		/// <summary>
		/// MediaItemAssetHeight.
		/// </summary>
		public int MediaItemAssetHeight { get; set; }
	}
}