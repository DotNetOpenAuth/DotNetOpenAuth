//-----------------------------------------------------------------------
// <copyright file="CryptoKeyCollisionException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Security.Permissions;

	/// <summary>
	/// Thrown by a hosting application or web site when a cryptographic key is created with a
	/// bucket and handle that conflicts with a previously stored and unexpired key.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Specialized exception has no need of a message parameter.")]
	[Serializable]
	public class CryptoKeyCollisionException : ArgumentException {
		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		public CryptoKeyCollisionException() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		/// <param name="inner">The inner exception to include.</param>
		public CryptoKeyCollisionException(Exception inner) : base(null, inner) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected CryptoKeyCollisionException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) {
			throw new NotImplementedException();
		}

#if false
		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
		/// </PermissionSet>
		[System.Security.SecurityCritical]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
			base.GetObjectData(info, context);
			throw new NotImplementedException();
		}
#endif
	}
}
