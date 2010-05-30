//-----------------------------------------------------------------------
// <copyright file="IOAuthDirectResponseFormat.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IOAuthDirectResponseFormat {
		ResponseFormat Format { get; }
	}
}
