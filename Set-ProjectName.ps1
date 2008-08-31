param ($libraryName = { throw "-libraryName required" } )

dir -rec . *YOURLIBNAME* |% { ren $_.FullName $_.Name.Replace("YOURLIBNAME", $libraryName) -whatif }

