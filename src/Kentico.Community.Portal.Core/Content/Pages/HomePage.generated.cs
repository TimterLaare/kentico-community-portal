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
using CMS.Websites;

namespace Kentico.Community.Portal.Core.Content
{
	/// <summary>
	/// Represents a page of type <see cref="HomePage"/>.
	/// </summary>
	public partial class HomePage : IWebPageFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "KenticoCommunity.HomePage";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// DocumentName.
		/// </summary>
		public string DocumentName { get; set; }


		/// <summary>
		/// HomePageTitle.
		/// </summary>
		public string HomePageTitle { get; set; }


		/// <summary>
		/// HomePageShortDescription.
		/// </summary>
		public string HomePageShortDescription { get; set; }
	}
}