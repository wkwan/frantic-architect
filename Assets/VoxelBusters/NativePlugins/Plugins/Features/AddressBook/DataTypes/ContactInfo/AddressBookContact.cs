using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;
using DownloadTexture = VoxelBusters.Utility.DownloadTexture;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Data container for each contact detail.
	/// </summary>
	[System.Serializable]
	public class AddressBookContact
	{
		#region Properties
		
		[SerializeField]
		private string 				m_firstName;

		/// <summary>
		///First Name of the contact.
		///	</summary>
		public string 				FirstName
		{
			get { return m_firstName; }
			protected set { m_firstName = value; }
		}

		[SerializeField]
		private string 				m_lastName;

		/// <summary>
		///Last Name of the contact.
		///	</summary>
		public string 				LastName
		{
			get { return m_lastName; }
			protected set { m_lastName = value; }
		}

		[SerializeField]
		private string				m_imagePath;

		/// <summary>
		///Absolute image path of the contact.
		///	</summary>
		public string 				ImagePath
		{
			get { return m_imagePath; }
			protected set { m_imagePath = value; }
		}

		[SerializeField]
		private List<string>		m_phoneNumberList;

		/// <summary>
		/// List of phone numbers in this contact.
		/// </summary>
		public List<string> 		PhoneNumberList
		{
			get { return m_phoneNumberList; }
			protected set { m_phoneNumberList = value; }
		}

		[SerializeField]
		private List<string>		m_emailIDList;

		/// <summary>
		/// List of Email ID's in this contact.
		/// </summary>
		public List<string> 		EmailIDList
		{
			get { return m_emailIDList; }
			protected set { m_emailIDList = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressBookContact"/> class.
		/// </summary>
		public AddressBookContact ()
		{
			FirstName		= string.Empty;
			LastName		= string.Empty;
			ImagePath		= string.Empty;
			PhoneNumberList	= new List<string>();
			EmailIDList		= new List<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressBookContact"/> class by details from _source.
		/// </summary>
		/// <param name="_source">Source to copy from.</param>
		public AddressBookContact (AddressBookContact _source)
		{
			FirstName		= _source.FirstName;
			LastName		= _source.LastName;
			ImagePath		= _source.ImagePath;
			PhoneNumberList	= _source.PhoneNumberList;
			EmailIDList		= _source.EmailIDList;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Helper for getting Texture from image path.
		/// </summary>
		/// <param name="_onCompletion">Callback to be triggered after downloading the image.</param>
		public void GetImageAsync (DownloadTexture.Completion _onCompletion)
		{
			URL _imagePathURL				= URL.FileURLWithPath(ImagePath);

			// Download image from given path
			DownloadTexture _newDownload	= new DownloadTexture(_imagePathURL, true, true);
			_newDownload.OnCompletion		= _onCompletion;

			// Start download
			_newDownload.StartRequest();
		}

		/// <summary>
		/// String representation of <see cref="AddressBookContact"/>.
		/// </summary>
		public override string ToString ()
		{
			System.Text.StringBuilder _builder	= new System.Text.StringBuilder();

			// Append first name, last name and icon
			_builder.AppendFormat("[AddressBookContact: FirstName={0}, LastName={1}, ImagePath={2}, ", FirstName, LastName, ImagePath);

			// Append mobile numbers
			_builder.AppendFormat("PhoneNumberList={0}", PhoneNumberList.ToJSON());

			// Append email id's
			_builder.AppendFormat("EmailIdList={0}]", EmailIDList.ToJSON());

			return _builder.ToString();
		}

		#endregion
	}
}