//-----------------------------------------------------------------------
// <copyright file="CryptoKeyCollisionException.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Security.Permissions;

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
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		public CryptoKeyCollisionException(string message) : base(message) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		/// <param name="inner">The inner exception to include.</param>
		public CryptoKeyCollisionException(string message, Exception inner) : base(message, inner) {
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
#if CLR4
		[SecurityCritical]
#else
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
			base.GetObjectData(info, context);
			throw new NotImplementedException();
		}
	}
}
